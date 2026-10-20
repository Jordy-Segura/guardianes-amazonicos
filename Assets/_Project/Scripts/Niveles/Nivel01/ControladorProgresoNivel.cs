using GuardianesAmazonicos.Data;
using UnityEngine;

namespace GuardianesAmazonicos.Niveles.Nivel01
{
    public sealed class ControladorProgresoNivel : MonoBehaviour
    {
        public DatosProgreso ProgresoActual { get; private set; }

        public DatosProgreso Cargar(int totalRetos)
        {
            ProgresoActual = GuardadoProgreso.Cargar(totalRetos);
            return ProgresoActual;
        }

        public void Iniciar(int totalRetos)
        {
            GuardadoProgreso.RegistrarInicio(1);
            ProgresoActual = GuardadoProgreso.Cargar(totalRetos);
        }
    }
}
