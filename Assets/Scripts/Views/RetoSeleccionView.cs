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
    /// Vista pasiva de la plantilla base de actividad. Soporta tanto jerarquías pre-horneadas
    /// en el editor (a través de ReferenciasActividad) como construcción dinámica en tiempo de
    /// ejecución cuando la escena no incluye GameObjects preconfigurados (HU-02).
    /// </summary>
    public sealed class RetoSeleccionView : MonoBehaviour
    {
        private const string CARPETA_LSEC = "LSEC";
        private const float ESPERA_CELEBRACION = 0.45f;
        private const float DURACION_ENTRADA = 0.25f;

        private static readonly Color Tinta = new Color(0.12f, 0.18f, 0.14f, 1f);
        private static readonly Color TintaBlanca = Color.white;
        private static readonly Color RojoFallo = new Color(0.85f, 0.18f, 0.18f, 1f);

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
        private bool _construidaDinamica = false;

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
        public bool RespuestaIncorrectaVisible => _enEstadoIncorrecto;

        /// <summary>Usado por el constructor de escenas del editor.</summary>
        public void Enlazar(ReferenciasActividad referencias)
        {
            _ref = referencias;
        }

        private void Awake()
        {
            if (_ref.tarjetas == null || _ref.tarjetas.Length == 0)
            {
                var presenter = GetComponent<GuardianesAmazonicos.Presenters.RetoSeleccionPresenter>();
                if (presenter != null)
                {
                    var campo = typeof(GuardianesAmazonicos.Presenters.RetoSeleccionPresenter).GetField("_configuracion",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (campo != null)
                    {
                        var config = campo.GetValue(presenter) as Models.RetoSeleccionConfiguracion;
                        if (config != null)
                        {
                            Construir(config);
                        }
                    }
                }
            }
        }

        public void Construir(Models.RetoSeleccionConfiguracion configuracion)
        {
            if (_construidaDinamica || (_ref.tarjetas != null && _ref.tarjetas.Length > 0 && _ref.tarjetas[0] != null && _ref.tarjetas[0].boton != null))
            {
                return;
            }

            ConstruirInterfazDinamica(configuracion);
            _construidaDinamica = true;
            if (isActiveAndEnabled)
            {
                RegistrarEventos();
            }
        }

        private void OnEnable()
        {
            RegistrarEventos();
        }

        private void RegistrarEventos()
        {
            int cantidad = _ref.tarjetas != null ? _ref.tarjetas.Length : 0;
            if (cantidad > 0)
            {
                _accionesTarjetas = new UnityAction[cantidad];
                for (int i = 0; i < cantidad; i++)
                {
                    int indice = i;
                    _accionesTarjetas[i] = () => OnTarjetaSeleccionada?.Invoke(indice);
                    if (_ref.tarjetas[i] != null && _ref.tarjetas[i].boton != null)
                    {
                        _ref.tarjetas[i].boton.onClick.RemoveListener(_accionesTarjetas[i]);
                        _ref.tarjetas[i].boton.onClick.AddListener(_accionesTarjetas[i]);
                    }
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
                _ref.capaActividad.interactable = !visible;
                _ref.capaActividad.blocksRaycasts = !visible;
            }
            Enfocar(visible ? _ref.botonComenzar : BotonTarjeta(0));
        }

        public void MostrarSinSeleccion()
        {
            PermitirBotonesDeApoyoSobreModal(false);
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

        public void MostrarSeleccion(int indice)
        {
            PermitirBotonesDeApoyoSobreModal(false);
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
            PermitirBotonesDeApoyoSobreModal(true);
            PrepararTarjetas(RetoSinIndice, indice, RetoSinIndice, true);
            Activar(_ref.globoPregunta, false);
            Activar(_ref.globoCorreccion, true);
            if (_ref.textoCorreccion != null) _ref.textoCorreccion.text = explicacion;
            Activar(_ref.botonAceptar, false);
            Activar(_ref.botonIntentarloDeNuevo, true);
            MostrarPersonajeGuiaIncorrecto();
            Enfocar(_ref.botonIntentarloDeNuevo);
        }

        public void MostrarCorrecta(int indice, string felicitacion)
        {
            PermitirBotonesDeApoyoSobreModal(false);
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
            PermitirBotonesDeApoyoSobreModal(visible);
            if (visible) PermitirLecturaSobreLaPista();
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

            // Cerrar la pista mediante su flujo normal reactiva al personaje de la escena.
            // Después se muestra Señas sobre la actividad ya restaurada, para que al pulsar X
            // el personaje siga visible detrás de la tarjeta.
            if (PistaVisible) MostrarPista(false);
            PermitirBotonesDeApoyoSobreModal(false);
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

        // ----- Apoyo interno y construcción dinámica -----

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

        private void PermitirBotonesDeApoyoSobreModal(bool permitir)
        {
            PermitirBotonSobreModal(_ref.botonLectura, permitir);
            PermitirBotonSobreModal(_ref.botonSenas, permitir);
        }

        private void PermitirLecturaSobreLaPista()
        {
            if (_ref.panelPista == null) return;
            Image[] imagenes = _ref.panelPista.GetComponentsInChildren<Image>(true);
            foreach (Image imagen in imagenes)
            {
                if (imagen != null && imagen.gameObject.name == "FondoLuz")
                {
                    imagen.raycastTarget = false;
                    break;
                }
            }
        }

        private static void PermitirBotonSobreModal(Button boton, bool permitir)
        {
            if (boton == null) return;
            CanvasGroup grupo = boton.GetComponent<CanvasGroup>();
            if (permitir && grupo == null) grupo = boton.gameObject.AddComponent<CanvasGroup>();
            if (grupo == null) return;

            grupo.ignoreParentGroups = permitir;
            grupo.interactable = true;
            grupo.blocksRaycasts = true;
            boton.interactable = true;
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
            if (boton != null)
            {
                boton.onClick.RemoveListener(accion);
                boton.onClick.AddListener(accion);
            }
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

        // ----- Construcción dinámica en tiempo de ejecución (HU-02) -----

        private void ConstruirInterfazDinamica(Models.RetoSeleccionConfiguracion c)
        {
            GameObject canvasGo = new GameObject("CanvasActividad", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform root = canvasGo.transform as RectTransform;

            GameObject capaBaseGo = new GameObject("CapaBase", typeof(RectTransform), typeof(CanvasGroup));
            capaBaseGo.transform.SetParent(root, false);
            RectTransform capaBase = capaBaseGo.transform as RectTransform;
            capaBase.anchorMin = Vector2.zero;
            capaBase.anchorMax = Vector2.one;
            capaBase.offsetMin = Vector2.zero;
            capaBase.offsetMax = Vector2.zero;
            _ref.capaBase = capaBaseGo.GetComponent<CanvasGroup>();

            // Fondo
            CrearImagenFull("Fondo", capaBase, c.Fondo);

            GameObject capaActividadGo = new GameObject("CapaActividad", typeof(RectTransform), typeof(CanvasGroup));
            capaActividadGo.transform.SetParent(capaBase, false);
            RectTransform capaActividad = capaActividadGo.transform as RectTransform;
            capaActividad.anchorMin = Vector2.zero;
            capaActividad.anchorMax = Vector2.one;
            capaActividad.offsetMin = Vector2.zero;
            capaActividad.offsetMax = Vector2.zero;
            _ref.capaActividad = capaActividadGo.GetComponent<CanvasGroup>();

            // Cabecera
            _ref.botonLectura = CrearBotonTopLeft("BotonLectura", capaBase, CargarSprite("BotonLectura"), new Vector2(30f, -16f), new Vector2(132f, 114f));
            _ref.botonSenas = CrearBotonTopLeft("BotonSenas", capaBase, CargarSprite("BotonSenas"), new Vector2(178f, -16f), new Vector2(132f, 114f));
            _ref.botonAyuda = CrearBotonTopLeft("BotonAyuda", capaBase, CargarSprite("BotonAyuda"), new Vector2(326f, -16f), new Vector2(132f, 114f));

            RectTransform placaTitulo = CrearImagenTopLeft("PlacaTitulo", capaBase, CargarSprite("PlacaTituloNivel"), Color.white, new Vector2(592f, -12f), new Vector2(484f, 108f)).rectTransform;
            string tituloMostrado = NormalizarTituloNivel(c.TituloNivel);
            CrearTextoRelativo("TextoTitulo", placaTitulo, tituloMostrado, new Vector2(65f, -24f), new Vector2(354f, 60f), 22f, Tinta, TextAlignmentOptions.Center, true, c.Fuente);

            RectTransform placaProgreso = CrearImagenTopLeft("PlacaProgreso", capaBase, CargarSprite("PlacaProgreso"), Color.white, new Vector2(1092f, -16f), new Vector2(236f, 94f)).rectTransform;
            string progresoTexto = !string.IsNullOrWhiteSpace(c.textoProgreso) ? c.textoProgreso : (c.NumeroEscena + " de 2");
            _ref.textoProgreso = CrearTextoRelativo("TextoProgreso", placaProgreso, progresoTexto, new Vector2(65f, -20f), new Vector2(150f, 54f), 24f, TintaBlanca, TextAlignmentOptions.Center, true, c.Fuente);

            _ref.botonRegresar = CrearBotonTopLeft("BotonRegresar", capaBase, CargarSprite("BotonRegresar"), new Vector2(1506f, -12f), new Vector2(84f, 84f));
            _ref.botonAvanzar = CrearBotonTopLeft("BotonAvanzar", capaBase, CargarSprite("BotonAvanzar"), new Vector2(1602f, -12f), new Vector2(84f, 84f));
            _ref.botonInicio = CrearBotonTopLeft("BotonInicio", capaBase, CargarSprite("BotonInicio"), new Vector2(1698f, -12f), new Vector2(84f, 84f));
            _ref.botonConfiguracion = CrearBotonTopLeft("BotonConfiguracion", capaBase, CargarSprite("BotonConfiguracion"), new Vector2(1794f, -12f), new Vector2(84f, 84f));

            // Personaje Guía a nivel del suelo
            Sprite spritePersonaje = c.Personaje != null ? c.Personaje : CargarSprite("Explorador");
            Image guia = CrearImagenTopLeft("PersonajeGuia", capaActividad, spritePersonaje, Color.white, new Vector2(147f, -495f), new Vector2(390f, 562f));
            guia.preserveAspect = true;
            guia.raycastTarget = false;
            _ref.personajeGuia = guia.gameObject;

            // Globo de pregunta
            RectTransform globo = CrearImagenTopLeft("GloboPregunta", capaActividad, CargarSprite("GloboPregunta"), Color.white, new Vector2(158f, -174f), new Vector2(684f, 240f)).rectTransform;
            string preguntaTexto = FormatearPreguntaConSignos(c.Pregunta);
            CrearTextoRelativo("TextoPregunta", globo, preguntaTexto, new Vector2(43f, -34f), new Vector2(598f, 150f), 26f, Tinta, TextAlignmentOptions.Center, true, c.Fuente);
            _ref.globoPregunta = globo.gameObject;

            // Globo de corrección
            RectTransform globoCorrec = CrearImagenTopLeft("GloboCorreccion", capaActividad, CargarSprite("GloboCorreccion"), Color.white, new Vector2(158f, -174f), new Vector2(684f, 240f)).rectTransform;
            _ref.textoCorreccion = CrearTextoRelativo("TextoCorreccion", globoCorrec, string.Empty, new Vector2(43f, -34f), new Vector2(598f, 150f), 26f, RojoFallo, TextAlignmentOptions.Center, true, c.Fuente);
            _ref.globoCorreccion = globoCorrec.gameObject;
            _ref.globoCorreccion.SetActive(false);

            // Marco con ilustración
            RectTransform marco = CrearImagenTopLeft("MarcoEscena", capaActividad, CargarSprite("MarcoEscena"), Color.white, new Vector2(942f, -206f), new Vector2(758f, 390f)).rectTransform;
            GameObject ventanaMask = new GameObject("VentanaMascara", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            ventanaMask.transform.SetParent(marco, false);
            RectTransform rectMascara = ventanaMask.transform as RectTransform;
            rectMascara.anchorMin = Vector2.zero;
            rectMascara.anchorMax = Vector2.one;
            rectMascara.offsetMin = new Vector2(16f, 16f);
            rectMascara.offsetMax = new Vector2(-16f, -16f);
            Image fondoMarco = ventanaMask.GetComponent<Image>();
            fondoMarco.color = new Color(1f, 0.972f, 0.91f, 1f);
            fondoMarco.raycastTarget = false;

            Sprite spEscena = c.ImagenEscena != null ? c.ImagenEscena : CargarSprite("EscenaSelvaHabitat");
            Image imgEscena = CrearImagenRelativa("Ilustracion", rectMascara, spEscena, Color.white, Vector2.zero, new Vector2(726f, 358f));
            imgEscena.preserveAspect = false;
            marco.gameObject.SetActive(c.MostrarImagenEscena);

            // Tarjetas de respuesta
            int cantidad = Mathf.Min(4, c.Opciones != null ? c.Opciones.Length : 0);
            float[] posX = cantidad == 4
                ? new[] { 745f, 990f, 1235f, 1480f }
                : new[] { 942f, 1204f, 1466f };
            float anchoTarjeta = cantidad == 4 ? 240f : 256f;
            float posicionTarjetasY = cantidad == 4 ? -535f : -663f;
            _ref.tarjetas = new TarjetaRespuestaVista[cantidad];

            for (int i = 0; i < cantidad; i++)
            {
                _ref.tarjetas[i] = new TarjetaRespuestaVista();
                RectTransform rectTarjeta = CrearImagenTopLeft("Respuesta" + (i + 1), capaActividad, CargarSprite("TarjetaRespuesta"), Color.white, new Vector2(posX[i], posicionTarjetasY), new Vector2(anchoTarjeta, 307f)).rectTransform;
                _ref.tarjetas[i].boton = rectTarjeta.gameObject.AddComponent<Button>();

                // Brillo
                Image brillo = CrearImagenRelativa("Brillo", rectTarjeta, CargarSprite("BrilloSeleccion"), Color.white, Vector2.zero, new Vector2(anchoTarjeta, 307f));
                brillo.gameObject.SetActive(false);
                _ref.tarjetas[i].brilloSeleccion = brillo.gameObject;

                // Bordes
                Image bordeInc = CrearImagenRelativa("BordeIncorrecto", rectTarjeta, CargarSprite("BordeRespuestaIncorrecta"), Color.white, Vector2.zero, new Vector2(anchoTarjeta, 307f));
                bordeInc.gameObject.SetActive(false);
                _ref.tarjetas[i].bordeIncorrecto = bordeInc.gameObject;

                Image bordeCor = CrearImagenRelativa("BordeCorrecto", rectTarjeta, CargarSprite("BordeRespuestaCorrecta"), Color.white, Vector2.zero, new Vector2(anchoTarjeta, 307f));
                bordeCor.gameObject.SetActive(false);
                _ref.tarjetas[i].bordeCorrecto = bordeCor.gameObject;

                // Ilustración de la opción centrada horizontal y verticalmente
                Sprite sp = (c.ImagenesOpciones != null && i < c.ImagenesOpciones.Length && c.ImagenesOpciones[i] != null)
                    ? c.ImagenesOpciones[i]
                    : ((c.imagenes != null && i < c.imagenes.Length && c.imagenes[i] != null)
                        ? c.imagenes[i]
                        : (c.Opciones != null && i < c.Opciones.Length ? CargarSprite(c.Opciones[i]) : null));
                GameObject goIlustracion = new GameObject("Ilustracion", typeof(RectTransform), typeof(Image));
                goIlustracion.transform.SetParent(rectTarjeta, false);
                RectTransform rectIlustracion = goIlustracion.transform as RectTransform;
                rectIlustracion.anchorMin = new Vector2(0.5f, 1f);
                rectIlustracion.anchorMax = new Vector2(0.5f, 1f);
                rectIlustracion.pivot = new Vector2(0.5f, 0.5f);
                rectIlustracion.anchoredPosition = new Vector2(0f, -112f);
                rectIlustracion.sizeDelta = new Vector2(anchoTarjeta - 48f, 180f);
                Image imgOp = goIlustracion.GetComponent<Image>();
                imgOp.sprite = sp;
                imgOp.color = Color.white;
                imgOp.preserveAspect = true;
                imgOp.raycastTarget = false;
                _ref.tarjetas[i].ilustracion = imgOp;

                // Nombre
                _ref.tarjetas[i].nombre = CrearTextoRelativo("Nombre", rectTarjeta, c.Opciones[i], new Vector2(15f, -216f), new Vector2(anchoTarjeta - 30f, 76f), 24f, Tinta, TextAlignmentOptions.Center, true, c.Fuente);
            }

            // Botones de confirmación
            _ref.botonAceptar = CrearBotonTopLeft("BotonAceptar", capaActividad, CargarSprite("BotonAceptar"), new Vector2(1171f, -978f), new Vector2(300f, 72f));
            _ref.botonAceptar.gameObject.SetActive(false);
            _ref.botonIntentarloDeNuevo = CrearBotonTopLeft("BotonIntentarloDeNuevo", capaActividad, CargarSprite("BotonIntentarloDeNuevo"), new Vector2(1101f, -976f), new Vector2(440f, 72f));
            _ref.botonIntentarloDeNuevo.gameObject.SetActive(false);

            // Ventana Estado Correcto
            GameObject estadoCorrectoGo = new GameObject("EstadoCorrecto", typeof(RectTransform));
            estadoCorrectoGo.transform.SetParent(root, false);
            RectTransform rectCorrecto = estadoCorrectoGo.transform as RectTransform;
            rectCorrecto.anchorMin = Vector2.zero;
            rectCorrecto.anchorMax = Vector2.one;
            rectCorrecto.offsetMin = Vector2.zero;
            rectCorrecto.offsetMax = Vector2.zero;
            _ref.estadoCorrecto = estadoCorrectoGo;

            RectTransform bloqueadorCorrecto = CrearImagenFull("FondoResplandor", rectCorrecto, CargarSprite("PlantillaVerdeIluminada")).rectTransform;
            _ref.contenidoCelebracion = bloqueadorCorrecto;
            CrearImagenTopLeft("LetreroMuyBien", bloqueadorCorrecto, CargarSprite("LetreroMuyBien"), Color.white, new Vector2(560f, -280f), new Vector2(800f, 260f));
            _ref.textoFelicitacion = CrearTextoRelativo("TextoFelicitacion", bloqueadorCorrecto, c.Felicitacion, new Vector2(460f, -560f), new Vector2(1000f, 100f), 32f, TintaBlanca, TextAlignmentOptions.Center, true, c.Fuente);
            _ref.botonSiguiente = CrearBotonTopLeft("BotonSiguiente", bloqueadorCorrecto, CargarSprite("BotonSiguiente"), new Vector2(760f, -700f), new Vector2(400f, 84f));
            _ref.estadoCorrecto.SetActive(false);

            // Panel de Pista
            GameObject panelPistaGo = new GameObject("PanelPista", typeof(RectTransform));
            panelPistaGo.transform.SetParent(root, false);
            RectTransform rectPista = panelPistaGo.transform as RectTransform;
            rectPista.anchorMin = Vector2.zero;
            rectPista.anchorMax = Vector2.one;
            rectPista.offsetMin = Vector2.zero;
            rectPista.offsetMax = Vector2.zero;
            _ref.panelPista = panelPistaGo;

            RectTransform fondoLuz = CrearImagenFull("FondoLuz", rectPista, CargarSprite("FondoAmarilloPista")).rectTransform;
            fondoLuz.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.55f);
            RectTransform tarjetaPista = CrearImagenTopLeft("TarjetaPista", rectPista, CargarSprite("PanelPista"), Color.white, new Vector2(560f, -240f), new Vector2(800f, 600f)).rectTransform;
            CrearTextoRelativo("TextoPista", tarjetaPista, c.Pista, new Vector2(60f, -140f), new Vector2(680f, 320f), 28f, Tinta, TextAlignmentOptions.Center, true, c.Fuente);
            _ref.botonVoyIntentarlo = CrearBotonTopLeft("BotonVoyIntentarlo", tarjetaPista, CargarSprite("BotonVoyIntentarlo"), new Vector2(250f, -480f), new Vector2(300f, 80f));
            _ref.panelPista.SetActive(false);

            // Panel de Señas
            GameObject panelSenasGo = new GameObject("PanelSenas", typeof(RectTransform));
            panelSenasGo.transform.SetParent(root, false);
            RectTransform rectSenas = panelSenasGo.transform as RectTransform;
            rectSenas.anchorMin = Vector2.zero;
            rectSenas.anchorMax = Vector2.one;
            rectSenas.offsetMin = Vector2.zero;
            rectSenas.offsetMax = Vector2.zero;
            _ref.panelSenas = panelSenasGo;

            RectTransform oscurecer = CrearImagenFull("Oscurecer", rectSenas, null).rectTransform;
            oscurecer.GetComponent<Image>().color = new Color(0.04f, 0.14f, 0.12f, 0.62f);
            RectTransform tarjetaSenas = CrearImagenTopLeft("TarjetaSenas", rectSenas, CargarSprite("PanelSenas"), Color.white, new Vector2(510f, -215f), new Vector2(900f, 650f)).rectTransform;
            _ref.botonCerrarSenas = CrearBotonTopLeft("BotonCerrar", tarjetaSenas, CargarSprite("BotonCerrar"), new Vector2(820f, -20f), new Vector2(64f, 64f));
            _ref.botonReproducirSenas = CrearBotonTopLeft("BotonReproducir", tarjetaSenas, CargarSprite("BotonReproducir"), new Vector2(400f, -540f), new Vector2(100f, 80f));
            _ref.textoEstadoSenas = CrearTextoRelativo("TextoEstado", tarjetaSenas, "Video en lengua de señas", new Vector2(100f, -240f), new Vector2(700f, 200f), 26f, Tinta, TextAlignmentOptions.Center, true, c.Fuente);
            _ref.panelSenas.SetActive(false);

            // Audio
            _ref.fuenteAudio = gameObject.GetComponent<AudioSource>();
            if (_ref.fuenteAudio == null) _ref.fuenteAudio = gameObject.AddComponent<AudioSource>();
            _ref.fuenteAudio.playOnAwake = false;
        }

        private static Sprite CargarSprite(string nombre)
        {
            Sprite s = Resources.Load<Sprite>("UI/Sprites/" + nombre);
            if (s != null) return s;
            s = Resources.Load<Sprite>(nombre);
            if (s != null) return s;
#if UNITY_EDITOR
            s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Characters/" + nombre + ".png");
            if (s != null) return s;
            s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Sprites/" + nombre + ".png");
            if (s != null) return s;
            s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/Sprites/" + nombre + ".png");
            if (s != null) return s;
            s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/" + nombre + ".png");
            if (s != null) return s;
            s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/Niveles/" + nombre + ".png");
            if (s != null) return s;
#endif
            return null;
        }

        private static Image CrearImagenFull(string nombre, RectTransform padre, Sprite sprite)
        {
            GameObject obj = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(padre, false);
            RectTransform r = obj.transform as RectTransform;
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            Image img = obj.GetComponent<Image>();
            img.sprite = sprite;
            img.color = Color.white;
            img.raycastTarget = false;
            return img;
        }

        private static Image CrearImagenTopLeft(string nombre, RectTransform padre, Sprite sprite, Color color, Vector2 pos, Vector2 size)
        {
            GameObject obj = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(padre, false);
            RectTransform r = obj.transform as RectTransform;
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(0f, 1f);
            r.pivot = new Vector2(0f, 1f);
            r.anchoredPosition = pos;
            r.sizeDelta = size;
            Image img = obj.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            return img;
        }

        private static Image CrearImagenRelativa(string nombre, RectTransform padre, Sprite sprite, Color color, Vector2 pos, Vector2 size)
        {
            return CrearImagenTopLeft(nombre, padre, sprite, color, pos, size);
        }

        private static Button CrearBotonTopLeft(string nombre, RectTransform padre, Sprite sprite, Vector2 pos, Vector2 size)
        {
            Image img = CrearImagenTopLeft(nombre, padre, sprite, Color.white, pos, size);
            img.raycastTarget = true;
            Button btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            return btn;
        }

        private static TMP_Text CrearTextoRelativo(string nombre, RectTransform padre, string texto, Vector2 pos, Vector2 size, float fontSize, Color color, TextAlignmentOptions align, bool bold, TMP_FontAsset font)
        {
            GameObject obj = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(padre, false);
            RectTransform r = obj.transform as RectTransform;
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(0f, 1f);
            r.pivot = new Vector2(0f, 1f);
            r.anchoredPosition = pos;
            r.sizeDelta = size;
            TMP_Text tmp = obj.GetComponent<TextMeshProUGUI>();
            tmp.text = texto;
            if (font != null) tmp.font = font;
            tmp.fontSize = fontSize;
            tmp.fontSizeMin = 12f;
            tmp.fontSizeMax = fontSize;
            tmp.enableAutoSizing = true;
            tmp.color = color;
            tmp.alignment = align;
            tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static string NormalizarTituloNivel(string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo)) return "Nivel 1. Conozco la Selva";
            string res = titulo.Trim();
            res = res.Replace("·", ".").Replace(" - ", ". ").Replace(" \xB7 ", ". ");
            if (res.Contains("selva")) res = res.Replace("selva", "Selva");
            return res;
        }

        private static string FormatearPreguntaConSignos(string pregunta)
        {
            if (string.IsNullOrWhiteSpace(pregunta)) return pregunta;
            string res = pregunta.Trim();
            if (!res.StartsWith("¿")) res = "¿" + res;
            if (!res.EndsWith("?"))
            {
                if (res.EndsWith(".")) res = res.Substring(0, res.Length - 1) + "?";
                else res += "?";
            }
            if (res.StartsWith("¿") && res.Length > 1 && res[1] != ' ' && !res.StartsWith("¿<space="))
            {
                res = "¿<space=0.18em>" + res.Substring(1);
            }
            return res;
        }
    }
}
