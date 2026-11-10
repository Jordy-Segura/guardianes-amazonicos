using System;
using System.IO;
using GuardianesAmazonicos.Models;
using GuardianesAmazonicos.Persistence;
using GuardianesAmazonicos.Views;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GuardianesAmazonicos.Presenters
{
    public sealed class GuacamayoPresenter : MonoBehaviour
    {
        [SerializeField] private RetoSeleccionConfiguracion _configuracion;
        [SerializeField] private GuacamayoView _vista;
        private IGuacamayoView _interfaz;
        private RetoSeleccionModel _modelo;
        private IProgresoRepository _repositorio;
        private int _seleccion = -1;
        private bool _resultadoMostrado;
        private bool _avanceGuardado;

        public void Configurar(RetoSeleccionConfiguracion configuracion, GuacamayoView vista)
        {
            _configuracion = configuracion;
            _vista = vista;
        }

        private void Awake()
        {
            if (_vista == null || _configuracion == null || _configuracion.opciones == null ||
                _configuracion.opciones.Length != 3 || _configuracion.indiceCorrecto < 0 ||
                _configuracion.indiceCorrecto >= 3 || _configuracion.imagenes == null ||
                _configuracion.imagenes.Length != 3 || Array.Exists(_configuracion.imagenes, imagen => imagen == null))
            {
                Debug.LogError("GuacamayoPresenter: faltan la vista, configuración o imágenes de HU-03.", this);
                enabled = false;
                return;
            }
            _modelo = new RetoSeleccionModel(_configuracion.indiceCorrecto);
            _repositorio = new JsonProgresoRepository();
            _vista.Construir(_configuracion);
            _interfaz = _vista;
        }

        private void OnEnable()
        {
            if (_interfaz == null || _modelo == null) return;
            OnDisable();
            _interfaz.OnOpcionSeleccionada += Seleccionar;
            _interfaz.OnAceptarSolicitado += Validar;
            _interfaz.OnAudioSolicitado += ReproducirAudio;
            _interfaz.OnReintentarSolicitado += Reintentar;
            _interfaz.OnSiguienteSolicitado += Continuar;
            _interfaz.OnAyudaSolicitada += MostrarAyuda;
            _interfaz.OnSenasSolicitadas += MostrarSenas;
            _interfaz.OnInicioSolicitado += IrAInicio;
            _interfaz.OnRegresarSolicitado += Regresar;
            _interfaz.OnAvanzarSolicitado += Continuar;
            _interfaz.OnConfiguracionSolicitada += MostrarConfiguracion;
        }

        private void OnDisable()
        {
            if (_interfaz == null) return;
            _interfaz.OnOpcionSeleccionada -= Seleccionar;
            _interfaz.OnAceptarSolicitado -= Validar;
            _interfaz.OnAudioSolicitado -= ReproducirAudio;
            _interfaz.OnReintentarSolicitado -= Reintentar;
            _interfaz.OnSiguienteSolicitado -= Continuar;
            _interfaz.OnAyudaSolicitada -= MostrarAyuda;
            _interfaz.OnSenasSolicitadas -= MostrarSenas;
            _interfaz.OnInicioSolicitado -= IrAInicio;
            _interfaz.OnRegresarSolicitado -= Regresar;
            _interfaz.OnAvanzarSolicitado -= Continuar;
            _interfaz.OnConfiguracionSolicitada -= MostrarConfiguracion;
        }

        private void Seleccionar(int indice)
        {
            if (_resultadoMostrado || indice < 0 || indice >= 3) return;
            _seleccion = indice;
            _interfaz.MostrarSeleccion(indice);
        }

        private void Validar()
        {
            if (_resultadoMostrado || _seleccion < 0) return;
            _resultadoMostrado = true;
            _interfaz.MostrarResultado(_modelo.Evaluar(_seleccion));
        }

        private void Reintentar()
        {
            if (_modelo.Resuelto) return;
            _seleccion = -1;
            _resultadoMostrado = false;
            _interfaz.ReiniciarSeleccion();
        }

        private void ReproducirAudio() { _interfaz.ReproducirAudio(); }
        private void MostrarAyuda() { _interfaz.MostrarAyuda(); }
        private void MostrarSenas() { _interfaz.MostrarSenas(); }
        private void MostrarConfiguracion() { _interfaz.MostrarConfiguracion(); }

        private void IrAInicio() { CargarBienvenida("Inicio"); }
        private void Regresar() { CargarBienvenida("Regresar"); }

        private void CargarBienvenida(string accion)
        {
            const string escenaBienvenida = "Nivel1_ConozcoLaSelva";
            if (Application.CanStreamedLevelBeLoaded(escenaBienvenida))
                SceneManager.LoadScene(escenaBienvenida);
            else
                _interfaz.MostrarAviso(accion + ": la bienvenida del nivel aún no está incluida en esta compilación.");
        }

        private void Continuar()
        {
            if (!_modelo.Resuelto) return;
            if (!_avanceGuardado)
            {
                try
                {
                    ProgresoEstudianteData progreso = _repositorio.Cargar();
                    progreso.retoGuacamayoCompletado = true;
                    progreso.aventuraIniciada = true;
                    progreso.puntoProgresoActual = Math.Max(progreso.puntoProgresoActual, 3);
                    _repositorio.Guardar(progreso);
                    _avanceGuardado = true;
                }
                catch (Exception excepcion) when (excepcion is IOException || excepcion is UnauthorizedAccessException || excepcion is ArgumentException)
                {
                    Debug.LogError("GuacamayoPresenter: no se pudo guardar el progreso. " + excepcion.Message, this);
                    _interfaz.MostrarAviso("No se pudo guardar el avance. Pulsa Siguiente para reintentar.");
                    return;
                }
            }
            if (!string.IsNullOrWhiteSpace(_configuracion.siguienteEscena) &&
                Application.CanStreamedLevelBeLoaded(_configuracion.siguienteEscena))
            {
                SceneManager.LoadScene(_configuracion.siguienteEscena);
            }
            else
            {
                _interfaz.MostrarAviso("¡Actividad completada y guardada! El próximo reto aún no está disponible.");
            }
        }
    }
}
