using System;
using System.IO;
using GuardianesAmazonicos.Models;
using GuardianesAmazonicos.Persistence;
using GuardianesAmazonicos.Views;
using UnityEngine;

namespace GuardianesAmazonicos.Presenters
{
    /// <summary>
    /// Bienvenida del nivel: al pulsar «¡Comenzar!» registra el inicio de la aventura
    /// y abre la primera actividad del nivel.
    /// </summary>
    public sealed class BienvenidaNivelPresenter : MonoBehaviour
    {
        private const int ESCENA_BIENVENIDA = 1;

        [SerializeField] private BienvenidaNivelView _vista;
        [SerializeField, Min(1)] private int _numeroEscena = ESCENA_BIENVENIDA;
        [Tooltip("Vacío: se carga la siguiente escena de Build Profiles.")]
        [SerializeField] private string _siguienteEscena;

        private IProgresoRepository _repositorio;
        private AventuraBienvenidaModel _modelo;

        private void Awake()
        {
            if (_vista == null)
            {
                Debug.LogError("BienvenidaNivelPresenter: falta asignar la vista de bienvenida.", this);
                enabled = false;
                return;
            }

            _repositorio = new JsonProgresoRepository();
            ProgresoEstudianteData progreso;

            try
            {
                progreso = _repositorio.Cargar();
            }
            catch (IOException)
            {
                progreso = new ProgresoEstudianteData();
            }
            catch (UnauthorizedAccessException)
            {
                progreso = new ProgresoEstudianteData();
            }

            _modelo = new AventuraBienvenidaModel(progreso);
        }

        private void OnEnable()
        {
            if (_vista == null || _modelo == null) return;

            _vista.OnComenzarSolicitado += IniciarAventura;
            _vista.OnAudioSolicitado += _vista.ReproducirBienvenida;
            _vista.OnSenasSolicitadas += _vista.MostrarPanelSenas;
            _vista.OnCerrarSenasSolicitado += _vista.CerrarPanelSenas;
            _vista.OnReproducirSenasSolicitado += _vista.ReproducirVideoSenas;
            _vista.OnRegresarSolicitado += Regresar;
            _vista.OnAvanzarSolicitado += Avanzar;
            _vista.OnInicioSolicitado += NavegacionNiveles.CargarInicio;
            _vista.OnConfiguracionSolicitada += AbrirConfiguracion;
        }

        private void OnDisable()
        {
            if (_vista == null || _modelo == null) return;

            _vista.OnComenzarSolicitado -= IniciarAventura;
            _vista.OnAudioSolicitado -= _vista.ReproducirBienvenida;
            _vista.OnSenasSolicitadas -= _vista.MostrarPanelSenas;
            _vista.OnCerrarSenasSolicitado -= _vista.CerrarPanelSenas;
            _vista.OnReproducirSenasSolicitado -= _vista.ReproducirVideoSenas;
            _vista.OnRegresarSolicitado -= Regresar;
            _vista.OnAvanzarSolicitado -= Avanzar;
            _vista.OnInicioSolicitado -= NavegacionNiveles.CargarInicio;
            _vista.OnConfiguracionSolicitada -= AbrirConfiguracion;
        }

        private void Start()
        {
            if (_vista == null || _modelo == null) return;
            _vista.MostrarProgreso(_numeroEscena, AventuraBienvenidaModel.TotalPuntosProgreso);
            _vista.PermitirNavegacion(NavegacionNiveles.HayAnterior(),
                _modelo.AventuraIniciada && NavegacionNiveles.HaySiguiente(_siguienteEscena));
        }

        private void IniciarAventura()
        {
            _vista.DetenerAudio();
            if (!_modelo.AventuraIniciada)
            {
                _modelo.IniciarAventura();
                try
                {
                    _repositorio.Guardar(_modelo.Progreso);
                }
                catch (IOException excepcion)
                {
                    Debug.LogError("BienvenidaNivelPresenter: no se pudo guardar el progreso. " + excepcion.Message, this);
                }
                catch (UnauthorizedAccessException excepcion)
                {
                    Debug.LogError("BienvenidaNivelPresenter: no hay permisos para guardar el progreso. " + excepcion.Message, this);
                }
            }

            if (!NavegacionNiveles.CargarSiguiente(_siguienteEscena))
            {
                if (Application.CanStreamedLevelBeLoaded("Escena02_HU02_Mono"))
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Escena02_HU02_Mono");
                }
                else
                {
                    Debug.Log("BienvenidaNivelPresenter: aún no hay una actividad siguiente en Build Profiles.", this);
                }
            }
        }

        private void Regresar()
        {
            NavegacionNiveles.CargarAnterior();
        }

        private void Avanzar()
        {
            if (_modelo.AventuraIniciada) NavegacionNiveles.CargarSiguiente(_siguienteEscena);
        }

        private void AbrirConfiguracion()
        {
            Debug.Log("BienvenidaNivelPresenter: la ventana de configuración se incorporará en una próxima versión.", this);
        }
    }
}
