#if UNITY_EDITOR || HU03_ACCEPTANCE
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GuardianesAmazonicos.Nivel1.HU03
{
    // Only development builds expose this acceptance run; the release has no test automation.
    public sealed class GuacamayoSmokeRunner : MonoBehaviour
    {
        private readonly List<string> _results = new List<string>();
        private readonly Dictionary<string, int?> _backup = new Dictionary<string, int?>();
        private string _output;
        private bool _failed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartIfRequested()
        {
            if (Environment.GetCommandLineArgs().Contains("-hu03-smoke"))
                new GameObject("HU03AcceptanceRunner", typeof(GuacamayoSmokeRunner));
        }

        private IEnumerator Start()
        {
            _output = Path.Combine(Application.dataPath, "..", "Validation");
            Application.runInBackground = true;
            Directory.CreateDirectory(_output);
            Application.logMessageReceived += ObserveLog;
            foreach (string key in new[] { GuacamayoProgress.CompletedKey, GuacamayoProgress.ProgressKey })
                _backup[key] = PlayerPrefs.HasKey(key) ? (int?)PlayerPrefs.GetInt(key) : null;
            PlayerPrefs.DeleteKey(GuacamayoProgress.CompletedKey);
            PlayerPrefs.SetInt(GuacamayoProgress.ProgressKey, 2);
            yield return null;
            yield return null;
            while (!UnityEngine.Rendering.SplashScreen.isFinished) yield return null;
            yield return new WaitForSecondsRealtime(1);
            var screen = FindAnyObjectByType<GuacamayoScreen>();
            Check(screen != null, "Escena HU-03 carga");
            if (screen == null) { Finish(); yield break; }
            Screen.SetResolution(1536, 1024, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(.5f);
            foreach (var resolution in new[] { new Vector2Int(1366, 768), new Vector2Int(1920, 1080), new Vector2Int(1536, 1024) })
            {
                Screen.SetResolution(resolution.x, resolution.y, FullScreenMode.Windowed);
                for (int frame = 0; frame < 12; frame++) yield return null;
                CheckLayout("intro-" + resolution.x);
                yield return Capture("intro-" + resolution.x);
            }
            Click("ListenInstruction");
            Check(screen.HasVoice, "Instrucción tiene AudioClip local");
            Click("ShowAnswers");
            yield return null;
            Check(screen.Activity.Stage == ActivityStage.Answers, "Ver respuestas muestra opciones");
            Click("RepeatWord");
            var voice = screen.GetComponent<AudioSource>();
            Check(voice.clip != null && voice.clip.name == "Palabra", "Repetición usa la palabra guacamayo");
            Click("RepeatWord");
            Check(voice.time < .15f && FindObjectsByType<AudioSource>().Count(audio => audio.isPlaying) <= 1,
                "Repetición reinicia sin voces superpuestas");
            CheckLayout("respuestas");
            yield return Capture("respuestas");
            Click("ChooseMono");
            yield return null;
            Check(screen.HasModal && screen.Activity.Stage == ActivityStage.Answers, "Error abre pista y conserva el reto");
            Check(!GuacamayoProgress.IsCompleted, "Error no guarda acierto");
            CheckLayout("pista");
            yield return Capture("pista");
            screen.Choose(AnimalChoice.Guacamayo);
            Check(screen.Activity.Stage == ActivityStage.Answers, "Modal impide responder a la actividad del fondo");
            Click("Retry");
            yield return null;
            Check(!screen.HasModal && EventSystem.current.currentSelectedGameObject.name == "ChooseMono", "Reintento devuelve el foco");
            Click("ChooseTortuga");
            yield return null;
            Click("Retry");
            yield return null;
            Click("Signs");
            yield return null;
            var play = GameObject.Find("PlaySigns").GetComponent<Button>();
            Check(!play.interactable && GameObject.Find("PendingVideo") != null, "LSEC sin video muestra pendiente y bloquea reproducción");
            CheckLayout("LSEC");
            yield return Capture("lsec-pendiente");
            Click("ExpandSigns");
            yield return null;
            CheckLayout("LSEC-ampliado");
            yield return Capture("lsec-ampliado");
            Click("BackFromSigns");
            yield return null;
            Click("Reading");
            yield return null;
            Click("ReadingSize");
            yield return null;
            Check(GameObject.Find("LargeText").GetComponent<TMP_Text>().fontSize == 76, "Lectura se amplía al 200 por ciento");
            yield return Capture("lectura-200");
            Click("BackFromReading");
            yield return null;
            Click("ChooseGuacamayo");
            yield return null;
            Check(screen.Activity.CanContinue && GuacamayoProgress.IsCompleted, "Acierto felicita, guarda y habilita continuar");
            Check(PlayerPrefs.GetInt(GuacamayoProgress.ProgressKey) == 3, "Progreso previo 2 avanza a 3");
            screen.Choose(AnimalChoice.Guacamayo);
            Check(PlayerPrefs.GetInt(GuacamayoProgress.ProgressKey) == 3, "Doble selección no suma progreso");
            CheckLayout("acierto");
            yield return Capture("acierto");
            Click("Continue");
            yield return null;
            Check(screen.Activity.Stage == ActivityStage.Finished, "Continuar llega al cierre cuando HU-04 aún no está integrada");
            yield return Capture("cierre");
            PlayerPrefs.Save();
            Check(GuacamayoProgress.IsCompleted, "Lectura de persistencia conserva acierto");
            Finish();
        }

        private void Click(string name)
        {
            var target = GameObject.Find(name);
            var button = target == null ? null : target.GetComponent<Button>();
            Check(button != null && button.IsInteractable(), "Control disponible: " + name);
            if (button == null || !button.IsInteractable()) return;
            EventSystem.current.SetSelectedGameObject(button.gameObject);
            button.onClick.Invoke();
        }

        private void CheckLayout(string context)
        {
            Canvas.ForceUpdateCanvases();
            foreach (var text in FindObjectsByType<TextMeshProUGUI>())
            {
                if (text.name == "LargeText") continue;
                text.ForceMeshUpdate();
                Check(!text.isTextOverflowing, context + ": texto completo " + text.transform.parent.name + "/" + text.name);
            }
            foreach (var button in FindObjectsByType<Button>())
            {
                if (!button.IsInteractable()) continue;
                var corners = new Vector3[4];
                button.GetComponent<RectTransform>().GetWorldCorners(corners);
                Check(corners[0].x >= -1 && corners[0].y >= -1 && corners[2].x <= Screen.width + 1 && corners[2].y <= Screen.height + 1,
                    context + ": control visible " + button.name);
                Check(corners[2].x - corners[0].x >= 48 && corners[2].y - corners[0].y >= 48,
                    context + ": tamaño mínimo " + button.name);
            }
        }

        private IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(_output, name + ".png"));
            yield return null;
            yield return null;
        }

        private void Check(bool condition, string label)
        {
            _results.Add((condition ? "PASS " : "FAIL ") + label);
            if (!condition) _failed = true;
        }

        private void ObserveLog(string message, string trace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            { _failed = true; _results.Add("ERROR " + message + "\n" + trace); }
        }

        private void Finish()
        {
            RestoreProgress();
            File.WriteAllLines(Path.Combine(_output, "HU03-runtime.txt"), _results);
            Application.logMessageReceived -= ObserveLog;
            Application.Quit(_failed ? 1 : 0);
        }

        private void RestoreProgress()
        {
            foreach (var pair in _backup)
            {
                if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value);
                else PlayerPrefs.DeleteKey(pair.Key);
            }
            PlayerPrefs.Save();
            _backup.Clear();
        }

        private void OnDestroy()
        {
            RestoreProgress();
            Application.logMessageReceived -= ObserveLog;
        }
    }
}
#endif
