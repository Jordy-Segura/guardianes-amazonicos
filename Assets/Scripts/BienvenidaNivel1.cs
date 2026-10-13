using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace GuardianesAmazonicos.Nivel1
{
    public sealed class BienvenidaNivel1 : MonoBehaviour
    {
        private const string TextoBienvenida = "¡Hola, explorador! Bienvenido a la selva. Hoy vamos a conocer algunos de sus tesoros. ¿Quieres comenzar?";
        private const string ClaveInicio = "guardianes.nivel1.iniciado";
        private const string ClaveProgreso = "guardianes.nivel1.progreso";
        private const int TotalRetos = 12;

        [SerializeField] private Sprite fondoSelva;
        [SerializeField] private AudioClip audioBienvenida;
        private AudioSource reproductor;
        private TMP_Text progreso;
        private TMP_Text mensaje;
        private TMP_Text estadoAudio;
        private TMP_Text botonAudioTexto;
        private Button botonComenzar;
        private bool aventuraIniciada;

        private void Start()
        {
            ConfigurarAccesibilidad();
            CrearInterfaz();
            ActualizarProgreso();
        }

        private void ConfigurarAccesibilidad()
        {
            reproductor = gameObject.AddComponent<AudioSource>();
            reproductor.playOnAwake = false;
            reproductor.clip = audioBienvenida;
            aventuraIniciada = PlayerPrefs.GetInt(ClaveInicio, 0) == 1;
        }

        private void CrearInterfaz()
        {
            var canvasObjeto = new GameObject("InterfazNivel1", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObjeto.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObjeto.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1536, 1024);
            scaler.matchWidthOrHeight = 0.5f;

            if (FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var fondo = CrearImagen("Fondo amazónico", canvas.transform, new Color(0.08f, 0.29f, 0.19f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            fondo.sprite = fondoSelva;
            fondo.preserveAspect = false;

            CrearPanel(canvas.transform);
        }

        private void CrearPanel(Transform padre)
        {
            var panel = CrearImagen("Panel de bienvenida", padre, new Color(1f, 0.96f, 0.82f), new Vector2(0.48f, 0.12f), new Vector2(0.96f, 0.91f), Vector2.zero, Vector2.zero);
            var borde = panel.gameObject.AddComponent<Outline>();
            borde.effectColor = new Color(0.85f, 0.57f, 0.16f);
            borde.effectDistance = new Vector2(5, -5);
            var p = panel.transform;

            CrearEtiqueta("Marca", p, "GUARDIANES DE LA AMAZONÍA LECTORA", 22, new Color(0.16f, 0.34f, 0.13f), new Vector2(0.05f, 0.89f), new Vector2(0.95f, 0.98f), TextAlignmentOptions.Center);
            CrearEtiqueta("Nivel", p, "NIVEL 1  ·  CONOZCO LA SELVA", 35, new Color(0.11f, 0.28f, 0.17f), new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.88f), TextAlignmentOptions.Center);
            CrearEtiqueta("Guardián", p, "El Guardián de la Selva", 29, new Color(0.38f, 0.22f, 0.08f), new Vector2(0.06f, 0.67f), new Vector2(0.94f, 0.77f), TextAlignmentOptions.Center);
            mensaje = CrearEtiqueta("Mensaje de bienvenida", p, TextoBienvenida, 32, new Color(0.09f, 0.18f, 0.13f), new Vector2(0.08f, 0.39f), new Vector2(0.92f, 0.65f), TextAlignmentOptions.Center);
            mensaje.enableAutoSizing = true;
            mensaje.fontSizeMin = 24;
            mensaje.fontSizeMax = 34;

            var audioBoton = CrearBoton("Botón AUDIO", p, "AUDIO  ·  ESCUCHAR", new Color(0.24f, 0.72f, 0.91f), new Vector2(0.24f, 0.27f), new Vector2(0.76f, 0.37f), 29, ReproducirBienvenida);
            botonAudioTexto = audioBoton.GetComponentInChildren<TMP_Text>();
            estadoAudio = CrearEtiqueta("Estado del audio", p, audioBienvenida == null ? "Audio de bienvenida no disponible" : "Escucha la bienvenida del Guardián", 16, new Color(0.27f, 0.33f, 0.29f), new Vector2(0.1f, 0.21f), new Vector2(0.9f, 0.26f), TextAlignmentOptions.Center);

            botonComenzar = CrearBoton("Botón COMENZAR", p, "COMENZAR  ▶", new Color(0.97f, 0.68f, 0.08f), new Vector2(0.18f, 0.07f), new Vector2(0.82f, 0.19f), 36, ComenzarAventura);
            progreso = CrearEtiqueta("Progreso del nivel", p, "PROGRESO   0 DE 12", 24, new Color(0.1f, 0.32f, 0.2f), new Vector2(0.05f, 0.005f), new Vector2(0.95f, 0.06f), TextAlignmentOptions.Center);
            progreso.gameObject.SetActive(false);
        }

        private void ReproducirBienvenida()
        {
            if (audioBienvenida == null)
            {
                estadoAudio.text = "La locución aún no está configurada.";
                return;
            }
            if (reproductor.isPlaying) reproductor.Stop();
            reproductor.Play();
            estadoAudio.text = "Reproduciendo la bienvenida…";
            CancelInvoke(nameof(IndicarFinAudio));
            Invoke(nameof(IndicarFinAudio), audioBienvenida.length);
        }

        private void IndicarFinAudio()
        {
            if (estadoAudio != null) estadoAudio.text = "Puedes volver a escuchar la bienvenida.";
        }

        private void ComenzarAventura()
        {
            if (aventuraIniciada) return;
            aventuraIniciada = true;
            PlayerPrefs.SetInt(ClaveInicio, 1);
            PlayerPrefs.SetInt(ClaveProgreso, 1);
            PlayerPrefs.Save();
            mensaje.text = "¡Qué alegría, explorador! La aventura comienza. Tu primer paso por la selva ya está registrado.";
            botonComenzar.interactable = false;
            botonComenzar.GetComponentInChildren<TMP_Text>().text = "¡AVENTURA INICIADA!";
            progreso.gameObject.SetActive(true);
            estadoAudio.text = "¡Muy bien! El Guardián te acompaña en esta aventura.";
            ActualizarProgreso();
        }

        private void ActualizarProgreso()
        {
            if (progreso == null) return;
            var puntos = PlayerPrefs.GetInt(ClaveProgreso, 0);
            progreso.text = $"PROGRESO   {puntos} DE {TotalRetos}";
            if (aventuraIniciada)
            {
                progreso.gameObject.SetActive(true);
                mensaje.text = "¡Hola de nuevo, explorador! Tu aventura por la selva continúa.";
                botonComenzar.interactable = false;
                botonComenzar.GetComponentInChildren<TMP_Text>().text = "¡AVENTURA INICIADA!";
            }
        }

        private static Image CrearImagen(string nombre, Transform padre, Color color, Vector2 anclaMin, Vector2 anclaMax, Vector2 desplazamientoMin, Vector2 desplazamientoMax)
        {
            var objeto = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            objeto.transform.SetParent(padre, false);
            var rect = objeto.GetComponent<RectTransform>();
            rect.anchorMin = anclaMin;
            rect.anchorMax = anclaMax;
            rect.offsetMin = desplazamientoMin;
            rect.offsetMax = desplazamientoMax;
            var imagen = objeto.GetComponent<Image>();
            imagen.color = color;
            return imagen;
        }

        private static TMP_Text CrearEtiqueta(string nombre, Transform padre, string texto, float tamano, Color color, Vector2 anclaMin, Vector2 anclaMax, TextAlignmentOptions alineacion)
        {
            var objeto = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            objeto.transform.SetParent(padre, false);
            var rect = objeto.GetComponent<RectTransform>();
            rect.anchorMin = anclaMin;
            rect.anchorMax = anclaMax;
            rect.offsetMin = new Vector2(8, 4);
            rect.offsetMax = new Vector2(-8, -4);
            var etiqueta = objeto.GetComponent<TextMeshProUGUI>();
            etiqueta.text = texto;
            etiqueta.fontSize = tamano;
            etiqueta.color = color;
            etiqueta.alignment = alineacion;
            etiqueta.textWrappingMode = TextWrappingModes.Normal;
            etiqueta.font = TMP_Settings.defaultFontAsset;
            return etiqueta;
        }

        private static Button CrearBoton(string nombre, Transform padre, string texto, Color color, Vector2 anclaMin, Vector2 anclaMax, float tamano, UnityEngine.Events.UnityAction accion)
        {
            var imagen = CrearImagen(nombre, padre, color, anclaMin, anclaMax, Vector2.zero, Vector2.zero);
            var boton = imagen.gameObject.AddComponent<Button>();
            var colores = boton.colors;
            colores.highlightedColor = Color.Lerp(color, Color.white, 0.22f);
            colores.pressedColor = Color.Lerp(color, Color.black, 0.15f);
            colores.disabledColor = new Color(0.58f, 0.63f, 0.53f);
            boton.colors = colores;
            boton.onClick.AddListener(accion);
            var borde = imagen.gameObject.AddComponent<Outline>();
            borde.effectColor = new Color(0.35f, 0.26f, 0.08f, 0.5f);
            borde.effectDistance = new Vector2(2, -2);
            var etiqueta = CrearEtiqueta("Texto " + nombre, imagen.transform, texto, tamano, new Color(0.07f, 0.15f, 0.1f), Vector2.zero, Vector2.one, TextAlignmentOptions.Center);
            etiqueta.fontStyle = FontStyles.Bold;
            return boton;
        }
    }
}


