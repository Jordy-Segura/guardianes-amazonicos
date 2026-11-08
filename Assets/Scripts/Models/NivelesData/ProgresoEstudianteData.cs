using System;

namespace GuardianesAmazonicos.Models
{
    [Serializable]
    public sealed class ProgresoEstudianteData
    {
        public bool bienvenidaSelvaCompletada;
        public bool aventuraIniciada;
        public int puntoProgresoActual;
        public int totalPuntosProgreso = 12;
        public string fechaInicioUtc;
    }
}
