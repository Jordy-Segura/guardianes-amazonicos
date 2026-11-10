using System;

namespace GuardianesAmazonicos.Views
{
    public interface IBienvenidaView
    {
        event Action OnComenzarSolicitado;
        event Action OnAudioSolicitado;
        event Action OnSenasSolicitadas;
        event Action OnCerrarSenasSolicitado;
        event Action OnReproducirSenasSolicitado;

        void MostrarProgreso(int puntoActual, int totalPuntos, bool aventuraIniciada);
        void ReproducirBienvenida();
        void MostrarPanelSenas();
        void CerrarPanelSenas();
        void ReproducirVideoSenas();
    }
}
