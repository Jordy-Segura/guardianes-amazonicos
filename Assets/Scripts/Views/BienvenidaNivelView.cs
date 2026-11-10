using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace GuardianesAmazonicos.Views
{
    public sealed class BienvenidaNivelView : MonoBehaviour, IBienvenidaView
    {
        private const string NombreVideoBienvenidaSenas = "BienvenidaLSEC.mp4";

        [Header("Contenido")]
        [SerializeField] private TMP_Text _textoBienvenida;
        [SerializeField] private TMP_Text _textoProgreso;
        [SerializeField] private TMP_Text _textoEstadoAudio;
        [SerializeField] private TMP_Text _textoEstadoSenas;
        [SerializeField] private TMP_Text _textoComenzar;

        [Header("Controles")]
        [SerializeField] private Button _botonComenzar;
        [SerializeField] private Button _botonAudio;
        [SerializeField] private Button _botonSenas;
        [SerializeField] private Button _botonCerrarSenas;
        [SerializeField] private Button _botonReproducirSenas;

        [Header("Canales multimodales")]
        [SerializeField] private AudioSource _fuenteAudio;
        [SerializeField] private VideoPlayer _reproductorSenas;
        [SerializeField] private GameObject _panelSenas;

        private Coroutine _actualizarEstadoAudio;

        public event Action OnComenzarSolicitado;
        public event Action OnAudioSolicitado;
        public event Action OnSenasSolicitadas;
        public event Action OnCerrarSenasSolicitado;
        public event Action OnReproducirSenasSolicitado;

        private void OnEnable()
        {
            if (_botonComenzar != null) _botonComenzar.onClick.AddListener(EmitirComenzar);
            if (_botonAudio != null) _botonAudio.onClick.AddListener(EmitirAudio);
            if (_botonSenas != null) _botonSenas.onClick.AddListener(EmitirSenas);
            if (_botonCerrarSenas != null) _botonCerrarSenas.onClick.AddListener(EmitirCerrarSenas);
            if (_botonReproducirSenas != null) _botonReproducirSenas.onClick.AddListener(EmitirReproducirSenas);
        }

        private void OnDisable()
        {
            if (_botonComenzar != null) _botonComenzar.onClick.RemoveListener(EmitirComenzar);
            if (_botonAudio != null) _botonAudio.onClick.RemoveListener(EmitirAudio);
            if (_botonSenas != null) _botonSenas.onClick.RemoveListener(EmitirSenas);
            if (_botonCerrarSenas != null) _botonCerrarSenas.onClick.RemoveListener(EmitirCerrarSenas);
            if (_botonReproducirSenas != null) _botonReproducirSenas.onClick.RemoveListener(EmitirReproducirSenas);

            if (_fuenteAudio != null && _fuenteAudio.isPlaying)
            {
                _fuenteAudio.Stop();
            }

            if (_reproductorSenas != null && _reproductorSenas.isPlaying)
            {
                _reproductorSenas.Stop();
            }

            if (_reproductorSenas != null)
            {
                _reproductorSenas.prepareCompleted -= ReproducirVideoPreparado;
            }
        }

        public void MostrarProgreso(int puntoActual, int totalPuntos, bool aventuraIniciada)
        {
            if (_textoProgreso != null)
            {
                _textoProgreso.text = aventuraIniciada
                    ? puntoActual + " de " + totalPuntos
                    : "0 de " + totalPuntos;
            }

            if (_textoComenzar != null)
            {
                _textoComenzar.text = aventuraIniciada ? "¡Aventura iniciada!" : "¡Comenzar!";
            }

            if (_botonComenzar != null)
            {
                _botonComenzar.interactable = !aventuraIniciada;
            }
        }

        public void ReproducirBienvenida()
        {
            if (_fuenteAudio == null || _fuenteAudio.clip == null)
            {
                MostrarEstadoAudio("La locución de bienvenida aún no está disponible.");
                return;
            }

            if (_fuenteAudio.isPlaying)
            {
                _fuenteAudio.Stop();
            }

            _fuenteAudio.Play();
            MostrarEstadoAudio("Reproduciendo la bienvenida...");

            if (_actualizarEstadoAudio != null)
            {
                StopCoroutine(_actualizarEstadoAudio);
            }

            _actualizarEstadoAudio = StartCoroutine(EsperarFinDeAudio());
        }

        public void MostrarPanelSenas()
        {
            if (_panelSenas != null)
            {
                _panelSenas.SetActive(true);
                _panelSenas.transform.SetAsLastSibling();
            }

            bool tieneVideo = ConfigurarVideoLocalSenas();
            if (_botonReproducirSenas != null)
            {
                _botonReproducirSenas.interactable = tieneVideo;
            }

            if (_textoEstadoSenas != null)
            {
                _textoEstadoSenas.text = tieneVideo
                    ? "Bienvenida disponible en LSEC."
                    : "Video de bienvenida en LSEC pendiente de grabación. La ventana reproducirá un archivo local cuando se incorpore.";
            }
        }

        public void CerrarPanelSenas()
        {
            if (_reproductorSenas != null && _reproductorSenas.isPlaying)
            {
                _reproductorSenas.Stop();
            }

            if (_panelSenas != null)
            {
                _panelSenas.SetActive(false);
            }
        }

        public void ReproducirVideoSenas()
        {
            if (_reproductorSenas == null)
            {
                MostrarPanelSenas();
                return;
            }

            _reproductorSenas.Stop();
            if (_reproductorSenas.clip != null)
            {
                _reproductorSenas.Play();
                return;
            }

            if (!ConfigurarVideoLocalSenas())
            {
                MostrarPanelSenas();
                return;
            }

            _reproductorSenas.prepareCompleted -= ReproducirVideoPreparado;
            _reproductorSenas.prepareCompleted += ReproducirVideoPreparado;
            _reproductorSenas.Prepare();
        }

        private bool ConfigurarVideoLocalSenas()
        {
            if (_reproductorSenas == null)
            {
                return false;
            }

            if (_reproductorSenas.clip != null)
            {
                return true;
            }

            string rutaVideo = Path.Combine(Application.streamingAssetsPath, "LSEC", NombreVideoBienvenidaSenas);
            if (!File.Exists(rutaVideo))
            {
                return false;
            }

            _reproductorSenas.source = VideoSource.Url;
            _reproductorSenas.url = rutaVideo;
            return true;
        }

        private void ReproducirVideoPreparado(VideoPlayer reproductor)
        {
            reproductor.prepareCompleted -= ReproducirVideoPreparado;
            reproductor.Play();
        }

        private IEnumerator EsperarFinDeAudio()
        {
            while (_fuenteAudio != null && _fuenteAudio.isPlaying)
            {
                yield return null;
            }

            MostrarEstadoAudio("Puedes volver a escuchar la bienvenida cuando quieras.");
            _actualizarEstadoAudio = null;
        }

        private void MostrarEstadoAudio(string mensaje)
        {
            if (_textoEstadoAudio != null)
            {
                _textoEstadoAudio.text = mensaje;
            }
        }

        private void EmitirComenzar() { OnComenzarSolicitado?.Invoke(); }
        private void EmitirAudio() { OnAudioSolicitado?.Invoke(); }
        private void EmitirSenas() { OnSenasSolicitadas?.Invoke(); }
        private void EmitirCerrarSenas() { OnCerrarSenasSolicitado?.Invoke(); }
        private void EmitirReproducirSenas() { OnReproducirSenasSolicitado?.Invoke(); }
    }
}
