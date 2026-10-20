using UnityEngine;

namespace GuardianesAmazonicos.Data
{
    public static class GuardadoProgreso
    {
        private const string ClaveInicio = "guardianes.nivel1.iniciado";
        private const string ClaveProgreso = "guardianes.nivel1.progreso";

        public static DatosProgreso Cargar(int totalRetos)
        {
            var progreso = Mathf.Clamp(PlayerPrefs.GetInt(ClaveProgreso, 0), 0, totalRetos);
            var iniciada = PlayerPrefs.GetInt(ClaveInicio, 0) == 1;
            return new DatosProgreso(iniciada, progreso);
        }

        public static void RegistrarInicio(int primerReto)
        {
            PlayerPrefs.SetInt(ClaveInicio, 1);
            PlayerPrefs.SetInt(ClaveProgreso, primerReto);
            PlayerPrefs.Save();
        }

        public static void RegistrarRetoCompleto(int retosCompletados)
        {
            var progresoActual = Mathf.Max(PlayerPrefs.GetInt(ClaveProgreso, 0), retosCompletados);
            PlayerPrefs.SetInt(ClaveProgreso, progresoActual);
            PlayerPrefs.Save();
        }

        public static void Reiniciar()
        {
            PlayerPrefs.DeleteKey(ClaveInicio);
            PlayerPrefs.DeleteKey(ClaveProgreso);
            PlayerPrefs.Save();
        }
    }
}
