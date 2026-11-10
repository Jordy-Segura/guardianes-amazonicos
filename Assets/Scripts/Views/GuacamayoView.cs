using System;
using System.Collections;
using System.IO;
using GuardianesAmazonicos.Models;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.Video;

namespace GuardianesAmazonicos.Views
{
    public sealed class GuacamayoView : MonoBehaviour, IGuacamayoView
    {
        private static readonly Color Crema = new Color(1f, 0.97f, 0.86f);
        private static readonly Color Verde = new Color(0.08f, 0.31f, 0.17f);
        private static readonly Color Dorado = new Color(1f, 0.72f, 0.08f);
        private static readonly Color Tinta = new Color(0.05f, 0.17f, 0.12f);
        private static readonly Color Rojo = new Color(0.73f, 0.16f, 0.12f);

        public event Action<int> OnOpcionSeleccionada;
        public event Action OnAceptarSolicitado;
        public event Action OnAudioSolicitado;
        public event Action OnReintentarSolicitado;
        public event Action OnSiguienteSolicitado;
        public event Action OnAyudaSolicitada;
        public event Action OnSenasSolicitadas;
        public event Action OnInicioSolicitado;
        public event Action OnRegresarSolicitado;
        public event Action OnAvanzarSolicitado;
        public event Action OnConfiguracionSolicitada;

        [SerializeField] private Sprite _tarjeta;
        [SerializeField] private Sprite _marco;
        [SerializeField] private Sprite _globo;
        [SerializeField] private Sprite[] _iconosToolbar;
        [SerializeField] private Sprite[] _iconosNavegacion;
        [SerializeField] private Sprite _imagenAceptar;
        [SerializeField] private Sprite _imagenReintentar;
        [SerializeField] private Sprite _imagenSiguiente;
        [SerializeField] private Sprite _letreroAcierto;
        [SerializeField] private Sprite _brilloSeleccion;
        [SerializeField] private Sprite _bannerNivel;
        [SerializeField] private Sprite _cartaPista;
        [SerializeField] private Sprite _tarjetaSenas;
        [SerializeField] private Sprite _botonCerrarModal;
        [SerializeField] private Sprite _botonReproducirModal;
        [SerializeField] private Sprite _personajePreocupado;
        [SerializeField] private Sprite _personajeCelebrando;
        [SerializeField] private Sprite _fondoAcierto;
        [SerializeField] private Sprite _iconoReintentar;
        private TMP_Text _pregunta;
        private Image _burbujaPregunta;
        private RetoSeleccionConfiguracion _configuracion;

        public void ConfigurarArte(Sprite tarjeta, Sprite marco, Sprite globo, Sprite[] iconosToolbar,
            Sprite[] iconosNavegacion, Sprite aceptar, Sprite reintentar, Sprite siguiente, Sprite letreroAcierto,
            Sprite brilloSeleccion, Sprite bannerNivel, Sprite cartaPista, Sprite tarjetaSenas,
            Sprite botonCerrarModal, Sprite botonReproducirModal, Sprite personajePreocupado,
            Sprite personajeCelebrando, Sprite fondoAcierto, Sprite iconoReintentar)
        {
            _tarjeta = tarjeta;
            _marco = marco;
            _globo = globo;
            _iconosToolbar = iconosToolbar;
            _iconosNavegacion = iconosNavegacion;
            _imagenAceptar = aceptar;
            _imagenReintentar = reintentar;
            _imagenSiguiente = siguiente;
            _letreroAcierto = letreroAcierto;
            _brilloSeleccion = brilloSeleccion;
            _bannerNivel = bannerNivel;
            _cartaPista = cartaPista;
            _tarjetaSenas = tarjetaSenas;
            _botonCerrarModal = botonCerrarModal;
            _botonReproducirModal = botonReproducirModal;
            _personajePreocupado = personajePreocupado;
            _personajeCelebrando = personajeCelebrando;
            _fondoAcierto = fondoAcierto;
            _iconoReintentar = iconoReintentar;
        }
        private readonly Button[] _opciones = new Button[3];
        private readonly Outline[] _bordes = new Outline[3];
        private Button _aceptar;
        private Button _reintentar;
        private Button _siguiente;
        private AudioSource _audio;
        private GameObject _modal;
        private VideoPlayer _video;
        private RenderTexture _texturaVideo;
        private GameObject _focoAnterior;
        private int _seleccion = -1;
        private Image _exploradorAnimado;
        private Image _guacamayoAnimado;
        private Sprite[] _fotogramasExplorador;
        private Sprite[] _fotogramasGuacamayo;
        private float _tiempoFotograma;
        private float _tiempoGuacamayo;
        private int _indiceFotogramaExplorador;
        private int _indiceFotogramaGuacamayo;
        private bool _saludoExploradorCompleto;
        private bool _animarGuacamayoTrasAcierto;
        private bool _animarExploradorVictoria;
        private float _tiempoExploradorVictoria;
        private Coroutine _ocultarResultado;
        private GameObject _estadoResultado;
        private TMP_Text _textoEstado;
        private Image _imagenExploradorVictoria;
        private AnimacionEntradaUI _animacionResultado;
        private bool _movimientoReducido;
        private Button _navAvanzar;

        public bool MovimientoReducido => _movimientoReducido;

        public void Construir(RetoSeleccionConfiguracion configuracion)
        {
            if (configuracion == null || configuracion.opciones == null || configuracion.opciones.Length != 3 ||
                configuracion.imagenes == null || configuracion.imagenes.Length != 3)
            {
                throw new ArgumentException("HU-03 requiere tres opciones con sus imágenes.", nameof(configuracion));
            }
            _configuracion = configuracion;
            CrearEventSystemSiHaceFalta();
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.loop = false;
            _audio.volume = PlayerPrefs.GetFloat("HU03_AudioVolumen", 1f);
            _audio.clip = configuracion.pronunciaciones != null && configuracion.pronunciaciones.Length > 0
                ? configuracion.pronunciaciones[0] : null;
            _movimientoReducido = PlayerPrefs.GetInt("HU03_MovimientoReducido", 0) == 1;
            _fotogramasExplorador = Resources.LoadAll<Sprite>("HU03/Animaciones/Explorador");
            _fotogramasGuacamayo = Resources.LoadAll<Sprite>("HU03/Animaciones/Guacamayo");
            Array.Sort(_fotogramasExplorador, (a, b) => string.CompareOrdinal(a.name, b.name));
            Array.Sort(_fotogramasGuacamayo, (a, b) => string.CompareOrdinal(a.name, b.name));
            RectTransform raiz = CrearCanvas().transform as RectTransform;
            CrearImagen("Fondo", raiz, configuracion.fondo, Color.white, true, false,
                Vector2.zero, new Vector2(1920, 1080), true);
            Boton("Audio", raiz, "AUDIO", 30, 16, 132, 114, () => OnAudioSolicitado?.Invoke());
            Boton("Senas", raiz, "SEÑAS", 170, 16, 132, 114, () => OnSenasSolicitadas?.Invoke());
            Boton("Ayuda", raiz, "AYUDA", 310, 16, 132, 114, () => OnAyudaSolicitada?.Invoke());
            CrearNavegacion(raiz);
            Panel("Titulo", raiz, 592, 12, 484, 108, Verde);
            Texto("TituloNivel", raiz, "Nivel 1 · Conozco la selva", 625, 28, 420, 70, 25, Tinta);
            if (configuracion.insigniaProgreso != null)
                Imagen("DistintivoProgreso", raiz, configuracion.insigniaProgreso, 1092, 16, 236, 94);
            else
                Panel("Progreso", raiz, 1092, 16, 236, 94, Verde);
            int totalRetos = Mathf.Max(1, configuracion.totalRetos);
            int numeroReto = Mathf.Clamp(configuracion.numeroEscena, 1, totalRetos);
            Texto("NumeroReto", raiz, numeroReto + " de " + totalRetos, 1102, 34, 216, 48, 30, Color.white);
            _burbujaPregunta = Panel("Globo", raiz, 158, 174, 684, 240, Crema).GetComponent<Image>();
            _pregunta = Texto("Pregunta", raiz, configuracion.pregunta, 201, 208, 598, 150, 40, Tinta);
            _exploradorAnimado = CrearImagen("Guardian", raiz, configuracion.explorador, Color.white, true, false,
                Vector2.zero, new Vector2(390, 562), false);
            Ubicar(_exploradorAnimado.rectTransform, 147, 428, 390, 562);
            CrearEscenaIlustrada(raiz, configuracion, 921, 188, 800, 425);
            Panel("Marco", raiz, 921, 188, 800, 425, Crema);
            for (int indice = 0; indice < 3; indice++)
            {
                int opcion = indice;
                float posicion = 921 + indice * 272;
                Button tarjeta = Boton("Opcion" + indice, raiz, "", posicion, 663, 256, 307,
                    () => OnOpcionSeleccionada?.Invoke(opcion));
                tarjeta.gameObject.AddComponent<RectMask2D>();
                _opciones[indice] = tarjeta;
                _bordes[indice] = tarjeta.gameObject.AddComponent<Outline>();
                _bordes[indice].effectDistance = new Vector2(6, -6);
                _bordes[indice].enabled = false;
                Imagen("Animal", tarjeta.transform as RectTransform, configuracion.imagenes[indice], 8, 15, 240, 276);
                if (indice == configuracion.indiceCorrecto)
                {
                    foreach (Image imagen in tarjeta.GetComponentsInChildren<Image>())
                        if (imagen.gameObject.name == "AnimalContenido") _guacamayoAnimado = imagen;
                }
            }
            _aceptar = Boton("Aceptar", raiz, "ACEPTAR", 1171, 990, 300, 74, () => OnAceptarSolicitado?.Invoke());
            _reintentar = Boton("Reintentar", raiz, "REINTENTAR", 1171, 990, 300, 74, () => OnReintentarSolicitado?.Invoke());
            _siguiente = Boton("Siguiente", raiz, "SIGUIENTE", 1171, 990, 300, 74, () => OnSiguienteSolicitado?.Invoke());
            _aceptar.interactable = false;
            _reintentar.gameObject.SetActive(false);
            _siguiente.gameObject.SetActive(false);
            if (_navAvanzar != null) _navAvanzar.interactable = false;
            AplicarImagenBoton(_aceptar, _imagenAceptar);
            ConfigurarBotonReintentar(_reintentar);
            AplicarImagenBoton(_siguiente, _imagenSiguiente);
            CrearEstadoResultado(raiz);
            EventSystem.current.SetSelectedGameObject(_opciones[0].gameObject);
        }

        public void MostrarSeleccion(int indice)
        {
            if (indice < 0 || indice >= _opciones.Length) return;
            _seleccion = indice;
            for (int opcion = 0; opcion < _bordes.Length; opcion++)
            {
                _bordes[opcion].enabled = opcion == indice;
                _bordes[opcion].effectColor = Dorado;
                Transform brilloAnterior = _opciones[opcion].transform.Find("BrilloSeleccion");
                if (brilloAnterior != null) Destroy(brilloAnterior.gameObject);
                if (opcion == indice && _brilloSeleccion != null)
                {
                    Image brillo = new GameObject("BrilloSeleccion", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                    brillo.transform.SetParent(_opciones[opcion].transform, false);
                    RectTransform rectBrillo = brillo.rectTransform;
                    rectBrillo.anchorMin = Vector2.zero;
                    rectBrillo.anchorMax = Vector2.one;
                    rectBrillo.offsetMin = Vector2.zero;
                    rectBrillo.offsetMax = Vector2.zero;
                    brillo.sprite = _brilloSeleccion;
                    brillo.type = Image.Type.Sliced;
                    brillo.preserveAspect = false;
                    brillo.raycastTarget = false;
                    brillo.transform.SetAsLastSibling();
                }
            }
            _aceptar.interactable = true;

        }

        public void MostrarResultado(bool correcto)
        {
            // El resultado conserva la presentación de las demás tarjetas. El Presenter ignora nuevas selecciones.
            if (_seleccion >= 0)
            {
                _bordes[_seleccion].effectColor = correcto ? Verde : Rojo;
                Transform brillo = _opciones[_seleccion].transform.Find("BrilloSeleccion");
                if (brillo != null) brillo.GetComponent<Image>().color = correcto ? Verde : Rojo;
            }
            _pregunta.text = correcto ? "¡Muy bien! Identificaste al guacamayo." : "¡Buen intento! Busca el ave de colores. Puedes reintentar.";
            if (_burbujaPregunta != null)
            {
                _burbujaPregunta.color = correcto ? Color.white : new Color(1f, 0.72f, 0.72f);
                Outline bordeMensaje = _burbujaPregunta.GetComponent<Outline>();
                if (!correcto && bordeMensaje == null) bordeMensaje = _burbujaPregunta.gameObject.AddComponent<Outline>();
                if (bordeMensaje != null)
                {
                    bordeMensaje.enabled = !correcto;
                    bordeMensaje.effectColor = Rojo;
                    bordeMensaje.effectDistance = new Vector2(10, -10);
                }
            }
            if (!correcto && _personajePreocupado != null && _exploradorAnimado != null)
            {
                _exploradorAnimado.sprite = _personajePreocupado;
                _saludoExploradorCompleto = true;
            }
            _estadoResultado.SetActive(correcto);
            _siguiente.gameObject.SetActive(correcto);
            if (correcto)
            {
                _textoEstado.text = "¡Lo lograste!";
                _estadoResultado.transform.SetAsLastSibling();
                _indiceFotogramaExplorador = 0;
                if (_personajeCelebrando != null) _imagenExploradorVictoria.sprite = _personajeCelebrando;
                else if (_fotogramasExplorador != null && _fotogramasExplorador.Length > 0)
                    _imagenExploradorVictoria.sprite = _fotogramasExplorador[0];
                _animarExploradorVictoria = _personajeCelebrando == null;
            }
            else _animarExploradorVictoria = false;
            _animarGuacamayoTrasAcierto = correcto;
            if (_ocultarResultado != null) StopCoroutine(_ocultarResultado);
            _ocultarResultado = null;
            _aceptar.gameObject.SetActive(false);
            _reintentar.gameObject.SetActive(!correcto);
            for (int indice = 0; indice < _opciones.Length; indice++) _opciones[indice].interactable = false;
            if (_navAvanzar != null) _navAvanzar.interactable = false;
            _animacionResultado.Reproducir(correcto);
            EventSystem.current.SetSelectedGameObject(correcto ? _siguiente.gameObject : _reintentar.gameObject);
        }

        public void ReiniciarSeleccion()
        {
            _animarGuacamayoTrasAcierto = false;
            _animarExploradorVictoria = false;
            if (_guacamayoAnimado != null && _configuracion != null && _configuracion.imagenes != null)
                _guacamayoAnimado.sprite = _configuracion.imagenes[_configuracion.indiceCorrecto];
            _seleccion = -1;
            _pregunta.text = _configuracion.pregunta;
            if (_burbujaPregunta != null) _burbujaPregunta.color = Color.white;
            if (_exploradorAnimado != null && _configuracion != null)
                _exploradorAnimado.sprite = _configuracion.explorador;
            _saludoExploradorCompleto = false;
            if (_burbujaPregunta != null)
            {
                Outline bordeMensaje = _burbujaPregunta.GetComponent<Outline>();
                if (bordeMensaje != null) bordeMensaje.enabled = false;
            }
            _estadoResultado.SetActive(false);
            if (_ocultarResultado != null) StopCoroutine(_ocultarResultado);
            _ocultarResultado = null;
            for (int indice = 0; indice < _opciones.Length; indice++)
            {
                _opciones[indice].interactable = true;
                _bordes[indice].enabled = false;
            }
            _aceptar.gameObject.SetActive(true);
            _aceptar.interactable = false;
            _reintentar.gameObject.SetActive(false);
            _siguiente.gameObject.SetActive(false);
            if (_navAvanzar != null) _navAvanzar.interactable = false;
            EventSystem.current.SetSelectedGameObject(_opciones[0].gameObject);
        }

        public void ReproducirAudio()
        {
            if (_audio.clip == null)
            {
                MostrarAviso("El audio del guacamayo no está disponible.");
                return;
            }
            _audio.Stop();
            _audio.Play();
        }

        public void MostrarAyuda()
        {
            RectTransform panel = AbrirModal("Ayuda", _cartaPista, 350, 118, 1220, 844);
            Texto("Pista", panel, _configuracion.pistaIncorrecta, 210, 330, 800, 190, 44, Tinta);
            BotonConImagen("CerrarAyuda", panel, _imagenReintentar, 390, 608, 440, 122, CerrarModal).Select();
        }

        public void MostrarSenas()
        {
            RectTransform panel = AbrirModal("Senas", _tarjetaSenas, 230, 140, 1460, 825);
            string ruta = Path.Combine(Application.streamingAssetsPath, "LSEC", "GuacamayoLSEC.mp4");
            if (!File.Exists(ruta))
            {
                Texto("SinVideo", panel, "El video de señas del guacamayo aún no está disponible.", 240, 320, 980, 180, 38, Tinta);
                BotonImagenModal("CerrarSenas", panel, _botonCerrarModal, 1300, 22, 112, 112, CerrarModal).Select();
                return;
            }
            _video = _modal.AddComponent<VideoPlayer>();
            _texturaVideo = new RenderTexture(960, 540, 0);
            _video.playOnAwake = false;
            _video.source = VideoSource.Url;
            _video.url = ruta;
            _video.renderMode = VideoRenderMode.RenderTexture;
            _video.targetTexture = _texturaVideo;
            _video.aspectRatio = VideoAspectRatio.FitInside;
            GameObject pantalla = new GameObject("Video", typeof(RectTransform), typeof(RawImage));
            pantalla.transform.SetParent(panel, false);
            Ubicar(pantalla.transform as RectTransform, 180, 225, 1100, 420);
            pantalla.GetComponent<RawImage>().texture = _texturaVideo;
            BotonImagenModal("Reproducir", panel, _botonReproducirModal, 675, 675, 118, 118, () =>
            {
                if (_video.isPlaying) _video.Pause();
                else _video.Play();
            }).Select();
            BotonImagenModal("CerrarSenas", panel, _botonCerrarModal, 1300, 22, 112, 112, CerrarModal);
        }

        public void MostrarAviso(string mensaje) { _pregunta.text = mensaje; }

        public void MostrarConfiguracion()
        {
            RectTransform panel = AbrirModal("Configuracion");
            Texto("TituloConfiguracion", panel, "CONFIGURACIÓN", 80, 48, 840, 86, 42, Tinta);
            Texto("TextoMovimiento", panel, "Animaciones", 110, 190, 330, 82, 34, Tinta);
            Button movimiento = Boton("ReducirMovimiento", panel, "MOVIMIENTO REDUCIDO: " + (_movimientoReducido ? "SÍ" : "NO"),
                440, 190, 430, 82, () =>
                {
                    _movimientoReducido = !_movimientoReducido;
                    PlayerPrefs.SetInt("HU03_MovimientoReducido", _movimientoReducido ? 1 : 0);
                    PlayerPrefs.Save();
                    CerrarModal();
                    MostrarConfiguracion();
                });
            movimiento.Select();
            float volumen = PlayerPrefs.GetFloat("HU03_AudioVolumen", 1f);
            Texto("TextoAudio", panel, "Volumen de lectura: " + Mathf.RoundToInt(volumen * 100f) + "%", 110, 310, 390, 82, 30, Tinta);
            Boton("BajarVolumen", panel, "−", 520, 310, 130, 82, () => AjustarVolumen(-0.1f));
            Boton("SubirVolumen", panel, "+", 670, 310, 130, 82, () => AjustarVolumen(0.1f));
            Boton("CerrarConfiguracion", panel, "GUARDAR Y CERRAR", 280, 550, 440, 82, CerrarModal).Select();
        }

        private void AjustarVolumen(float delta)
        {
            float volumen = Mathf.Clamp01(PlayerPrefs.GetFloat("HU03_AudioVolumen", 1f) + delta);
            PlayerPrefs.SetFloat("HU03_AudioVolumen", volumen);
            PlayerPrefs.Save();
            if (_audio != null) _audio.volume = volumen;
            CerrarModal();
            MostrarConfiguracion();
        }

        private void Update()
        {
            if (_movimientoReducido) return;
            _tiempoFotograma += Time.unscaledDeltaTime;
            if (_tiempoFotograma >= 1f / 24f)
            {
                _tiempoFotograma -= 1f / 24f;
                if (!_saludoExploradorCompleto)
                {
                    AvanzarFotograma(_fotogramasExplorador, ref _indiceFotogramaExplorador, _exploradorAnimado);
                    if (_fotogramasExplorador != null && _indiceFotogramaExplorador >= _fotogramasExplorador.Length)
                    {
                        _indiceFotogramaExplorador = _fotogramasExplorador.Length - 1;
                        _saludoExploradorCompleto = true;
                    }
                }
            }
            if (_animarGuacamayoTrasAcierto)
            {
                _tiempoGuacamayo += Time.unscaledDeltaTime;
                if (_tiempoGuacamayo >= 1f / 16f)
                {
                    _tiempoGuacamayo -= 1f / 16f;
                    AvanzarFotograma(_fotogramasGuacamayo, ref _indiceFotogramaGuacamayo, _guacamayoAnimado);
                }
            }
            if (_animarExploradorVictoria)
            {
                _tiempoExploradorVictoria += Time.unscaledDeltaTime;
                if (_tiempoExploradorVictoria >= 1f / 12f)
                {
                    _tiempoExploradorVictoria -= 1f / 12f;
                    AvanzarFotograma(_fotogramasExplorador, ref _indiceFotogramaExplorador, _imagenExploradorVictoria);
                }
            }
        }

        private void CrearEscenaIlustrada(RectTransform raiz, RetoSeleccionConfiguracion configuracion,
            float x, float y, float ancho, float alto)
        {
            GameObject contenedor = new GameObject("IlustracionFauna", typeof(RectTransform), typeof(RectMask2D));
            contenedor.transform.SetParent(raiz, false);
            RectTransform rectContenedor = contenedor.transform as RectTransform;
            Ubicar(rectContenedor, x, y, ancho, alto);
            Sprite ilustracion = configuracion.imagenEscena != null
                ? configuracion.imagenEscena
                : configuracion.fondo;
            CrearImagen("FaunaAmazonica", rectContenedor, ilustracion, Color.white, true, false,
                Vector2.zero, new Vector2(ancho, alto), true);
        }

        private IEnumerator OcultarResultadoTrasTiempo(float segundos)
        {
            yield return new WaitForSecondsRealtime(segundos);
            if (_estadoResultado != null) _estadoResultado.SetActive(false);
            _ocultarResultado = null;
        }

        private void CrearNavegacion(RectTransform raiz)
        {
            string[] nombres = { "Regresar", "Avanzar", "Inicio", "Configuracion" };
            Action[] acciones = { () => OnRegresarSolicitado?.Invoke(), () => OnAvanzarSolicitado?.Invoke(),
                () => OnInicioSolicitado?.Invoke(), () => OnConfiguracionSolicitada?.Invoke() };
            for (int i = 0; i < nombres.Length; i++)
            {
                Button boton = Boton(nombres[i], raiz, nombres[i] == "Configuracion" ? "AJUSTES" : nombres[i].ToUpperInvariant(),
                    1390 + i * 128, 20, 118, 94, acciones[i]);
                if (_iconosNavegacion != null && i < _iconosNavegacion.Length) AplicarImagenBoton(boton, _iconosNavegacion[i]);
                if (i == 1) _navAvanzar = boton;
            }
        }

        private void CrearEstadoResultado(RectTransform raiz)
        {
            GameObject objeto = new GameObject("EstadoResultado", typeof(RectTransform), typeof(Image));
            objeto.transform.SetParent(raiz, false);
            Ubicar(objeto.transform as RectTransform, 0, 0, 1920, 1080);
            Image imagen = objeto.GetComponent<Image>();
            imagen.color = new Color(0.01f, 0.08f, 0.05f, 0.82f);
            imagen.raycastTarget = true;
            if (_fondoAcierto != null)
            {
                Image fondoVerde = CrearImagen("FondoVerdeAcierto", objeto.transform as RectTransform,
                    _fondoAcierto, new Color(1f, 1f, 1f, 0.78f), true, false,
                    Vector2.zero, new Vector2(1920, 1080), true);
                fondoVerde.transform.SetAsFirstSibling();
            }
            _imagenExploradorVictoria = CrearImagen("ExploradorVictoria", objeto.transform as RectTransform,
                _configuracion.explorador, Color.white, false, false, Vector2.zero, new Vector2(540, 690), false);
            Ubicar(_imagenExploradorVictoria.rectTransform, 270, 235, 540, 690);
            Image letrero = CrearImagen("LetreroMuyBien", objeto.transform as RectTransform, _letreroAcierto,
                Color.white, false, false, Vector2.zero, new Vector2(870, 285), false);
            Ubicar(letrero.rectTransform, 805, 210, 870, 285);
            _textoEstado = Texto("MensajeResultado", objeto.transform as RectTransform, "¡Lo lograste!",
                850, 500, 780, 100, 62, Color.white);
            _siguiente.transform.SetParent(objeto.transform, false);
            Ubicar(_siguiente.transform as RectTransform, 945, 660, 555, 155);
            _siguiente.gameObject.SetActive(false);
            _animacionResultado = letrero.gameObject.AddComponent<AnimacionEntradaUI>();
            _estadoResultado = objeto;
            _estadoResultado.SetActive(false);
        }

        private void ConfigurarBotonReintentar(Button boton)
        {
            if (boton == null) return;
            RectTransform rect = boton.transform as RectTransform;
            Ubicar(rect, 1075, 965, 500, 84);
            RoundedRectangleGraphic fondo = boton.GetComponent<RoundedRectangleGraphic>();
            if (fondo != null) fondo.color = new Color(0.02f, 0.42f, 0.94f);
            boton.colors = ColoresBoton(new Color(0.02f, 0.42f, 0.94f), new Color(0.15f, 0.58f, 1f));
            TMP_Text etiqueta = boton.GetComponentInChildren<TMP_Text>();
            if (etiqueta != null)
            {
                etiqueta.text = "INTENTARLO DE NUEVO";
                etiqueta.color = Color.white;
                etiqueta.fontSize = 24;
                etiqueta.fontSizeMin = 22;
                etiqueta.fontSizeMax = 24;
                RectTransform textoRect = etiqueta.rectTransform;
                textoRect.sizeDelta = new Vector2(360, 76);
                textoRect.anchoredPosition = new Vector2(65, 0);
            }
            if (_iconoReintentar != null)
            {
                Image icono = CrearImagen("IconoReintentar", rect, _iconoReintentar, Color.white,
                    false, false, Vector2.zero, new Vector2(64, 64), false);
                Ubicar(icono.rectTransform, 64, 10, 64, 64);
            }
        }

        private static void AplicarImagenBoton(Button boton, Sprite sprite)
        {
            if (boton == null || sprite == null) return;
            RoundedRectangleGraphic fondo = boton.GetComponent<RoundedRectangleGraphic>();
            if (fondo != null) fondo.enabled = false;
            TMP_Text etiqueta = boton.GetComponentInChildren<TMP_Text>();
            if (etiqueta != null) etiqueta.text = string.Empty;
            GameObject objetoImagen = new GameObject("IconoAprobado", typeof(RectTransform), typeof(Image));
            objetoImagen.transform.SetParent(boton.transform, false);
            RectTransform rect = objetoImagen.transform as RectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image imagen = objetoImagen.GetComponent<Image>();
            imagen.sprite = sprite;
            imagen.preserveAspect = true;
            imagen.raycastTarget = true;
            boton.targetGraphic = imagen;
            boton.colors = ColoresBoton(Color.white, new Color(1f, 0.94f, 0.7f));
        }

        private static void AvanzarFotograma(Sprite[] fotogramas, ref int indice, Image imagen)
        {
            if (fotogramas == null || fotogramas.Length == 0 || imagen == null) return;
            imagen.sprite = fotogramas[indice++ % fotogramas.Length];
        }

        private RectTransform AbrirModal(string nombre, Sprite fondoModal = null,
            float x = 460, float y = 200, float ancho = 1000, float alto = 680)
        {
            if (_modal != null) CerrarModal();
            _focoAnterior = EventSystem.current.currentSelectedGameObject;
            RectTransform raiz = _pregunta.canvas.transform as RectTransform;
            _modal = new GameObject("Modal" + nombre, typeof(RectTransform), typeof(Image));
            _modal.transform.SetParent(raiz, false);
            Ubicar(_modal.transform as RectTransform, 0, 0, 1920, 1080);
            _modal.GetComponent<Image>().color = new Color(0, 0, 0, 0.65f);
            // Keep keyboard navigation inside the modal as well as blocking pointer input.
            foreach (Selectable control in raiz.GetComponentsInChildren<Selectable>())
            {
                if (control.interactable) _controlesModal.Add(control);
                control.interactable = false;
            }
            if (fondoModal == null) return Panel("Panel", _modal.transform as RectTransform, x, y, ancho, alto, Crema);
            Image panelImagen = CrearImagen("Panel", _modal.transform as RectTransform, fondoModal, Color.white,
                false, false, Vector2.zero, new Vector2(ancho, alto), true);
            Ubicar(panelImagen.rectTransform, x, y, ancho, alto);
            return panelImagen.rectTransform;
        }

        private Button BotonConImagen(string nombre, RectTransform padre, Sprite sprite,
            float x, float y, float ancho, float alto, Action accion)
        {
            Button boton = Boton(nombre, padre, string.Empty, x, y, ancho, alto, accion);
            AplicarImagenBoton(boton, sprite);
            return boton;
        }

        private Button BotonImagenModal(string nombre, RectTransform padre, Sprite sprite,
            float x, float y, float ancho, float alto, Action accion)
        {
            return BotonConImagen(nombre, padre, sprite, x, y, ancho, alto, accion);
        }

        private readonly System.Collections.Generic.List<Selectable> _controlesModal = new System.Collections.Generic.List<Selectable>();

        public void CerrarModal()
        {
            if (_video != null) _video.Stop();
            if (_texturaVideo != null) { _texturaVideo.Release(); Destroy(_texturaVideo); }
            if (_modal != null) { _modal.SetActive(false); Destroy(_modal); }
            _modal = null;
            foreach (Selectable control in _controlesModal) if (control != null) control.interactable = true;
            _controlesModal.Clear();
            if (_focoAnterior != null && _focoAnterior.activeInHierarchy) EventSystem.current.SetSelectedGameObject(_focoAnterior);
        }

        private void OnDisable()
        {
            if (_audio != null) _audio.Stop();
            CerrarModal();
        }

        private static void Ubicar(RectTransform rect, float x, float y, float ancho, float alto)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(ancho, alto);
        }

        private RectTransform Panel(string nombre, RectTransform padre, float x, float y, float ancho, float alto, Color color)
        {
            Sprite sprite = nombre == "Globo" ? _globo
                : nombre == "Marco" ? _marco
                : nombre == "Titulo" ? _bannerNivel
                : null;
            RectTransform panel;
            if (sprite != null)
            {
                panel = CrearImagen(nombre, padre, sprite, Color.white, false, false, Vector2.zero,
                    new Vector2(ancho, alto), true).rectTransform;
            }
            else
            {
                panel = CrearPanel(nombre, padre, Vector2.zero, new Vector2(ancho, alto), color);
            }
            Ubicar(panel, x, y, ancho, alto);
            return panel;
        }

        private TMP_Text Texto(string nombre, RectTransform padre, string texto, float x, float y, float ancho, float alto, float tamano, Color color)
        {
            TMP_Text etiqueta = CrearTexto(nombre, padre, texto, Vector2.zero, new Vector2(ancho, alto), tamano, color, TextAlignmentOptions.Center, true);
            Ubicar(etiqueta.rectTransform, x, y, ancho, alto);
            return etiqueta;
        }

        private void Imagen(string nombre, RectTransform padre, Sprite sprite, float x, float y, float ancho, float alto)
        {
            GameObject contenedor = new GameObject(nombre, typeof(RectTransform), typeof(RectMask2D));
            contenedor.transform.SetParent(padre, false);
            RectTransform rect = contenedor.transform as RectTransform;
            Ubicar(rect, x, y, ancho, alto);
            CrearImagen(nombre + "Contenido", rect, sprite, Color.white, true, false,
                Vector2.zero, new Vector2(ancho, alto), false);
        }

        private Button Boton(string nombre, RectTransform padre, string texto, float x, float y, float ancho, float alto, Action accion)
        {
            Button boton = CrearBoton(nombre, padre, texto, Vector2.zero, new Vector2(ancho, alto), Crema, Tinta);
            int indiceIcono = nombre == "Audio" ? 0 : nombre == "Senas" ? 1 : nombre == "Ayuda" ? 2 : -1;
            if (indiceIcono >= 0 && _iconosToolbar != null && indiceIcono < _iconosToolbar.Length && _iconosToolbar[indiceIcono] != null)
            {
                RoundedRectangleGraphic formaBase = boton.GetComponent<RoundedRectangleGraphic>();
                if (formaBase != null) formaBase.enabled = false;
                TMP_Text etiqueta = boton.GetComponentInChildren<TMP_Text>();
                if (etiqueta != null) etiqueta.text = string.Empty;
                GameObject contenedorIcono = new GameObject("Icono" + nombre, typeof(RectTransform), typeof(Image));
                contenedorIcono.transform.SetParent(boton.transform, false);
                RectTransform rectIcono = contenedorIcono.transform as RectTransform;
                rectIcono.anchorMin = Vector2.zero;
                rectIcono.anchorMax = Vector2.one;
                rectIcono.offsetMin = Vector2.zero;
                rectIcono.offsetMax = Vector2.zero;
                Image icono = contenedorIcono.GetComponent<Image>();
                icono.sprite = _iconosToolbar[indiceIcono];
                icono.preserveAspect = true;
                boton.targetGraphic = icono;
                boton.colors = ColoresBoton(Color.white, new Color(0.9f, 0.97f, 0.82f));
            }
            Ubicar(boton.transform as RectTransform, x, y, ancho, alto);
            boton.onClick.AddListener(() => accion());
            return boton;
        }
        private Canvas CrearCanvas()
        {
            GameObject objeto = new GameObject("CanvasGuacamayo", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = objeto.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = objeto.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private void CrearEventSystemSiHaceFalta()
        {
            if (EventSystem.current != null) return;
            GameObject objeto = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            InputSystemUIInputModule modulo = objeto.GetComponent<InputSystemUIInputModule>();
            modulo.AssignDefaultActions();
        }

        private Image CrearImagen(string nombre, RectTransform padre, Sprite sprite, Color color,
            bool estirar, bool interceptarRaycast, Vector2 posicion, Vector2 tamano, bool fondo)
        {
            GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            objeto.transform.SetParent(padre, false);
            RectTransform rect = objeto.transform as RectTransform;
            if (estirar)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            else
            {
                PrepararRect(rect, posicion, tamano);
            }
            Image imagen = objeto.GetComponent<Image>();
            imagen.sprite = sprite;
            imagen.color = color;
            imagen.preserveAspect = !fondo;
            imagen.raycastTarget = interceptarRaycast;
            return imagen;
        }

        private RectTransform CrearPanel(string nombre, RectTransform padre, Vector2 posicion, Vector2 tamano, Color color)
        {
            GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer));
            objeto.transform.SetParent(padre, false);
            RectTransform rect = objeto.transform as RectTransform;
            PrepararRect(rect, posicion, tamano);
            RoundedRectangleGraphic fondo = objeto.AddComponent<RoundedRectangleGraphic>();
            fondo.color = color;
            fondo.raycastTarget = false;
            return rect;
        }

        private TMP_Text CrearTexto(string nombre, RectTransform padre, string texto, Vector2 posicion,
            Vector2 tamano, float tamanoFuente, Color color, TextAlignmentOptions alineacion, bool negrita)
        {
            GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            objeto.transform.SetParent(padre, false);
            RectTransform rect = objeto.transform as RectTransform;
            PrepararRect(rect, posicion, tamano);
            TMP_Text etiqueta = objeto.GetComponent<TextMeshProUGUI>();
            etiqueta.text = texto;
            etiqueta.font = _configuracion.fuente;
            etiqueta.fontSize = tamanoFuente;
            etiqueta.enableAutoSizing = true;
            etiqueta.fontSizeMin = Mathf.Max(16f, tamanoFuente - 8f);
            etiqueta.fontSizeMax = tamanoFuente;
            etiqueta.color = color;
            etiqueta.alignment = alineacion;
            etiqueta.fontStyle = negrita ? FontStyles.Bold : FontStyles.Normal;
            etiqueta.textWrappingMode = TextWrappingModes.Normal;
            etiqueta.raycastTarget = false;
            return etiqueta;
        }

        private Button CrearBoton(string nombre, RectTransform padre, string etiqueta,
            Vector2 posicion, Vector2 tamano, Color fondoColor, Color textoColor)
        {
            GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer));
            objeto.transform.SetParent(padre, false);
            RectTransform rect = objeto.transform as RectTransform;
            PrepararRect(rect, posicion, tamano);
            Graphic fondo;
            if (nombre.StartsWith("Opcion", StringComparison.Ordinal) && _tarjeta != null)
            {
                Image imagen = objeto.AddComponent<Image>();
                imagen.sprite = _tarjeta;
                fondo = imagen;
            }
            else
            {
                fondo = objeto.AddComponent<RoundedRectangleGraphic>();
            }
            fondo.color = fondoColor;
            Button boton = objeto.AddComponent<Button>();
            boton.targetGraphic = fondo;
            boton.colors = ColoresBoton(fondoColor, Color.white);
            CrearTexto("Etiqueta", rect, etiqueta, Vector2.zero, tamano - new Vector2(16f, 12f), 27f,
                textoColor, TextAlignmentOptions.Center, true);
            return boton;
        }

        private static ColorBlock ColoresBoton(Color normal, Color resaltado)
        {
            ColorBlock colores = ColorBlock.defaultColorBlock;
            colores.normalColor = normal;
            colores.highlightedColor = resaltado;
            colores.pressedColor = Dorado;
            colores.selectedColor = resaltado;
            colores.disabledColor = new Color(0.65f, 0.65f, 0.6f, 0.75f);
            colores.colorMultiplier = 1f;
            return colores;
        }

        private static void PrepararRect(RectTransform rect, Vector2 posicion, Vector2 tamano)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = posicion;
            rect.sizeDelta = tamano;
        }
    }
}
