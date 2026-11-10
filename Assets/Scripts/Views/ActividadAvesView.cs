using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace GuardianesAmazonicos.Views
{
    /// <summary>
    /// HU-08 - Escena 8 del Nivel 1. Asignada a: Jirleidy Raquel Santos Muentes (Sprint 1).
    /// Vista pasiva de la actividad de aves: muestra el enunciado escrito, la imagen
    /// de apoyo, tres tarjetas de respuesta y retroalimentación. Expone audio (LECTURA),
    /// video LSEC (SEÑAS) y pista (AYUDA) conforme al diseño multimodal DUA.
    /// </summary>
    public sealed class ActividadAvesView : MonoBehaviour, IActividadAvesView
    {
        private const string NombreVideoSenasEscena = "AvesLSEC.mp4";
        private const string RutaRecursosEscena = "Nivel01/Escena08/";

        // El brillo de seleccion es una imagen superpuesta (BrilloRespuestaSeleccionada):
        // neutro significa invisible para no ensuciar las tarjetas no elegidas.
        private static readonly Color ColorBordeNeutro = new Color(1f, 1f, 1f, 0f);
        private static readonly Color ColorBordeSeleccion = new Color(1.00f, 0.84f, 0.35f);
        private static readonly Color ColorBordeExito = new Color(0.20f, 0.62f, 0.25f);
        private static readonly Color ColorBordeError = new Color(0.80f, 0.25f, 0.25f);
        private static readonly Color ColorTextoExito = new Color(0.10f, 0.45f, 0.15f);
        private static readonly Color ColorTextoError = new Color(0.60f, 0.10f, 0.10f);
        private static readonly Color ColorTextoAviso = new Color(0.11f, 0.32f, 0.50f);

        [Serializable]
        public sealed class TarjetaRespuestaAves
        {
            [SerializeField] private Button boton;
            [SerializeField] private Image ilustracion;
            [SerializeField] private TMP_Text nombre;
            [SerializeField] private Graphic borde;
            [SerializeField] private GameObject placeholder;

            public Button Boton
            {
                get { return boton; }
            }

            public Image Ilustracion
            {
                get { return ilustracion; }
            }

            public TMP_Text Nombre
            {
                get { return nombre; }
            }

            public Graphic Borde
            {
                get { return borde; }
            }

            public GameObject Placeholder
            {
                get { return placeholder; }
            }
        }

        [Header("Textos de la actividad")]
        [SerializeField] private TMP_Text _textoPregunta;
        [SerializeField] private TMP_Text _textoProgreso;
        [SerializeField] private TMP_Text _textoRetroalimentacion;
        [SerializeField] private TMP_Text _textoEstadoAudio;
        [SerializeField] private TMP_Text _textoEstadoSenas;

        [Header("Botones de cabecera y navegación")]
        [SerializeField] private Button _botonLectura;
        [SerializeField] private Button _botonSenas;
        [SerializeField] private Button _botonAyuda;
        [SerializeField] private Button _botonRegresar;
        [SerializeField] private Button _botonAvanzar;
        [SerializeField] private Button _botonInicio;
        [SerializeField] private Button _botonConfiguracion;

        [Header("Botones de la actividad")]
        [SerializeField] private Button _botonAceptar;
        [SerializeField] private Button _botonSiguiente;
        [SerializeField] private Button _botonCerrarAyuda;
        [SerializeField] private Button _botonReproducirSenas;
        [SerializeField] private Button _botonCerrarSenas;

        [Header("Tarjetas de respuesta")]
        [SerializeField] private TarjetaRespuestaAves[] _tarjetas;

        [Header("Paneles y recursos multimodales")]
        [SerializeField] private GameObject _globoRetroalimentacion;
        [SerializeField] private GameObject _letreroExito;
        [SerializeField] private GameObject _panelAyuda;
        [SerializeField] private GameObject _panelSenas;
        [SerializeField] private Image _imagenPrincipal;
        [SerializeField] private GameObject _placeholderImagenPrincipal;
        [SerializeField] private AudioSource _fuenteAudio;
        [SerializeField] private VideoPlayer _reproductorSenas;

        private Coroutine _actualizarEstadoAudio;

        public event Action<int> OnOpcionSeleccionada;
        public event Action OnAceptarSolicitado;
        public event Action OnSiguienteSolicitado;
        public event Action OnLecturaSolicitada;
        public event Action OnAyudaSolicitada;
        public event Action OnCerrarAyudaSolicitado;
        public event Action OnSenasSolicitadas;
        public event Action OnCerrarSenasSolicitado;
        public event Action OnReproducirSenasSolicitado;
        public event Action OnRegresarSolicitado;
        public event Action OnAvanzarSolicitado;
        public event Action OnInicioSolicitado;
        public event Action OnConfiguracionSolicitada;

        private void Awake()
        {
            ConfigurarRecursosOpcionales();
        }

        private void OnEnable()
        {
            if (_tarjetas != null)
            {
                for (int i = 0; i < _tarjetas.Length; i++)
                {
                    int indice = i;
                    if (_tarjetas[i] != null && _tarjetas[i].Boton != null)
                    {
                        _tarjetas[i].Boton.onClick.AddListener(() => EmitirOpcion(indice));
                    }
                }
            }

            if (_botonAceptar != null) _botonAceptar.onClick.AddListener(EmitirAceptar);
            if (_botonSiguiente != null) _botonSiguiente.onClick.AddListener(EmitirSiguiente);
            if (_botonLectura != null) _botonLectura.onClick.AddListener(EmitirLectura);
            if (_botonSenas != null) _botonSenas.onClick.AddListener(EmitirSenas);
            if (_botonAyuda != null) _botonAyuda.onClick.AddListener(EmitirAyuda);
            if (_botonRegresar != null) _botonRegresar.onClick.AddListener(EmitirRegresar);
            if (_botonAvanzar != null) _botonAvanzar.onClick.AddListener(EmitirAvanzar);
            if (_botonInicio != null) _botonInicio.onClick.AddListener(EmitirInicio);
            if (_botonConfiguracion != null) _botonConfiguracion.onClick.AddListener(EmitirConfiguracion);
            if (_botonCerrarAyuda != null) _botonCerrarAyuda.onClick.AddListener(EmitirCerrarAyuda);
            if (_botonReproducirSenas != null) _botonReproducirSenas.onClick.AddListener(EmitirReproducirSenas);
            if (_botonCerrarSenas != null) _botonCerrarSenas.onClick.AddListener(EmitirCerrarSenas);

            if (_botonAceptar != null)
            {
                _botonAceptar.interactable = false;
            }

            if (_botonSiguiente != null)
            {
                _botonSiguiente.gameObject.SetActive(false);
            }

            if (_letreroExito != null)
            {
                _letreroExito.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (_tarjetas != null)
            {
                for (int i = 0; i < _tarjetas.Length; i++)
                {
                    if (_tarjetas[i] != null && _tarjetas[i].Boton != null)
                    {
                        _tarjetas[i].Boton.onClick.RemoveAllListeners();
                    }
                }
            }

            if (_botonAceptar != null) _botonAceptar.onClick.RemoveListener(EmitirAceptar);
            if (_botonSiguiente != null) _botonSiguiente.onClick.RemoveListener(EmitirSiguiente);
            if (_botonLectura != null) _botonLectura.onClick.RemoveListener(EmitirLectura);
            if (_botonSenas != null) _botonSenas.onClick.RemoveListener(EmitirSenas);
            if (_botonAyuda != null) _botonAyuda.onClick.RemoveListener(EmitirAyuda);
            if (_botonRegresar != null) _botonRegresar.onClick.RemoveListener(EmitirRegresar);
            if (_botonAvanzar != null) _botonAvanzar.onClick.RemoveListener(EmitirAvanzar);
            if (_botonInicio != null) _botonInicio.onClick.RemoveListener(EmitirInicio);
            if (_botonConfiguracion != null) _botonConfiguracion.onClick.RemoveListener(EmitirConfiguracion);
            if (_botonCerrarAyuda != null) _botonCerrarAyuda.onClick.RemoveListener(EmitirCerrarAyuda);
            if (_botonReproducirSenas != null) _botonReproducirSenas.onClick.RemoveListener(EmitirReproducirSenas);
            if (_botonCerrarSenas != null) _botonCerrarSenas.onClick.RemoveListener(EmitirCerrarSenas);

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

        public void MostrarEnunciado(string enunciado)
        {
            if (_textoPregunta != null)
            {
                _textoPregunta.text = enunciado ?? string.Empty;
            }
        }

        public void AsignarOpcion(int indice, string palabra)
        {
            if (_tarjetas == null || indice < 0 || indice >= _tarjetas.Length)
            {
                return;
            }

            TarjetaRespuestaAves tarjeta = _tarjetas[indice];
            if (tarjeta == null)
            {
                return;
            }

            if (tarjeta.Nombre != null)
            {
                tarjeta.Nombre.text = palabra ?? string.Empty;
            }

            if (tarjeta.Placeholder != null)
            {
                TMP_Text textoPlaceholder = tarjeta.Placeholder.GetComponent<TMP_Text>();
                if (textoPlaceholder != null)
                {
                    textoPlaceholder.text = palabra ?? string.Empty;
                }
            }
        }

        public void MostrarProgreso(int puntoActual, int totalPuntos)
        {
            if (_textoProgreso != null)
            {
                _textoProgreso.text = "PROGRESO  " + puntoActual + "  DE  " + totalPuntos;
            }
        }

        public void ResaltarOpcion(int indice)
        {
            if (_tarjetas == null)
            {
                return;
            }

            for (int i = 0; i < _tarjetas.Length; i++)
            {
                if (_tarjetas[i] == null || _tarjetas[i].Borde == null)
                {
                    continue;
                }

                _tarjetas[i].Borde.color = i == indice ? ColorBordeSeleccion : ColorBordeNeutro;
            }

            if (_botonAceptar != null)
            {
                _botonAceptar.interactable = true;
            }

            OcultarRetroalimentacion();
        }

        public void MostrarResultadoCorrecto(int indice, string mensaje)
        {
            AplicarBorde(indice, ColorBordeExito);

            if (_botonAceptar != null)
            {
                _botonAceptar.interactable = false;
                _botonAceptar.gameObject.SetActive(false);
            }

            if (_botonSiguiente != null)
            {
                _botonSiguiente.gameObject.SetActive(true);
                _botonSiguiente.interactable = true;
            }

            OcultarRetroalimentacion();
            MostrarLetreroExito();
        }

        public void MostrarResultadoIncorrecto(int indice, string mensaje)
        {
            AplicarBorde(indice, ColorBordeError);

            if (_botonAceptar != null)
            {
                _botonAceptar.interactable = false;
            }

            OcultarLetreroExito();
            MostrarRetroalimentacion(mensaje, ColorTextoError);
        }

        public void OcultarRetroalimentacion()
        {
            if (_globoRetroalimentacion != null)
            {
                _globoRetroalimentacion.SetActive(false);
            }

            if (_textoRetroalimentacion != null)
            {
                _textoRetroalimentacion.text = string.Empty;
            }

            OcultarLetreroExito();
        }

        public void MostrarAviso(string mensaje)
        {
            MostrarRetroalimentacion(mensaje, ColorTextoAviso);
        }

        public void ReproducirAudioPalabra()
        {
            if (_fuenteAudio == null || _fuenteAudio.clip == null)
            {
                MostrarEstadoAudio("La locución de la palabra GUACAMAYO aún no está disponible.");
                return;
            }

            if (_fuenteAudio.isPlaying)
            {
                _fuenteAudio.Stop();
            }

            _fuenteAudio.Play();
            MostrarEstadoAudio("Reproduciendo la palabra GUACAMAYO...");

            if (_actualizarEstadoAudio != null)
            {
                StopCoroutine(_actualizarEstadoAudio);
            }

            _actualizarEstadoAudio = StartCoroutine(EsperarFinDeAudio());
        }

        public void MostrarPanelAyuda()
        {
            if (_panelAyuda != null)
            {
                _panelAyuda.SetActive(true);
                _panelAyuda.transform.SetAsLastSibling();
            }
        }

        public void CerrarPanelAyuda()
        {
            if (_panelAyuda != null)
            {
                _panelAyuda.SetActive(false);
            }
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
                    ? "Palabra GUACAMAYO disponible en LSEC."
                    : "Video de la palabra en LSEC pendiente de grabación. La ventana reproducirá un archivo local cuando se incorpore.";
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

        private void AplicarBorde(int indice, Color color)
        {
            if (_tarjetas == null || indice < 0 || indice >= _tarjetas.Length)
            {
                return;
            }

            if (_tarjetas[indice] != null && _tarjetas[indice].Borde != null)
            {
                _tarjetas[indice].Borde.color = color;
            }
        }

        private void MostrarRetroalimentacion(string mensaje, Color color)
        {
            if (_globoRetroalimentacion != null)
            {
                _globoRetroalimentacion.SetActive(true);
                _globoRetroalimentacion.transform.SetAsLastSibling();
            }

            if (_textoRetroalimentacion != null)
            {
                _textoRetroalimentacion.text = mensaje ?? string.Empty;
                _textoRetroalimentacion.color = color;
            }
        }

        private void MostrarLetreroExito()
        {
            if (_letreroExito == null)
            {
                return;
            }

            _letreroExito.SetActive(true);
            _letreroExito.transform.SetAsLastSibling();
        }

        private void OcultarLetreroExito()
        {
            if (_letreroExito != null)
            {
                _letreroExito.SetActive(false);
            }
        }

        private void ConfigurarRecursosOpcionales()
        {
            if (_imagenPrincipal != null)
            {
                Sprite ilustracionPregunta = CargarSprite("pregunta_01_08_escena");
                if (ilustracionPregunta != null)
                {
                    _imagenPrincipal.sprite = ilustracionPregunta;
                }

                if (_placeholderImagenPrincipal != null)
                {
                    _placeholderImagenPrincipal.SetActive(_imagenPrincipal.sprite == null);
                }
            }

            if (_tarjetas != null)
            {
                for (int i = 0; i < _tarjetas.Length; i++)
                {
                    TarjetaRespuestaAves tarjeta = _tarjetas[i];
                    if (tarjeta == null || tarjeta.Ilustracion == null)
                    {
                        continue;
                    }

                    string letra = ((char)('a' + i)).ToString();
                    Sprite ilustracion = CargarSprite("respuesta_01_08_" + letra);
                    if (ilustracion != null)
                    {
                        tarjeta.Ilustracion.sprite = ilustracion;
                    }

                    if (tarjeta.Placeholder != null)
                    {
                        tarjeta.Placeholder.SetActive(tarjeta.Ilustracion.sprite == null);
                    }
                }
            }

            if (_fuenteAudio != null && _fuenteAudio.clip == null)
            {
                _fuenteAudio.clip = Resources.Load<AudioClip>(RutaRecursosEscena + "audio_01_08");
            }
        }

        private static Sprite CargarSprite(string nombreRecurso)
        {
            return Resources.Load<Sprite>(RutaRecursosEscena + nombreRecurso);
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

            string rutaVideo = Path.Combine(Application.streamingAssetsPath, "LSEC", NombreVideoSenasEscena);
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

            MostrarEstadoAudio("Puedes volver a escuchar la palabra cuando quieras.");
            _actualizarEstadoAudio = null;
        }

        private void MostrarEstadoAudio(string mensaje)
        {
            if (_textoEstadoAudio != null)
            {
                _textoEstadoAudio.text = mensaje;
            }
        }

        private void EmitirOpcion(int indice) { OnOpcionSeleccionada?.Invoke(indice); }
        private void EmitirAceptar() { OnAceptarSolicitado?.Invoke(); }
        private void EmitirSiguiente() { OnSiguienteSolicitado?.Invoke(); }
        private void EmitirLectura() { OnLecturaSolicitada?.Invoke(); }
        private void EmitirSenas() { OnSenasSolicitadas?.Invoke(); }
        private void EmitirAyuda() { OnAyudaSolicitada?.Invoke(); }
        private void EmitirCerrarAyuda() { OnCerrarAyudaSolicitado?.Invoke(); }
        private void EmitirReproducirSenas() { OnReproducirSenasSolicitado?.Invoke(); }
        private void EmitirCerrarSenas() { OnCerrarSenasSolicitado?.Invoke(); }
        private void EmitirRegresar() { OnRegresarSolicitado?.Invoke(); }
        private void EmitirAvanzar() { OnAvanzarSolicitado?.Invoke(); }
        private void EmitirInicio() { OnInicioSolicitado?.Invoke(); }
        private void EmitirConfiguracion() { OnConfiguracionSolicitada?.Invoke(); }
    }
}
