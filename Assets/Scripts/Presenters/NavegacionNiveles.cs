using UnityEngine;
using UnityEngine.SceneManagement;

namespace GuardianesAmazonicos.Presenters
{
    /// <summary>
    /// Navegación común de la cabecera (Regresar, Avanzar, Inicio) y de los botones
    /// Comenzar/Siguiente. Usa el orden de Build Profiles para no depender de escenas
    /// que todavía no existen.
    /// </summary>
    public static class NavegacionNiveles
    {
        public static bool HayAnterior()
        {
            return SceneManager.GetActiveScene().buildIndex > 0;
        }

        public static bool HaySiguiente(string nombreEscena)
        {
            if (!string.IsNullOrWhiteSpace(nombreEscena) && Application.CanStreamedLevelBeLoaded(nombreEscena))
            {
                return true;
            }

            int siguiente = SceneManager.GetActiveScene().buildIndex + 1;
            return siguiente > 0 && siguiente < SceneManager.sceneCountInBuildSettings;
        }

        public static bool CargarSiguiente(string nombreEscena)
        {
            if (!string.IsNullOrWhiteSpace(nombreEscena) && Application.CanStreamedLevelBeLoaded(nombreEscena))
            {
                SceneManager.LoadScene(nombreEscena);
                return true;
            }

            int siguiente = SceneManager.GetActiveScene().buildIndex + 1;
            if (siguiente <= 0 || siguiente >= SceneManager.sceneCountInBuildSettings)
            {
                return false;
            }

            SceneManager.LoadScene(siguiente);
            return true;
        }

        public static bool CargarAnterior()
        {
            int anterior = SceneManager.GetActiveScene().buildIndex - 1;
            if (anterior < 0)
            {
                return false;
            }

            SceneManager.LoadScene(anterior);
            return true;
        }

        public static void CargarInicio()
        {
            if (SceneManager.sceneCountInBuildSettings > 0)
            {
                SceneManager.LoadScene(0);
            }
        }
    }
}
