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
                _configuracion.Opciones.Length != RetoSeleccionConfiguracion.CANTIDAD_OPCIONES ||
                _configuracion.ImagenesOpciones == null ||
                _configuracion.ImagenesOpciones.Length != RetoSeleccionConfiguracion.CANTIDAD_OPCIONES ||
                Array.Exists(_configuracion.ImagenesOpciones, imagen => imagen == null) ||
                _configuracion.IndiceCorrecto < 0 || _configuracion.IndiceCorrecto >= RetoSeleccionConfiguracion.CANTIDAD_OPCIONES)
            {
                Debug.LogError("GuacamayoPresenter: faltan la vista, configuración o ilustraciones de HU-03.", this);
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
            _vista.AlternarAudio(_configuracion.AudioLectura);
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
                _vista.MostrarCorrecta(seleccionada, "¡Lo lograste!");
                _vista.AlternarAudio(_configuracion.AudioFelicitacion);
                _vista.PermitirAvanzar(NavegacionNiveles.HaySiguiente(_configuracion.SiguienteEscena));
                return;
            }

            if (_personajeGuia != null && _personajePreocupado != null)
                _personajeGuia.sprite = _personajePreocupado;
            _vista.MostrarIncorrecta(seleccionada, _configuracion.ObtenerExplicacion(seleccionada));
            _vista.AlternarAudio(_configuracion.ObtenerAudioCorreccion(seleccionada));
        }

        private void Reintentar()
        {
            if (_modelo.Resuelto) return;
            _modelo.Reintentar();
            if (_personajeGuia != null) _personajeGuia.sprite = _configuracion.Personaje;
            _vista.MostrarSinSeleccion();
            _vista.EnfocarTarjeta(0);
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
            AudioClip clip = _vista.PistaVisible
                ? _configuracion.AudioPista
                : _vista.RespuestaIncorrectaVisible
                    ? _configuracion.ObtenerAudioCorreccion(_modelo.IndiceSeleccionado)
                    : _configuracion.AudioLectura;
            _vista.AlternarAudio(clip != null ? clip : _configuracion.AudioLectura);
        }

        private void MostrarAyuda() => _vista.MostrarPista(true);

        private void CerrarAyuda()
        {
            _vista.MostrarPista(false);
            _vista.EnfocarTarjeta(_modelo.IndiceSeleccionado);
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
            if (YaCompletado()) Continuar();
        }

        private void IrAInicio() => NavegacionNiveles.CargarInicio();

        private void MostrarConfiguracion()
        {
            Debug.Log("HU-03: la configuración de preferencias se administra desde el menú principal.", this);
        }

        private bool YaCompletado()
        {
            try { return _repositorio.Cargar().retoGuacamayoCompletado; }
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
                progreso.retoGuacamayoCompletado = true;
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
