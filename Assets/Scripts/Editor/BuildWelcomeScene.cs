using GuardianesAmazonicos.Presenters;
using GuardianesAmazonicos.Views;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace GuardianesAmazonicos.Editor
{
    /// <summary>
    /// Construye la pantalla de bienvenida con elementos de uGUI y textos editables.
    /// La composición sigue la plantilla visual y el Canvas de referencia 1920 x 1080.
    /// </summary>
    public static class BuildWelcomeScene
    {
        private const string ScenePath = "Assets/Scenes/Niveles/Nivel01/BienvenidaDelGuardian.unity";
        private const string FontPath = "Assets/Fonts/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        private const string BackgroundPath = "Assets/Art/Backgrounds/N1_ConozcoLaSelva.png";
        private const string BoyPath = "Assets/Art/Characters/Explorador.png";
        private const string JaguarPath = "Assets/Art/Characters/Jaguar.png";
        private const string LevelPlaquePath = "Assets/Art/UI/FondoNivelActual.png";
        private const string WelcomeCardPath = "Assets/Art/UI/TarjetaRespuesta.png";
        private const string ImageFramePath = "Assets/Art/UI/MarcoImagenPregunta.png";
        private const string PrimaryButtonPath = "Assets/Art/Botones/principal_Normal.png";
        private const string AudioClipPath = "Assets/Audio/Voice/BienvenidaSelva.wav";
        private const string SignIconPath = "Assets/Art/Iconos/Senas_Oscuro.png";

        private static readonly Color Forest = new Color(23f / 255f, 63f / 255f, 53f / 255f);
        private static readonly Color Cream = new Color(1f, 248f / 255f, 232f / 255f);
        private static readonly Color Leaf = new Color(36f / 255f, 160f / 255f, 18f / 255f);
        private static readonly Color Gold = new Color(1f, 190f / 255f, 35f / 255f);
        private static readonly Color Purple = new Color(230f / 255f, 216f / 255f, 1f);
        private static TMP_FontAsset _font;

        [MenuItem("Guardianes/Construir bienvenida según plantilla")]
        public static void Build()
        {
            ImportSprites();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (_font == null)
                throw new System.InvalidOperationException("No se encontró la fuente TMP: " + FontPath);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            CreateEventSystem();

            Canvas canvas = CreateCanvas();
            AddBackground(canvas.transform);
            AddCharacter(canvas.transform);
            AddHeader(canvas.transform, out TMP_Text progressText);
            AddWelcomeCard(canvas.transform, progressText, out BienvenidaNivelView view);
            CreateSignLanguageModal(canvas.transform, view);
            CreatePresenter(view);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Pantalla de bienvenida reconstruida con Canvas adaptable y textos editables: " + ScenePath);
        }

        private static void ImportSprites()
        {
            string[] paths =
            {
                BackgroundPath, BoyPath, JaguarPath, LevelPlaquePath, WelcomeCardPath, ImageFramePath,
                PrimaryButtonPath, SignIconPath
            };

            foreach (string path in paths)
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 2048;
                importer.SaveAndReimport();
            }
        }

        private static Canvas CreateCanvas()
        {
            GameObject root = new GameObject("CanvasBienvenida", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void AddBackground(Transform parent)
        {
            Image background = CreateImage("FondoSelvaNivel", parent,
                AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath), 0f, 0f, 1920f, 1080f);
            background.preserveAspect = false;
            background.raycastTarget = false;
        }

        private static void AddCharacter(Transform parent)
        {
            Image boy = CreateImage("GuardianExplorador", parent,
                AssetDatabase.LoadAssetAtPath<Sprite>(BoyPath), -585f, -65f, 650f, 870f);
            boy.preserveAspect = true;
            boy.raycastTarget = false;
        }

        private static void AddHeader(Transform parent, out TMP_Text progress)
        {
            Sprite plaque = AssetDatabase.LoadAssetAtPath<Sprite>(LevelPlaquePath);
            Image titlePlaque = CreateImage("PlacaNivel", parent, plaque, 0f, 450f, 560f, 122f);
            titlePlaque.preserveAspect = true;
            titlePlaque.raycastTarget = false;
            CreateText("TextoNivel", titlePlaque.transform, "Nivel 1 · Conozco la selva",
                0f, -4f, 470f, 74f, 48f, Forest, FontStyles.Bold);

            Image progressPlaque = CreateImage("PlacaProgreso", parent, plaque, 770f, 450f, 285f, 118f);
            progressPlaque.preserveAspect = true;
            progressPlaque.raycastTarget = false;
            progress = CreateText("TextoProgreso", progressPlaque.transform, "0 de 12",
                28f, -8f, 190f, 62f, 42f, Cream, FontStyles.Bold);

            CreateAccessButton(parent, "BotonSenas", "SEÑAS", SignIconPath,
                new Color(194f / 255f, 132f / 255f, 241f / 255f), -645f, 450f, "Lengua de señas");
        }

        private static void AddWelcomeCard(Transform parent, TMP_Text progress, out BienvenidaNivelView view)
        {
            RectTransform card = CreateRect("TarjetaBienvenida", parent, 430f, -40f, 850f, 780f);
            Image cardGraphic = card.gameObject.AddComponent<Image>();
            cardGraphic.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(WelcomeCardPath);
            cardGraphic.preserveAspect = false;
            RectTransform inner = CreateRect("InteriorTarjeta", card, 0f, 0f, 812f, 744f);

            RectTransform imageArea = CreateRect("MarcoMision", inner, 0f, 214f, 590f, 280f);
            Image frame = imageArea.gameObject.AddComponent<Image>();
            frame.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ImageFramePath);
            frame.preserveAspect = false;
            frame.raycastTarget = false;
            RectTransform artRect = CreateRect("ImagenMisionJaguar", imageArea, 0f, 0f, 530f, 225f);
            Image art = artRect.gameObject.AddComponent<Image>();
            art.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(JaguarPath);
            art.preserveAspect = true;
            art.raycastTarget = false;

            RectTransform titleBanner = CreateRect("CabeceraBienvenida", inner, 0f, 18f, 620f, 94f);
            AddPanel(titleBanner.gameObject, Leaf);
            CreateText("TextoBienvenidaTitulo", titleBanner, "¡Hola, guardián!",
                0f, 0f, 590f, 78f, 52f, Color.white, FontStyles.Bold);

            CreateText("TextoBienvenida", inner,
                "Conoce la selva amazónica y descubre sus animales y plantas.",
                0f, -88f, 720f, 96f, 38f, Forest, FontStyles.Bold);

            RectTransform audioRect = CreateRect("ControlAudioBienvenida", inner, -210f, -205f, 340f, 82f);
            Image audioBg = AddPanel(audioRect.gameObject,
                new Color(28f / 255f, 170f / 255f, 220f / 255f));
            audioBg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PrimaryButtonPath);
            audioBg.color = Color.white;
            audioBg.preserveAspect = false;
            Button audioButton = audioRect.gameObject.AddComponent<Button>();
            ConfigureButton(audioButton, audioBg, new Color(28f / 255f, 170f / 255f, 220f / 255f));
            CreateText("TextoBotonAudio", audioRect, "AUDIO · ESCUCHAR",
                0f, 0f, 318f, 70f, 31f, Forest, FontStyles.Bold);

            TMP_Text audioStatus = CreateText("EstadoAudio", inner,
                "Escucha la bienvenida cuando quieras.", 0f, -265f, 700f, 40f, 25f, Forest, FontStyles.Normal);

            RectTransform startRect = CreateRect("ControlComenzar", inner, 165f, -205f, 490f, 120f);
            Image startBorder = startRect.gameObject.AddComponent<Image>();
            startBorder.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(LevelPlaquePath);
            startBorder.preserveAspect = false;
            Button startButton = startRect.gameObject.AddComponent<Button>();
            ConfigureButton(startButton, startBorder, Leaf);
            ColorBlock startColors = startButton.colors;
            startColors.disabledColor = Color.white;
            startButton.colors = startColors;
            CreateText("TextoComenzar", startRect, "¡Comenzar!",
                28f, 0f, 405f, 86f, 42f, Color.white, FontStyles.Bold);
            CreateText("IconoComenzar", startRect, ">",
                -170f, 0f, 64f, 70f, 42f, Color.white, FontStyles.Bold);

            view = card.gameObject.AddComponent<BienvenidaNivelView>();
            SetReference(view, "_textoBienvenida", inner.Find("TextoBienvenida").GetComponent<TMP_Text>());
            SetReference(view, "_textoProgreso", progress);
            SetReference(view, "_textoEstadoAudio", audioStatus);
            SetReference(view, "_textoEstadoSenas", null);
            SetReference(view, "_textoComenzar", startRect.Find("TextoComenzar").GetComponent<TMP_Text>());
            SetReference(view, "_botonComenzar", startButton);
            SetReference(view, "_botonAudio", audioButton);

            AudioSource audio = card.gameObject.AddComponent<AudioSource>();
            audio.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioClipPath);
            audio.playOnAwake = false;
            audio.loop = false;
            audio.volume = 1f;
            SetReference(view, "_fuenteAudio", audio);
        }

        private static void CreateSignLanguageModal(Transform canvas, BienvenidaNivelView view)
        {
            GameObject overlay = new GameObject("ModalLenguaDeSenas", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(canvas, false);
            SetRect(overlay.GetComponent<RectTransform>(), 0f, 0f, 1920f, 1080f);
            Image shade = overlay.GetComponent<Image>();
            shade.color = new Color(10f / 255f, 36f / 255f, 44f / 255f, 0.74f);
            shade.raycastTarget = true;

            RectTransform panel = CreateRect("PanelLSEC", overlay.transform, 0f, 0f, 1040f, 720f);
            AddPanel(panel.gameObject, new Color(181f / 255f, 126f / 255f, 228f / 255f));
            RectTransform panelInner = CreateRect("InteriorLSEC", panel, 0f, 12f, 1014f, 692f);
            AddPanel(panelInner.gameObject, Cream);

            CreateText("TituloLSEC", panelInner, "Lengua de señas",
                0f, 286f, 850f, 68f, 44f, Forest, FontStyles.Bold);

            RectTransform videoPanel = CreateRect("AreaVideoLSEC", panelInner, 0f, 20f, 880f, 380f);
            AddPanel(videoPanel.gameObject, Purple);
            CreateText("EstadoLSEC", videoPanel,
                "Video de bienvenida en LSEC pendiente de grabación.",
                0f, 16f, 800f, 170f, 34f, new Color(67f / 255f, 38f / 255f, 117f / 255f), FontStyles.Bold);

            RectTransform closeRect = CreateRect("CerrarLSEC", panelInner, 430f, 300f, 72f, 72f);
            Image closeGraphic = AddPanel(closeRect.gameObject, Gold);
            Button closeButton = closeRect.gameObject.AddComponent<Button>();
            ConfigureButton(closeButton, closeGraphic, Gold);
            CreateText("TextoCerrarLSEC", closeRect, "X", 0f, 0f, 64f, 64f, 40f, Forest, FontStyles.Bold);

            RectTransform playRect = CreateRect("ReproducirLSEC", panelInner, -190f, -280f, 300f, 82f);
            Image playGraphic = AddPanel(playRect.gameObject, new Color(204f / 255f, 204f / 255f, 204f / 255f));
            Button playButton = playRect.gameObject.AddComponent<Button>();
            ConfigureButton(playButton, playGraphic, new Color(204f / 255f, 204f / 255f, 204f));
            playButton.interactable = false;
            CreateText("TextoReproducirLSEC", playRect, ">  Reproducir",
                0f, 0f, 280f, 66f, 28f, new Color(92f / 255f, 92f / 255f, 92f / 255f), FontStyles.Bold);

            RectTransform backRect = CreateRect("VolverBienvenidaLSEC", panelInner, 190f, -280f, 420f, 82f);
            Image backGraphic = AddPanel(backRect.gameObject, Gold);
            Button backButton = backRect.gameObject.AddComponent<Button>();
            ConfigureButton(backButton, backGraphic, Gold);
            CreateText("TextoVolverLSEC", backRect, "Volver a la bienvenida",
                0f, 0f, 390f, 66f, 27f, Forest, FontStyles.Bold);

            SetReference(view, "_botonSenas", FindButton(canvas, "BotonSenas"));
            SetReference(view, "_botonCerrarSenas", closeButton);
            SetReference(view, "_botonReproducirSenas", playButton);
            SetReference(view, "_panelSenas", overlay);
            SetReference(view, "_textoEstadoSenas", panelInner.Find("AreaVideoLSEC/EstadoLSEC").GetComponent<TMP_Text>());

            VideoPlayer player = overlay.AddComponent<VideoPlayer>();
            player.playOnAwake = false;
            player.isLooping = false;
            SetReference(view, "_reproductorSenas", player);
            overlay.SetActive(false);
        }

        private static void CreatePresenter(BienvenidaNivelView view)
        {
            GameObject presenterObject = new GameObject("ControladorBienvenida");
            BienvenidaNivelPresenter presenter = presenterObject.AddComponent<BienvenidaNivelPresenter>();
            SerializedObject serialized = new SerializedObject(presenter);
            serialized.FindProperty("_vista").objectReferenceValue = view;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(presenter);
        }

        private static void CreateAccessButton(Transform parent, string name, string label, string iconPath,
            Color fill, float x, float y, string accessibilityLabel)
        {
            RectTransform rect = CreateRect(name, parent, x, y, 128f, 112f);
            Image graphic = AddPanel(rect.gameObject, fill);
            Button button = rect.gameObject.AddComponent<Button>();
            ConfigureButton(button, graphic, fill);

            Image icon = CreateImage("Icono" + label, rect, AssetDatabase.LoadAssetAtPath<Sprite>(iconPath), 0f, 18f, 44f, 44f);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            CreateText("Etiqueta" + label, rect, label, 0f, -30f, 120f, 32f, 21f, Forest, FontStyles.Bold);
            rect.gameObject.name = name + " · " + accessibilityLabel;
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite,
            float x, float y, float width, float height)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            SetRect(obj.GetComponent<RectTransform>(), x, y, width, height);
            Image image = obj.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            return image;
        }

        private static TMP_Text CreateText(string name, Transform parent, string value,
            float x, float y, float width, float height, float size, Color color, FontStyles style)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
            SetRect(obj.GetComponent<RectTransform>(), x, y, width, height);
            TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
            text.font = _font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Max(18f, size * 0.72f);
            text.fontSizeMax = size;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform CreateRect(string name, Transform parent,
            float x, float y, float width, float height)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            SetRect(rect, x, y, width, height);
            return rect;
        }

        private static void SetRect(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static Image AddPanel(GameObject obj, Color color)
        {
            Image graphic = obj.AddComponent<Image>();
            graphic.color = color;
            return graphic;
        }

        private static void ConfigureButton(Button button, Graphic target, Color normal)
        {
            button.targetGraphic = target;
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = normal;
            colors.highlightedColor = Color.Lerp(normal, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(normal, Forest, 0.32f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(normal.r, normal.g, normal.b, 0.5f);
            colors.colorMultiplier = 1f;
            button.colors = colors;
        }

        private static void SetReference(Object target, string propertyName, Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static Button FindButton(Transform parent, string name)
        {
            foreach (Button button in parent.GetComponentsInChildren<Button>(true))
                if (button.gameObject.name.StartsWith(name)) return button;
            return null;
        }

        private static void CreateCamera()
        {
            GameObject obj = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            obj.tag = "MainCamera";
            Camera camera = obj.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.04f, 0.16f, 0.1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            obj.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static void CreateEventSystem()
        {
            GameObject obj = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            obj.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
    }
}
