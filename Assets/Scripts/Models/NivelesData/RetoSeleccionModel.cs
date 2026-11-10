namespace GuardianesAmazonicos.Models
{
    public sealed class RetoSeleccionModel
    {
        private readonly int _indiceCorrecto;

        public bool Resuelto { get; private set; }

        public RetoSeleccionModel(int indiceCorrecto)
        {
            _indiceCorrecto = indiceCorrecto;
        }

        public bool Evaluar(int indiceSeleccionado)
        {
            Resuelto = indiceSeleccionado == _indiceCorrecto;
            return Resuelto;
        }
    }
}
