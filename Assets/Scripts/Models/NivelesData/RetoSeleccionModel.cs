namespace GuardianesAmazonicos.Models
{
    /// <summary>
    /// Reglas de la actividad: Sin selección → Seleccionada → Aceptar → Correcta o Incorrecta.
    /// Un error no suma avance y permite reintentar sin penalización.
    /// </summary>
    public sealed class RetoSeleccionModel
    {
        public const int SIN_SELECCION = -1;

        private readonly int _indiceCorrecto;
        private readonly int _cantidadOpciones;

        public int IndiceSeleccionado { get; private set; } = SIN_SELECCION;
        public bool Resuelto { get; private set; }
        public bool HaySeleccion => IndiceSeleccionado != SIN_SELECCION;

        public RetoSeleccionModel(int indiceCorrecto, int cantidadOpciones)
        {
            _indiceCorrecto = indiceCorrecto;
            _cantidadOpciones = cantidadOpciones;
        }

        /// <summary>Guarda el índice elegido; se puede cambiar antes de confirmar.</summary>
        public bool Seleccionar(int indice)
        {
            if (Resuelto || indice < 0 || indice >= _cantidadOpciones)
            {
                return false;
            }

            IndiceSeleccionado = indice;
            return true;
        }

        /// <summary>Valida la selección al tocar «Aceptar».</summary>
        public bool Confirmar()
        {
            if (!HaySeleccion || Resuelto)
            {
                return Resuelto;
            }

            Resuelto = IndiceSeleccionado == _indiceCorrecto;
            return Resuelto;
        }

        public void Reintentar()
        {
            if (!Resuelto)
            {
                IndiceSeleccionado = SIN_SELECCION;
            }
        }
    }
}
