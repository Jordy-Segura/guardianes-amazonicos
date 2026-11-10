using System;

namespace GuardianesAmazonicos.Models
{
    /// <summary>
    /// POCO de dominio que describe una opcion de respuesta de la actividad de plantas.
    /// </summary>
    [Serializable]
    public sealed class OpcionSelvaData
    {
        public string Palabra { get; }

        public bool EsCorrecta { get; }

        public string RetroalimentacionIncorrecta { get; }

        public OpcionSelvaData(string palabra, bool esCorrecta, string retroalimentacionIncorrecta)
        {
            Palabra = palabra ?? string.Empty;
            EsCorrecta = esCorrecta;
            RetroalimentacionIncorrecta = retroalimentacionIncorrecta ?? string.Empty;
        }
    }
}
