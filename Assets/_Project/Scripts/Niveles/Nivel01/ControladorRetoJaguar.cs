using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GuardianesAmazonicos.Audio;
using GuardianesAmazonicos.Data;
using GuardianesAmazonicos.UI;

namespace GuardianesAmazonicos.Niveles.Nivel01
{
    public sealed class ControladorRetoJaguar : MonoBehaviour
    {
        private const string TextoPregunta = "¿Cuál de estos animales es el jaguar?";
        private const string TextoPista = "Busca el felino de pelaje amarillo con manchas negras en forma de roseta.";
        private const int CantidadOpciones = 4;
        private const int TotalRetosNivel = 12;
        private const int ProgresoAlCompletar = 2;
        private static readonly Color _colorTexto = new Color32(16, 58, 36, 255);
        private static readonly Color _colorCrema = new Color32(255, 248, 232, 255);
        private static TMP_FontAsset _fuenteCompartida;
        private static Sprite _spriteTarjeta;

        [SerializeField] private Sprite _fondoSelva;
        [SerializeField] private TMP_FontAsset _tipografia;
        [SerializeField] private Sprite[] _imagenesAnimales;
        [SerializeField] private AudioClip _audioConsigna;

        private GestorAudio _gestorAudio;
        private TMP_Text _textoEstado;
        private TMP_Text _textoProgreso;
        private Button _botonSiguiente;
        private Image _tarjetaJaguar;
        private GameObject _modalApoyo;
        private bool _retoCompletado;

        private void Start()
        {
            if (!ValidarRecursos())
                return;

            _fuenteCompartida = _tipografia != null ? _tipografia : TMP_Settings.defaultFontAsset;
            _fuenteCompartida?.TryAddCharacters("…");
            AgregarRespaldoTipografico(_fuenteCompartida);
            CrearCamaraSiHaceFalta();
            CrearInterfaz();
            RestaurarProgreso();
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
            }

            if (camara.GetComponent<AudioListener>() == null)
                camara.gameObject.AddComponent<AudioListener>();
        }

        private static void AgregarRespaldoTipografico(TMP_FontAsset fuente)
        {
            var respaldo = TMP_Settings.defaultFontAsset;
            if (fuente == null || respaldo == null || fuente == respaldo)
                return;

            fuente.fallbackFontAssetTable ??= new List<TMP_FontAsset>();
            if (!fuente.fallbackFontAssetTable.Contains(respaldo))
                fuente.fallbackFontAssetTable.Add(respaldo);
        }

        private bool ValidarRecursos()
        {
            if (_fondoSelva == null)
            {
                Debug.LogError("Asigna la ilustración de fondo al reto del jaguar.", this);
                return false;
            }

            if (_imagenesAnimales == null || _imagenesAnimales.Length != CantidadOpciones)
            {
                Debug.LogError("El reto necesita cuatro ilustraciones de animales.", this);
                return false;
            }

            for (var indice = 0; indice < _imagenesAnimales.Length; indice++)
            {
                if (_imagenesAnimales[indice] == null)
                {
                    Debug.LogError($"Asigna la ilustración de la opción {indice + 1}.", this);
                    return false;
                }
            }

            return true;
        }

        private void CrearInterfaz()
        {
            var canvasObjeto = new GameObject("InterfazRetoJaguar", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObjeto.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var escalador = canvasObjeto.GetComponent<CanvasScaler>();
            escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escalador.referenceResolution = new Vector2(1366f, 768f);
            escalador.matchWidthOrHeight = 0.5f;

            if (FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var fondo = CrearImagen("FondoSelva", canvas.transform, Vector2.zero, Vector2.one, Color.white);
            fondo.sprite = _fondoSelva;
            fondo.preserveAspect = false;
            fondo.raycastTarget = false;
            CrearCabecera(canvas.transform);
            CrearPanelPregunta(canvas.transform);
            CrearTarjetas(canvas.transform);
            CrearBarraInferior(canvas.transform);
        }

        private void CrearCabecera(Transform padre)
        {
            CrearPanel("PlacaLogo", padre, new Vector2(0.02f, 0.9f), new Vector2(0.30f, 0.99f), new Color32(92, 57, 25, 245));
            CrearPanel("PlacaNivel", padre, new Vector2(0.34f, 0.9f), new Vector2(0.69f, 0.99f), _colorCrema);
            CrearPanel("PlacaProgreso", padre, new Vector2(0.83f, 0.9f), new Vector2(0.98f, 0.99f), new Color32(46, 125, 50, 255));
            CrearTexto("LogoJuego", padre, "GUARDIANES DE LA AMAZONÍA LECTORA", 21f, _colorCrema,
                new Vector2(0.02f, 0.9f), new Vector2(0.30f, 0.99f), TextAlignmentOptions.Center, true);
            CrearTexto("TituloNivel", padre, "Nivel 1 · Conozco la selva", 26f, _colorTexto,
                new Vector2(0.34f, 0.9f), new Vector2(0.69f, 0.99f), TextAlignmentOptions.Center, true);
            _textoProgreso = CrearTexto("Progreso", padre, "1 de 12", 22f, _colorCrema,
                new Vector2(0.83f, 0.9f), new Vector2(0.98f, 0.99f), TextAlignmentOptions.Center, true);
        }

        private void CrearPanelPregunta(Transform padre)
        {
            CrearPanel("PanelConsigna", padre, new Vector2(0.035f, 0.29f), new Vector2(0.345f, 0.87f), _colorCrema);
            CrearTexto("TituloMision", padre, "Misión de la selva", 26f, new Color32(46, 125, 50, 255),
                new Vector2(0.055f, 0.78f), new Vector2(0.325f, 0.85f), TextAlignmentOptions.Center, true);
            var pregunta = CrearTexto("ConsignaJaguar", padre, TextoPregunta, 33f, _colorTexto,
                new Vector2(0.06f, 0.59f), new Vector2(0.32f, 0.77f), TextAlignmentOptions.Center, true);
            pregunta.fontSizeMin = 24f;
            _textoEstado = CrearTexto("Retroalimentacion", padre, "Toca una imagen para elegir.", 20f, _colorTexto,
                new Vector2(0.06f, 0.42f), new Vector2(0.32f, 0.57f), TextAlignmentOptions.Center, false);
            _textoEstado.fontSizeMin = 17f;
            CrearBoton("BotonAudio", padre, "ESCUCHAR", new Vector2(0.06f, 0.32f), new Vector2(0.20f, 0.40f),
                new Color32(167, 221, 247, 255), ReproducirConsigna);
            CrearBoton("BotonAyuda", padre, "VER PISTA", new Vector2(0.21f, 0.32f), new Vector2(0.32f, 0.40f),
                new Color32(246, 196, 69, 255), MostrarPista);
        }

        private void CrearTarjetas(Transform padre)
        {
            var etiquetas = new[] { "Jaguar", "Guacamayo", "Pecarí", "Perezoso" };
            var identificadores = new[] { "jaguar", "guacamayo", "pecari", "perezoso" };
            var anclasX = new[] { 0.39f, 0.665f };
            var anclasY = new[] { 0.53f, 0.15f };

            for (var indice = 0; indice < CantidadOpciones; indice++)
            {
                var fila = indice / 2;
                var columna = indice % 2;
                var anclaMin = new Vector2(anclasX[columna], anclasY[fila]);
                var anclaMax = new Vector2(anclasX[columna] + 0.255f, anclasY[fila] + 0.33f);
                var tarjeta = CrearPanel("Tarjeta" + etiquetas[indice], padre, anclaMin, anclaMax, _colorCrema);
                var boton = tarjeta.gameObject.AddComponent<Button>();
                boton.targetGraphic = tarjeta;
                var colores = boton.colors;
                colores.normalColor = Color.white;
                colores.highlightedColor = new Color32(224, 246, 216, 255);
                colores.pressedColor = new Color32(191, 228, 182, 255);
                colores.selectedColor = colores.highlightedColor;
                boton.colors = colores;
                var identificador = identificadores[indice];
                boton.onClick.AddListener(() => SeleccionarAnimal(identificador));

                var ilustracion = CrearImagen("Ilustracion" + etiquetas[indice], tarjeta.transform,
                    new Vector2(0.06f, 0.22f), new Vector2(0.94f, 0.94f), Color.white);
                ilustracion.sprite = _imagenesAnimales[indice];
                ilustracion.preserveAspect = true;
                ilustracion.raycastTarget = false;
                CrearTexto("Nombre" + etiquetas[indice], tarjeta.transform, etiquetas[indice], 23f, _colorTexto,
                    new Vector2(0.04f, 0.03f), new Vector2(0.96f, 0.24f), TextAlignmentOptions.Center, true).raycastTarget = false;

                if (indice == 0)
                    _tarjetaJaguar = tarjeta;
            }
        }

        private void CrearBarraInferior(Transform padre)
        {
            CrearTexto("AyudaAccesibilidad", padre, "Aprende a tu ritmo · Toca una imagen para seleccionar", 16f, _colorCrema,
                new Vector2(0.025f, 0.025f), new Vector2(0.55f, 0.09f), TextAlignmentOptions.Left, true);
            _botonSiguiente = CrearBoton("BotonSiguienteReto", padre, "SIGUIENTE RETO  ›",
                new Vector2(0.39f, 0.035f), new Vector2(0.62f, 0.115f), new Color32(246, 196, 69, 255), ContinuarReto);
            _botonSiguiente.interactable = false;
            CrearBoton("BotonSenias", padre, "SEÑAS", new Vector2(0.76f, 0.025f), new Vector2(0.86f, 0.115f),
                new Color32(179, 136, 255, 255), MostrarSenias);
            CrearBoton("BotonLectura", padre, "LECTURA", new Vector2(0.87f, 0.025f), new Vector2(0.98f, 0.115f),
                new Color32(167, 221, 247, 255), MostrarLectura);
        }

        private void SeleccionarAnimal(string identificadorAnimal)
        {
            if (_retoCompletado)
                return;

            if (!ValidadorRespuestaAnimal.EsJaguar(identificadorAnimal))
            {
                _textoEstado.text = "Inténtalo otra vez. " + TextoPista;
                _textoEstado.color = new Color32(151, 72, 10, 255);
                _tarjetaJaguar.color = new Color32(226, 245, 225, 255);
                return;
            }

            _retoCompletado = true;
            _textoEstado.text = "¡Muy bien! Encontraste al jaguar. El siguiente reto está disponible.";
            _textoEstado.color = new Color32(35, 115, 48, 255);
            _tarjetaJaguar.color = new Color32(211, 244, 201, 255);
            _botonSiguiente.interactable = true;
            GuardadoProgreso.RegistrarRetoCompleto(ProgresoAlCompletar);
            _textoProgreso.text = "2 de 12";
        }

        private void RestaurarProgreso()
        {
            var progreso = GuardadoProgreso.Cargar(TotalRetosNivel);
            _textoProgreso.text = $"{Mathf.Max(1, progreso.RetosCompletados)} de {TotalRetosNivel}";
            if (progreso.RetosCompletados < ProgresoAlCompletar)
                return;

            _retoCompletado = true;
            _textoEstado.text = "¡Muy bien! Ya encontraste al jaguar. El siguiente reto está disponible.";
            _textoEstado.color = new Color32(35, 115, 48, 255);
            _tarjetaJaguar.color = new Color32(211, 244, 201, 255);
            _botonSiguiente.interactable = true;
        }

        private void MostrarPista()
        {
            if (!_retoCompletado)
                _textoEstado.text = TextoPista;
        }

        private void ReproducirConsigna()
        {
            if (_audioConsigna == null)
            {
                _textoEstado.text = "La locución de esta consigna todavía no está disponible.";
                return;
            }

            if (_gestorAudio == null)
            {
                _gestorAudio = GetComponent<GestorAudio>();
                if (_gestorAudio == null)
                    _gestorAudio = gameObject.AddComponent<GestorAudio>();
                _gestorAudio.Configurar();
            }

            if (_gestorAudio.Reproducir(_audioConsigna))
                _textoEstado.text = "Reproduciendo la consigna. Puedes escucharla otra vez cuando quieras.";
        }

        private void ContinuarReto()
        {
            if (_retoCompletado)
                _textoEstado.text = "¡Reto completado! El siguiente desafío se ha desbloqueado.";
        }

        private Button CrearBoton(string nombre, Transform padre, string texto, Vector2 anclaMin, Vector2 anclaMax,
            Color color, UnityEngine.Events.UnityAction accion)
        {
            var fondo = CrearPanel(nombre, padre, anclaMin, anclaMax, color);
            var boton = fondo.gameObject.AddComponent<Button>();
            boton.targetGraphic = fondo;
            ControladorBotonAccesible.Configurar(boton, nombre, accion);
            var etiqueta = CrearTexto("Texto" + nombre, fondo.transform, texto, 17f, _colorTexto,
                Vector2.zero, Vector2.one, TextAlignmentOptions.Center, true);
            etiqueta.raycastTarget = false;
            etiqueta.fontSizeMin = 12f;
            return boton;
        }

        private static Image CrearPanel(string nombre, Transform padre, Vector2 anclaMin, Vector2 anclaMax, Color color)
        {
            var imagen = CrearImagen(nombre, padre, anclaMin, anclaMax, color);
            if (_spriteTarjeta == null)
                _spriteTarjeta = CrearSpriteTarjeta();
            imagen.sprite = _spriteTarjeta;
            imagen.type = Image.Type.Sliced;
            return imagen;
        }

        private static Sprite CrearSpriteTarjeta()
        {
            const int tamano = 64;
            var textura = new Texture2D(tamano, tamano, TextureFormat.RGBA32, false);
            textura.name = "Borde redondeado de tarjeta";
            textura.filterMode = FilterMode.Bilinear;
            for (var y = 0; y < tamano; y++)
            for (var x = 0; x < tamano; x++)
            {
                var cercaEsquina = (x < 10 || x >= 54) && (y < 10 || y >= 54);
                var distanciaX = x < 10 ? 9 - x : x - 54;
                var distanciaY = y < 10 ? 9 - y : y - 54;
                var dentro = !cercaEsquina || distanciaX * distanciaX + distanciaY * distanciaY <= 81;
                var borde = x < 3 || y < 3 || x >= tamano - 3 || y >= tamano - 3;
                textura.SetPixel(x, y, dentro ? (borde ? new Color32(218, 168, 68, 255) : Color.white) : Color.clear);
            }
            textura.Apply(false, true);
            return Sprite.Create(textura, new Rect(0, 0, tamano, tamano), new Vector2(0.5f, 0.5f), 100f,
                0u, SpriteMeshType.FullRect, new Vector4(14, 14, 14, 14));
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

        private static TMP_Text CrearTexto(string nombre, Transform padre, string texto, float tamano, Color color,
            Vector2 anclaMin, Vector2 anclaMax, TextAlignmentOptions alineacion, bool negrita)
        {
            var objeto = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            objeto.transform.SetParent(padre, false);
            var rect = objeto.GetComponent<RectTransform>();
            rect.anchorMin = anclaMin;
            rect.anchorMax = anclaMax;
            rect.offsetMin = new Vector2(8f, 4f);
            rect.offsetMax = new Vector2(-8f, -4f);
            var etiqueta = objeto.GetComponent<TextMeshProUGUI>();
            etiqueta.overflowMode = TextOverflowModes.Truncate;
            etiqueta.text = texto;
            etiqueta.font = _fuenteCompartida != null ? _fuenteCompartida : TMP_Settings.defaultFontAsset;
            etiqueta.fontSize = tamano;
            etiqueta.fontSizeMin = Mathf.Max(12f, tamano * 0.7f);
            etiqueta.fontSizeMax = tamano;
            etiqueta.enableAutoSizing = true;
            etiqueta.textWrappingMode = TextWrappingModes.Normal;
            etiqueta.overflowMode = TextOverflowModes.Truncate;
            etiqueta.color = color;
            etiqueta.alignment = alineacion;
            etiqueta.raycastTarget = false;
            if (negrita)
                etiqueta.fontStyle = FontStyles.Bold;
            return etiqueta;
        }

        private void MostrarSenias()
        {
            MostrarModalApoyo("Lengua de señas ecuatoriana", "El video de esta actividad está en preparación.", new Color32(179, 136, 255, 255));
        }

        private void MostrarLectura()
        {
            MostrarModalApoyo("Lectura de la consigna", TextoPregunta, new Color32(167, 221, 247, 255));
        }

        private void MostrarModalApoyo(string titulo, string mensaje, Color color)
        {
            if (_modalApoyo != null)
                Destroy(_modalApoyo);

            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
                return;

            _modalApoyo = new GameObject("ModalApoyoAccesible", typeof(RectTransform), typeof(Image));
            _modalApoyo.transform.SetParent(canvas.transform, false);
            _modalApoyo.GetComponent<Image>().color = new Color(0.02f, 0.11f, 0.1f, 0.78f);
            Estirar(_modalApoyo.GetComponent<RectTransform>());

            var panel = CrearPanel("PanelApoyo", _modalApoyo.transform, new Vector2(0.22f, 0.23f), new Vector2(0.78f, 0.77f), _colorCrema);
            CrearTexto("TituloApoyo", panel.transform, titulo, 30f, _colorTexto,
                new Vector2(0.06f, 0.70f), new Vector2(0.94f, 0.94f), TextAlignmentOptions.Center, true);
            CrearTexto("MensajeApoyo", panel.transform, mensaje, 25f, _colorTexto,
                new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.68f), TextAlignmentOptions.Center, false);
            CrearBoton("CerrarApoyo", panel.transform, "VOLVER A LA ACTIVIDAD", new Vector2(0.22f, 0.08f),
                new Vector2(0.78f, 0.25f), color, () =>
                {
                    Destroy(_modalApoyo);
                    _modalApoyo = null;
                });
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
