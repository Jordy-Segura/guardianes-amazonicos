using UnityEngine;

namespace GuardianesAmazonicos.Nivel1.HU02
{
    public static class RetoMonoProgress
    {
        public const string CompletedKey = "guardianes.nivel1.hu02.completada";
        public const string ProgressKey = "guardianes.nivel1.progreso";
        public static bool IsCompleted => PlayerPrefs.GetInt(CompletedKey, 0) == 1;

        // Preserve the shared counter. A standalone HU-02 run must not complete HU-01.
        public static int AdvanceContiguousProgress(int current)
        {
            return current == 1 ? 2 : current;
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
