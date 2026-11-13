using System;
using System.IO;
using GuardianesAmazonicos.Models;
using GuardianesAmazonicos.Persistence;
using GuardianesAmazonicos.Views;
using UnityEngine;

namespace GuardianesAmazonicos.Presenters
{
    /// <summary>Flujo y voz de retroalimentación propios de HU-11.</summary>
    public sealed class ReconocerArbolPresenter : MonoBehaviour
    {
        [SerializeField] private RetoSeleccionConfiguracion _configuracion;
        [SerializeField] private RetoSeleccionView _vista;
        [SerializeField] private AudioClip _audioAcierto;
        [SerializeField] private AudioClip _audioError;
        private RetoSeleccionModel _modelo;
        private IProgresoRepository _repositorio;
        private bool _avanceGuardado;

        public void Configurar(RetoSeleccionConfiguracion configuracion, RetoSeleccionView vista,
            AudioClip audioAcierto, AudioClip audioError)
        {
            _configuracion = configuracion;
            _vista = vista;
            _audioAcierto = audioAcierto;
            _audioError = audioError;
        }

        private void Awake()
        {
            if (_vista == null) _vista = GetComponent<RetoSeleccionView>();
            if (_vista == null || _configuracion == null || _configuracion.Opciones == null ||
                _configuracion.Opciones.Length != 3 || _configuracion.IndiceCorrecto < 0 || _configuracion.IndiceCorrecto > 2)
            {
                Debug.LogError("HU-11: falta la configuración o vista de la actividad.", this);
                enabled = false;
                return;
            }
            _modelo = new RetoSeleccionModel(_configuracion.IndiceCorrecto, _configuracion.Opciones.Length);
            _repositorio = new JsonProgresoRepository();
        }

        private void OnEnable()
        {
            if (_vista == null || _modelo == null) return;
            _vista.OnTarjetaSeleccionada += Seleccionar;
            _vista.OnAceptarSolicitado += Validar;
            _vista.OnIntentarloDeNuevoSolicitado += Reintentar;
            _vista.OnSiguienteSolicitado += Siguiente;
            _vista.OnLecturaSolicitada += Leer;
            _vista.OnAyudaSolicitada += AbrirPista;
            _vista.OnCerrarPistaSolicitado += CerrarPista;
            _vista.OnSenasSolicitadas += AbrirSenas;
            _vista.OnCerrarSenasSolicitado += CerrarSenas;
            _vista.OnReproducirSenasSolicitado += _vista.AlternarVideo;
            _vista.OnRegresarSolicitado += Regresar;
            _vista.OnAvanzarSolicitado += Avanzar;
            _vista.OnInicioSolicitado += IrInicio;
            _vista.OnConfiguracionSolicitada += Configuracion;
        }

        private void OnDisable()
        {
            if (_vista == null) return;
            _vista.OnTarjetaSeleccionada -= Seleccionar;
            _vista.OnAceptarSolicitado -= Validar;
            _vista.OnIntentarloDeNuevoSolicitado -= Reintentar;
            _vista.OnSiguienteSolicitado -= Siguiente;
            _vista.OnLecturaSolicitada -= Leer;
            _vista.OnAyudaSolicitada -= AbrirPista;
            _vista.OnCerrarPistaSolicitado -= CerrarPista;
            _vista.OnSenasSolicitadas -= AbrirSenas;
            _vista.OnCerrarSenasSolicitado -= CerrarSenas;
            _vista.OnReproducirSenasSolicitado -= _vista.AlternarVideo;
            _vista.OnRegresarSolicitado -= Regresar;
            _vista.OnAvanzarSolicitado -= Avanzar;
            _vista.OnInicioSolicitado -= IrInicio;
            _vista.OnConfiguracionSolicitada -= Configuracion;
        }

        private void Start()
        {
            _vista.MostrarProgreso(_configuracion.NumeroEscena, _configuracion.TotalEscenas);
            _vista.MostrarSinSeleccion();
            _vista.MostrarPista(false);
            _vista.MostrarSenas(false, null);
            _vista.PermitirRegresar(NavegacionNiveles.HayAnterior());
            _vista.PermitirAvanzar(false);
            _vista.AlternarAudio(_configuracion.AudioLectura);
        }

        private void Seleccionar(int indice)
        {
            if (_modelo.Seleccionar(indice)) _vista.MostrarSeleccion(indice);
        }

        private void Validar()
        {
            if (!_modelo.HaySeleccion || _modelo.Resuelto) return;
            int indice = _modelo.IndiceSeleccionado;
            if (_modelo.Confirmar())
            {
                GuardarProgreso();
                _vista.MostrarCorrecta(indice, _configuracion.Felicitacion);
                _vista.AlternarAudio(_audioAcierto);
                _vista.PermitirAvanzar(true);
            }
            else
            {
                _vista.MostrarIncorrecta(indice, _configuracion.ObtenerExplicacion(indice));
                _vista.AlternarAudio(_configuracion.ObtenerAudioCorreccion(indice) ?? _audioError);
            }
        }

        private void Reintentar()
        {
            if (_modelo.Resuelto) return;
            _vista.DetenerAudio();
            _modelo.Reintentar();
            _vista.MostrarSinSeleccion();
            _vista.EnfocarTarjeta(0);
        }

        private void Leer()
        {
            AudioClip clip = _vista.PistaVisible
                ? _configuracion.AudioPista
                : _vista.EstadoCorrectoVisible
                    ? _audioAcierto
                : _vista.RespuestaIncorrectaVisible
                    ? _configuracion.ObtenerAudioCorreccion(_modelo.IndiceSeleccionado) ?? _audioError
                    : _configuracion.AudioLectura;
            _vista.AlternarAudio(clip != null ? clip : _configuracion.AudioLectura);
        }
        private void AbrirPista()
        {
            _vista.MostrarPista(true);
            _vista.ReproducirAudio(_configuracion.AudioPista);
        }
        private void CerrarPista()
        {
            _vista.DetenerAudio();
            _vista.MostrarPista(false);
            _vista.EnfocarTarjeta(_modelo.IndiceSeleccionado);
        }
        private void AbrirSenas() { _vista.DetenerAudio(); _vista.MostrarSenas(true, _configuracion.VideoSenas); }
        private void CerrarSenas() { _vista.MostrarSenas(false, null); _vista.EnfocarTarjeta(_modelo.IndiceSeleccionado); }
        private void Regresar() => NavegacionNiveles.CargarAnterior();
        private void IrInicio() => NavegacionNiveles.CargarInicio();
        private void Configuracion() => Debug.Log("HU-11: preferencias disponibles desde el menú principal.", this);

        private void Avanzar()
        {
            if (_modelo.Resuelto) Siguiente();
        }

        private void Siguiente()
        {
            if (!_modelo.Resuelto) return;
            _vista.DetenerAudio();
            if (!NavegacionNiveles.CargarSiguiente(_configuracion.SiguienteEscena))
                _vista.MostrarAvisoSinSiguiente("¡Reto completado! La siguiente actividad se agregará pronto.");
        }

        private void GuardarProgreso()
        {
            if (_avanceGuardado) return;
            try
            {
                ProgresoEstudianteData progreso = _repositorio.Cargar();
                progreso.aventuraIniciada = true;
                progreso.puntoProgresoActual = Math.Max(progreso.puntoProgresoActual, _configuracion.NumeroEscena);
                progreso.totalPuntosProgreso = Math.Max(progreso.totalPuntosProgreso, _configuracion.TotalEscenas);
                _repositorio.Guardar(progreso);
                _avanceGuardado = true;
            }
            catch (Exception excepcion) when (excepcion is IOException || excepcion is UnauthorizedAccessException || excepcion is ArgumentException)
            {
                Debug.LogError("HU-11: no se pudo guardar el progreso. " + excepcion.Message, this);
            }
        }
    }
}
