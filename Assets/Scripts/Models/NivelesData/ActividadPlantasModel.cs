using System;

namespace GuardianesAmazonicos.Models
{
    /// <summary>
    /// HU-04 - Escena 4 del Nivel 1.
    /// "Como estudiante, quiero reconocer una planta entre varias imagenes,
    /// para asociar la palabra con un elemento de la selva."
    /// Modelo de dominio (POCO observador): valida la respuesta, registra intentos
    /// sin penalizacion (RNF-04) y actualiza el punto de progreso 4 de 12.
    /// </summary>
    public sealed class ActividadPlantasModel
    {
        public const int TotalPuntosProgreso = 12;

        private const int PuntoProgresoEscena = 4;

        public event Action<int> OnOpcionSeleccionada;
        public event Action<int, string> OnRespuestaCorrecta;
        public event Action<int, string> OnRespuestaIncorrecta;

        private readonly ProgresoEstudianteData _progreso;
        private readonly OpcionSelvaData[] _opciones;

        public ActividadPlantasModel(ProgresoEstudianteData progreso)
        {
            _progreso = progreso ?? new ProgresoEstudianteData();
            Enunciado = "¿Cuál de estas imágenes representa una planta?";
            _progreso.totalPuntosProgreso = TotalPuntosProgreso;
            _progreso.puntoProgresoActual = Math.Max(0, Math.Min(_progreso.puntoProgresoActual, TotalPuntosProgreso));

            PalabraObjetivo = "PLANTA";
            _opciones = new[]
            {
                new OpcionSelvaData(
                    "PLANTA",
                    true,
                    string.Empty),
                new OpcionSelvaData(
                    "JAGUAR",
                    false,
                    "Casi: ese es el JAGUAR, un animal que camina y ruge. La PLANTA se queda quieta y le crecen hojas. Inténtalo otra vez sin miedo."),
                new OpcionSelvaData(
                    "MONO",
                    false,
                    "Casi: ese es el MONO, un animal que salta entre los árboles. La PLANTA no salta: tiene hojas y raíces. Inténtalo otra vez sin miedo.")
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
                "¡MUY BIEN! La PLANTA es un ser vivo de la selva: le crecen hojas, tallo y raíces.");
            return true;
        }
    }
}
