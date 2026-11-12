using System;
using System.IO;
using GuardianesAmazonicos.Models;
using GuardianesAmazonicos.Persistence;
using GuardianesAmazonicos.Views;
using UnityEngine;

namespace GuardianesAmazonicos.Presenters
{
    /// <summary>
    /// Coordina la plantilla de actividad: la vista emite acciones, el modelo valida
    /// y aquí se decide el estado siguiente, el audio, el video y el progreso guardado.
    /// </summary>
    public sealed class RetoSeleccionPresenter : MonoBehaviour
    {
        private const string MENSAJE_SIN_SIGUIENTE = "¡Progreso guardado! La siguiente actividad se agregará pronto.";

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

            int cantidad = _configuracion.Opciones != null ? _configuracion.Opciones.Length : 0;
            _modelo = new RetoSeleccionModel(_configuracion.IndiceCorrecto, cantidad);
            _repositorio = new JsonProgresoRepository();
        }

        private void OnEnable()
        {
            if (_vista == null || _modelo == null) return;
            _vista.OnComenzarSolicitado += Comenzar;
            _vista.OnTarjetaSeleccionada += SeleccionarTarjeta;
            _vista.OnAceptarSolicitado += Aceptar;
            _vista.OnIntentarloDeNuevoSolicitado += IntentarloDeNuevo;
            _vista.OnSiguienteSolicitado += Siguiente;
            _vista.OnLecturaSolicitada += Leer;
            _vista.OnAyudaSolicitada += AbrirPista;
            _vista.OnCerrarPistaSolicitado += CerrarPista;
            _vista.OnSenasSolicitadas += AbrirSenas;
            _vista.OnCerrarSenasSolicitado += CerrarSenas;
            _vista.OnReproducirSenasSolicitado += _vista.AlternarVideo;
            _vista.OnRegresarSolicitado += Regresar;
            _vista.OnAvanzarSolicitado += Avanzar;
            _vista.OnInicioSolicitado += IrAlInicio;
            _vista.OnConfiguracionSolicitada += AbrirConfiguracion;
        }

        private void OnDisable()
        {
            if (_vista == null || _modelo == null) return;
            _vista.OnComenzarSolicitado -= Comenzar;
            _vista.OnTarjetaSeleccionada -= SeleccionarTarjeta;
            _vista.OnAceptarSolicitado -= Aceptar;
            _vista.OnIntentarloDeNuevoSolicitado -= IntentarloDeNuevo;
            _vista.OnSiguienteSolicitado -= Siguiente;
            _vista.OnLecturaSolicitada -= Leer;
            _vista.OnAyudaSolicitada -= AbrirPista;
            _vista.OnCerrarPistaSolicitado -= CerrarPista;
            _vista.OnSenasSolicitadas -= AbrirSenas;
            _vista.OnCerrarSenasSolicitado -= CerrarSenas;
            _vista.OnReproducirSenasSolicitado -= _vista.AlternarVideo;
            _vista.OnRegresarSolicitado -= Regresar;
            _vista.OnAvanzarSolicitado -= Avanzar;
            _vista.OnInicioSolicitado -= IrAlInicio;
            _vista.OnConfiguracionSolicitada -= AbrirConfiguracion;
        }

        private void Start()
        {
            if (_modelo == null) return;
            _vista.MostrarProgreso(_configuracion.NumeroEscena, _configuracion.TotalEscenas);
            _vista.MostrarSinSeleccion();
            _vista.MostrarPista(false);
            _vista.MostrarSenas(false, null);
            _vista.MostrarBienvenida(_configuracion.MostrarBienvenida);
            _vista.PermitirRegresar(NavegacionNiveles.HayAnterior());
            _vista.PermitirAvanzar(RetoYaCompletado() && NavegacionNiveles.HaySiguiente(_configuracion.SiguienteEscena));
        }

        private void Comenzar()
        {
            _vista.DetenerAudio();
            _vista.MostrarBienvenida(false);
        }

        private void SeleccionarTarjeta(int indice)
        {
            if (_modelo.Seleccionar(indice)) _vista.MostrarSeleccion(indice);
        }

        private void Aceptar()
        {
            if (!_modelo.HaySeleccion || _modelo.Resuelto) return;

            int indice = _modelo.IndiceSeleccionado;
            if (_modelo.Confirmar())
            {
                GuardarProgreso();
                _vista.MostrarCorrecta(indice, _configuracion.Felicitacion);
                _vista.AlternarAudio(_configuracion.AudioFelicitacion);
                _vista.PermitirAvanzar(NavegacionNiveles.HaySiguiente(_configuracion.SiguienteEscena));
            }
            else
            {
                _vista.MostrarIncorrecta(indice, _configuracion.ObtenerExplicacion(indice));
                _vista.AlternarAudio(_configuracion.ObtenerAudioCorreccion(indice));
            }
        }

        private void IntentarloDeNuevo()
        {
            _modelo.Reintentar();
            _vista.MostrarSinSeleccion();
            _vista.EnfocarTarjeta(0);
        }

        private void Siguiente()
        {
            _vista.DetenerAudio();
            if (!NavegacionNiveles.CargarSiguiente(_configuracion.SiguienteEscena))
            {
                _vista.MostrarAvisoSinSiguiente(MENSAJE_SIN_SIGUIENTE);
            }
        }

        private void Leer()
        {
            AudioClip clip = _vista.BienvenidaVisible
                ? _configuracion.BienvenidaAudio
                : _vista.PistaVisible
                    ? _configuracion.AudioPista
                    : _vista.RespuestaIncorrectaVisible
                        ? _configuracion.ObtenerAudioCorreccion(_modelo.IndiceSeleccionado)
                        : _configuracion.AudioLectura;
            if (clip == null) clip = _vista.BienvenidaVisible ? _configuracion.BienvenidaAudio : _configuracion.AudioLectura;
            _vista.AlternarAudio(clip);
        }

        private void AbrirPista()
        {
            if (_vista.BienvenidaVisible || _modelo.Resuelto) return;
            _vista.MostrarPista(true);
        }

        private void CerrarPista()
        {
            _vista.MostrarPista(false);
            _vista.EnfocarTarjeta(_modelo.IndiceSeleccionado);
        }

        private void AbrirSenas()
        {
            string video = _vista.BienvenidaVisible ? _configuracion.BienvenidaVideoSenas : _configuracion.VideoSenas;
            _vista.DetenerAudio();
            _vista.MostrarSenas(true, video);
        }

        private void CerrarSenas()
        {
            _vista.MostrarSenas(false, null);
            if (!_vista.BienvenidaVisible) _vista.EnfocarTarjeta(_modelo.IndiceSeleccionado);
        }

        private void Regresar()
        {
            NavegacionNiveles.CargarAnterior();
        }

        private void Avanzar()
        {
            if (_modelo.Resuelto || RetoYaCompletado())
            {
                NavegacionNiveles.CargarSiguiente(_configuracion.SiguienteEscena);
            }
        }

        private void IrAlInicio()
        {
            NavegacionNiveles.CargarInicio();
        }

        private void AbrirConfiguracion()
        {
            Debug.Log("RetoSeleccionPresenter: la ventana de configuración se incorporará en una próxima versión.", this);
        }

        private void GuardarProgreso()
        {
            try
            {
                ProgresoEstudianteData progreso = _repositorio.Cargar();
                int nivel = _configuracion.Nivel;
                int escena = _configuracion.NumeroEscena;
                if (nivel == 1 && escena == 2)
                    progreso.retoMonoCompletado = true;
                else if (nivel == 1 && escena == 5)
                    progreso.retoJaguarCompletado = true;
                else if (nivel == 1 && escena == 9)
                    progreso.retoPlatanoCompletado = true;
                else if (nivel == 2 && escena == 1)
                    progreso.RetoMonoCuriosoCompletado = true;

                if (nivel == 1)
                {
                    progreso.aventuraIniciada = true;
                    progreso.puntoProgresoActual = Math.Max(progreso.puntoProgresoActual, escena);
                    progreso.totalPuntosProgreso = AventuraBienvenidaModel.TotalPuntosProgreso;
                }
                else if (nivel == 2)
                {
                    progreso.ProgresoNivel2 = Math.Max(progreso.ProgresoNivel2, escena);
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
                int nivel = _configuracion.Nivel;
                int escena = _configuracion.NumeroEscena;
                if (nivel == 1 && escena == 2) return progreso.retoMonoCompletado;
                if (nivel == 1 && escena == 5) return progreso.retoJaguarCompletado;
                if (nivel == 1 && escena == 9) return progreso.retoPlatanoCompletado;
                return nivel == 2 && escena == 1 && progreso.RetoMonoCuriosoCompletado;
            }
            catch (Exception excepcion) when (excepcion is IOException || excepcion is UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
