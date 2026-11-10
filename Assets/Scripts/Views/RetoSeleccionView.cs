using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using GuardianesAmazonicos.Models;

namespace GuardianesAmazonicos.Views
{
    public sealed class RetoSeleccionView : MonoBehaviour
    {
        private static readonly Color Crema = new Color(1f, 0.97f, 0.86f, 0.98f);
        private static readonly Color Verde = new Color(0.08f, 0.31f, 0.17f, 1f);
        private static readonly Color VerdeClaro = new Color(0.78f, 0.91f, 0.61f, 1f);
        private static readonly Color Dorado = new Color(1f, 0.68f, 0.13f, 1f);
        private static readonly Color Tinta = new Color(0.05f, 0.17f, 0.12f, 1f);

        public event Action<int> OnOpcionSeleccionada;
        public event Action OnAudioSolicitado;
        public event Action OnContinuarSolicitado;

        public int IndiceSeleccionado { get; private set; } = -1;

        private RetoSeleccionConfiguracion _configuracion;
        private Button[] _botonesOpciones;
        private Outline[] _bordesOpciones;
        private TMP_Text _textoEstado;
        private Button _botonContinuar;
        private Button _botonAudio;
        private AudioSource _audioSource;
        private Sprite[] _spritesAtlas;

        public void Construir(RetoSeleccionConfiguracion configuracion)
        {
            _configuracion = configuracion;
            CrearEventSystemSiHaceFalta();
            _audioSource = gameObject.GetComponent<AudioSource>();
            if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;

            Canvas canvas = CrearCanvas();
            RectTransform root = canvas.transform as RectTransform;
            CrearImagen("FondoSelva", root, _configuracion.fondo, Color.white, true, true,
                Vector2.zero, new Vector2(1920f, 1080f), true);
            CrearImagen("SombraSuave", root, null, new Color(0.01f, 0.12f, 0.07f, 0.17f),
                true, false, Vector2.zero, new Vector2(1920f, 1080f), true);

            CrearTexto("Marca", root, "GUARDIANES DE LA AMAZONÍA LECTORA", new Vector2(-690f, 485f),
                new Vector2(500f, 54f), 23f, Tinta, TextAlignmentOptions.Left, false);
            CrearPanel("PlacaNivel", root, new Vector2(0f, 480f), new Vector2(540f, 70f), Verde);
            CrearTexto("TituloNivel", root, _configuracion.titulo, new Vector2(0f, 480f),
                new Vector2(510f, 56f), 31f, Color.white, TextAlignmentOptions.Center, true);
            CrearPanel("InsigniaProgreso", root, new Vector2(760f, 480f), new Vector2(220f, 70f), Verde);
            CrearTexto("Progreso", root, _configuracion.numeroEscena + " de 12", new Vector2(760f, 480f),
                new Vector2(210f, 56f), 30f, Color.white, TextAlignmentOptions.Center, true);

            Image personaje = CrearImagen("Explorador", root, _configuracion.explorador, Color.white,
                false, false, new Vector2(-710f, -105f), new Vector2(500f, 680f), false);
            personaje.preserveAspect = true;

            RectTransform panel = CrearPanel("PanelReto", root, new Vector2(215f, -5f), new Vector2(1240f, 850f), Crema);
            CrearTexto("Pregunta", panel, _configuracion.pregunta, new Vector2(0f, 352f),
                new Vector2(1130f, 86f), 48f, Tinta, TextAlignmentOptions.Center, true);
            CrearTexto("Instruccion", panel, _configuracion.instruccion, new Vector2(0f, 293f),
                new Vector2(1080f, 52f), 27f, Verde, TextAlignmentOptions.Center, true);

            if (_configuracion.atlasDosPorDos != null)
            {
                _spritesAtlas = CrearSpritesAtlas(_configuracion.atlasDosPorDos);
            }

            CrearTarjetas(panel);
            _textoEstado = CrearTexto("Estado", panel, "Elige una imagen para comenzar.", new Vector2(0f, -290f),
                new Vector2(1080f, 56f), 27f, Verde, TextAlignmentOptions.Center, true);

            if (_configuracion.pronunciaciones != null && _configuracion.pronunciaciones.Length > 0)
            {
                _botonAudio = CrearBoton("BotonAudio", panel, "AUDIO · ESCUCHAR NOMBRE",
                    new Vector2(-255f, -368f), new Vector2(430f, 78f), VerdeClaro, Tinta);
                _botonAudio.onClick.AddListener(() => OnAudioSolicitado?.Invoke());
            }

            _botonContinuar = CrearBoton("BotonContinuar", panel, _configuracion.textoContinuar,
                new Vector2(310f, -368f), new Vector2(450f, 78f), Dorado, Tinta);
            _botonContinuar.interactable = false;
            _botonContinuar.onClick.AddListener(() => OnContinuarSolicitado?.Invoke());
        }

        public void MostrarSeleccion(int indice)
        {
            IndiceSeleccionado = indice;
            for (int i = 0; i < _bordesOpciones.Length; i++)
            {
                _bordesOpciones[i].effectColor = i == indice ? Verde : new Color(0.45f, 0.34f, 0.15f, 0.4f);
                _bordesOpciones[i].effectDistance = i == indice ? new Vector2(5f, -5f) : new Vector2(2f, -2f);
            }
            _textoEstado.text = "Seleccionaste: " + _configuracion.opciones[indice] + ".";
            _textoEstado.color = Verde;
        }

        public void MostrarIncorrecta(string pista)
        {
            _textoEstado.text = "¡Buen intento! " + pista;
            _textoEstado.color = new Color(0.45f, 0.25f, 0.03f, 1f);
        }

        public void MostrarCorrecta(string mensaje)
        {
            _textoEstado.text = mensaje;
            _textoEstado.color = Verde;
            _botonContinuar.interactable = true;
            for (int i = 0; i < _botonesOpciones.Length; i++) _botonesOpciones[i].interactable = false;
        }

        public void MostrarSiguienteEnPreparacion()
        {
            _textoEstado.text = "¡Progreso guardado! El siguiente reto se añadirá después.";
            _textoEstado.color = Verde;
        }

        public void ReproducirPronunciacion(int indice)
        {
            if (_configuracion.pronunciaciones == null || _configuracion.pronunciaciones.Length == 0) return;
            if (indice < 0 || indice >= _configuracion.pronunciaciones.Length) indice = _configuracion.indiceCorrecto;
            AudioClip clip = _configuracion.pronunciaciones[indice];
            if (clip == null) return;
            _audioSource.Stop();
            _audioSource.PlayOneShot(clip);
            _textoEstado.text = "Escucha: " + _configuracion.opciones[indice] + ".";
            _textoEstado.color = Verde;
        }

        private void CrearTarjetas(RectTransform panel)
        {
            _botonesOpciones = new Button[_configuracion.opciones.Length];
            _bordesOpciones = new Outline[_configuracion.opciones.Length];
            Vector2[] posiciones =
            {
                new Vector2(-265f, 145f), new Vector2(265f, 145f),
                new Vector2(-265f, -125f), new Vector2(265f, -125f)
            };

            for (int i = 0; i < _configuracion.opciones.Length; i++)
            {
                int indice = i;
                GameObject tarjeta = new GameObject("TarjetaOpcion" + (i + 1), typeof(RectTransform), typeof(CanvasRenderer));
                tarjeta.transform.SetParent(panel, false);
                RectTransform rect = tarjeta.transform as RectTransform;
                PrepararRect(rect, posiciones[i], new Vector2(480f, 245f));
                RoundedRectangleGraphic fondo = tarjeta.AddComponent<RoundedRectangleGraphic>();
                fondo.color = new Color(1f, 0.995f, 0.96f, 1f);
                Button boton = tarjeta.AddComponent<Button>();
                boton.targetGraphic = fondo;
                boton.transition = Selectable.Transition.ColorTint;
                boton.colors = ColoresBoton(Color.white, new Color(0.96f, 0.91f, 0.72f, 1f));
                Outline borde = tarjeta.AddComponent<Outline>();
                borde.effectColor = new Color(0.45f, 0.34f, 0.15f, 0.4f);
                borde.effectDistance = new Vector2(2f, -2f);
                _botonesOpciones[i] = boton;
                _bordesOpciones[i] = borde;

                Sprite sprite = ObtenerSprite(i);
                Image imagen = CrearImagen("Imagen", rect, sprite, Color.white, false, false,
                    new Vector2(0f, 32f), new Vector2(205f, 150f), false);
                imagen.preserveAspect = true;
                CrearTexto("Nombre", rect, _configuracion.opciones[i], new Vector2(0f, -88f),
                    new Vector2(430f, 58f), 34f, Tinta, TextAlignmentOptions.Center, true);
                boton.onClick.AddListener(() =>
                {
                    MostrarSeleccion(indice);
                    OnOpcionSeleccionada?.Invoke(indice);
                });
            }
        }

        private Sprite ObtenerSprite(int indice)
        {
            if (_configuracion.imagenes != null && indice < _configuracion.imagenes.Length && _configuracion.imagenes[indice] != null)
                return _configuracion.imagenes[indice];
            if (_spritesAtlas != null && indice < _spritesAtlas.Length) return _spritesAtlas[indice];
            return null;
        }

        private static Sprite[] CrearSpritesAtlas(Texture2D atlas)
        {
            int ancho = atlas.width / 2;
            int alto = atlas.height / 2;
            return new[]
            {
                Sprite.Create(atlas, new Rect(0, alto, ancho, alto), new Vector2(0.5f, 0.5f), 100f),
                Sprite.Create(atlas, new Rect(ancho, alto, ancho, alto), new Vector2(0.5f, 0.5f), 100f),
                Sprite.Create(atlas, new Rect(0, 0, ancho, alto), new Vector2(0.5f, 0.5f), 100f),
                Sprite.Create(atlas, new Rect(ancho, 0, ancho, alto), new Vector2(0.5f, 0.5f), 100f)
            };
        }

        private Canvas CrearCanvas()
        {
            GameObject objeto = new GameObject("CanvasReto", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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
            TMP_Text tmp = objeto.GetComponent<TextMeshProUGUI>();
            tmp.text = texto;
            tmp.font = _configuracion.fuente;
            tmp.fontSize = tamanoFuente;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = Mathf.Max(16f, tamanoFuente - 8f);
            tmp.fontSizeMax = tamanoFuente;
            tmp.color = color;
            tmp.alignment = alineacion;
            tmp.fontStyle = negrita ? FontStyles.Bold : FontStyles.Normal;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            return tmp;
        }

        private Button CrearBoton(string nombre, RectTransform padre, string etiqueta,
            Vector2 posicion, Vector2 tamano, Color fondoColor, Color textoColor)
        {
            GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer));
            objeto.transform.SetParent(padre, false);
            RectTransform rect = objeto.transform as RectTransform;
            PrepararRect(rect, posicion, tamano);
            RoundedRectangleGraphic fondo = objeto.AddComponent<RoundedRectangleGraphic>();
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
