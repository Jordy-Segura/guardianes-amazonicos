using System;

namespace GuardianesAmazonicos.Models
{
    /// <summary>
    /// HU-08 - Escena 8 del Nivel 1. Asignada a: Jirleidy Raquel Santos Muentes (Sprint 1).
    /// "Como estudiante, quiero reconocer un guacamayo entre varias imagenes,
    /// para asociar la palabra con un elemento de la selva."
    /// Modelo de dominio (POCO observador): valida la respuesta, registra intentos
    /// sin penalizacion (RNF-04) y actualiza el punto de progreso 8 de 12.
    /// </summary>
    public sealed class ActividadAvesModel
    {
        public const int TotalPuntosProgreso = 12;

        private const int PuntoProgresoEscena = 8;

        public event Action<int> OnOpcionSeleccionada;
        public event Action<int, string> OnRespuestaCorrecta;
        public event Action<int, string> OnRespuestaIncorrecta;

        private readonly ProgresoEstudianteData _progreso;
        private readonly OpcionSelvaData[] _opciones;

        public ActividadAvesModel(ProgresoEstudianteData progreso)
        {
            _progreso = progreso ?? new ProgresoEstudianteData();
            _progreso.totalPuntosProgreso = TotalPuntosProgreso;
            _progreso.puntoProgresoActual = Math.Max(0, Math.Min(_progreso.puntoProgresoActual, TotalPuntosProgreso));

            PalabraObjetivo = "GUACAMAYO";
            Enunciado = "El guardián trae un reto: toca el GUACAMAYO de la selva y luego toca ACEPTAR.";
            _opciones = new[]
            {
                new OpcionSelvaData(
                    "GUACAMAYO",
                    true,
                    string.Empty),
                new OpcionSelvaData(
                    "TUCÁN",
                    false,
                    "Casi: ese es el TUCÁN, un ave de pico grande y colorido. El GUACAMAYO tiene plumas rojas, azules y amarillas. Inténtalo otra vez sin miedo."),
                new OpcionSelvaData(
                    "LORO",
                    false,
                    "Casi: ese es el LORO, un ave verde que imita palabras con su pico. El GUACAMAYO tiene plumas rojas, azules y amarillas. Inténtalo otra vez sin miedo.")
            };
            IndiceSeleccionado = -1;
        }

        public string Enunciado { get; }

        public string PalabraObjetivo { get; }

        public int IndiceSeleccionado { get; private set; }

        public bool RespondidaCorrectamente { get; private set; }

        public int Intentos { get; private set; }

        public ProgresoEstudianteData Progreso
        {
            get { return _progreso; }
        }

        public OpcionSelvaData[] Opciones
        {
            get { return _opciones; }
        }

        public bool HaySeleccion
        {
            get { return IndiceSeleccionado >= 0 && IndiceSeleccionado < _opciones.Length; }
        }

        public int PuntoProgresoVisible
        {
            get { return Math.Max(_progreso.puntoProgresoActual, PuntoProgresoEscena); }
        }

        public void SeleccionarOpcion(int indice)
        {
            if (RespondidaCorrectamente)
            {
                return;
            }

            if (indice < 0 || indice >= _opciones.Length)
            {
                return;
            }

            IndiceSeleccionado = indice;
            OnOpcionSeleccionada?.Invoke(indice);
        }

        public bool VerificarRespuestaSeleccionada()
        {
            if (RespondidaCorrectamente)
            {
                return true;
            }

            if (!HaySeleccion)
            {
                return false;
            }

            Intentos++;

            OpcionSelvaData opcion = _opciones[IndiceSeleccionado];
            if (!opcion.EsCorrecta)
            {
                OnRespuestaIncorrecta?.Invoke(IndiceSeleccionado, opcion.RetroalimentacionIncorrecta);
                return false;
            }

            RespondidaCorrectamente = true;
            _progreso.totalPuntosProgreso = TotalPuntosProgreso;
            _progreso.puntoProgresoActual = Math.Max(_progreso.puntoProgresoActual, PuntoProgresoEscena);
            OnRespuestaCorrecta?.Invoke(
                IndiceSeleccionado,
                "¡MUY BIEN! El GUACAMAYO es un ave de la selva con plumas rojas, azules y amarillas, y un pico curvo y fuerte.");
            return true;
        }
    }
}
