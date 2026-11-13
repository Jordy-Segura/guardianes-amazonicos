using GuardianesAmazonicos.Views;
using AnimacionPersonajePNG = GuardianesAmazonicos.Views.AnimacionPersonajePNG;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.Video;

namespace GuardianesAmazonicos.Editor
{
    /// <summary>
    /// Construye en el editor la plantilla aprobada de «Guardianes de la Amazonía – Guía Unity».
    /// Todos los RectTransform usan Anchor Min = Max = (0, 1) y Pivot = (0, 1); por eso
    /// anchoredPosition y sizeDelta coinciden con la tabla de coordenadas del documento
    /// (Canvas 1920 × 1080, origen arriba a la izquierda, Y negativa hacia abajo).
    /// Las ventanas (correcta, pista, señas, bienvenida) siguen las capturas de referencia.
    /// </summary>
    public static class PlantillaUIBuilder
    {
        public const float ANCHO = 1920f;
        public const float ALTO = 1080f;

        // Recursos base de la plantilla (no se redibujan ni se modifica su importación).
        private const string UI = "Assets/Art/UI/";
        public const string SPRITE_LECTURA = UI + "BotonLectura.png";
        public const string SPRITE_SENAS = UI + "BotonSenas.png";
        public const string SPRITE_AYUDA = UI + "BotonAyuda.png";
        public const string SPRITE_REGRESAR = UI + "BotonRegresar.png";
        public const string SPRITE_AVANZAR = UI + "BotonAvanzar.png";
        public const string SPRITE_INICIO = UI + "BotonInicio.png";
        public const string SPRITE_CONFIGURACION = UI + "BotonConfiguracion.png";
        public const string SPRITE_TITULO = UI + "BarraProgresoNivel.png";
        public const string SPRITE_PROGRESO = UI + "FondoNivelActual.png";
        public const string SPRITE_GLOBO = UI + "TarjetaActividad.png";
        public const string SPRITE_MARCO = UI + "MarcoImagenPregunta.png";
        public const string SPRITE_TARJETA = UI + "TarjetaRespuesta.png";
        public const string SPRITE_BRILLO = UI + "BrilloRespuestaSeleccionada.png";
        public const string SPRITE_ACEPTAR = UI + "BotonAceptar.png";
        public const string SPRITE_MUY_BIEN = UI + "LetreroMuyBien.png";
        public const string SPRITE_SIGUIENTE = UI + "BotonSiguiente.png";
        public const string SPRITE_RESPLANDOR = UI + "PlantillaVerdeIluminada.png";
        public const string SPRITE_CARTA_PISTA = UI + "CartaPista.png";
        public const string SPRITE_VOY_INTENTARLO = UI + "BotonVoyIntentarlo.png";
        public const string SPRITE_FONDO_PISTA = UI + "FondoAmarilloPista.png";
        public const string SPRITE_TARJETA_SENAS = UI + "TarjetaSenas.png";
        public const string SPRITE_REPRODUCIR = UI + "BotonReproducir.png";
        public const string SPRITE_CERRAR = UI + "BotonCerrar.png";

        // Componentes que aparecen en la guía y se incorporaron al proyecto.
        public const string SPRITE_PRESENTACION = UI + "TarjetaPresentacionNivel.png";
        public const string SPRITE_COMENZAR = UI + "BotonComenzar.png";
        public const string SPRITE_GLOBO_CORRECCION = UI + "GloboCorreccion.png";
        public const string SPRITE_BORDE_INCORRECTO = UI + "BordeRespuestaIncorrecta.png";
        public const string SPRITE_BORDE_CORRECTO = UI + "BordeRespuestaCorrecta.png";
        public const string SPRITE_INTENTARLO = UI + "BotonIntentarloDeNuevo.png";

        public const string FUENTE = "Assets/Fonts/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        public const string RENDER_TEXTURE_LSEC = "Assets/Art/LSEC_RenderTexture.renderTexture";
        public const string AUDIO_AMBIENTE_SELVA = "Assets/Audio/SFX/ambiente_selva_suave.wav";

        public static readonly string[] SPRITES_NUEVOS =
        {
            SPRITE_PRESENTACION, SPRITE_COMENZAR, SPRITE_GLOBO_CORRECCION,
            SPRITE_BORDE_INCORRECTO, SPRITE_BORDE_CORRECTO, SPRITE_INTENTARLO
        };

        public static readonly Color TINTA = new Color32(0x24, 0x1B, 0x0E, 0xFF);
        public static readonly Color MADERA = new Color32(0x4A, 0x2A, 0x0C, 0xFF);
        public static readonly Color VERDE_OSCURO = new Color32(0x16, 0x4A, 0x1E, 0xFF);
        public static readonly Color ROJO_TEXTO = new Color32(0x6B, 0x15, 0x10, 0xFF);
        public static readonly Color DORADO = new Color32(0xFF, 0xD9, 0x4A, 0xFF);

        private static TMP_FontAsset _fuente;

        public sealed class Cabecera
        {
            public Button lectura, senas, ayuda, regresar, avanzar, inicio, configuracion;
            public TMP_Text titulo, progreso;
        }

        public sealed class Bienvenida
        {
            public GameObject raiz;
            public Button comenzar;
            public AnimacionPersonajePNG animacion;
        }

        public sealed class PanelSenas
        {
            public GameObject raiz;
            public Button reproducir, cerrar;
            public TMP_Text estado;
            public RawImage pantalla;
            public VideoPlayer reproductor;
        }

        public struct DatosBienvenida
        {
            public string etiquetaNivel;
            public string nombreNivel;
            public string saludo;
            public string descripcion;
            public Sprite fondo;
            public Sprite imagen;
            public Sprite personaje;
            public Sprite[] fotogramas;
        }

        // ---------- Preparación ----------

        public static void Preparar()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (string ruta in SPRITES_NUEVOS) ImportarComoSprite(ruta);
            _fuente = Cargar<TMP_FontAsset>(FUENTE);
        }

        /// <summary>Solo para recursos nuevos de estas escenas: los compartidos no se reimportan.</summary>
        public static void ImportarComoSprite(string ruta)
        {
            TextureImporter importer = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (importer == null) throw new System.InvalidOperationException("No se encontró la textura: " + ruta);
            if (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Single
                && !importer.mipmapEnabled && importer.alphaIsTransparency)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }

        public static T Cargar<T>(string ruta) where T : Object
        {
            T recurso = AssetDatabase.LoadAssetAtPath<T>(ruta);
            if (recurso == null) throw new System.InvalidOperationException("No se encontró el recurso: " + ruta);
            return recurso;
        }

        public static Sprite S(string ruta)
        {
            return Cargar<Sprite>(ruta);
        }

        // ---------- Escena y Canvas ----------

        public static void CrearCamaraYEventos()
        {
            GameObject camara = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camara.tag = "MainCamera";
            camara.transform.position = new Vector3(0f, 0f, -10f);
            Camera componente = camara.GetComponent<Camera>();
            componente.clearFlags = CameraClearFlags.SolidColor;
            componente.backgroundColor = new Color(0.04f, 0.16f, 0.1f, 1f);
            componente.orthographic = true;

            GameObject eventos = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventos.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

            AudioClip clipAmbiente = AssetDatabase.LoadAssetAtPath<AudioClip>(AUDIO_AMBIENTE_SELVA);
            if (clipAmbiente != null)
            {
                GameObject ambiente = new GameObject("AmbienteSelva", typeof(AudioSource));
                AudioSource fuenteAmbiente = ambiente.GetComponent<AudioSource>();
                fuenteAmbiente.clip = clipAmbiente;
                fuenteAmbiente.playOnAwake = true;
                fuenteAmbiente.loop = true;
                fuenteAmbiente.volume = 0.22f;
                fuenteAmbiente.spatialBlend = 0f;
            }
        }

        public static Canvas CrearCanvas(string nombre)
        {
            GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = objeto.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler escalador = objeto.GetComponent<CanvasScaler>();
            escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escalador.referenceResolution = new Vector2(ANCHO, ALTO);
            escalador.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            escalador.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        /// <summary>Contenedor de pantalla completa con el mismo origen que el Canvas.</summary>
        public static RectTransform Capa(string nombre, Transform padre)
        {
            return Rect(nombre, padre, 0f, 0f, ANCHO, ALTO);
        }

        // ---------- Primitivas con el sistema de coordenadas de la guía ----------

        public static RectTransform Rect(string nombre, Transform padre, float x, float y, float ancho, float alto)
        {
            GameObject objeto = new GameObject(nombre, typeof(RectTransform));
            objeto.transform.SetParent(padre, false);
            RectTransform rect = (RectTransform)objeto.transform;
            Colocar(rect, x, y, ancho, alto);
            return rect;
        }

        public static void Colocar(RectTransform rect, float x, float y, float ancho, float alto)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(ancho, alto);
            rect.localScale = Vector3.one;
        }

        public static Image Imagen(string nombre, Transform padre, Sprite sprite, float x, float y, float ancho, float alto,
            bool conservarProporcion = false, bool bloquearToques = false)
        {
            RectTransform rect = Rect(nombre, padre, x, y, ancho, alto);
            Image imagen = rect.gameObject.AddComponent<Image>();
            imagen.sprite = sprite;
            imagen.preserveAspect = conservarProporcion;
            imagen.raycastTarget = bloquearToques;
            return imagen;
        }

        /// <summary>
        /// Coloca un PNG con márgenes transparentes de modo que su parte visible
        /// (minX, minY, maxX, maxY en píxeles del PNG) ocupe el rectángulo indicado.
        /// </summary>
        public static Image ImagenVisible(string nombre, Transform padre, Sprite sprite,
            int minX, int minY, int maxX, int maxY, float x, float y, float anchoVisible)
        {
            // Las medidas están en píxeles del PNG original; Unity puede reducir texturas mayores de 2048.
            float anchoPng = sprite.rect.width;
            float altoPng = sprite.rect.height;
            TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite)) as TextureImporter;
            if (importer != null)
            {
                importer.GetSourceTextureWidthAndHeight(out int anchoOriginal, out int altoOriginal);
                anchoPng = anchoOriginal;
                altoPng = altoOriginal;
            }

            float escala = anchoVisible / (maxX - minX + 1);
            return Imagen(nombre, padre, sprite, x - minX * escala, y + minY * escala,
                anchoPng * escala, altoPng * escala);
        }

        public static TMP_Text Texto(string nombre, Transform padre, string contenido, float x, float y, float ancho, float alto,
            float tamano, Color color, FontStyles estilo = FontStyles.Bold, TextAlignmentOptions alineacion = TextAlignmentOptions.Center)
        {
            RectTransform rect = Rect(nombre, padre, x, y, ancho, alto);
            TextMeshProUGUI texto = rect.gameObject.AddComponent<TextMeshProUGUI>();
            texto.font = _fuente;
            texto.text = contenido;
            texto.fontSize = tamano;
            texto.enableAutoSizing = true;
            texto.fontSizeMax = tamano;
            texto.fontSizeMin = Mathf.Max(20f, tamano * 0.6f);
            texto.fontStyle = estilo;
            texto.color = color;
            texto.alignment = alineacion;
            texto.textWrappingMode = TextWrappingModes.Normal;
            texto.overflowMode = TextOverflowModes.Ellipsis;
            texto.richText = true;
            texto.raycastTarget = false;
            return texto;
        }

        public static Button Boton(Image grafico, string nombreAccesible)
        {
            grafico.raycastTarget = true;
            Button boton = grafico.gameObject.AddComponent<Button>();
            boton.targetGraphic = grafico;
            boton.transition = Selectable.Transition.ColorTint;
            ColorBlock colores = ColorBlock.defaultColorBlock;
            colores.normalColor = Color.white;
            colores.highlightedColor = new Color(1f, 0.96f, 0.82f, 1f);
            colores.pressedColor = new Color(0.86f, 0.8f, 0.62f, 1f);
            colores.selectedColor = new Color(1f, 0.93f, 0.7f, 1f);
            colores.disabledColor = new Color(0.62f, 0.62f, 0.62f, 0.7f);
            colores.fadeDuration = 0.08f;
            boton.colors = colores;
            boton.navigation = new Navigation { mode = Navigation.Mode.Automatic };
            if (!string.IsNullOrEmpty(nombreAccesible)) grafico.gameObject.name = nombreAccesible;
            return boton;
        }

        public static Image Bloqueador(string nombre, Transform padre, Sprite sprite, Color color)
        {
            Image imagen = Imagen(nombre, padre, sprite, 0f, 0f, ANCHO, ALTO, false, true);
            imagen.color = color;
            return imagen;
        }

        // ---------- Bloques de la plantilla ----------

        /// <summary>Cabecera aprobada: Lectura, Señas, Ayuda, título, progreso y navegación.</summary>
        public static Cabecera ConstruirCabecera(Transform padre, string titulo, int escena, int total, bool incluirAyuda)
        {
            Cabecera c = new Cabecera();
            RectTransform capa = Capa("Cabecera", padre);

            c.lectura = Boton(Imagen("Lectura", capa, S(SPRITE_LECTURA), 30f, -16f, 132f, 114f), "BotonLectura");
            c.senas = Boton(Imagen("Senas", capa, S(SPRITE_SENAS), 170f, -16f, 132f, 114f), "BotonSenas");
            if (incluirAyuda)
            {
                c.ayuda = Boton(Imagen("Ayuda", capa, S(SPRITE_AYUDA), 310f, -16f, 132f, 114f), "BotonAyuda");
            }

            Image placaTitulo = Imagen("TituloNivel", capa, S(SPRITE_TITULO), 592f, -12f, 484f, 108f);
            c.titulo = Texto("TextoTituloNivel", placaTitulo.transform, titulo, 66f, -22f, 352f, 64f, 28f, TINTA);
            c.titulo.textWrappingMode = TextWrappingModes.NoWrap;
            c.titulo.fontSizeMin = 18f;

            Image placaProgreso = Imagen("Progreso", capa, S(SPRITE_PROGRESO), 1092f, -16f, 236f, 94f);
            c.progreso = Texto("TextoProgreso", placaProgreso.transform, RetoSeleccionView.FormatearProgreso(escena, total),
                70f, -14f, 148f, 64f, 36f, Color.white);
            c.progreso.textWrappingMode = TextWrappingModes.NoWrap;

            c.regresar = Boton(Imagen("Regresar", capa, S(SPRITE_REGRESAR), 1506f, -12f, 84f, 84f), "BotonRegresar");
            c.avanzar = Boton(Imagen("Avanzar", capa, S(SPRITE_AVANZAR), 1606f, -12f, 84f, 84f), "BotonAvanzar");
            c.inicio = Boton(Imagen("Inicio", capa, S(SPRITE_INICIO), 1706f, -12f, 84f, 84f), "BotonInicio");
            c.configuracion = Boton(Imagen("Configuracion", capa, S(SPRITE_CONFIGURACION), 1806f, -12f, 84f, 84f), "BotonConfiguracion");
            return c;
        }

        /// <summary>
        /// Pantalla principal de la actividad con las coordenadas aprobadas.
        /// Capas: personaje → globo + texto → escena + marco → tres tarjetas → Aceptar / Intentarlo.
        /// </summary>
        public static void ConstruirActividad(Transform padre, string pregunta, string[] opciones, Sprite[] imagenes,
            Sprite personaje, Sprite personajeIncorrecto, Sprite[] fotogramas, Sprite imagenEscena, Sprite elementoEscena, Rect rectElemento,
            RetoSeleccionView.ReferenciasActividad r)
        {
            RectTransform capa = Capa("Actividad", padre);
            r.capaActividad = capa.gameObject.AddComponent<CanvasGroup>();

            Sprite spriteInicial = fotogramas != null && fotogramas.Length > 0 ? fotogramas[0] : personaje;
            Image guia = Imagen("Personaje", capa, spriteInicial, 147f, -428f, 390f, 562f, true);
            guia.raycastTarget = false;
            r.personajeGuia = guia.gameObject;
            r.imagenPersonajeGuia = guia;
            r.spritePersonajeNormal = spriteInicial;
            r.spritePersonajeIncorrecto = personajeIncorrecto != null ? personajeIncorrecto : personaje;
            r.animacionPersonajeGuia = ConfigurarAnimacionPersonaje(guia, fotogramas, 24f, true);

            r.globoPregunta = Imagen("GloboPregunta", capa, S(SPRITE_GLOBO), 158f, -174f, 684f, 240f).gameObject;
            Texto("TextoPregunta", r.globoPregunta.transform, pregunta, 201f - 158f, -208f + 174f, 598f, 150f, 40f, TINTA);

            // Globo de corrección: misma posición que el globo de pregunta.
            Image correccion = ImagenVisible("GloboCorreccion", capa, S(SPRITE_GLOBO_CORRECCION), 12, 124, 698, 363, 158f, -174f, 684f);
            r.globoCorreccion = correccion.gameObject;
            Vector2 origen = ((RectTransform)correccion.transform).anchoredPosition;
            r.textoCorreccion = Texto("TextoCorreccion", correccion.transform, string.Empty,
                201f - origen.x, -208f - origen.y, 598f, 150f, 36f, ROJO_TEXTO);
            r.globoCorreccion.SetActive(false);

            // La ilustración de referencia es opcional para actividades que solo requieren tarjetas.
            if (imagenEscena != null)
            {
                RectTransform ventana = Rect("ImagenPrincipal", capa, 942f, -206f, 758f, 390f);
                // Máscara redondeada para que las esquinas de la ilustración no salgan del marco.
                ventana.gameObject.AddComponent<CanvasRenderer>();
                RoundedRectangleGraphic forma = ventana.gameObject.AddComponent<RoundedRectangleGraphic>();
                forma.raycastTarget = false;
                SerializedObject datosForma = new SerializedObject(forma);
                datosForma.FindProperty("_radioEsquina").floatValue = 34f;
                datosForma.ApplyModifiedPropertiesWithoutUndo();
                ventana.gameObject.AddComponent<Mask>().showMaskGraphic = false;
                Imagen("EscenaPregunta", ventana, imagenEscena, 0f, 0f, 758f, 390f);
                if (elementoEscena != null)
                {
                    Imagen("ElementoEscena", ventana, elementoEscena, rectElemento.x, rectElemento.y, rectElemento.width, rectElemento.height, true);
                }
                Imagen("MarcoImagen", capa, S(SPRITE_MARCO), 921f, -188f, 800f, 425f);
            }

            bool cuatroTarjetas = opciones != null && opciones.Length == 4;
            float[] columnas = cuatroTarjetas
                ? new[] { 1020f, 1340f, 1020f, 1340f }
                : new[] { 921f, 1193f, 1465f };
            r.tarjetas = new RetoSeleccionView.TarjetaRespuestaVista[opciones.Length];
            for (int i = 0; i < opciones.Length; i++)
            {
                r.tarjetas[i] = ConstruirTarjeta(capa, i, columnas[i], opciones[i], imagenes != null && i < imagenes.Length ? imagenes[i] : null, cuatroTarjetas);
            }

            r.botonAceptar = Boton(ImagenVisible("Aceptar", capa, S(SPRITE_ACEPTAR), 18, 18, 1227, 387, 1171f, -978f, 300f), "BotonAceptar");
            r.botonAceptar.gameObject.SetActive(false);
            r.botonIntentarloDeNuevo = Boton(ImagenVisible("IntentarloDeNuevo", capa, S(SPRITE_INTENTARLO), 35, 56, 767, 216, 1101f, -976f, 440f),
                "BotonIntentarloDeNuevo");
            r.botonIntentarloDeNuevo.gameObject.SetActive(false);
        }

        private static RetoSeleccionView.TarjetaRespuestaVista ConstruirTarjeta(Transform padre, int indice, float x, string nombre, Sprite imagen, bool cuatroTarjetas)
        {
            RetoSeleccionView.TarjetaRespuestaVista t = new RetoSeleccionView.TarjetaRespuestaVista();
            float anchoTarjeta = cuatroTarjetas ? 240f : 256f;
            float posicionY = cuatroTarjetas ? (indice < 2 ? -235f : -560f) : -663f;
            float escalaDecoracion = anchoTarjeta / 256f;
            Image base_ = Imagen("Respuesta" + (indice + 1), padre, S(SPRITE_TARJETA), x, posicionY, anchoTarjeta, 307f);
            t.boton = Boton(base_, "Respuesta" + (indice + 1));

            float anchoIlustracion = anchoTarjeta - 52f;
            t.ilustracion = Imagen("Ilustracion", base_.transform, imagen, (anchoTarjeta - anchoIlustracion) / 2f, -22f, anchoIlustracion, 188f, true);
            t.nombre = Texto("Nombre", base_.transform, nombre, 15f, -216f, anchoTarjeta - 30f, 76f, 28f, TINTA);
            t.nombre.maxVisibleLines = 2;
            t.nombre.margin = new Vector4(8f, 0f, 8f, 0f);

            // Superposiciones alineadas con el borde dorado de la tarjeta (medido en los PNG).
            t.brilloSeleccion = Imagen("BrilloSeleccion", base_.transform, S(SPRITE_BRILLO), -40.6f * escalaDecoracion, 56.4f * escalaDecoracion, 351f * escalaDecoracion, 390.4f * escalaDecoracion).gameObject;
            t.bordeIncorrecto = Imagen("BordeIncorrecto", base_.transform, S(SPRITE_BORDE_INCORRECTO), -15.5f * escalaDecoracion, 32.8f * escalaDecoracion, 288.2f * escalaDecoracion, 374.1f * escalaDecoracion).gameObject;
            t.bordeCorrecto = Imagen("BordeCorrecto", base_.transform, S(SPRITE_BORDE_CORRECTO), -15.5f * escalaDecoracion, 32.8f * escalaDecoracion, 288.2f * escalaDecoracion, 374.1f * escalaDecoracion).gameObject;
            t.brilloSeleccion.SetActive(false);
            t.bordeIncorrecto.SetActive(false);
            t.bordeCorrecto.SetActive(false);
            return t;
        }

        /// <summary>Bienvenida e inicio de cada nivel: tarjeta de presentación y «¡Comenzar!».</summary>
        public static Bienvenida ConstruirBienvenida(Transform padre, DatosBienvenida d)
        {
            Bienvenida b = new Bienvenida();
            RectTransform capa = Capa("Bienvenida", padre);
            b.raiz = capa.gameObject;

            Imagen("FondoBienvenida", capa, d.fondo, 0f, 0f, ANCHO, ALTO, false, true);
            Sprite spriteBienvenida = d.fotogramas != null && d.fotogramas.Length > 0 ? d.fotogramas[0] : d.personaje;
            Image personajeBienvenida = Imagen("PersonajeBienvenida", capa, spriteBienvenida, 100f, -320f, 480f, 720f, true);
            b.animacion = ConfigurarAnimacionPersonaje(personajeBienvenida, d.fotogramas, 24f, true);

            // La ventana transparente de la tarjeta (PNG 668 × 653, hueco 80-595 × 252-486)
            // deja ver la imagen de presentación del nivel.
            const float escala = 700f / 662f;
            float tx = 610f - 6f * escala;
            float ty = -120f + 6f * escala;
            RectTransform ventana = Rect("ImagenPresentacion", capa, tx + 74f * escala, ty - 246f * escala, 527f * escala, 246f * escala);
            ventana.gameObject.AddComponent<RectMask2D>();
            Imagen("Ilustracion", ventana, d.imagen, 0f, 0f, 527f * escala, 246f * escala);

            Image tarjeta = Imagen("TarjetaPresentacion", capa, S(SPRITE_PRESENTACION), tx, ty, 668f * escala, 653f * escala);
            // Tablas de madera medidas en el PNG: superior x 198-457, y 25-100; inferior x 75-594, y 100-202.
            Texto("TextoNivel", tarjeta.transform, d.etiquetaNivel, 210f * escala, -32f * escala, 238f * escala, 64f * escala, 40f, MADERA);
            TMP_Text nombre = Texto("TextoNombreNivel", tarjeta.transform, d.nombreNivel, 95f * escala, -112f * escala, 480f * escala, 84f * escala, 54f, DORADO);
            AplicarContorno(nombre);
            Texto("TextoSaludo", tarjeta.transform, d.saludo, 70f * escala, -492f * escala, 528f * escala, 52f * escala, 40f, TINTA);
            // Entre las hojas de las esquinas inferiores (x 130-540 en el PNG).
            Texto("TextoDescripcion", tarjeta.transform, d.descripcion, 128f * escala, -544f * escala, 412f * escala, 80f * escala, 26f, TINTA, FontStyles.Normal);

            b.comenzar = Boton(ImagenVisible("Comenzar", capa, S(SPRITE_COMENZAR), 8, 8, 600, 194, 730f, -830f, 460f), "BotonComenzar");
            return b;
        }

        /// <summary>Respuesta correcta: resplandor verde, «MUY BIEN», frase breve y «Siguiente».</summary>
        public static void ConstruirEstadoCorrecto(Transform padre, Sprite personaje, Sprite[] fotogramas, string felicitacion,
            RetoSeleccionView.ReferenciasActividad r)
        {
            RectTransform capa = Capa("EstadoCorrecto", padre);
            r.estadoCorrecto = capa.gameObject;
            Image resplandor = Bloqueador("ResplandorVerde", capa, S(SPRITE_RESPLANDOR), Color.white);
            resplandor.raycastTarget = false;

            Image celebracion = Imagen("PersonajeCelebracion", capa, fotogramas != null && fotogramas.Length > 0 ? fotogramas[0] : personaje,
                300f, -290f, 470f, 705f, true);
            r.animacionCelebracion = ConfigurarAnimacionPersonaje(celebracion, fotogramas, 24f, true);

            RectTransform contenido = Rect("Felicitacion", capa, 820f, -260f, 880f, 560f);
            // Pivote al centro para que la entrada breve escale desde el centro del mensaje.
            contenido.pivot = new Vector2(0.5f, 0.5f);
            contenido.anchoredPosition = new Vector2(820f + 440f, -260f - 280f);
            r.contenidoCelebracion = contenido;
            ImagenVisible("LetreroMuyBien", contenido, S(SPRITE_MUY_BIEN), 0, 19, 1079, 331, 20f, -30f, 840f);
            r.textoFelicitacion = Texto("TextoFelicitacion", contenido, felicitacion, 20f, -290f, 840f, 80f, 52f, Color.white);
            AplicarContorno(r.textoFelicitacion);
            r.botonSiguiente = Boton(ImagenVisible("Siguiente", contenido, S(SPRITE_SIGUIENTE), 4, 4, 710, 203, 200f, -390f, 480f), "BotonSiguiente");
            capa.gameObject.SetActive(false);
        }

        /// <summary>Ayuda: ventana de pista centrada; bloquea las tarjetas mientras está abierta.</summary>
        public static void ConstruirPanelPista(Transform padre, string pista, Sprite personaje, Sprite[] fotogramas, RetoSeleccionView.ReferenciasActividad r)
        {
            RectTransform capa = Capa("PanelPista", padre);
            r.panelPista = capa.gameObject;
            Image fondoLuz = Bloqueador("FondoLuz", capa, S(SPRITE_FONDO_PISTA), new Color(1f, 1f, 1f, 0.55f));
            fondoLuz.raycastTarget = false;
            Sprite spritePista = fotogramas != null && fotogramas.Length > 0 ? fotogramas[0] : personaje;
            Image personajePista = Imagen("PersonajePista", capa, spritePista, 147f, -428f, 390f, 562f, true);
            r.animacionPista = ConfigurarAnimacionPersonaje(personajePista, fotogramas, 24f, true);
            ImagenVisible("CartaPista", capa, S(SPRITE_CARTA_PISTA), 45, 110, 1414, 965, 560f, -245f, 880f);
            Texto("TextoPista", capa, pista, 640f, -440f, 720f, 150f, 40f, VERDE_OSCURO);
            r.botonVoyIntentarlo = Boton(ImagenVisible("VoyAIntentarlo", capa, S(SPRITE_VOY_INTENTARLO), 164, 108, 2005, 570, 750f, -610f, 500f),
                "BotonVoyAIntentarlo");
            capa.gameObject.SetActive(false);
        }

        public static AnimacionPersonajePNG ConfigurarAnimacionPersonaje(Image destino, Sprite[] fotogramas, float fps = 24f, bool enBucle = true)
        {
            if (destino == null || fotogramas == null || fotogramas.Length == 0) return null;
            AnimacionPersonajePNG animacion = destino.gameObject.AddComponent<AnimacionPersonajePNG>();
            SerializedObject datos = new SerializedObject(animacion);
            datos.FindProperty("_destinoCanvas").objectReferenceValue = destino;
            datos.FindProperty("_imagenesPorSegundo").floatValue = fps;
            datos.FindProperty("_reducirMovimiento").boolValue = false;
            datos.FindProperty("_reproducirAlActivar").boolValue = true;
            datos.FindProperty("_enBucle").boolValue = enBucle;
            SerializedProperty lista = datos.FindProperty("_fotogramas");
            lista.arraySize = fotogramas.Length;
            for (int i = 0; i < fotogramas.Length; i++) lista.GetArrayElementAtIndex(i).objectReferenceValue = fotogramas[i];
            datos.ApplyModifiedPropertiesWithoutUndo();
            return animacion;
        }

        /// <summary>Señas: VideoPlayer → RenderTexture → RawImage dentro del panel, con reproducir/pausar y X.</summary>
        public static PanelSenas ConstruirPanelSenas(Transform padre)
        {
            PanelSenas p = new PanelSenas();
            RectTransform capa = Capa("PanelSenas", padre);
            p.raiz = capa.gameObject;
            Bloqueador("Oscurecer", capa, null, new Color(0.04f, 0.14f, 0.12f, 0.62f));
            ImagenVisible("TarjetaSenas", capa, S(SPRITE_TARJETA_SENAS), 16, 15, 1163, 687, 560f, -200f, 1000f);

            RenderTexture destino = Cargar<RenderTexture>(RENDER_TEXTURE_LSEC);
            RectTransform area = Rect("AreaVideo", capa, 661f, -341f, 798f, 312f);
            p.pantalla = area.gameObject.AddComponent<RawImage>();
            p.pantalla.texture = destino;
            p.pantalla.raycastTarget = false;
            p.pantalla.enabled = false;
            p.estado = Texto("EstadoVideo", capa, "El video en lengua de señas está pendiente de grabación.", 700f, -400f, 720f, 190f, 32f,
                new Color32(0x2E, 0x2A, 0x5C, 0xFF));

            p.reproducir = Boton(ImagenVisible("Reproducir", capa, S(SPRITE_REPRODUCIR), 14, 14, 475, 471, 1010f, -658f, 100f), "BotonReproducirSenas");
            p.cerrar = Boton(ImagenVisible("Cerrar", capa, S(SPRITE_CERRAR), 15, 16, 351, 358, 1500f, -170f, 84f), "BotonCerrarSenas");

            p.reproductor = capa.gameObject.AddComponent<VideoPlayer>();
            p.reproductor.playOnAwake = false;
            p.reproductor.isLooping = false;
            p.reproductor.renderMode = VideoRenderMode.RenderTexture;
            p.reproductor.targetTexture = destino;
            p.reproductor.audioOutputMode = VideoAudioOutputMode.Direct;
            capa.gameObject.SetActive(false);
            return p;
        }

        private static void AplicarContorno(TMP_Text texto)
        {
            Material contorno = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Fonts/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Outline.mat");
            if (contorno != null) texto.fontSharedMaterial = contorno;
        }
    }
}
