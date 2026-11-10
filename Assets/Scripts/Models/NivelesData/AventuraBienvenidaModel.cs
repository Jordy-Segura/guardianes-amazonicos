using System;

namespace GuardianesAmazonicos.Models
{
    public sealed class AventuraBienvenidaModel
    {
        public const int TotalPuntosProgreso = 12;

        private readonly ProgresoEstudianteData _progreso;

        public event Action<int, int> OnProgresoActualizado;

        public ProgresoEstudianteData Progreso
        {
            get { return _progreso; }
        }

        public bool AventuraIniciada
        {
            get { return _progreso.aventuraIniciada; }
        }

        public int PuntoProgresoActual
        {
            get { return _progreso.puntoProgresoActual; }
        }

        public AventuraBienvenidaModel(ProgresoEstudianteData progreso)
        {
            _progreso = progreso ?? new ProgresoEstudianteData();
            _progreso.totalPuntosProgreso = TotalPuntosProgreso;
            _progreso.puntoProgresoActual = Math.Max(0, Math.Min(_progreso.puntoProgresoActual, TotalPuntosProgreso));
        }

        public void IniciarAventura()
        {
            if (_progreso.aventuraIniciada)
            {
                return;
            }

            _progreso.bienvenidaSelvaCompletada = true;
            _progreso.aventuraIniciada = true;
            _progreso.puntoProgresoActual = 1;
            _progreso.totalPuntosProgreso = TotalPuntosProgreso;
            _progreso.fechaInicioUtc = DateTime.UtcNow.ToString("O");
        }

        public void NotificarProgresoActualizado()
        {
            OnProgresoActualizado?.Invoke(_progreso.puntoProgresoActual, _progreso.totalPuntosProgreso);
        }
    }
}
