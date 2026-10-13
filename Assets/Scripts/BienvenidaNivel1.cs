using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GuardianesAmazonicos.Nivel1
{
    public sealed class BienvenidaNivel1 : MonoBehaviour
    {
        private const string TextoBienvenida = "¡Hola, explorador! Bienvenido a la selva. Hoy vamos a conocer algunos de sus tesoros. ¿Quieres comenzar?";
        private const string ClaveInicio = "guardianes.nivel1.iniciado";
        private const string ClaveProgreso = "guardianes.nivel1.progreso";
        private const int TotalRetos = 12;
        private const string TextoPista = "Observa la selva, escucha al Guardián y presiona COMENZAR cuando estés listo para explorar.";
        private static TMP_FontAsset _tipografiaCompartida;

        [SerializeField] private Sprite _fondoSelva;
        [SerializeField] private AudioClip _audioBienvenida;

        private AudioSource _reproductor;
        private TMP_Text _mensaje;
        private TMP_Text _progreso;
        private TMP_Text _estadoAudio;
        private TMP_Text _botonComenzarTexto;
        private Button _botonComenzar;
        private bool _aventuraIniciada;
        private GameObject _modalActual;
        private RectTransform _panelSeniasRect;
        private TMP_Text _etiquetaAmpliarLsec;
        private string _textoSeniasActual = TextoBienvenida;
        private string _tituloSeniasActual = "Bienvenida del Guardián";
        private bool _visorLsecAmpliado;

        private void Start()
        {
            CargarRecursosLocales();
            ConfigurarAudio();
            CrearCamaraSiHaceFalta();
            CrearInterfaz();
            RestaurarProgreso();
        }

        private void CargarRecursosLocales()
        {
            var ilustracion = Resources.Load<Sprite>("BienvenidaNivel1_Pantalla");
            if (ilustracion != null)
                _fondoSelva = ilustracion;

            _tipografiaCompartida = Resources.Load<TMP_FontAsset>("NunitoBoldSDF");
            if (_tipografiaCompartida == null)
                _tipografiaCompartida = TMP_Settings.defaultFontAsset;
        }

        private void ConfigurarAudio()
        {
            _reproductor = gameObject.AddComponent<AudioSource>();
            _reproductor.playOnAwake = false;
            _reproductor.spatialBlend = 0f;
            _reproductor.clip = _audioBienvenida;
        }

        private static void CrearCamaraSiHaceFalta()
        {
            var camara = Camera.main;
            if (camara == null)
            {
                var objetoCamara = new GameObject("Main Camera");
                objetoCamara.tag = "MainCamera";
                objetoCamara.transform.position = new Vector3(0f, 0f, -10f);
                camara = objetoCamara.AddComponent<Camera>();
                camara.orthographic = true;
                camara.clearFlags = CameraClearFlags.SolidColor;
                camara.backgroundColor = new Color(0.08f, 0.29f, 0.19f);
                camara.cullingMask = 0;
            }

            if (camara.GetComponent<AudioListener>() == null)
                camara.gameObject.AddComponent<AudioListener>();
        }

        private void CrearInterfaz()
        {
            var canvasObjeto = new GameObject("InterfazNivel1", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObjeto.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var escalador = canvasObjeto.GetComponent<CanvasScaler>();
            escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escalador.referenceResolution = new Vector2(1536f, 1024f);
            escalador.matchWidthOrHeight = 1f;

            if (FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var fondoCompleto = CrearImagen("Fondo de selva", canvas.transform, Vector2.zero, Vector2.one, new Color(0.3f, 0.48f, 0.31f, 1f));
            fondoCompleto.sprite = _fondoSelva;
            fondoCompleto.preserveAspect = true;
            fondoCompleto.raycastTarget = false;
            var ajusteFondo = fondoCompleto.gameObject.AddComponent<AspectRatioFitter>();
            ajusteFondo.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            ajusteFondo.aspectRatio = 1536f / 1024f;

            var marco = new GameObject("Marco de diseño 3 a 2", typeof(RectTransform));
            marco.transform.SetParent(canvas.transform, false);
            var rectMarco = marco.GetComponent<RectTransform>();
            rectMarco.anchorMin = new Vector2(0.5f, 0.5f);
            rectMarco.anchorMax = new Vector2(0.5f, 0.5f);
            rectMarco.pivot = new Vector2(0.5f, 0.5f);
            rectMarco.sizeDelta = new Vector2(1536f, 1024f);
            rectMarco.anchoredPosition = Vector2.zero;

            var arte = CrearImagen("Arte de bienvenida", marco.transform, Vector2.zero, Vector2.one, Color.white);
            arte.sprite = _fondoSelva;
            arte.preserveAspect = true;
            arte.raycastTarget = false;

            CrearMarcaYProgreso(marco.transform);
            CrearPanelBienvenida(marco.transform);
            CrearNavegacionInferior(marco.transform);
            CrearSelloInferior(marco.transform);
        }

        private void CrearMarcaYProgreso(Transform padre)
        {
            CrearEtiqueta("Marca principal", padre, "GUARDIANES", 32f, new Color(1f, 0.95f, 0.76f), new Vector2(0.025f, 0.916f), new Vector2(0.29f, 0.977f), TextAlignmentOptions.Center, true);
            CrearEtiqueta("Subtítulo de marca", padre, "DE LA AMAZONÍA LECTORA", 24f, new Color(0.72f, 0.97f, 0.37f), new Vector2(0.025f, 0.891f), new Vector2(0.29f, 0.925f), TextAlignmentOptions.Center, true);
            CrearEtiqueta("Nombre del nivel", padre, "NIVEL 1  ·  CONOZCO LA SELVA", 28f, new Color(0.08f, 0.18f, 0.12f), new Vector2(0.37f, 0.912f), new Vector2(0.69f, 0.974f), TextAlignmentOptions.Center, true);

            _progreso = CrearEtiqueta("Progreso del nivel", padre, "0 de 12", 32f, new Color(1f, 0.9f, 0.3f), new Vector2(0.835f, 0.914f), new Vector2(0.987f, 0.98f), TextAlignmentOptions.Center, true);
        }

        private void CrearPanelBienvenida(Transform padre)
        {
            CrearEtiqueta("Título de bienvenida", padre, "Bienvenido a la selva", 40f, Color.white, new Vector2(0.42f, 0.765f), new Vector2(0.9f, 0.845f), TextAlignmentOptions.Center, true);

            _mensaje = CrearEtiqueta("Mensaje del Guardián", padre, TextoBienvenida, 40f, new Color(0.08f, 0.2f, 0.14f), new Vector2(0.405f, 0.43f), new Vector2(0.925f, 0.70f), TextAlignmentOptions.Center, true);
            _mensaje.enableAutoSizing = true;
            _mensaje.fontSizeMin = 32f;
            _mensaje.fontSizeMax = 40f;
            _mensaje.textWrappingMode = TextWrappingModes.Normal;

            var botonAudio = CrearBotonTransparente("Botón AUDIO", padre, new Vector2(0.407f, 0.315f), new Vector2(0.63f, 0.41f), "AUDIO  ·  ESCUCHAR", 32f, ReproducirBienvenida);
            botonAudio.interactable = _audioBienvenida != null;
            _estadoAudio = CrearEtiqueta("Estado del audio", padre, "Escucha la bienvenida del Guardián", 17f, new Color(0.24f, 0.28f, 0.2f), new Vector2(0.405f, 0.275f), new Vector2(0.63f, 0.31f), TextAlignmentOptions.Center, false);

            _botonComenzar = CrearBotonTransparente("Botón COMENZAR", padre, new Vector2(0.655f, 0.315f), new Vector2(0.901f, 0.41f), "COMENZAR", 34f, ComenzarAventura);
            _botonComenzarTexto = _botonComenzar.GetComponentInChildren<TMP_Text>();
        }

        private void CrearNavegacionInferior(Transform padre)
        {
            CrearBotonTransparente("Botón AYUDA", padre, new Vector2(0.647f, 0.008f), new Vector2(0.746f, 0.09f), "Ayuda", 32f, MostrarAyuda);
            CrearBotonTransparente("Botón SEÑAS", padre, new Vector2(0.75f, 0.008f), new Vector2(0.865f, 0.09f), "Señas", 32f, MostrarSenias);
            CrearBotonTransparente("Botón LECTURA", padre, new Vector2(0.87f, 0.008f), new Vector2(0.981f, 0.09f), "Lectura", 32f, MostrarLectura);
        }

        private static void CrearSelloInferior(Transform padre)
        {
            CrearEtiqueta("Lema de aprendizaje", padre, "Aprende a tu ritmo", 24f, new Color(1f, 0.96f, 0.82f), new Vector2(0.025f, 0.026f), new Vector2(0.23f, 0.08f), TextAlignmentOptions.Center, true);
        }

        private void ReproducirBienvenida()
        {
            if (_audioBienvenida == null)
            {
                _estadoAudio.text = "La locución no está disponible.";
                return;
            }

            if (_reproductor.isPlaying)
                _reproductor.Stop();

            _reproductor.Play();
            _estadoAudio.text = "Reproduciendo la bienvenida…";
            CancelInvoke(nameof(IndicarFinAudio));
            Invoke(nameof(IndicarFinAudio), _audioBienvenida.length);
        }

        private void IndicarFinAudio()
        {
            if (_estadoAudio != null)
                _estadoAudio.text = "Puedes escucharla otra vez cuando quieras.";
        }

        private void ComenzarAventura()
        {
            if (_aventuraIniciada)
                return;

            _aventuraIniciada = true;
            PlayerPrefs.SetInt(ClaveInicio, 1);
            PlayerPrefs.SetInt(ClaveProgreso, 1);
            PlayerPrefs.Save();

            _mensaje.text = "¡Muy bien, explorador! Ya diste tu primer paso. El Guardián te acompaña por la selva.";
            _botonComenzar.interactable = false;
            _botonComenzarTexto.text = "¡NIVEL INICIADO!";
            _progreso.text = "1 de 12";
            _estadoAudio.text = "¡Tu aventura ha comenzado!";
        }

        private void RestaurarProgreso()
        {
            var cantidad = Mathf.Clamp(PlayerPrefs.GetInt(ClaveProgreso, 0), 0, TotalRetos);
            _aventuraIniciada = PlayerPrefs.GetInt(ClaveInicio, 0) == 1;
            _progreso.text = $"{cantidad} de {TotalRetos}";

            if (!_aventuraIniciada)
                return;

            _mensaje.text = "¡Hola de nuevo, explorador! Tu aventura por la selva continúa.";
            _botonComenzar.interactable = false;
            _botonComenzarTexto.text = "¡NIVEL INICIADO!";
        }

        private void MostrarAyuda()
        {
            MostrarModal("UNA PISTA", TextoPista, false);
        }

        private void MostrarSenias()
        {
            MostrarSenias(TextoBienvenida, "Bienvenida del Guardián");
        }

        private void MostrarSenias(string texto, string titulo)
        {
            _textoSeniasActual = texto;
            _tituloSeniasActual = titulo;
            CerrarModal();
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
                return;

            _modalActual = new GameObject("Ventana Lengua de señas", typeof(RectTransform));
            _modalActual.transform.SetParent(canvas.transform, false);
            var oscurecedor = _modalActual.AddComponent<Image>();
            oscurecedor.color = new Color(0.02f, 0.11f, 0.1f, 0.78f);
            Estirar(_modalActual.GetComponent<RectTransform>());

            var panel = new GameObject("Tarjeta de lengua de señas", typeof(RectTransform), typeof(Image), typeof(Outline));
            panel.transform.SetParent(_modalActual.transform, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.15f, 0.1f);
            rect.anchorMax = new Vector2(0.85f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(1f, 0.96f, 0.84f);
            panel.GetComponent<Outline>().effectColor = new Color(0.86f, 0.57f, 0.15f);
            panel.GetComponent<Outline>().effectDistance = new Vector2(5f, -5f);

            CrearEtiqueta("Título LSEC", panel.transform, "LENGUA DE SEÑAS ECUATORIANA", 34f, new Color(0.09f, 0.28f, 0.14f), new Vector2(0.06f, 0.88f), new Vector2(0.94f, 0.98f), TextAlignmentOptions.Center, true);
            CrearEtiqueta("Contenido actual en señas", panel.transform, _tituloSeniasActual, 21f, new Color(0.28f, 0.32f, 0.27f), new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.89f), TextAlignmentOptions.Center, false);

            var visor = CrearImagen("Visor LSEC 16 a 9", panel.transform, new Vector2(0.12f, 0.34f), new Vector2(0.88f, 0.81f), new Color(0.82f, 0.75f, 0.98f));
            var relacionVisor = visor.gameObject.AddComponent<AspectRatioFitter>();
            relacionVisor.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            relacionVisor.aspectRatio = 16f / 9f;
            var fondoVisor = visor.gameObject.AddComponent<Outline>();
            fondoVisor.effectColor = new Color(0.55f, 0.39f, 0.83f);
            fondoVisor.effectDistance = new Vector2(2f, -2f);
            CrearEtiqueta("Estado del video LSEC", visor.transform, "Video en preparación", 31f, new Color(0.24f, 0.12f, 0.46f), new Vector2(0.04f, 0.25f), new Vector2(0.96f, 0.75f), TextAlignmentOptions.Center, true);

            CrearEtiqueta("Texto equivalente LSEC", panel.transform, _textoSeniasActual, 24f, new Color(0.08f, 0.17f, 0.13f), new Vector2(0.08f, 0.25f), new Vector2(0.92f, 0.33f), TextAlignmentOptions.Center, true);

            CrearBotonLsec(panel.transform, "Reproducir", new Vector2(0.09f, 0.14f), new Vector2(0.31f, 0.23f), "REPRODUCIR", new Color(0.53f, 0.85f, 1f), false, null);
            CrearBotonLsec(panel.transform, "Repetir", new Vector2(0.34f, 0.14f), new Vector2(0.56f, 0.23f), "REPETIR", new Color(0.53f, 0.85f, 1f), false, null);
            var botonAmpliar = CrearBotonLsec(panel.transform, "Ampliar", new Vector2(0.59f, 0.14f), new Vector2(0.91f, 0.23f), "AMPLIAR VISOR", new Color(0.95f, 0.93f, 0.85f), true, AmpliarVisorLsec);
            _etiquetaAmpliarLsec = botonAmpliar.GetComponentInChildren<TMP_Text>();
            _panelSeniasRect = rect;
            _visorLsecAmpliado = false;
            CrearBotonModal("Cerrar señas", panel.transform, "VOLVER A LA ACTIVIDAD", new Vector2(0.25f, 0.035f), new Vector2(0.75f, 0.125f), CerrarModal);
        }

        private Button CrearBotonLsec(Transform padre, string nombre, Vector2 anclaMin, Vector2 anclaMax, string texto, Color color, bool activo, UnityEngine.Events.UnityAction accion)
        {
            var imagen = CrearImagen(nombre, padre, anclaMin, anclaMax, color);
            var boton = imagen.gameObject.AddComponent<Button>();
            boton.interactable = activo;
            if (accion != null)
                boton.onClick.AddListener(accion);

            var etiqueta = CrearEtiqueta("Texto " + nombre, imagen.transform, texto, 18f, new Color(0.08f, 0.17f, 0.13f), Vector2.zero, Vector2.one, TextAlignmentOptions.Center, true);
            etiqueta.textWrappingMode = TextWrappingModes.NoWrap;
            etiqueta.enableAutoSizing = true;
            etiqueta.fontSizeMin = 14f;
            etiqueta.fontSizeMax = 18f;
            etiqueta.raycastTarget = false;
            return boton;
        }

        private void AmpliarVisorLsec()
        {
            if (_panelSeniasRect == null)
                return;

            _visorLsecAmpliado = !_visorLsecAmpliado;
            _panelSeniasRect.anchorMin = _visorLsecAmpliado ? new Vector2(0.04f, 0.025f) : new Vector2(0.15f, 0.1f);
            _panelSeniasRect.anchorMax = _visorLsecAmpliado ? new Vector2(0.96f, 0.975f) : new Vector2(0.85f, 0.9f);
            _etiquetaAmpliarLsec.text = _visorLsecAmpliado ? "VISTA NORMAL" : "AMPLIAR VISOR";
        }
        private void MostrarLectura()
        {
            MostrarModal("LECTURA", TextoBienvenida, true);
        }

        private void MostrarModal(string titulo, string texto, bool incluirAudio)
        {
            CerrarModal();
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
                return;

            _modalActual = new GameObject("Ventana " + titulo, typeof(RectTransform));
            _modalActual.transform.SetParent(canvas.transform, false);
            var fondoOscuro = _modalActual.AddComponent<Image>();
            fondoOscuro.color = new Color(0.02f, 0.11f, 0.1f, 0.78f);
            Estirar(_modalActual.GetComponent<RectTransform>());

            var panel = new GameObject("Tarjeta del mensaje", typeof(RectTransform), typeof(Image), typeof(Outline));
            panel.transform.SetParent(_modalActual.transform, false);
            var rectPanel = panel.GetComponent<RectTransform>();
            rectPanel.anchorMin = new Vector2(0.2f, 0.19f);
            rectPanel.anchorMax = new Vector2(0.8f, 0.81f);
            rectPanel.offsetMin = Vector2.zero;
            rectPanel.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(1f, 0.96f, 0.84f);
            panel.GetComponent<Outline>().effectColor = new Color(0.86f, 0.57f, 0.15f);
            panel.GetComponent<Outline>().effectDistance = new Vector2(5f, -5f);

            CrearEtiqueta("Título de ayuda", panel.transform, titulo, 40f, new Color(0.09f, 0.28f, 0.14f), new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.93f), TextAlignmentOptions.Center, true);
            var cuerpo = CrearEtiqueta("Texto de ayuda", panel.transform, texto, 32f, new Color(0.08f, 0.17f, 0.13f), new Vector2(0.09f, 0.35f), new Vector2(0.91f, 0.72f), TextAlignmentOptions.Center, true);
            cuerpo.enableAutoSizing = true;
            cuerpo.fontSizeMin = 24f;
            cuerpo.fontSizeMax = 34f;
            cuerpo.textWrappingMode = TextWrappingModes.Normal;

            CrearBotonModal("Cerrar ayuda", panel.transform, "VOLVER A LA SELVA", new Vector2(0.22f, 0.08f), new Vector2(0.78f, 0.23f), CerrarModal);
            if (incluirAudio)
                CrearBotonModal("Escuchar lectura", panel.transform, "AUDIO  ·  ESCUCHAR", new Vector2(0.28f, 0.25f), new Vector2(0.72f, 0.36f), ReproducirBienvenida);
            else if (titulo == "UNA PISTA")
                CrearBotonModal("Señas de la pista", panel.transform, "VER PISTA EN SEÑAS", new Vector2(0.28f, 0.25f), new Vector2(0.72f, 0.36f), () => MostrarSenias(TextoPista, "Pista de la actividad"));
        }

        private void CerrarModal()
        {
            if (_modalActual != null)
                Destroy(_modalActual);
            _modalActual = null;
        }

        private Button CrearBotonModal(string nombre, Transform padre, string texto, Vector2 anclaMin, Vector2 anclaMax, UnityEngine.Events.UnityAction accion)
        {
            var fondo = CrearImagen(nombre, padre, anclaMin, anclaMax, new Color(0.98f, 0.68f, 0.08f));
            var boton = fondo.gameObject.AddComponent<Button>();
            boton.onClick.AddListener(accion);
            var etiqueta = CrearEtiqueta("Texto " + nombre, fondo.transform, texto, 25f, new Color(0.1f, 0.17f, 0.1f), Vector2.zero, Vector2.one, TextAlignmentOptions.Center, true);
            etiqueta.raycastTarget = false;
            return boton;
        }

        private Button CrearBotonTransparente(string nombre, Transform padre, Vector2 anclaMin, Vector2 anclaMax, string texto, float tamano, UnityEngine.Events.UnityAction accion)
        {
            var imagen = CrearImagen(nombre, padre, anclaMin, anclaMax, new Color(1f, 1f, 1f, 0f));
            var boton = imagen.gameObject.AddComponent<Button>();
            boton.transition = Selectable.Transition.ColorTint;
            ColorBlock colores = boton.colors;
            colores.normalColor = new Color(1f, 1f, 1f, 0f);
            colores.highlightedColor = new Color(1f, 1f, 1f, 0.14f);
            colores.pressedColor = new Color(1f, 1f, 1f, 0.24f);
            colores.selectedColor = new Color(1f, 1f, 1f, 0.08f);
            colores.fadeDuration = 0.12f;
            boton.colors = colores;
            boton.onClick.AddListener(accion);
            var etiqueta = CrearEtiqueta("Texto " + nombre, imagen.transform, texto, tamano, new Color(0.07f, 0.15f, 0.1f), Vector2.zero, Vector2.one, TextAlignmentOptions.Center, true);
            etiqueta.enableAutoSizing = true;
            etiqueta.fontSizeMin = Mathf.Max(16f, tamano * 0.72f);
            etiqueta.fontSizeMax = tamano;
            etiqueta.textWrappingMode = TextWrappingModes.NoWrap;
            etiqueta.overflowMode = TextOverflowModes.Ellipsis;
            etiqueta.raycastTarget = false;
            return boton;
        }

        private static Image CrearImagen(string nombre, Transform padre, Vector2 anclaMin, Vector2 anclaMax, Color color)
        {
            var objeto = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            objeto.transform.SetParent(padre, false);
            var rect = objeto.GetComponent<RectTransform>();
            rect.anchorMin = anclaMin;
            rect.anchorMax = anclaMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var imagen = objeto.GetComponent<Image>();
            imagen.color = color;
            return imagen;
        }

        private static TMP_Text CrearEtiqueta(string nombre, Transform padre, string texto, float tamano, Color color, Vector2 anclaMin, Vector2 anclaMax, TextAlignmentOptions alineacion, bool negrita)
        {
            var objeto = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            objeto.transform.SetParent(padre, false);
            var rect = objeto.GetComponent<RectTransform>();
            rect.anchorMin = anclaMin;
            rect.anchorMax = anclaMax;
            rect.offsetMin = new Vector2(7f, 3f);
            rect.offsetMax = new Vector2(-7f, -3f);
            var etiqueta = objeto.GetComponent<TextMeshProUGUI>();
            etiqueta.text = texto;
            etiqueta.font = _tipografiaCompartida != null ? _tipografiaCompartida : TMP_Settings.defaultFontAsset;
            etiqueta.fontSize = tamano;
            etiqueta.color = color;
            etiqueta.alignment = alineacion;
            etiqueta.characterSpacing = 0f;
            etiqueta.wordSpacing = 0f;
            etiqueta.textWrappingMode = TextWrappingModes.Normal;
            if (negrita)
                etiqueta.fontStyle = FontStyles.Bold;
            return etiqueta;
        }

        private static void Estirar(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}



