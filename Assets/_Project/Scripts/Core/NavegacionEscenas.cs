using UnityEngine.SceneManagement;

namespace GuardianesAmazonicos.Core
{
    public static class NavegacionEscenas
    {
        public static void Cargar(string nombreEscena)
        {
            if (!string.IsNullOrWhiteSpace(nombreEscena))
                SceneManager.LoadScene(nombreEscena);
        }
    }
}
