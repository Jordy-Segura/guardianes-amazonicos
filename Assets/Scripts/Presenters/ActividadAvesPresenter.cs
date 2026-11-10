using System;
using System.IO;
using GuardianesAmazonicos.Models;
using GuardianesAmazonicos.Persistence;
using GuardianesAmazonicos.Views;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GuardianesAmazonicos.Presenters
{
    /// <summary>
    /// HU-08 - Escena 8 del Nivel 1. Asignada a: Jirleidy Raquel Santos Muentes (Sprint 1).
    /// Orquesta la actividad de aves: suscribe la vista pasiva al modelo de dominio
    /// (patrón MVP + Observer), persiste el punto de progreso 8 de 12 y gestiona la
    /// navegación entre escenas del nivel.
    /// </summary>
    public sealed class ActividadAvesPresenter : MonoBehaviour
    {
        [Header("Vista de la actividad")]
        [SerializeField] private ActividadAvesView _vista;

        [Header("Navegación entre escenas")]
        [SerializeField] private string _nombreEscenaAnterior = "Nivel1_ConozcoLaSelva";
        [SerializeField] private string _nombreEscenaSiguiente = "Escena9";

        private IProgresoRepository _repositorio;
        private ActividadAvesModel _modelo;

        private void Awake()
        {
            if (_vista == null)
            {
                Debug.LogError("ActividadAvesPresenter: falta asignar la vista de la actividad de aves.", this);
                enabled = false;
                return;
            }

            _repositorio = new JsonProgresoRepository();
            ProgresoEstudianteData progreso;

            try
            {
                progreso = _repositorio.Cargar();
            }
            catch (IOException error)
            {
                Debug.LogWarning(
                    "ActividadAvesPresenter: no se pudo cargar el progreso ("
                    + error.Message + "). Se inicia un progreso nuevo.", this);
                progreso = new ProgresoEstudianteData();
            }

            _modelo = new ActividadAvesModel(progreso);
        }

        private void OnEnable()
        {
            _vista.OnOpcionSeleccionada += ManejarOpcionSeleccionada;
            _vista.OnAceptarSolicitado += ManejarAceptarSolicitado;
            _vista.OnSiguienteSolicitado += ManejarSiguienteSolicitado;
            _vista.OnLecturaSolicitada += ManejarLecturaSolicitada;
            _vista.OnAyudaSolicitada += ManejarAyudaSolicitada;
            _vista.OnCerrarAyudaSolicitado += ManejarCerrarAyudaSolicitado;
            _vista.OnSenasSolicitadas += ManejarSenasSolicitadas;
            _vista.OnCerrarSenasSolicitado += ManejarCerrarSenasSolicitado;
            _vista.OnReproducirSenasSolicitado += ManejarReproducirSenasSolicitado;
            _vista.OnRegresarSolicitado += ManejarRegresarSolicitado;
            _vista.OnAvanzarSolicitado += ManejarAvanzarSolicitado;
            _vista.OnInicioSolicitado += ManejarInicioSolicitado;
            _vista.OnConfiguracionSolicitada += ManejarConfiguracionSolicitada;

            _modelo.OnRespuestaCorrecta += ManejarRespuestaCorrecta;
            _modelo.OnRespuestaIncorrecta += ManejarRespuestaIncorrecta;

            InicializarVista();
        }

        private void OnDisable()
        {
            _vista.OnOpcionSeleccionada -= ManejarOpcionSeleccionada;
            _vista.OnAceptarSolicitado -= ManejarAceptarSolicitado;
            _vista.OnSiguienteSolicitado -= ManejarSiguienteSolicitado;
            _vista.OnLecturaSolicitada -= ManejarLecturaSolicitada;
            _vista.OnAyudaSolicitada -= ManejarAyudaSolicitada;
            _vista.OnCerrarAyudaSolicitado -= ManejarCerrarAyudaSolicitado;
            _vista.OnSenasSolicitadas -= ManejarSenasSolicitadas;
            _vista.OnCerrarSenasSolicitado -= ManejarCerrarSenasSolicitado;
            _vista.OnReproducirSenasSolicitado -= ManejarReproducirSenasSolicitado;
            _vista.OnRegresarSolicitado -= ManejarRegresarSolicitado;
            _vista.OnAvanzarSolicitado -= ManejarAvanzarSolicitado;
            _vista.OnInicioSolicitado -= ManejarInicioSolicitado;
            _vista.OnConfiguracionSolicitada -= ManejarConfiguracionSolicitada;

            _modelo.OnRespuestaCorrecta -= ManejarRespuestaCorrecta;
            _modelo.OnRespuestaIncorrecta -= ManejarRespuestaIncorrecta;
        }

        private void InicializarVista()
        {
            _vista.MostrarEnunciado(_modelo.Enunciado);

            for (int i = 0; i < _modelo.Opciones.Length; i++)
            {
                _vista.AsignarOpcion(i, _modelo.Opciones[i].Palabra);
            }

            _vista.OcultarRetroalimentacion();
            _vista.MostrarProgreso(_modelo.PuntoProgresoVisible, ActividadAvesModel.TotalPuntosProgreso);
        }

        private void ManejarOpcionSeleccionada(int indice)
        {
            _modelo.SeleccionarOpcion(indice);
        }

        private void ManejarAceptarSolicitado()
        {
            if (!_modelo.VerificarRespuestaSeleccionada())
            {
                return;
            }

            try
            {
                _repositorio.Guardar(_modelo.Progreso);
            }
            catch (IOException error)
            {
                Debug.LogWarning(
                    "ActividadAvesPresenter: no se pudo guardar el progreso ("
                    + error.Message + ").", this);
            }

            _vista.MostrarProgreso(_modelo.PuntoProgresoVisible, ActividadAvesModel.TotalPuntosProgreso);
        }

        private void ManejarRespuestaCorrecta(int indice, string mensaje)
        {
            _vista.MostrarResultadoCorrecto(indice, mensaje);
        }

        private void ManejarRespuestaIncorrecta(int indice, string mensaje)
        {
            _vista.MostrarResultadoIncorrecto(indice, mensaje);
        }

        private void ManejarSiguienteSolicitado()
        {
            CargarEscena(_nombreEscenaSiguiente);
        }

        private void ManejarRegresarSolicitado()
        {
            CargarEscena(_nombreEscenaAnterior);
        }

        private void ManejarAvanzarSolicitado()
        {
            CargarEscena(_nombreEscenaSiguiente);
        }

        private void ManejarLecturaSolicitada()
        {
            _vista.ReproducirAudioPalabra();
        }

        private void ManejarAyudaSolicitada()
        {
            _vista.MostrarPanelAyuda();
        }

        private void ManejarCerrarAyudaSolicitado()
        {
            _vista.CerrarPanelAyuda();
        }

        private void ManejarSenasSolicitadas()
        {
            _vista.MostrarPanelSenas();
        }

        private void ManejarCerrarSenasSolicitado()
        {
            _vista.CerrarPanelSenas();
        }

        private void ManejarReproducirSenasSolicitado()
        {
            _vista.ReproducirVideoSenas();
        }

        private void ManejarInicioSolicitado()
        {
            _vista.MostrarAviso("La pantalla de inicio se integrará en una escena futura del juego.");
        }

        private void ManejarConfiguracionSolicitada()
        {
            _vista.MostrarAviso("Los ajustes del juego se integrarán en una escena futura.");
        }

        private void CargarEscena(string nombreEscena)
        {
            if (string.IsNullOrEmpty(nombreEscena))
            {
                _vista.MostrarAviso("Esta escena aún no forma parte del juego.");
                return;
            }

            if (!ExisteEscenaEnBuild(nombreEscena))
            {
                _vista.MostrarAviso("La escena \"" + nombreEscena + "\" aún no está construida. ¡Sigue así!");
                return;
            }

            SceneManager.LoadScene(nombreEscena);
        }

        private static bool ExisteEscenaEnBuild(string nombreEscena)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string ruta = SceneUtility.GetScenePathByBuildIndex(i);
                if (string.IsNullOrEmpty(ruta))
                {
                    continue;
                }

                string nombre = Path.GetFileNameWithoutExtension(ruta);
                if (string.Equals(nombre, nombreEscena, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
