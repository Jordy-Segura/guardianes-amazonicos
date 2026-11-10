using System;

namespace GuardianesAmazonicos.Views
{
    /// <summary>
    /// Contrato de la vista pasiva de la actividad de aves (multimodal: texto, audio y LSEC).
    /// </summary>
    public interface IActividadAvesView
    {
        event Action<int> OnOpcionSeleccionada;
        event Action OnAceptarSolicitado;
        event Action OnSiguienteSolicitado;
        event Action OnLecturaSolicitada;
        event Action OnAyudaSolicitada;
        event Action OnCerrarAyudaSolicitado;
        event Action OnSenasSolicitadas;
        event Action OnCerrarSenasSolicitado;
        event Action OnReproducirSenasSolicitado;
        event Action OnRegresarSolicitado;
        event Action OnAvanzarSolicitado;
        event Action OnInicioSolicitado;
        event Action OnConfiguracionSolicitada;

        void MostrarEnunciado(string enunciado);

        void AsignarOpcion(int indice, string palabra);

        void MostrarProgreso(int puntoActual, int totalPuntos);

        void ResaltarOpcion(int indice);

        void MostrarResultadoCorrecto(int indice, string mensaje);

        void MostrarResultadoIncorrecto(int indice, string mensaje);

        void OcultarRetroalimentacion();

        void MostrarAviso(string mensaje);

        void ReproducirAudioPalabra();

        void MostrarPanelAyuda();

        void CerrarPanelAyuda();

        void MostrarPanelSenas();

        void CerrarPanelSenas();

        void ReproducirVideoSenas();
    }
}
