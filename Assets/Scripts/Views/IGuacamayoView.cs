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
        void MostrarSeleccion(int indice);
        void MostrarResultado(bool correcto);
        void ReiniciarSeleccion();
        void ReproducirAudio();
        void MostrarAyuda();
        void MostrarSenas();
        void MostrarAviso(string mensaje);
    }
}
