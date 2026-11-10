using System;
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

        [SerializeField] private Sprite _tarjeta;
        [SerializeField] private Sprite _marco;
        [SerializeField] private Sprite _globo;
        [SerializeField] private Sprite[] _iconosToolbar;
        private TMP_Text _pregunta;
        private RetoSeleccionConfiguracion _configuracion;

        public void ConfigurarArte(Sprite tarjeta, Sprite marco, Sprite globo, Sprite[] iconosToolbar)
        {
            _tarjeta = tarjeta;
            _marco = marco;
            _globo = globo;
            _iconosToolbar = iconosToolbar;
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
            _audio.clip = configuracion.pronunciaciones != null && configuracion.pronunciaciones.Length > 0
                ? configuracion.pronunciaciones[0] : null;
            RectTransform raiz = CrearCanvas().transform as RectTransform;
            CrearImagen("Fondo", raiz, configuracion.fondo, Color.white, true, false,
                Vector2.zero, new Vector2(1920, 1080), true);
            Boton("Audio", raiz, "AUDIO", 30, 16, 132, 114, () => OnAudioSolicitado?.Invoke());
            Boton("Senas", raiz, "SEÑAS", 170, 16, 132, 114, () => OnSenasSolicitadas?.Invoke());
            Boton("Ayuda", raiz, "AYUDA", 310, 16, 132, 114, () => OnAyudaSolicitada?.Invoke());
            Panel("Titulo", raiz, 592, 12, 484, 108, Verde);
            Texto("TituloNivel", raiz, "Nivel 1 · Conozco la selva", 602, 22, 464, 88, 30, Color.white);
            Panel("Progreso", raiz, 1092, 16, 236, 94, Verde);
            Texto("NumeroReto", raiz, "3 de 12", 1102, 26, 216, 74, 32, Color.white);
            Panel("Globo", raiz, 158, 174, 684, 240, Crema);
            _pregunta = Texto("Pregunta", raiz, configuracion.pregunta, 201, 208, 598, 150, 40, Tinta);
            Imagen("Guardian", raiz, configuracion.explorador, 147, 428, 390, 562);
            Panel("Marco", raiz, 921, 188, 800, 425, Crema);
            Imagen("Escena", raiz, configuracion.imagenes[configuracion.indiceCorrecto], 942, 206, 758, 390);
            for (int indice = 0; indice < 3; indice++)
            {
                int opcion = indice;
                float posicion = 921 + indice * 272;
                Button tarjeta = Boton("Opcion" + indice, raiz, "", posicion, 663, 256, 307,
                    () => OnOpcionSeleccionada?.Invoke(opcion));
                _opciones[indice] = tarjeta;
                _bordes[indice] = tarjeta.gameObject.AddComponent<Outline>();
                _bordes[indice].effectDistance = new Vector2(6, -6);
                _bordes[indice].enabled = false;
                Imagen("Animal", tarjeta.transform as RectTransform, configuracion.imagenes[indice], 18, 22, 220, 188);
                Texto("Nombre", tarjeta.transform as RectTransform, configuracion.opciones[indice], 15, 216, 226, 76, 30, Tinta);
            }
            _aceptar = Boton("Aceptar", raiz, "ACEPTAR", 1171, 990, 300, 74, () => OnAceptarSolicitado?.Invoke());
            _reintentar = Boton("Reintentar", raiz, "REINTENTAR", 1171, 990, 300, 74, () => OnReintentarSolicitado?.Invoke());
            _siguiente = Boton("Siguiente", raiz, "SIGUIENTE", 1171, 990, 300, 74, () => OnSiguienteSolicitado?.Invoke());
            _aceptar.interactable = false;
            _reintentar.gameObject.SetActive(false);
            _siguiente.gameObject.SetActive(false);
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
            }
            _aceptar.interactable = true;

        }

        public void MostrarResultado(bool correcto)
        {
            // El resultado conserva la presentación de las demás tarjetas. El Presenter ignora nuevas selecciones.
            if (_seleccion >= 0) _bordes[_seleccion].effectColor = correcto ? Verde : Rojo;
            _pregunta.text = correcto ? "¡MUY BIEN!\nIdentificaste al guacamayo." : "¡Buen intento!\nBusca el ave de colores. Puedes reintentar.";
            _aceptar.gameObject.SetActive(false);
            _reintentar.gameObject.SetActive(!correcto);
            _siguiente.gameObject.SetActive(correcto);
            EventSystem.current.SetSelectedGameObject(correcto ? _siguiente.gameObject : _reintentar.gameObject);
        }

        public void ReiniciarSeleccion()
        {
            _seleccion = -1;
            _pregunta.text = _configuracion.pregunta;
            for (int indice = 0; indice < _opciones.Length; indice++)
            {
                _opciones[indice].interactable = true;
                _bordes[indice].enabled = false;
            }
            _aceptar.gameObject.SetActive(true);
            _aceptar.interactable = false;
            _reintentar.gameObject.SetActive(false);
            _siguiente.gameObject.SetActive(false);
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
            RectTransform panel = AbrirModal("Ayuda");
            Texto("Pista", panel, _configuracion.pistaIncorrecta, 70, 55, 860, 130, 38, Tinta);
            Imagen("PistaVisual", panel, _configuracion.imagenes[_configuracion.indiceCorrecto], 300, 200, 400, 330);
            Boton("CerrarAyuda", panel, "¡VOY A INTENTARLO!", 280, 550, 440, 82, CerrarModal).Select();
        }

        public void MostrarSenas()
        {
            RectTransform panel = AbrirModal("Senas");
            string ruta = Path.Combine(Application.streamingAssetsPath, "LSEC", "GuacamayoLSEC.mp4");
            if (!File.Exists(ruta))
            {
                Texto("SinVideo", panel, "El video de señas del guacamayo aún no está disponible.", 70, 110, 860, 300, 38, Tinta);
                Boton("CerrarSenas", panel, "CERRAR", 350, 550, 300, 82, CerrarModal).Select();
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
            Ubicar(pantalla.transform as RectTransform, 70, 30, 860, 484);
            pantalla.GetComponent<RawImage>().texture = _texturaVideo;
            Boton("Reproducir", panel, "REPRODUCIR / PAUSAR", 50, 550, 550, 82, () =>
            {
                if (_video.isPlaying) _video.Pause();
                else _video.Play();
            }).Select();
            Boton("CerrarSenas", panel, "CERRAR", 650, 550, 300, 82, CerrarModal);
        }

        public void MostrarAviso(string mensaje) { _pregunta.text = mensaje; }

        private RectTransform AbrirModal(string nombre)
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
            return Panel("Panel", _modal.transform as RectTransform, 460, 200, 1000, 680, Crema);
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
            Sprite sprite = nombre == "Globo" ? _globo : nombre == "Marco" ? _marco : null;
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
