using System;

namespace GuardianesAmazonicos.Models
{
    /// <summary>
    /// "Como estudiante, quiero reconocer una tortuga entre varias imagenes,
    /// para asociar la palabra con un elemento de la selva."
    /// Modelo de dominio (POCO observador): valida la respuesta, registra intentos
    /// sin penalizacion (RNF-04) y completa el punto de progreso 12 de 12.
    /// </summary>
    public sealed class ActividadTortugasModel
    {
        public const int TotalPuntosProgreso = 12;

        private const int PuntoProgresoEscena = 12;

        public event Action<int> OnOpcionSeleccionada;
        public event Action<int, string> OnRespuestaCorrecta;
        public event Action<int, string> OnRespuestaIncorrecta;

        private readonly ProgresoEstudianteData _progreso;
        private readonly OpcionSelvaData[] _opciones;

        public ActividadTortugasModel(ProgresoEstudianteData progreso)
        {
            _progreso = progreso ?? new ProgresoEstudianteData();
            _progreso.totalPuntosProgreso = TotalPuntosProgreso;
            _progreso.puntoProgresoActual = Math.Max(0, Math.Min(_progreso.puntoProgresoActual, TotalPuntosProgreso));

            PalabraObjetivo = "TORTUGA";
            Enunciado = "El guardián trae un reto: toca la TORTUGA de la selva y luego toca ACEPTAR.";
            _opciones = new[]
            {
                new OpcionSelvaData(
                    "TORTUGA",
                    true,
                    string.Empty),
                new OpcionSelvaData(
                    "RÍO",
                    false,
                    "Casi: ese es el RÍO, agua que corre entre las plantas de la selva. La TORTUGA es un animal con caparazón que camina despacito. Inténtalo otra vez sin miedo."),
                new OpcionSelvaData(
                    "ÁRBOL",
                    false,
                    "Casi: ese es el ÁRBOL, una planta grande con tronco y hojas. La TORTUGA es un animal con caparazón que camina despacito. Inténtalo otra vez sin miedo.")
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
                "¡MUY BIEN! La TORTUGA es un animal de la selva con caparazón duro, patas cortas y camina despacito.");
            return true;
        }
    }
}
