using System;
using System.IO;
using GuardianesAmazonicos.Models;
using GuardianesAmazonicos.Persistence;
using GuardianesAmazonicos.Views;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GuardianesAmazonicos.Presenters
{
    /// <summary>Flujo propio de HU-03 conectado a la vista base de actividades.</summary>
    public sealed class GuacamayoPresenter : MonoBehaviour
    {
        private const string MENSAJE_SIN_SIGUIENTE = "¡Reto completado! La siguiente actividad se agregará pronto.";

        [SerializeField] private RetoSeleccionConfiguracion _configuracion;
        [SerializeField] private RetoSeleccionView _vista;
        [SerializeField] private Image _personajeGuia;
        [SerializeField] private Sprite _personajePreocupado;

        private RetoSeleccionModel _modelo;
        private IProgresoRepository _repositorio;
        private bool _progresoGuardado;

        public void Configurar(RetoSeleccionConfiguracion configuracion, RetoSeleccionView vista,
            Image personajeGuia, Sprite personajePreocupado)
        {
            _configuracion = configuracion;
            _vista = vista;
            _personajeGuia = personajeGuia;
            _personajePreocupado = personajePreocupado;
        }

        private void Awake()
        {
            if (_vista == null) _vista = GetComponent<RetoSeleccionView>();
            if (_vista == null || _configuracion == null || _configuracion.Opciones == null ||
                _configuracion.Opciones.Length == 0 ||
                _configuracion.IndiceCorrecto < 0 || _configuracion.IndiceCorrecto >= _configuracion.Opciones.Length)
            {
                Debug.LogError("GuacamayoPresenter: faltan la vista o configuración de la actividad.", this);
                enabled = false;
                return;
            }

            _vista.Construir(_configuracion);
            _modelo = new RetoSeleccionModel(_configuracion.IndiceCorrecto, _configuracion.Opciones.Length);
            _repositorio = new JsonProgresoRepository();
        }

        private void OnEnable()
        {
            if (_vista == null || _modelo == null) return;
            _vista.OnTarjetaSeleccionada += Seleccionar;
            _vista.OnAceptarSolicitado += Validar;
            _vista.OnIntentarloDeNuevoSolicitado += Reintentar;
            _vista.OnSiguienteSolicitado += Continuar;
            _vista.OnLecturaSolicitada += Leer;
            _vista.OnAyudaSolicitada += MostrarAyuda;
            _vista.OnCerrarPistaSolicitado += CerrarAyuda;
            _vista.OnSenasSolicitadas += MostrarSenas;
            _vista.OnCerrarSenasSolicitado += CerrarSenas;
            _vista.OnReproducirSenasSolicitado += _vista.AlternarVideo;
            _vista.OnRegresarSolicitado += Regresar;
            _vista.OnAvanzarSolicitado += Avanzar;
            _vista.OnInicioSolicitado += IrAInicio;
            _vista.OnConfiguracionSolicitada += MostrarConfiguracion;
        }

        private void OnDisable()
        {
            if (_vista == null) return;
            _vista.OnTarjetaSeleccionada -= Seleccionar;
            _vista.OnAceptarSolicitado -= Validar;
            _vista.OnIntentarloDeNuevoSolicitado -= Reintentar;
            _vista.OnSiguienteSolicitado -= Continuar;
            _vista.OnLecturaSolicitada -= Leer;
            _vista.OnAyudaSolicitada -= MostrarAyuda;
            _vista.OnCerrarPistaSolicitado -= CerrarAyuda;
            _vista.OnSenasSolicitadas -= MostrarSenas;
            _vista.OnCerrarSenasSolicitado -= CerrarSenas;
            _vista.OnReproducirSenasSolicitado -= _vista.AlternarVideo;
            _vista.OnRegresarSolicitado -= Regresar;
            _vista.OnAvanzarSolicitado -= Avanzar;
            _vista.OnInicioSolicitado -= IrAInicio;
            _vista.OnConfiguracionSolicitada -= MostrarConfiguracion;
        }

        private void Start()
        {
            if (_modelo == null) return;
            _vista.MostrarProgreso(_configuracion.NumeroEscena, _configuracion.TotalEscenas);
            _vista.MostrarSinSeleccion();
            _vista.MostrarPista(false);
            _vista.MostrarSenas(false, null);
            _vista.PermitirRegresar(NavegacionNiveles.HayAnterior());
            _vista.PermitirAvanzar(YaCompletado() && NavegacionNiveles.HaySiguiente(_configuracion.SiguienteEscena));
            _vista.ActualizarEstadoVisualLectura(RetoSeleccionView.ModoLecturaActivo);
            if (RetoSeleccionView.ModoLecturaActivo)
            {
                ReproducirEstadoActual();
            }
        }

        private void Seleccionar(int indice)
        {
            if (_modelo.Seleccionar(indice)) _vista.MostrarSeleccion(indice);
        }

        private void Validar()
        {
            if (!_modelo.HaySeleccion || _modelo.Resuelto) return;
            int seleccionada = _modelo.IndiceSeleccionado;
            if (_modelo.Confirmar())
            {
                GuardarProgreso();
                string felicitacion = !string.IsNullOrWhiteSpace(_configuracion.Felicitacion) ? _configuracion.Felicitacion : "¡Lo lograste!";
                _vista.MostrarCorrecta(seleccionada, felicitacion);
                if (RetoSeleccionView.ModoLecturaActivo)
                {
                    _vista.ReproducirAudio(_configuracion.AudioFelicitacion);
                }
                _vista.PermitirAvanzar(NavegacionNiveles.HaySiguiente(_configuracion.SiguienteEscena));
                return;
            }

            if (_personajeGuia != null && _personajePreocupado != null)
                _personajeGuia.sprite = _personajePreocupado;
            _vista.MostrarIncorrecta(seleccionada, _configuracion.ObtenerExplicacion(seleccionada));
            if (RetoSeleccionView.ModoLecturaActivo)
            {
                _vista.ReproducirAudio(_configuracion.ObtenerAudioCorreccion(seleccionada));
            }
        }

        private void Reintentar()
        {
            if (_modelo.Resuelto) return;
            _vista.DetenerAudio();
            _modelo.Reintentar();
            if (_personajeGuia != null) _personajeGuia.sprite = _configuracion.Personaje;
            _vista.MostrarSinSeleccion();
            _vista.EnfocarTarjeta(0);
            if (RetoSeleccionView.ModoLecturaActivo)
            {
                ReproducirEstadoActual();
            }
        }

        private void Continuar()
        {
            if (!_modelo.Resuelto) return;
            _vista.DetenerAudio();
            if (!NavegacionNiveles.CargarSiguiente(_configuracion.SiguienteEscena))
                _vista.MostrarAvisoSinSiguiente(MENSAJE_SIN_SIGUIENTE);
        }

        private void Leer()
        {
            RetoSeleccionView.ModoLecturaActivo = !RetoSeleccionView.ModoLecturaActivo;
            _vista.ActualizarEstadoVisualLectura(RetoSeleccionView.ModoLecturaActivo);
            if (!RetoSeleccionView.ModoLecturaActivo)
            {
                _vista.DetenerAudio();
                return;
            }

            ReproducirEstadoActual();
        }

        private void ReproducirEstadoActual()
        {
            AudioClip clip = _vista.PistaVisible
                ? _configuracion.AudioPista
                : _vista.EstadoCorrectoVisible
                    ? _configuracion.AudioFelicitacion
                : _vista.RespuestaIncorrectaVisible
                    ? _configuracion.ObtenerAudioCorreccion(_modelo.IndiceSeleccionado)
                    : _configuracion.AudioLectura;
            _vista.ReproducirAudio(clip != null ? clip : _configuracion.AudioLectura);
        }

        private void MostrarAyuda()
        {
            _vista.MostrarPista(true);
            if (RetoSeleccionView.ModoLecturaActivo)
            {
                ReproducirEstadoActual();
            }
        }

        private void CerrarAyuda()
        {
            _vista.DetenerAudio();
            _vista.MostrarPista(false);
            _vista.EnfocarTarjeta(_modelo.IndiceSeleccionado);
            if (RetoSeleccionView.ModoLecturaActivo)
            {
                ReproducirEstadoActual();
            }
        }

        private void MostrarSenas()
        {
            _vista.DetenerAudio();
            _vista.MostrarSenas(true, _configuracion.VideoSenas);
        }

        private void CerrarSenas()
        {
            _vista.MostrarSenas(false, null);
            _vista.EnfocarTarjeta(_modelo.IndiceSeleccionado);
        }

        private void Regresar()
        {
            if (!NavegacionNiveles.CargarAnterior())
                _vista.MostrarAvisoSinSiguiente("Esta es la primera escena disponible.");
        }

        private void Avanzar()
        {
            if ((_modelo != null && _modelo.Resuelto) || YaCompletado())
            {
                NavegacionNiveles.CargarSiguiente(_configuracion.SiguienteEscena);
            }
        }

        private void IrAInicio() => NavegacionNiveles.CargarInicio();

        private void MostrarConfiguracion()
        {
            _vista.DetenerAudio();
            _vista.MostrarConfiguracion(true);
        }

        private bool YaCompletado()
        {
            try
            {
                ProgresoEstudianteData progreso = _repositorio.Cargar();
                if (_configuracion != null && _configuracion.NumeroEscena != 3)
                {
                    return progreso.puntoProgresoActual >= _configuracion.NumeroEscena;
                }
                return progreso.retoGuacamayoCompletado || (_configuracion != null && progreso.puntoProgresoActual >= _configuracion.NumeroEscena);
            }
            catch (Exception excepcion) when (excepcion is IOException || excepcion is UnauthorizedAccessException || excepcion is ArgumentException)
            {
                Debug.LogWarning("GuacamayoPresenter: no se pudo leer el progreso guardado. " + excepcion.Message, this);
                return false;
            }
        }

        private void GuardarProgreso()
        {
            if (_progresoGuardado) return;
            try
            {
                ProgresoEstudianteData progreso = _repositorio.Cargar();
                if (_configuracion == null || _configuracion.NumeroEscena == 3)
                {
                    progreso.retoGuacamayoCompletado = true;
                }
                progreso.aventuraIniciada = true;
                progreso.puntoProgresoActual = Math.Max(progreso.puntoProgresoActual, _configuracion.NumeroEscena);
                progreso.totalPuntosProgreso = Math.Max(progreso.totalPuntosProgreso, _configuracion.TotalEscenas);
                _repositorio.Guardar(progreso);
                _progresoGuardado = true;
            }
            catch (Exception excepcion) when (excepcion is IOException || excepcion is UnauthorizedAccessException || excepcion is ArgumentException)
            {
                Debug.LogError("GuacamayoPresenter: no se pudo guardar el progreso de HU-03. " + excepcion.Message, this);
                _vista.MostrarAvisoSinSiguiente("No se pudo guardar el avance. Puedes volver a intentarlo.");
            }
        }
    }
}
