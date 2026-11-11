using System;
using System.Collections;
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
    /// Vista pasiva de la plantilla base de actividad. La jerarquía (fondo, cabecera,
    /// globo, marco, tres tarjetas y ventanas) se construye en el editor con las
    /// coordenadas aprobadas de la guía; aquí solo se activan los estados:
    /// Bienvenida, Sin selección, Seleccionada, Incorrecta, Correcta, Pista y Señas.
    /// </summary>
    public sealed class RetoSeleccionView : MonoBehaviour
    {
        private const string CARPETA_LSEC = "LSEC";
        private const float ESPERA_CELEBRACION = 0.45f;
        private const float DURACION_ENTRADA = 0.25f;

        [Serializable]
        public sealed class TarjetaRespuestaVista
        {
            public Button boton;
            public Image ilustracion;
            public TMP_Text nombre;
            public GameObject brilloSeleccion;
            public GameObject bordeIncorrecto;
            public GameObject bordeCorrecto;
        }

        [Serializable]
        public sealed class ReferenciasActividad
        {
            [Header("Cabecera")]
            public Button botonLectura;
            public Button botonSenas;
            public Button botonAyuda;
            public Button botonRegresar;
            public Button botonAvanzar;
            public Button botonInicio;
            public Button botonConfiguracion;
            public TMP_Text textoProgreso;

            [Header("Actividad")]
            public CanvasGroup capaBase;
            public CanvasGroup capaActividad;
            public GameObject personajeGuia;
            public Image imagenPersonajeGuia;
            public AnimacionPersonajePNG animacionPersonajeGuia;
            public Sprite spritePersonajeNormal;
            public Sprite spritePersonajeIncorrecto;
            public GameObject globoPregunta;
            public GameObject globoCorreccion;
            public TMP_Text textoCorreccion;
            public TarjetaRespuestaVista[] tarjetas;
            public Button botonAceptar;
            public Button botonIntentarloDeNuevo;

            [Header("Respuesta correcta")]
            public GameObject estadoCorrecto;
            public RectTransform contenidoCelebracion;
            public TMP_Text textoFelicitacion;
            public Button botonSiguiente;
            public AnimacionPersonajePNG animacionCelebracion;

            [Header("Ventana de pista")]
            public GameObject panelPista;
            public Button botonVoyIntentarlo;
            public AnimacionPersonajePNG animacionPista;

            [Header("Ventana de señas")]
            public GameObject panelSenas;
            public Button botonReproducirSenas;
            public Button botonCerrarSenas;
            public TMP_Text textoEstadoSenas;
            public RawImage pantallaVideo;
            public VideoPlayer reproductorSenas;

            [Header("Bienvenida del nivel")]
            public GameObject estadoBienvenida;
            public Button botonComenzar;
            public AnimacionPersonajePNG animacionBienvenida;

            [Header("Audio")]
            public AudioSource fuenteAudio;
        }

        [SerializeField] private ReferenciasActividad _ref = new ReferenciasActividad();

        private UnityAction[] _accionesTarjetas;
        private Coroutine _celebracion;
        private Coroutine _reaccionIncorrecta;
        private bool _enEstadoIncorrecto;

        public event Action OnComenzarSolicitado;
        public event Action<int> OnTarjetaSeleccionada;
        public event Action OnAceptarSolicitado;
        public event Action OnIntentarloDeNuevoSolicitado;
        public event Action OnSiguienteSolicitado;
        public event Action OnLecturaSolicitada;
        public event Action OnAyudaSolicitada;
        public event Action OnCerrarPistaSolicitado;
        public event Action OnSenasSolicitadas;
        public event Action OnCerrarSenasSolicitado;
        public event Action OnReproducirSenasSolicitado;
        public event Action OnRegresarSolicitado;
        public event Action OnAvanzarSolicitado;
        public event Action OnInicioSolicitado;
        public event Action OnConfiguracionSolicitada;

        public bool BienvenidaVisible => _ref.estadoBienvenida != null && _ref.estadoBienvenida.activeSelf;
        public bool PistaVisible => _ref.panelPista != null && _ref.panelPista.activeSelf;
        public bool SenasVisibles => _ref.panelSenas != null && _ref.panelSenas.activeSelf;

        /// <summary>Usado por el constructor de escenas del editor.</summary>
        public void Enlazar(ReferenciasActividad referencias)
        {
            _ref = referencias;
        }

        private void OnEnable()
        {
            int cantidad = _ref.tarjetas != null ? _ref.tarjetas.Length : 0;
            _accionesTarjetas = new UnityAction[cantidad];
            for (int i = 0; i < cantidad; i++)
            {
                int indice = i;
                _accionesTarjetas[i] = () => OnTarjetaSeleccionada?.Invoke(indice);
                if (_ref.tarjetas[i] != null && _ref.tarjetas[i].boton != null)
                {
                    _ref.tarjetas[i].boton.onClick.AddListener(_accionesTarjetas[i]);
                }
            }

            Escuchar(_ref.botonComenzar, EmitirComenzar);
            Escuchar(_ref.botonAceptar, EmitirAceptar);
            Escuchar(_ref.botonIntentarloDeNuevo, EmitirIntentarloDeNuevo);
            Escuchar(_ref.botonSiguiente, EmitirSiguiente);
            Escuchar(_ref.botonLectura, EmitirLectura);
            Escuchar(_ref.botonAyuda, EmitirAyuda);
            Escuchar(_ref.botonVoyIntentarlo, EmitirCerrarPista);
            Escuchar(_ref.botonSenas, EmitirSenas);
            Escuchar(_ref.botonCerrarSenas, EmitirCerrarSenas);
            Escuchar(_ref.botonReproducirSenas, EmitirReproducirSenas);
            Escuchar(_ref.botonRegresar, EmitirRegresar);
            Escuchar(_ref.botonAvanzar, EmitirAvanzar);
            Escuchar(_ref.botonInicio, EmitirInicio);
            Escuchar(_ref.botonConfiguracion, EmitirConfiguracion);
        }

        private void OnDisable()
        {
            if (_ref.tarjetas != null && _accionesTarjetas != null)
            {
                for (int i = 0; i < _ref.tarjetas.Length && i < _accionesTarjetas.Length; i++)
                {
                    if (_ref.tarjetas[i] != null && _ref.tarjetas[i].boton != null)
                    {
                        _ref.tarjetas[i].boton.onClick.RemoveListener(_accionesTarjetas[i]);
                    }
                }
            }

            Olvidar(_ref.botonComenzar, EmitirComenzar);
            Olvidar(_ref.botonAceptar, EmitirAceptar);
            Olvidar(_ref.botonIntentarloDeNuevo, EmitirIntentarloDeNuevo);
            Olvidar(_ref.botonSiguiente, EmitirSiguiente);
            Olvidar(_ref.botonLectura, EmitirLectura);
            Olvidar(_ref.botonAyuda, EmitirAyuda);
            Olvidar(_ref.botonVoyIntentarlo, EmitirCerrarPista);
            Olvidar(_ref.botonSenas, EmitirSenas);
            Olvidar(_ref.botonCerrarSenas, EmitirCerrarSenas);
            Olvidar(_ref.botonReproducirSenas, EmitirReproducirSenas);
            Olvidar(_ref.botonRegresar, EmitirRegresar);
            Olvidar(_ref.botonAvanzar, EmitirAvanzar);
            Olvidar(_ref.botonInicio, EmitirInicio);
            Olvidar(_ref.botonConfiguracion, EmitirConfiguracion);

            DetenerAudio();
            DetenerVideo();
        }

        // ----- Estados de la plantilla -----

        public void MostrarBienvenida(bool visible)
        {
            Activar(_ref.estadoBienvenida, visible);
            if (_ref.animacionBienvenida != null)
            {
                if (visible) _ref.animacionBienvenida.ReproducirContinuo();
                else _ref.animacionBienvenida.Pausar();
            }
            // La bienvenida no tiene pista: Ayuda solo aparece en la actividad.
            if (_ref.botonAyuda != null) _ref.botonAyuda.gameObject.SetActive(!visible);
            if (_ref.capaActividad != null)
            {
                // Evita que el teclado llegue a las tarjetas ocultas detrás de la bienvenida.
                _ref.capaActividad.interactable = !visible;
                _ref.capaActividad.blocksRaycasts = !visible;
            }
            Enfocar(visible ? _ref.botonComenzar : BotonTarjeta(0));
        }

        public void MostrarSinSeleccion()
        {
            PrepararTarjetas(RetoSinIndice, RetoSinIndice, RetoSinIndice, true);
            Activar(_ref.globoPregunta, true);
            Activar(_ref.globoCorreccion, false);
            Activar(_ref.botonAceptar, false);
            Activar(_ref.botonIntentarloDeNuevo, false);
            Activar(_ref.estadoCorrecto, false);
            Activar(_ref.personajeGuia, true);
            MostrarPersonajeGuiaNormal();
            BloquearCapaBase(false);
        }

        /// <summary>Destello amarillo solo en la tarjeta elegida; no revela el resultado.</summary>
        public void MostrarSeleccion(int indice)
        {
            PrepararTarjetas(indice, RetoSinIndice, RetoSinIndice, true);
            Activar(_ref.globoPregunta, true);
            Activar(_ref.globoCorreccion, false);
            Activar(_ref.botonIntentarloDeNuevo, false);
            Activar(_ref.botonAceptar, true);
            MostrarPersonajeGuiaNormal();
        }

        /// <summary>Borde rojo en la opción errónea, cambio de reacción del personaje y globo con explicación amable.</summary>
        public void MostrarIncorrecta(int indice, string explicacion)
        {
            PrepararTarjetas(RetoSinIndice, indice, RetoSinIndice, true);
            Activar(_ref.globoPregunta, false);
            Activar(_ref.globoCorreccion, true);
            if (_ref.textoCorreccion != null) _ref.textoCorreccion.text = explicacion;
            Activar(_ref.botonAceptar, false);
            Activar(_ref.botonIntentarloDeNuevo, true);
            MostrarPersonajeGuiaIncorrecto();
            Enfocar(_ref.botonIntentarloDeNuevo);
        }

        /// <summary>Tarjeta correcta en verde y felicitación centrada con «Siguiente».</summary>
        public void MostrarCorrecta(int indice, string felicitacion)
        {
            PrepararTarjetas(RetoSinIndice, RetoSinIndice, indice, false);
            Activar(_ref.globoPregunta, true);
            Activar(_ref.globoCorreccion, false);
            Activar(_ref.botonAceptar, false);
            Activar(_ref.botonIntentarloDeNuevo, false);
            if (_ref.textoFelicitacion != null) _ref.textoFelicitacion.text = felicitacion;

            if (!Application.isPlaying)
            {
                Activar(_ref.personajeGuia, false);
                Activar(_ref.estadoCorrecto, true);
                BloquearCapaBase(true);
                return;
            }

            if (_celebracion != null) StopCoroutine(_celebracion);
            _celebracion = StartCoroutine(MostrarCelebracion());
        }

        public void MostrarAvisoSinSiguiente(string mensaje)
        {
            if (_ref.textoFelicitacion != null) _ref.textoFelicitacion.text = mensaje;
        }

        public void MostrarPista(bool visible)
        {
            if (visible) MostrarSenas(false, null);
            Activar(_ref.panelPista, visible);
            if (_ref.estadoCorrecto == null || !_ref.estadoCorrecto.activeSelf)
            {
                Activar(_ref.personajeGuia, !visible);
                if (!visible)
                {
                    if (_enEstadoIncorrecto) MostrarPersonajeGuiaIncorrecto();
                    else MostrarPersonajeGuiaNormal();
                }
            }
            if (_ref.animacionPista != null)
            {
                if (visible) _ref.animacionPista.ReproducirContinuo();
                else _ref.animacionPista.Pausar();
            }
            BloquearCapaBase(visible);
            if (visible) Enfocar(_ref.botonVoyIntentarlo);
        }

        public void MostrarSenas(bool visible, string archivoVideo)
        {
            if (!visible)
            {
                DetenerVideo();
                if (SenasVisibles) BloquearCapaBase(false);
                Activar(_ref.panelSenas, false);
                return;
            }

            Activar(_ref.panelPista, false);
            Activar(_ref.panelSenas, true);
            BloquearCapaBase(true);
            bool disponible = PrepararVideo(archivoVideo);
            if (_ref.botonReproducirSenas != null) _ref.botonReproducirSenas.interactable = disponible;
            if (_ref.pantallaVideo != null) _ref.pantallaVideo.enabled = false;
            if (_ref.textoEstadoSenas != null)
            {
                _ref.textoEstadoSenas.text = disponible
                    ? "Toca reproducir para ver la explicación en lengua de señas."
                    : "El video en lengua de señas de esta actividad está pendiente de grabación con la intérprete aprobada.";
            }

            Enfocar(disponible ? _ref.botonReproducirSenas : _ref.botonCerrarSenas);
        }

        /// <summary>Reproducir / pausar el video de señas dentro del panel.</summary>
        public void AlternarVideo()
        {
            VideoPlayer reproductor = _ref.reproductorSenas;
            if (reproductor == null || string.IsNullOrEmpty(reproductor.url)) return;

            if (reproductor.isPlaying)
            {
                reproductor.Pause();
                return;
            }

            if (_ref.pantallaVideo != null) _ref.pantallaVideo.enabled = true;
            if (_ref.textoEstadoSenas != null) _ref.textoEstadoSenas.text = string.Empty;
            if (reproductor.isPrepared)
            {
                reproductor.Play();
                return;
            }

            reproductor.prepareCompleted -= ReproducirPreparado;
            reproductor.prepareCompleted += ReproducirPreparado;
            reproductor.Prepare();
        }

        public void PermitirAvanzar(bool permitido)
        {
            if (_ref.botonAvanzar != null) _ref.botonAvanzar.interactable = permitido;
        }

        public void PermitirRegresar(bool permitido)
        {
            if (_ref.botonRegresar != null) _ref.botonRegresar.interactable = permitido;
        }

        public void MostrarProgreso(int escenaActual, int totalEscenas)
        {
            if (_ref.textoProgreso != null)
            {
                _ref.textoProgreso.text = FormatearProgreso(escenaActual, totalEscenas);
            }
        }

        public static string FormatearProgreso(int escenaActual, int totalEscenas)
        {
            return "<color=#FFD43B>" + escenaActual + "</color> de " + totalEscenas;
        }

        /// <summary>Lectura: reproduce o detiene la locución, sin superponer audios.</summary>
        public void AlternarAudio(AudioClip clip)
        {
            AudioSource fuente = _ref.fuenteAudio;
            if (fuente == null || clip == null) return;

            if (fuente.isPlaying && fuente.clip == clip)
            {
                fuente.Stop();
                return;
            }

            fuente.Stop();
            fuente.clip = clip;
            fuente.Play();
        }

        public void DetenerAudio()
        {
            if (_ref.fuenteAudio != null && _ref.fuenteAudio.isPlaying) _ref.fuenteAudio.Stop();
        }

        public void EnfocarTarjeta(int indice)
        {
            Enfocar(BotonTarjeta(indice < 0 ? 0 : indice));
        }

        // ----- Apoyo interno -----

        private const int RetoSinIndice = -1;

        private void MostrarPersonajeGuiaNormal()
        {
            _enEstadoIncorrecto = false;
            if (_reaccionIncorrecta != null)
            {
                StopCoroutine(_reaccionIncorrecta);
                _reaccionIncorrecta = null;
            }

            if (_ref.imagenPersonajeGuia != null)
            {
                RectTransform rect = _ref.imagenPersonajeGuia.rectTransform;
                rect.localEulerAngles = Vector3.zero;
                rect.localScale = Vector3.one;
                if (_ref.spritePersonajeNormal != null)
                {
                    _ref.imagenPersonajeGuia.sprite = _ref.spritePersonajeNormal;
                }
            }

            if (_ref.animacionPersonajeGuia != null && Application.isPlaying)
            {
                _ref.animacionPersonajeGuia.ReproducirContinuo();
            }
        }

        private void MostrarPersonajeGuiaIncorrecto()
        {
            _enEstadoIncorrecto = true;
            if (_ref.animacionPersonajeGuia != null)
            {
                _ref.animacionPersonajeGuia.Pausar();
            }

            if (_ref.imagenPersonajeGuia != null && _ref.spritePersonajeIncorrecto != null)
            {
                _ref.imagenPersonajeGuia.sprite = _ref.spritePersonajeIncorrecto;
            }

            if (!Application.isPlaying || _ref.imagenPersonajeGuia == null) return;

            if (_reaccionIncorrecta != null) StopCoroutine(_reaccionIncorrecta);
            _reaccionIncorrecta = StartCoroutine(AnimarReaccionIncorrecta(_ref.imagenPersonajeGuia.rectTransform));
        }

        private IEnumerator AnimarReaccionIncorrecta(RectTransform rect)
        {
            const float duracion = 0.36f;
            float tiempo = 0f;
            while (tiempo < duracion && rect != null)
            {
                tiempo += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(tiempo / duracion);
                float amortiguacion = 1f - t;
                float angulo = Mathf.Sin(t * Mathf.PI * 3f) * 4.5f * amortiguacion;
                float escala = 1f + Mathf.Sin(t * Mathf.PI) * 0.035f;
                rect.localEulerAngles = new Vector3(0f, 0f, angulo);
                rect.localScale = new Vector3(escala, escala, 1f);
                yield return null;
            }

            if (rect != null)
            {
                rect.localEulerAngles = Vector3.zero;
                rect.localScale = Vector3.one;
            }

            _reaccionIncorrecta = null;
        }

        private void PrepararTarjetas(int brillo, int incorrecta, int correcta, bool interactuables)
        {
            if (_ref.tarjetas == null) return;
            for (int i = 0; i < _ref.tarjetas.Length; i++)
            {
                TarjetaRespuestaVista tarjeta = _ref.tarjetas[i];
                if (tarjeta == null) continue;
                Activar(tarjeta.brilloSeleccion, i == brillo);
                Activar(tarjeta.bordeIncorrecto, i == incorrecta);
                Activar(tarjeta.bordeCorrecto, i == correcta);
                if (tarjeta.boton != null) tarjeta.boton.interactable = interactuables;
            }
        }

        private IEnumerator MostrarCelebracion()
        {
            BloquearCapaBase(true);
            yield return new WaitForSecondsRealtime(ESPERA_CELEBRACION);
            // El personaje celebra en la ventana de acierto; se oculta el guía para no duplicarlo.
            Activar(_ref.personajeGuia, false);
            Activar(_ref.estadoCorrecto, true);
            if (_ref.animacionCelebracion != null) _ref.animacionCelebracion.ReproducirContinuo();
            Enfocar(_ref.botonSiguiente);

            RectTransform contenido = _ref.contenidoCelebracion;
            if (contenido != null)
            {
                float tiempo = 0f;
                while (tiempo < DURACION_ENTRADA)
                {
                    tiempo += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(tiempo / DURACION_ENTRADA);
                    contenido.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, 1f - (1f - t) * (1f - t));
                    yield return null;
                }

                contenido.localScale = Vector3.one;
            }

            _celebracion = null;
        }

        private bool PrepararVideo(string archivoVideo)
        {
            VideoPlayer reproductor = _ref.reproductorSenas;
            if (reproductor == null || string.IsNullOrWhiteSpace(archivoVideo)) return false;

            string ruta = Path.Combine(Application.streamingAssetsPath, CARPETA_LSEC, archivoVideo);
            if (!File.Exists(ruta)) return false;

            reproductor.source = VideoSource.Url;
            reproductor.url = ruta;
            return true;
        }

        private void DetenerVideo()
        {
            if (_ref.reproductorSenas == null) return;
            _ref.reproductorSenas.prepareCompleted -= ReproducirPreparado;
            if (_ref.reproductorSenas.isPlaying || _ref.reproductorSenas.isPaused) _ref.reproductorSenas.Stop();
            if (_ref.pantallaVideo != null) _ref.pantallaVideo.enabled = false;
        }

        private void ReproducirPreparado(VideoPlayer reproductor)
        {
            reproductor.prepareCompleted -= ReproducirPreparado;
            reproductor.Play();
        }

        private void BloquearCapaBase(bool bloquear)
        {
            if (_ref.capaBase == null) return;
            _ref.capaBase.interactable = !bloquear;
            _ref.capaBase.blocksRaycasts = !bloquear;
        }

        private Button BotonTarjeta(int indice)
        {
            if (_ref.tarjetas == null || indice < 0 || indice >= _ref.tarjetas.Length || _ref.tarjetas[indice] == null)
            {
                return null;
            }

            return _ref.tarjetas[indice].boton;
        }

        private static void Enfocar(Selectable control)
        {
            if (!Application.isPlaying || control == null || EventSystem.current == null) return;
            if (!control.gameObject.activeInHierarchy || !control.interactable) return;
            EventSystem.current.SetSelectedGameObject(control.gameObject);
        }

        private static void Activar(GameObject objeto, bool activo)
        {
            if (objeto != null && objeto.activeSelf != activo) objeto.SetActive(activo);
        }

        private static void Activar(Component componente, bool activo)
        {
            if (componente != null) Activar(componente.gameObject, activo);
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
        private void EmitirAceptar() { OnAceptarSolicitado?.Invoke(); }
        private void EmitirIntentarloDeNuevo() { OnIntentarloDeNuevoSolicitado?.Invoke(); }
        private void EmitirSiguiente() { OnSiguienteSolicitado?.Invoke(); }
        private void EmitirLectura() { OnLecturaSolicitada?.Invoke(); }
        private void EmitirAyuda() { OnAyudaSolicitada?.Invoke(); }
        private void EmitirCerrarPista() { OnCerrarPistaSolicitado?.Invoke(); }
        private void EmitirSenas() { OnSenasSolicitadas?.Invoke(); }
        private void EmitirCerrarSenas() { OnCerrarSenasSolicitado?.Invoke(); }
        private void EmitirReproducirSenas() { OnReproducirSenasSolicitado?.Invoke(); }
        private void EmitirRegresar() { OnRegresarSolicitado?.Invoke(); }
        private void EmitirAvanzar() { OnAvanzarSolicitado?.Invoke(); }
        private void EmitirInicio() { OnInicioSolicitado?.Invoke(); }
        private void EmitirConfiguracion() { OnConfiguracionSolicitada?.Invoke(); }
    }
}
