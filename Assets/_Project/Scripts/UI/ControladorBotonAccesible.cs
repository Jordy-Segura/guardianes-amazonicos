using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace GuardianesAmazonicos.UI
{
    public static class ControladorBotonAccesible
    {
        public static void Configurar(Button boton, string nombreAccesible, UnityAction accion)
        {
            if (boton == null)
                return;

            if (!string.IsNullOrWhiteSpace(nombreAccesible))
                boton.gameObject.name = nombreAccesible;

            var navegacion = boton.navigation;
            navegacion.mode = Navigation.Mode.Automatic;
            boton.navigation = navegacion;
            boton.onClick.RemoveAllListeners();
            if (accion != null)
                boton.onClick.AddListener(accion);

            var etiqueta = boton.GetComponentInChildren<TMP_Text>();
            if (etiqueta != null)
                etiqueta.raycastTarget = false;
        }
    }
}
