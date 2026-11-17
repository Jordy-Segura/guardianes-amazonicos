using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GuardianesAmazonicos.Views
{
    /// <summary>
    /// Panel de configuración de accesibilidad compartido por las vistas de la
    /// plantilla (actividad y bienvenida). Guarda las preferencias en PlayerPrefs
    /// y las aplica de inmediato a la escena: movimiento reducido para las
    /// animaciones por fotogramas y volumen para la locución de lectura.
    /// Se construye en tiempo de ejecución para no duplicar jerarquías en cada
    /// escena y se reutiliza la misma instancia mientras la vista permanezca viva.
    /// </summary>
    public static class PanelConfiguracionAccesibilidad
    {
        public const string ClaveMovimientoReducido = "HU03_MovimientoReducido";
        public const string ClaveVolumenLectura = "HU03_AudioVolumen";

        private static readonly Color FondoModal = new Color(0.03f, 0.10f, 0.08f, 0.72f);
        private static readonly Color FondoTarjeta = new Color(1f, 0.972f, 0.91f, 1f);
        private static readonly Color VerdeBoton = new Color(0.16f, 0.42f, 0.24f, 1f);
        private static readonly Color Tinta = new Color(0.12f, 0.18f, 0.14f, 1f);
        private static readonly Color TextoBoton = Color.white;

        public static bool MovimientoReducidoActivo => PlayerPrefs.GetInt(ClaveMovimientoReducido, 0) == 1;

        public static float VolumenLectura => Mathf.Clamp01(PlayerPrefs.GetFloat(ClaveVolumenLectura, 1f));

        /// <summary>
        /// Construye el modal dentro de la raíz indicada. Devuelve el GameObject del
        /// panel para que la vista pueda mostrarlo u ocultarlo.
        /// </summary>
        public static GameObject Construir(RectTransform raiz, AudioSource fuenteAudio, Action alCerrar,
            TMP_FontAsset fuente = null)
        {
            if (raiz == null) return null;
            if (fuente == null) fuente = TMP_Settings.defaultFontAsset;

            GameObject panelGo = new GameObject("PanelConfiguracion", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(raiz, false);
            RectTransform panel = panelGo.transform as RectTransform;
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            Image fondo = panelGo.GetComponent<Image>();
            fondo.color = FondoModal;
            fondo.raycastTarget = true;

            GameObject tarjetaGo = new GameObject("TarjetaConfiguracion", typeof(RectTransform), typeof(Image));
            tarjetaGo.transform.SetParent(panel, false);
            RectTransform tarjeta = tarjetaGo.transform as RectTransform;
            tarjeta.anchorMin = new Vector2(0.5f, 0.5f);
            tarjeta.anchorMax = new Vector2(0.5f, 0.5f);
            tarjeta.pivot = new Vector2(0.5f, 0.5f);
            tarjeta.anchoredPosition = Vector2.zero;
            tarjeta.sizeDelta = new Vector2(900f, 620f);
            tarjetaGo.GetComponent<Image>().color = FondoTarjeta;

            CrearTexto(tarjeta, "Titulo", "CONFIGURACIÓN", new Vector2(0f, -28f), new Vector2(900f, 70f),
                38f, Tinta, TextAlignmentOptions.Center, fuente);
            CrearTexto(tarjeta, "Subtitulo", "Ajusta la experiencia a tus necesidades",
                new Vector2(0f, -98f), new Vector2(900f, 50f), 22f,
                new Color(0.28f, 0.34f, 0.30f, 1f), TextAlignmentOptions.Center, fuente);

            // Movimiento reducido
            CrearTexto(tarjeta, "TextoMovimiento", "Movimiento reducido", new Vector2(70f, -170f),
                new Vector2(370f, 70f), 28f, Tinta, TextAlignmentOptions.Left, fuente);

            Button botonMovimiento = CrearBoton(tarjeta, "ReducirMovimiento", TextoMovimiento(),
                new Vector2(470f, -165f), new Vector2(360f, 80f), fuente);
            TMP_Text etiquetaMovimiento = botonMovimiento.GetComponentInChildren<TMP_Text>();
            botonMovimiento.onClick.AddListener(() =>
            {
                bool reducir = !MovimientoReducidoActivo;
                PlayerPrefs.SetInt(ClaveMovimientoReducido, reducir ? 1 : 0);
                PlayerPrefs.Save();
                AplicarMovimientoReducido(reducir);
                if (etiquetaMovimiento != null) etiquetaMovimiento.text = TextoMovimiento();
            });

            // Volumen de lectura
            TMP_Text etiquetaVolumen = CrearTexto(tarjeta, "TextoAudio", TextoVolumen(),
                new Vector2(70f, -290f), new Vector2(370f, 70f), 26f, Tinta,
                TextAlignmentOptions.Left, fuente);

            CrearBoton(tarjeta, "BajarVolumen", "−", new Vector2(470f, -285f),
                new Vector2(100f, 80f), fuente).onClick.AddListener(() =>
                {
                    AjustarVolumen(-0.1f, fuenteAudio, etiquetaVolumen);
                });
            CrearBoton(tarjeta, "SubirVolumen", "+", new Vector2(590f, -285f),
                new Vector2(100f, 80f), fuente).onClick.AddListener(() =>
                {
                    AjustarVolumen(0.1f, fuenteAudio, etiquetaVolumen);
                });

            Button cerrar = CrearBoton(tarjeta, "CerrarConfiguracion", "GUARDAR Y CERRAR",
                new Vector2(250f, -490f), new Vector2(400f, 84f), fuente);
            cerrar.onClick.AddListener(() =>
            {
                PlayerPrefs.Save();
                if (panelGo != null) panelGo.SetActive(false);
                alCerrar?.Invoke();
            });

            panelGo.SetActive(false);
            return panelGo;
        }

        /// <summary>Aplica las preferencias guardadas al audio y a las animaciones actuales.</summary>
        public static void AplicarPreferencias(AudioSource fuenteAudio)
        {
            if (fuenteAudio != null) fuenteAudio.volume = VolumenLectura;
            AplicarMovimientoReducido(MovimientoReducidoActivo);
        }

        public static void AplicarMovimientoReducido(bool reducir)
        {
            AnimacionPersonajePNG[] animaciones = UnityEngine.Object.FindObjectsByType<AnimacionPersonajePNG>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (AnimacionPersonajePNG animacion in animaciones)
            {
                if (animacion != null) animacion.EstablecerReducirMovimiento(reducir);
            }
        }

        private static void AjustarVolumen(float delta, AudioSource fuenteAudio, TMP_Text etiqueta)
        {
            float volumen = Mathf.Clamp01(VolumenLectura + delta);
            PlayerPrefs.SetFloat(ClaveVolumenLectura, volumen);
            PlayerPrefs.Save();
            if (fuenteAudio != null) fuenteAudio.volume = volumen;
            if (etiqueta != null) etiqueta.text = TextoVolumen();
        }

        private static string TextoMovimiento()
        {
            return MovimientoReducidoActivo ? "SÍ" : "NO";
        }

        private static string TextoVolumen()
        {
            return "Volumen de lectura: " + Mathf.RoundToInt(VolumenLectura * 100f) + "%";
        }

        private static Button CrearBoton(RectTransform padre, string nombre, string texto, Vector2 pos,
            Vector2 size, TMP_FontAsset fuente)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(padre, false);
            RectTransform rect = go.transform as RectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            Image imagen = go.GetComponent<Image>();
            imagen.color = VerdeBoton;
            imagen.raycastTarget = true;

            Button boton = go.GetComponent<Button>();
            boton.targetGraphic = imagen;

            CrearTexto(rect, "Texto", texto, Vector2.zero, size, 26f, TextoBoton,
                TextAlignmentOptions.Center, fuente, true);
            return boton;
        }

        private static TMP_Text CrearTexto(RectTransform padre, string nombre, string texto, Vector2 pos,
            Vector2 size, float tamano, Color color, TextAlignmentOptions alineacion, TMP_FontAsset fuente,
            bool relativo = false)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(padre, false);
            RectTransform rect = go.transform as RectTransform;
            if (relativo)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            else
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = pos;
                rect.sizeDelta = size;
            }

            TMP_Text tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = texto;
            if (fuente != null) tmp.font = fuente;
            tmp.fontSize = tamano;
            tmp.fontSizeMin = 12f;
            tmp.fontSizeMax = tamano;
            tmp.enableAutoSizing = true;
            tmp.color = color;
            tmp.alignment = alineacion;
            tmp.fontStyle = FontStyles.Bold;
            tmp.raycastTarget = false;
            return tmp;
        }
    }
}
