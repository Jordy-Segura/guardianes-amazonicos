using System;
using System.IO;
using GuardianesAmazonicos.Models;
using GuardianesAmazonicos.Persistence;
using GuardianesAmazonicos.Views;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GuardianesAmazonicos.Presenters
{
    public sealed class RetoSeleccionPresenter : MonoBehaviour
    {
        [SerializeField] private RetoSeleccionConfiguracion _configuracion;
        [SerializeField] private RetoSeleccionView _vista;

        private RetoSeleccionModel _modelo;
        private IProgresoRepository _repositorio;

        public void Configurar(RetoSeleccionConfiguracion configuracion, RetoSeleccionView vista)
        {
            _configuracion = configuracion;
            _vista = vista;
        }

        private void Awake()
        {
            if (_vista == null) _vista = GetComponent<RetoSeleccionView>();
            if (_vista == null || _configuracion == null)
            {
                Debug.LogError("RetoSeleccionPresenter: falta la vista o la configuración del reto.", this);
                enabled = false;
                return;
            }

            _modelo = new RetoSeleccionModel(_configuracion.indiceCorrecto);
            _repositorio = new JsonProgresoRepository();
            _vista.Construir(_configuracion);
        }

        private void OnEnable()
        {
            if (_vista == null) return;
            _vista.OnOpcionSeleccionada += EvaluarOpcion;
            _vista.OnAudioSolicitado += ReproducirPronunciacion;
            _vista.OnContinuarSolicitado += Continuar;
        }

        private void OnDisable()
        {
            if (_vista == null) return;
            _vista.OnOpcionSeleccionada -= EvaluarOpcion;
            _vista.OnAudioSolicitado -= ReproducirPronunciacion;
            _vista.OnContinuarSolicitado -= Continuar;
        }

        private void Start()
        {
            if (RetoYaCompletado()) _vista.MostrarCorrecta(_configuracion.mensajeCorrecto);
        }

        private void EvaluarOpcion(int indice)
        {
            if (_modelo.Resuelto) return;

            if (_modelo.Evaluar(indice))
            {
                GuardarProgreso();
                _vista.MostrarCorrecta(_configuracion.mensajeCorrecto);
            }
            else
            {
                _vista.MostrarIncorrecta(_configuracion.pistaIncorrecta);
            }
        }

        private void ReproducirPronunciacion()
        {
            _vista.ReproducirPronunciacion(_vista.IndiceSeleccionado);
        }

        private void Continuar()
        {
            if (!_modelo.Resuelto && !RetoYaCompletado()) return;
            if (!string.IsNullOrWhiteSpace(_configuracion.siguienteEscena))
            {
                SceneManager.LoadScene(_configuracion.siguienteEscena);
            }
            else
            {
                _vista.MostrarSiguienteEnPreparacion();
            }
        }

        private void GuardarProgreso()
        {
            try
            {
                ProgresoEstudianteData progreso = _repositorio.Cargar();
                if (_configuracion.Nivel == 1 && _configuracion.numeroEscena == 5)
                    progreso.retoJaguarCompletado = true;
                else if (_configuracion.Nivel == 1 && _configuracion.numeroEscena == 9)
                    progreso.retoPlatanoCompletado = true;
                else if (_configuracion.Nivel == 2 && _configuracion.numeroEscena == 1)
                {
                    progreso.RetoMonoCuriosoCompletado = true;
                    progreso.ProgresoNivel2 = Math.Max(progreso.ProgresoNivel2, _configuracion.numeroEscena);
                }

                if (_configuracion.Nivel == 1)
                {
                    progreso.aventuraIniciada = true;
                    progreso.puntoProgresoActual = Math.Max(progreso.puntoProgresoActual, _configuracion.numeroEscena);
                    progreso.totalPuntosProgreso = AventuraBienvenidaModel.TotalPuntosProgreso;
                }
                _repositorio.Guardar(progreso);
            }
            catch (IOException excepcion)
            {
                Debug.LogError("RetoSeleccionPresenter: no se pudo guardar el avance. " + excepcion.Message, this);
            }
            catch (UnauthorizedAccessException excepcion)
            {
                Debug.LogError("RetoSeleccionPresenter: no hay permiso para guardar el avance. " + excepcion.Message, this);
            }
        }

        private bool RetoYaCompletado()
        {
            try
            {
                ProgresoEstudianteData progreso = _repositorio.Cargar();
                if (_configuracion.Nivel == 1 && _configuracion.numeroEscena == 5)
                    return progreso.retoJaguarCompletado;
                if (_configuracion.Nivel == 1 && _configuracion.numeroEscena == 9)
                    return progreso.retoPlatanoCompletado;
                return _configuracion.Nivel == 2 && _configuracion.numeroEscena == 1
                    && progreso.RetoMonoCuriosoCompletado;
            }
            catch (Exception excepcion) when (excepcion is IOException || excepcion is UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
