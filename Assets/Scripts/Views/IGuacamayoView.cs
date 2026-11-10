using System;

namespace GuardianesAmazonicos.Views
{
    public interface IGuacamayoView
    {
        event Action<int> OnOpcionSeleccionada;
        event Action OnAceptarSolicitado;
        event Action OnAudioSolicitado;
        event Action OnReintentarSolicitado;
        event Action OnSiguienteSolicitado;
        event Action OnAyudaSolicitada;
        event Action OnSenasSolicitadas;
        event Action OnInicioSolicitado;
        event Action OnRegresarSolicitado;
        event Action OnAvanzarSolicitado;
        event Action OnConfiguracionSolicitada;
        void MostrarSeleccion(int indice);
        void MostrarResultado(bool correcto);
        void ReiniciarSeleccion();
        void ReproducirAudio();
        void MostrarAyuda();
        void MostrarSenas();
        void MostrarAviso(string mensaje);
        bool MovimientoReducido { get; }
        void MostrarConfiguracion();
    }
}
