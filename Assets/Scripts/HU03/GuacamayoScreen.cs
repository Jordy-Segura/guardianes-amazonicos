using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace GuardianesAmazonicos.Nivel1.HU03
{
    public sealed class GuacamayoScreen : MonoBehaviour
    {
        [SerializeField] private string _nextSceneName = "PlantasNivel1";
        [SerializeField] private UnityEvent _onContinue = new UnityEvent();
        private GuacamayoActivity _activity;
        private Canvas _canvas;
        private RectTransform _root;
        private RectTransform _main;
        private RectTransform _modal;
        private CanvasGroup _mainGroup;
        private AudioSource _voice;
        private VideoPlayer _signPlayer;
        private RenderTexture _signTexture;
        private readonly List<Button> _mainButtons = new List<Button>();
        private readonly List<Button> _modalButtons = new List<Button>();
        private GameObject _returnFocus;
        private TextMeshProUGUI _audioStatus;
        private string _context = "Instruccion";
        private string _modalKind;
        private string _modalContext;
        private int _readingScale = 1;
        private int _width;
        private int _height;
        private bool _audioWasPlaying;

        public GuacamayoActivity Activity => _activity;
        public bool HasModal => _modal != null;
        public bool HasVoice => _voice != null && _voice.clip != null;
        private float Height => Mathf.Max(864, 1536f * Screen.height / Mathf.Max(1, Screen.width));

        private void Start()
        {
            _activity = new GuacamayoActivity();
            _activity.Completed += GuacamayoProgress.SaveCompletion;
            _activity.ContinueRequested += ContinueToNextActivity;
            _voice = gameObject.AddComponent<AudioSource>();
            _voice.playOnAwake = false;
            _voice.spatialBlend = 0;
            CreateCanvas();
            DrawMain();
        }

        private void CreateCanvas()
        {
            if (Camera.main == null)
            {
                var cameraObject = new GameObject("HU03Camera", typeof(Camera), typeof(AudioListener));
                cameraObject.tag = "MainCamera";
                cameraObject.transform.SetParent(transform);
                cameraObject.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            }
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("HU03EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                events.transform.SetParent(transform);
            }
            _root = new GameObject("HU03Canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<RectTransform>();
            _root.SetParent(transform, false);
            _canvas = _root.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1536, 864);
            scaler.matchWidthOrHeight = 0;
        }

        private void Update()
        {
            if (_activity == null) return;
            if (_width != Screen.width || _height != Screen.height)
            {
                string modalKind = _modalKind;
                string context = _modalContext;
                CloseModal();
                DrawMain();
                if (modalKind == "Signs") ShowSigns(context);
                else if (modalKind == "Reading") ShowReading();
                else if (modalKind == "Hint") ShowHint(false);
                else if (modalKind == "Retry") ShowHint(true);
            }
            if (Input.GetKeyDown(KeyCode.Escape) && HasModal) CloseModal();
            // Submit in the shared InputManager already maps Enter and Space.
            // Dispatching Space here too would activate a newly opened modal twice.
            if (_audioStatus != null && _audioWasPlaying != _voice.isPlaying)
            {
                _audioWasPlaying = _voice.isPlaying;
                _audioStatus.text = _audioWasPlaying ? "Audio en reproducción" : "Puedes escuchar otra vez";
            }
        }

        private void OnGUI()
        {
            Event keyEvent = Event.current;
            if (_activity == null || keyEvent.type != EventType.KeyDown || keyEvent.keyCode != KeyCode.Tab)
                return;
            // Read modifiers from the key event, even if Shift is released before the next frame.
            MoveFocus(keyEvent.shift ? -1 : 1);
            keyEvent.Use();
        }

        private void DrawMain()
        {
            _width = Screen.width;
            _height = Screen.height;
            if (_main != null) { _main.gameObject.SetActive(false); Destroy(_main.gameObject); }
            _mainButtons.Clear();
            _main = JungleUi.Rect("Activity", _root, 0, 0, 1536, Height);
            _mainGroup = _main.gameObject.AddComponent<CanvasGroup>();
            float backgroundHeight = Mathf.Max(Height, 1024);
            float backgroundWidth = backgroundHeight * 1.5f;
            JungleUi.Image(_main, "JungleBackground", LoadSprite("FondoSelva"), Color.white,
                (1536 - backgroundWidth) / 2, (Height - backgroundHeight) / 2,
                backgroundWidth, backgroundHeight);
            var brand = JungleUi.Panel(_main, "Brand", new Color32(91, 53, 20, 255), 28, 28, 470, 110);
            JungleUi.Text(brand, "Title", "GUARDIANES", 40, 18, 3, 434, 62, Color.white);
            JungleUi.Text(brand, "Subtitle", "DE LA AMAZONÍA LECTORA", 26, 18, 63, 434, 40, new Color32(188, 239, 127, 255));
            var level = JungleUi.Panel(_main, "Level", JungleUi.Cream, 548, 35, 618, 76);
            JungleUi.Text(level, "LevelTitle", "Nivel 1 · Exploro la selva", 35, 12, 8, 594, 60);
            var counter = JungleUi.Panel(_main, "Scene", JungleUi.Green, 1240, 35, 264, 76);
            JungleUi.Text(counter, "SceneCounter", "3 de 12", 42, 12, 8, 240, 60, Color.white);
            var pace = JungleUi.Panel(_main, "Pace", new Color32(91, 53, 20, 255), 28, Height - 92, 415, 60);
            JungleUi.Text(pace, "PaceLabel", "Aprende a tu ritmo", 29, 8, 8, 399, 44, Color.white);

            if (_activity.Stage == ActivityStage.Introduction) DrawIntroduction();
            else if (_activity.Stage == ActivityStage.Answers) DrawAnswers();
            else DrawSuccess();
            DrawSupports();
            if (GuacamayoProgress.IsCompleted)
            {
                var saved = JungleUi.Panel(_main, "SavedProgress", JungleUi.Cream, 30, Height - 163, 412, 52);
                JungleUi.Text(saved, "SavedLabel", "Acierto guardado", 27, 5, 5, 402, 42);
            }
            FocusFirst();
        }

        private void DrawIntroduction()
        {
            _context = "Instruccion";
            float panelHeight = Height - 335;
            var panel = JungleUi.Panel(_main, "Introduction", JungleUi.Cream, 555, 165, 943, panelHeight);
            var title = JungleUi.Panel(panel, "TitlePlaque", JungleUi.Green, 108, -22, 727, 73);
            JungleUi.Text(title, "TitleText", "El amigo de colores", 42, 16, 7, 695, 58, Color.white);
            JungleUi.Image(panel, "MacawGuide", LoadSprite("Guacamayo"), Color.white, 26, 74, 270, panelHeight - 104, true);
            JungleUi.Text(panel, "Instruction", GuacamayoContent.Introduction, 44, 310, 85, 594, panelHeight - 285);
            _mainButtons.Add(JungleUi.Button(panel, "ListenInstruction", "Escuchar", JungleUi.Blue,
                344, panelHeight - 192, 530, 74, () => PlayVoice("Instruccion"), 35));
            _mainButtons.Add(JungleUi.Button(panel, "ShowAnswers", "Ver respuestas  >", JungleUi.Gold,
                320, panelHeight - 102, 578, 80, ShowAnswers, 38));
        }

        public void ShowAnswers()
        {
            StopMedia();
            _activity.ShowAnswers();
            DrawMain();
        }

        private void DrawAnswers()
        {
            _context = "Respuestas";
            var panel = JungleUi.Panel(_main, "Question", JungleUi.Cream, 555, 164, 943, 142);
            JungleUi.Text(panel, "QuestionText", GuacamayoContent.Question, 42, 22, 8, 562, 126);
            _mainButtons.Add(JungleUi.Button(panel, "RepeatWord", "Repetir audio", JungleUi.Blue,
                594, 23, 325, 88, () => PlayVoice("Palabra"), 32));
            float cardHeight = Mathf.Min(355, Height - 525);
            AddAnimal(AnimalChoice.Mono, 555, cardHeight);
            AddAnimal(AnimalChoice.Guacamayo, 879, cardHeight);
            AddAnimal(AnimalChoice.Tortuga, 1203, cardHeight);
            var instruction = JungleUi.Panel(_main, "SelectPrompt", JungleUi.Cream, 770, 351 + cardHeight, 515, 54);
            JungleUi.Text(instruction, "SelectText", "Selecciona una imagen", 30, 8, 6, 499, 42);
        }

        private void AddAnimal(AnimalChoice animal, float x, float height)
        {
            string label = animal.ToString();
            var button = JungleUi.Button(_main, "Choose" + label, "", JungleUi.Cream,
                x, 325, 295, height, () => Choose(animal));
            JungleUi.Image(button.transform, "Animal", LoadSprite(label), Color.white, 18, 18, 259, height - 92, true);
            JungleUi.Text(button.transform, "AnimalName", label, 34, 10, height - 66, 275, 50);
            _mainButtons.Add(button);
        }

        public void Choose(AnimalChoice animal)
        {
            if (HasModal || _activity.Stage != ActivityStage.Answers) return;
            StopMedia();
            if (_activity.Choose(animal)) DrawMain();
            else ShowHint(true);
        }

        private void DrawSuccess()
        {
            float panelHeight = Height - 335;
            var panel = JungleUi.Panel(_main, "Success", JungleUi.Cream, 555, 165, 943, panelHeight);
            JungleUi.Image(panel, "FoundMacaw", LoadSprite("Guacamayo"), Color.white, 26, 55, 275, panelHeight - 83, true);
            bool finished = _activity.Stage == ActivityStage.Finished;
            _context = finished ? "Cierre" : "Acierto";
            JungleUi.Text(panel, "SuccessTitle", finished ? "¡Reto completado!" : "¡Excelente!", 54, 310, 44, 594, 90, JungleUi.Green);
            JungleUi.Text(panel, "SuccessText", finished ? "Tu acierto está guardado.\nPróximo reto: las plantas de la selva." : "Encontraste al guacamayo.",
                40, 310, 143, 594, panelHeight - 350);
            _mainButtons.Add(JungleUi.Button(panel, "ListenSuccess", "Escuchar", JungleUi.Blue,
                344, panelHeight - 192, 530, 74, () => PlayVoice(finished ? "Cierre" : "Acierto"), 35));
            _mainButtons.Add(JungleUi.Button(panel, finished ? "ReturnHome" : "Continue", finished ? "Volver al inicio" : "Continuar  >", JungleUi.Gold,
                320, panelHeight - 102, 578, 80,
                () => { if (finished) ReturnHome(); else _activity.Continue(); }, 38));
        }

        private void DrawSupports()
        {
            float y = Height - 126;
            _mainButtons.Add(JungleUi.Button(_main, "Help", "?  Ayuda", JungleUi.Gold, 888, y, 193, 94, () => ShowHint(false)));
            _mainButtons.Add(JungleUi.Button(_main, "Signs", "Señas\nLSEC", JungleUi.Purple, 1098, y, 193, 94, () => ShowSigns(_context), 28));
            _mainButtons.Add(JungleUi.Button(_main, "Reading", "Lectura", new Color32(228, 246, 255, 255), 1308, y, 190, 94, ShowReading));
            var audioPanel = JungleUi.Panel(_main, "AudioStatusPanel", JungleUi.Cream, 484, y + 5, 380, 82);
            _audioStatus = JungleUi.Text(audioPanel, "AudioStatus", "Escucha a tu ritmo", 27, 8, 8, 364, 66);
        }

        public void PlayVoice(string resource)
        {
            if (_signPlayer != null) _signPlayer.Pause();
            _voice.Stop();
            _voice.clip = Resources.Load<AudioClip>("HU03/Audio/" + resource);
            if (_voice.clip != null) _voice.Play();
            else Debug.LogError("Falta el audio local HU03/Audio/" + resource);
        }

        public void ShowHint(bool afterError)
        {
            string context = afterError ? "Error" : "Pista";
            var panel = BeginModal(afterError ? "Retry" : "Hint", context, 950, 620);
            JungleUi.Text(panel, "HintTitle", afterError ? "Inténtalo otra vez" : "Una pista", 51, 40, 32, 820, 76);
            JungleUi.Image(panel, "VisualHint", LoadSprite("Guacamayo"), Color.white, 34, 135, 230, 325, true);
            JungleUi.Text(panel, "HintText", GuacamayoContent.Hint, 39, 287, 123, 608, 244);
            _modalButtons.Add(JungleUi.Button(panel, "ListenHint", "Escuchar pista", JungleUi.Blue, 284, 389, 340, 76, () => PlayVoice(afterError ? "Error" : "Pista"), 30));
            _modalButtons.Add(JungleUi.Button(panel, "HintSigns", "Señas LSEC", JungleUi.Purple, 643, 389, 267, 76, () => ShowSigns(afterError ? "Error" : "Pista"), 29));
            _modalButtons.Add(JungleUi.Button(panel, "Retry", "¡Voy a intentarlo!", JungleUi.Gold, 260, 500, 565, 80, CloseModal, 38));
            FocusFirst();
        }

        private RectTransform BeginModal(string kind, string context, float width, float height)
        {
            bool replacing = _modal != null;
            if (!replacing) _returnFocus = EventSystem.current.currentSelectedGameObject;
            StopMedia();
            DestroyModal();
            _modalKind = kind;
            _modalContext = context;
            _mainGroup.interactable = false;
            _mainGroup.blocksRaycasts = false;
            _modalButtons.Clear();
            _modal = JungleUi.Rect("ModalOverlay", _root, 0, 0, 1536, Height);
            var dim = _modal.gameObject.AddComponent<Image>();
            dim.color = new Color32(0, 23, 24, 190);
            dim.raycastTarget = true;
            var panel = JungleUi.Panel(_modal, "ModalPanel", JungleUi.Cream,
                (1536 - width) / 2, (Height - height) / 2, width, height);
            _modalButtons.Add(JungleUi.Button(panel, "Close", "X", JungleUi.Gold,
                width - 82, 18, 60, 60, CloseModal, 31));
            return panel;
        }

        public void ShowReading()
        {
            string text = GuacamayoContent.SignsText(_context);
            var panel = BeginModal("Reading", _context, 1100, 744);
            JungleUi.Text(panel, "ReadingTitle", "Lectura", 49, 42, 25, 912, 72);
            var viewport = JungleUi.Rect("ReadingViewport", panel, 50, 127, 1000, 426);
            var background = viewport.gameObject.AddComponent<Image>();
            background.color = Color.white;
            viewport.gameObject.AddComponent<RectMask2D>();
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            var content = JungleUi.Rect("ReadingContent", viewport, 22, 12, 952, 420);
            var textLabel = JungleUi.Text(content, "LargeText", text, 38 * _readingScale, 0, 0, 940, 410,
                null, TextAlignmentOptions.TopLeft);
            float contentHeight = Mathf.Max(400, textLabel.GetPreferredValues(text, 940, Mathf.Infinity).y + 40);
            content.sizeDelta = new Vector2(952, contentHeight);
            textLabel.rectTransform.sizeDelta = new Vector2(940, contentHeight);
            scroll.content = content;
            scroll.viewport = viewport;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 45;
            _modalButtons.Add(JungleUi.Button(panel, "ReadingSize", _readingScale == 1 ? "Texto 200 %" : "Texto 100 %", JungleUi.Blue, 50, 582, 302, 68,
                () => { _readingScale = _readingScale == 1 ? 2 : 1; ShowReading(); }, 28));
            _modalButtons.Add(JungleUi.Button(panel, "ScrollReading", "Bajar texto", JungleUi.Blue, 370, 582, 297, 68,
                () => scroll.verticalNormalizedPosition = scroll.verticalNormalizedPosition > .05f ? Mathf.Max(0, scroll.verticalNormalizedPosition - .35f) : 1, 28));
            _modalButtons.Add(JungleUi.Button(panel, "ReadingSigns", "Señas LSEC", JungleUi.Purple, 687, 582, 360, 68, () => ShowSigns(_context), 29));
            _modalButtons.Add(JungleUi.Button(panel, "BackFromReading", "Volver a la actividad", JungleUi.Gold, 252, 664, 598, 62, CloseModal, 31));
            FocusFirst();
        }

        public void ShowSigns(string context, bool expanded = false)
        {
            float width = expanded ? 1450 : 1120;
            float height = expanded ? Height - 24 : 790;
            float videoHeight = Mathf.Min(641, height - 351);
            float videoWidth = videoHeight * 16 / 9;
            float controlsOffset = (width - 1120) / 2;
            var panel = BeginModal("Signs", context, width, height);
            JungleUi.Text(panel, "SignsTitle", "Lengua de señas ecuatoriana", 43, 38, 24, width - 140, 67);
            JungleUi.Text(panel, "SignsContext", context == "Pista" ? "Pista de esta actividad" : "Apoyo de esta actividad · LSEC", 27, 50, 93, 660, 44);
            _modalButtons.Add(JungleUi.Button(panel, "ExpandSigns", expanded ? "Reducir" : "Ampliar", JungleUi.Blue,
                width - 305, 85, 245, 60, () => ShowSigns(context, !expanded), 27));
            var videoArea = JungleUi.Panel(panel, "VideoArea", JungleUi.Purple, (width - videoWidth) / 2, 154, videoWidth, videoHeight);
            var video = Resources.Load<VideoClip>("HU03/LSEC/" + context);
            if (video == null)
            {
                JungleUi.Text(videoArea, "PendingVideo", GuacamayoContent.MissingSigns, 45, 30, videoHeight / 2 - 70, videoWidth - 60, 85);
                JungleUi.Text(videoArea, "PendingDetail", "El texto de apoyo está disponible abajo.", 29, 45, videoHeight / 2 + 25, videoWidth - 90, 86);
            }
            else
            {
                var display = JungleUi.Rect("SignVideo", videoArea, 4, 4, videoWidth - 8, videoHeight - 8).gameObject.AddComponent<RawImage>();
                display.raycastTarget = false;
                _signTexture = new RenderTexture(1280, 720, 0);
                display.texture = _signTexture;
                _signPlayer = videoArea.gameObject.AddComponent<VideoPlayer>();
                _signPlayer.playOnAwake = false;
                _signPlayer.audioOutputMode = VideoAudioOutputMode.None;
                _signPlayer.clip = video;
                _signPlayer.renderMode = VideoRenderMode.RenderTexture;
                _signPlayer.targetTexture = _signTexture;
                _signPlayer.aspectRatio = VideoAspectRatio.FitInside;
                _signPlayer.errorReceived += (player, message) =>
                {
                    JungleUi.Text(videoArea, "VideoError", "No se pudo abrir el video.\nPuedes usar el texto de apoyo.", 32, 30, 130, 720, 150);
                    Debug.LogWarning("No se pudo reproducir el video LSEC: " + message);
                };
            }
            JungleUi.Text(panel, "SignTranscript", GuacamayoContent.SignsText(context), 27, 42, height - 187, width - 84, 79);
            var play = JungleUi.Button(panel, "PlaySigns", "Reproducir / Pausa", JungleUi.Blue, controlsOffset + 42, height - 103, 370, 69,
                () => { _voice.Stop(); if (_signPlayer.isPlaying) _signPlayer.Pause(); else _signPlayer.Play(); }, 27);
            var repeat = JungleUi.Button(panel, "RepeatSigns", "Repetir", JungleUi.Purple, controlsOffset + 430, height - 103, 236, 69,
                () => { _voice.Stop(); _signPlayer.Stop(); _signPlayer.Play(); }, 28);
            play.interactable = repeat.interactable = video != null;
            _modalButtons.Add(play);
            _modalButtons.Add(repeat);
            _modalButtons.Add(JungleUi.Button(panel, "BackFromSigns", "Volver a la actividad", JungleUi.Gold, controlsOffset + 684, height - 103, 392, 69, CloseModal, 27));
            FocusFirst();
        }

        public void CloseModal()
        {
            StopMedia();
            DestroyModal();
            _modalKind = _modalContext = null;
            if (_mainGroup != null) { _mainGroup.interactable = true; _mainGroup.blocksRaycasts = true; }
            if (_returnFocus != null && _returnFocus.activeInHierarchy) EventSystem.current.SetSelectedGameObject(_returnFocus);
            else FocusFirst();
        }

        private void DestroyModal()
        {
            if (_modal != null) { _modal.gameObject.SetActive(false); Destroy(_modal.gameObject); _modal = null; }
            _signPlayer = null;
            if (_signTexture != null) { _signTexture.Release(); Destroy(_signTexture); _signTexture = null; }
            _modalButtons.Clear();
        }

        private void StopMedia()
        {
            if (_voice != null) _voice.Stop();
            if (_signPlayer != null) _signPlayer.Stop();
        }

        private void FocusFirst()
        {
            var buttons = HasModal ? _modalButtons : _mainButtons;
            foreach (var button in buttons)
            {
                if (button != null && button.IsInteractable())
                { EventSystem.current.SetSelectedGameObject(button.gameObject); return; }
            }
        }

        private void MoveFocus(int direction)
        {
            var buttons = HasModal ? _modalButtons : _mainButtons;
            var available = buttons.FindAll(button => button != null && button.IsInteractable() && button.gameObject.activeInHierarchy);
            if (available.Count == 0) return;
            int current = available.FindIndex(button => button.gameObject == EventSystem.current.currentSelectedGameObject);
            int next = (current + direction + available.Count) % available.Count;
            EventSystem.current.SetSelectedGameObject(available[next].gameObject);
        }

        private void ContinueToNextActivity()
        {
            StopMedia();
            _onContinue.Invoke();
            if (!string.IsNullOrEmpty(_nextSceneName) && Application.CanStreamedLevelBeLoaded(_nextSceneName))
                SceneManager.LoadScene(_nextSceneName);
            else DrawMain();
        }

        private void ReturnHome()
        {
            StopMedia();
            if (Application.CanStreamedLevelBeLoaded("BienvenidaNivel1")) SceneManager.LoadScene("BienvenidaNivel1");
            else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private static Sprite LoadSprite(string resource)
        {
            return Resources.Load<Sprite>("HU03/" + resource);
        }

        private void OnDestroy()
        {
            StopMedia();
            if (_signTexture != null) { _signTexture.Release(); Destroy(_signTexture); }
        }
    }
}
