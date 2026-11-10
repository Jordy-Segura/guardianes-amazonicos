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
        event Action OnRegresarSolicitado;
        event Action OnAvanzarSolicitado;
        event Action OnInicioSolicitado;
        event Action OnConfiguracionSolicitada;

        void MostrarProgreso(int escenaActual, int totalEscenas);
        void PermitirNavegacion(bool regresar, bool avanzar);
        void ReproducirBienvenida();
        void MostrarPanelSenas();
        void CerrarPanelSenas();
        void ReproducirVideoSenas();
    }
}
