namespace GuardianesAmazonicos.Data
{
    public sealed class DatosProgreso
    {
        public bool AventuraIniciada { get; }
        public int RetosCompletados { get; }

        public DatosProgreso(bool aventuraIniciada, int retosCompletados)
        {
            AventuraIniciada = aventuraIniciada;
            RetosCompletados = retosCompletados;
        }
    }
}
