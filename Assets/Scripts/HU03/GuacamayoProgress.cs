using UnityEngine;

namespace GuardianesAmazonicos.Nivel1.HU03
{
    public static class GuacamayoProgress
    {
        public const string CompletedKey = "guardianes.nivel1.hu03.completada";
        public const string ProgressKey = "guardianes.nivel1.progreso";
        public static bool IsCompleted => PlayerPrefs.GetInt(CompletedKey, 0) == 1;

        // Preserve the shared counter. A standalone HU-03 run must not complete HU-01/HU-02.
        public static int AdvanceContiguousProgress(int current)
        {
            return current == 2 ? 3 : current;
        }

        public static void SaveCompletion()
        {
            PlayerPrefs.SetInt(CompletedKey, 1);
            PlayerPrefs.SetInt(ProgressKey,
                AdvanceContiguousProgress(PlayerPrefs.GetInt(ProgressKey, 0)));
            PlayerPrefs.Save();
        }
    }
}
