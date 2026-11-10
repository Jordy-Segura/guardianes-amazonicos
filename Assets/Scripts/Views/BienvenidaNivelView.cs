using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace GuardianesAmazonicos.Views
{
    /// <summary>
    /// Vista pasiva de la pantalla «Bienvenida e inicio de cada nivel» de la guía:
    /// tarjeta de presentación, botón ¡Comenzar!, cabecera con Lectura y Señas, y
    /// ventana de video. La jerarquía se construye en el editor según la plantilla.
    /// </summary>
    public sealed class BienvenidaNivelView : MonoBehaviour, IBienvenidaView
    {
        private const string NombreVideoBienvenidaSenas = "BienvenidaLSEC.mp4";

        [Header("Contenido")]
        [SerializeField] private TMP_Text _textoProgreso;
        [SerializeField] private TMP_Text _textoEstadoSenas;

        [Header("Controles")]
        [SerializeField] private Button _botonComenzar;
        [SerializeField] private Button _botonAudio;
        [SerializeField] private Button _botonSenas;
        [SerializeField] private Button _botonCerrarSenas;
        [SerializeField] private Button _botonReproducirSenas;
        [SerializeField] private Button _botonRegresar;
        [SerializeField] private Button _botonAvanzar;
        [SerializeField] private Button _botonInicio;
        [SerializeField] private Button _botonConfiguracion;

        [Header("Canales multimodales")]
        [SerializeField] private AudioSource _fuenteAudio;
        [SerializeField] private VideoPlayer _reproductorSenas;
        [SerializeField] private RawImage _pantallaVideo;
        [SerializeField] private GameObject _panelSenas;
        [SerializeField] private CanvasGroup _capaBase;

        public event Action OnComenzarSolicitado;
        public event Action OnAudioSolicitado;
        public event Action OnSenasSolicitadas;
        public event Action OnCerrarSenasSolicitado;
        public event Action OnReproducirSenasSolicitado;
        public event Action OnRegresarSolicitado;
        public event Action OnAvanzarSolicitado;
        public event Action OnInicioSolicitado;
        public event Action OnConfiguracionSolicitada;

        private void OnEnable()
        {
            Escuchar(_botonComenzar, EmitirComenzar);
            Escuchar(_botonAudio, EmitirAudio);
            Escuchar(_botonSenas, EmitirSenas);
            Escuchar(_botonCerrarSenas, EmitirCerrarSenas);
            Escuchar(_botonReproducirSenas, EmitirReproducirSenas);
            Escuchar(_botonRegresar, EmitirRegresar);
            Escuchar(_botonAvanzar, EmitirAvanzar);
            Escuchar(_botonInicio, EmitirInicio);
            Escuchar(_botonConfiguracion, EmitirConfiguracion);
        }

        private void OnDisable()
        {
            Olvidar(_botonComenzar, EmitirComenzar);
            Olvidar(_botonAudio, EmitirAudio);
            Olvidar(_botonSenas, EmitirSenas);
            Olvidar(_botonCerrarSenas, EmitirCerrarSenas);
            Olvidar(_botonReproducirSenas, EmitirReproducirSenas);
            Olvidar(_botonRegresar, EmitirRegresar);
            Olvidar(_botonAvanzar, EmitirAvanzar);
            Olvidar(_botonInicio, EmitirInicio);
            Olvidar(_botonConfiguracion, EmitirConfiguracion);

            if (_fuenteAudio != null && _fuenteAudio.isPlaying) _fuenteAudio.Stop();
            if (_reproductorSenas != null)
            {
                _reproductorSenas.prepareCompleted -= ReproducirVideoPreparado;
                if (_reproductorSenas.isPlaying) _reproductorSenas.Stop();
            }
        }

        private void Start()
        {
            Enfocar(_botonComenzar);
        }

        public void MostrarProgreso(int escenaActual, int totalEscenas)
        {
            if (_textoProgreso != null)
            {
                _textoProgreso.text = RetoSeleccionView.FormatearProgreso(escenaActual, totalEscenas);
            }
        }

        public void PermitirNavegacion(bool regresar, bool avanzar)
        {
            if (_botonRegresar != null) _botonRegresar.interactable = regresar;
            if (_botonAvanzar != null) _botonAvanzar.interactable = avanzar;
        }

        /// <summary>Lectura: reproduce o detiene la locución de bienvenida.</summary>
        public void ReproducirBienvenida()
        {
            if (_fuenteAudio == null || _fuenteAudio.clip == null) return;
            if (_fuenteAudio.isPlaying)
            {
                _fuenteAudio.Stop();
                return;
            }

            _fuenteAudio.Play();
        }

        public void DetenerAudio()
        {
            if (_fuenteAudio != null && _fuenteAudio.isPlaying) _fuenteAudio.Stop();
        }

        public void MostrarPanelSenas()
        {
            DetenerAudio();
            if (_panelSenas != null) _panelSenas.SetActive(true);
            if (_capaBase != null)
            {
                _capaBase.interactable = false;
                _capaBase.blocksRaycasts = false;
            }

            bool tieneVideo = ConfigurarVideoLocalSenas();
            if (_botonReproducirSenas != null) _botonReproducirSenas.interactable = tieneVideo;
            if (_pantallaVideo != null) _pantallaVideo.enabled = false;
            if (_textoEstadoSenas != null)
            {
                _textoEstadoSenas.text = tieneVideo
                    ? "Toca reproducir para ver la bienvenida en lengua de señas."
                    : "El video de bienvenida en lengua de señas está pendiente de grabación con la intérprete aprobada.";
            }

            Enfocar(tieneVideo ? _botonReproducirSenas : _botonCerrarSenas);
        }

        public void CerrarPanelSenas()
        {
            if (_reproductorSenas != null && (_reproductorSenas.isPlaying || _reproductorSenas.isPaused))
            {
                _reproductorSenas.Stop();
            }

            if (_pantallaVideo != null) _pantallaVideo.enabled = false;
            if (_panelSenas != null) _panelSenas.SetActive(false);
            if (_capaBase != null)
            {
                _capaBase.interactable = true;
                _capaBase.blocksRaycasts = true;
            }

            Enfocar(_botonComenzar);
        }

        /// <summary>Reproducir / pausar el video local de LSEC.</summary>
        public void ReproducirVideoSenas()
        {
            if (_reproductorSenas == null || !ConfigurarVideoLocalSenas()) return;

            if (_reproductorSenas.isPlaying)
            {
                _reproductorSenas.Pause();
                return;
            }

            if (_pantallaVideo != null) _pantallaVideo.enabled = true;
            if (_textoEstadoSenas != null) _textoEstadoSenas.text = string.Empty;
            if (_reproductorSenas.isPrepared)
            {
                _reproductorSenas.Play();
                return;
            }

            _reproductorSenas.prepareCompleted -= ReproducirVideoPreparado;
            _reproductorSenas.prepareCompleted += ReproducirVideoPreparado;
            _reproductorSenas.Prepare();
        }

        private bool ConfigurarVideoLocalSenas()
        {
            if (_reproductorSenas == null) return false;
            if (_reproductorSenas.clip != null) return true;

            string rutaVideo = Path.Combine(Application.streamingAssetsPath, "LSEC", NombreVideoBienvenidaSenas);
            if (!File.Exists(rutaVideo)) return false;

            _reproductorSenas.source = VideoSource.Url;
            _reproductorSenas.url = rutaVideo;
            return true;
        }

        private void ReproducirVideoPreparado(VideoPlayer reproductor)
        {
            reproductor.prepareCompleted -= ReproducirVideoPreparado;
            reproductor.Play();
        }

        private static void Enfocar(Selectable control)
        {
            if (!Application.isPlaying || control == null || EventSystem.current == null) return;
            if (!control.gameObject.activeInHierarchy || !control.interactable) return;
            EventSystem.current.SetSelectedGameObject(control.gameObject);
        }

        private static void Escuchar(Button boton, UnityAction accion)
        {
            if (boton != null) boton.onClick.AddListener(accion);
        }

        private static void Olvidar(Button boton, UnityAction accion)
        {
            if (boton != null) boton.onClick.RemoveListener(accion);
        }

        private void EmitirComenzar() { OnComenzarSolicitado?.Invoke(); }
        private void EmitirAudio() { OnAudioSolicitado?.Invoke(); }
        private void EmitirSenas() { OnSenasSolicitadas?.Invoke(); }
        private void EmitirCerrarSenas() { OnCerrarSenasSolicitado?.Invoke(); }
        private void EmitirReproducirSenas() { OnReproducirSenasSolicitado?.Invoke(); }
        private void EmitirRegresar() { OnRegresarSolicitado?.Invoke(); }
        private void EmitirAvanzar() { OnAvanzarSolicitado?.Invoke(); }
        private void EmitirInicio() { OnInicioSolicitado?.Invoke(); }
        private void EmitirConfiguracion() { OnConfiguracionSolicitada?.Invoke(); }
    }
}
