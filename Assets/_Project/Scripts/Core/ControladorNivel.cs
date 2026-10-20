using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using GuardianesAmazonicos.Audio;
using GuardianesAmazonicos.Niveles.Nivel01;
using GuardianesAmazonicos.Data;
using GuardianesAmazonicos.UI;

namespace GuardianesAmazonicos.Core
{
    public sealed class ControladorNivel : MonoBehaviour
    {
        private const string TextoBienvenida = "¡Hola, explorador! Bienvenido a la selva. Hoy vamos a conocer algunos de sus tesoros. ¿Quieres comenzar?";
        private const int TotalRetos = 12;
        private const string TextoPista = "Observa la selva, escucha al Guardián y presiona COMENZAR cuando estés listo para explorar.";
        private const string FrasePruebaApi = "hello explorer welcome to the jungle";
        private const string UrlApiSenias = "https://three.ws/api/sign?text=";
        private static TMP_FontAsset _tipografiaCompartida;
        private static Sprite _spriteTarjetaRedondeada;

        [SerializeField] private Sprite _fondoSelva;
        [SerializeField] private TMP_FontAsset _tipografia;
        [SerializeField] private AudioClip _audioBienvenida;
        [SerializeField] private AudioClip _audioInicio;
        [SerializeField] private AudioClip _audioPista;
        [SerializeField] private AudioClip _audioContinuacion;

        private GestorAudio _gestorAudio;
        private ControladorProgresoNivel _controladorProgreso;
        private TMP_Text _mensaje;
        private TMP_Text _progreso;
        private TMP_Text _estadoAudio;
        private TMP_Text _botonComenzarTexto;
        private Button _botonComenzar;
        private Button _botonAudio;
        private bool _aventuraIniciada;
        private GameObject _modalActual;
        private RectTransform _panelSeniasRect;
        private TMP_Text _etiquetaAmpliarLsec;
        private string _textoSeniasActual = TextoBienvenida;
        private string _tituloSeniasActual = "Bienvenida del Guardián";
        private bool _visorLsecAmpliado;
        private AudioClip _audioModalActual;
        private TMP_Text _estadoApiSenias;
        private TMP_Text _detalleApiSenias;
        private Button _botonAvatarApi;
        private string _urlAvatarApi;
        private Coroutine _solicitudApiSenias;

        private void Start()
        {
            CargarRecursosLocales();
            ConfigurarAudio();
            CrearCamaraSiHaceFalta();
            CrearInterfaz();
            RestaurarProgreso();
        }

        private void CargarRecursosLocales()
        {
            _tipografiaCompartida = _tipografia;
            if (_tipografiaCompartida == null)
                _tipografiaCompartida = TMP_Settings.defaultFontAsset;
            _tipografiaCompartida?.TryAddCharacters("…");
            AgregarRespaldoTipografico(_tipografiaCompartida);
        }

        private static void AgregarRespaldoTipografico(TMP_FontAsset fuente)
        {
            var respaldo = TMP_Settings.defaultFontAsset;
            if (fuente == null || respaldo == null || fuente == respaldo)
                return;

            fuente.fallbackFontAssetTable ??= new List<TMP_FontAsset>();
            if (!fuente.fallbackFontAssetTable.Contains(respaldo))
                fuente.fallbackFontAssetTable.Add(respaldo);
        }

        private void ConfigurarAudio()
        {
            _gestorAudio = GetComponent<GestorAudio>();
            if (_gestorAudio == null)
                _gestorAudio = gameObject.AddComponent<GestorAudio>();
            _gestorAudio.Configurar();
        }

        private static void CrearCamaraSiHaceFalta()
        {
            var camara = Camera.main;
            if (camara == null)
            {
                var objetoCamara = new GameObject("Main Camera");
                objetoCamara.tag = "MainCamera";
                objetoCamara.transform.position = new Vector3(0f, 0f, -10f);
                camara = objetoCamara.AddComponent<Camera>();
                camara.orthographic = true;
                camara.clearFlags = CameraClearFlags.SolidColor;
                camara.backgroundColor = new Color(0.08f, 0.29f, 0.19f);
            }

            if (camara.GetComponent<AudioListener>() == null)
                camara.gameObject.AddComponent<AudioListener>();
        }

        private void CrearInterfaz()
        {
            var canvasObjeto = new GameObject("InterfazNivel1", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObjeto.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var escalador = canvasObjeto.GetComponent<CanvasScaler>();
            escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escalador.referenceResolution = new Vector2(1366f, 768f);
            escalador.matchWidthOrHeight = 1f;

            if (FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            CrearImagen("Fondo para formato panorámico", canvas.transform, Vector2.zero, Vector2.one, new Color(0.035f, 0.16f, 0.105f, 1f)).raycastTarget = false;

            var marco = new GameObject("Marco de diseño 16 a 9", typeof(RectTransform));
            marco.transform.SetParent(canvas.transform, false);
            var rectMarco = marco.GetComponent<RectTransform>();
            rectMarco.anchorMin = Vector2.zero;
            rectMarco.anchorMax = Vector2.one;
            rectMarco.pivot = new Vector2(0.5f, 0.5f);
            rectMarco.sizeDelta = Vector2.zero;
            rectMarco.anchoredPosition = Vector2.zero;

            var arte = CrearImagen("Arte de bienvenida", marco.transform, Vector2.zero, Vector2.one, Color.white);
            arte.sprite = _fondoSelva;
            arte.preserveAspect = false;
            arte.raycastTarget = false;

            CrearMarcaYProgreso(marco.transform);
            CrearPanelBienvenida(marco.transform);
            CrearNavegacionInferior(marco.transform);
            CrearSelloInferior(marco.transform);
        }

        private void CrearMarcaYProgreso(Transform padre)
        {
            CrearPanelMarco("Placa de identidad", padre, new Vector2(0.02f, 0.9f), new Vector2(0.30f, 0.99f), new Color32(141, 90, 43, 245));
            CrearPanelMarco("Placa del nivel", padre, new Vector2(0.35f, 0.9f), new Vector2(0.69f, 0.99f), new Color32(255, 248, 232, 245));
            CrearPanelMarco("Placa de progreso", padre, new Vector2(0.83f, 0.9f), new Vector2(0.98f, 0.99f), new Color32(46, 125, 50, 245));
            CrearEtiqueta("Marca principal", padre, "GUARDIANES", 28f, new Color(1f, 0.95f, 0.76f), new Vector2(0.025f, 0.945f), new Vector2(0.29f, 0.982f), TextAlignmentOptions.Center, true);
            CrearEtiqueta("Subtítulo de marca", padre, "DE LA AMAZONÍA LECTORA", 19f, new Color(0.72f, 0.97f, 0.37f), new Vector2(0.025f, 0.904f), new Vector2(0.29f, 0.945f), TextAlignmentOptions.Center, true);
            CrearEtiqueta("Nombre del nivel", padre, "NIVEL 1  ·  CONOZCO LA SELVA", 28f, new Color(0.08f, 0.18f, 0.12f), new Vector2(0.37f, 0.912f), new Vector2(0.69f, 0.974f), TextAlignmentOptions.Center, true);

            _progreso = CrearEtiqueta("Progreso del nivel", padre, "0 de 12", 32f, new Color(1f, 0.9f, 0.3f), new Vector2(0.835f, 0.914f), new Vector2(0.987f, 0.98f), TextAlignmentOptions.Center, true);
        }

        private void CrearPanelBienvenida(Transform padre)
        {
            var panel = CrearImagen("Panel de bienvenida", padre, new Vector2(0.39f, 0.19f), new Vector2(0.96f, 0.84f), new Color32(255, 248, 232, 250));
            AplicarTarjetaRedondeada(panel, new Color32(255, 248, 232, 250));
            panel.raycastTarget = false;
            CrearEtiqueta("Título de bienvenida", panel.transform, "Bienvenido a la selva", 36f, new Color(0.09f, 0.28f, 0.14f), new Vector2(0.07f, 0.82f), new Vector2(0.93f, 0.96f), TextAlignmentOptions.Center, true);

            _mensaje = CrearEtiqueta("Mensaje del Guardián", panel.transform, TextoBienvenida, 31f, new Color(0.08f, 0.2f, 0.14f), new Vector2(0.08f, 0.43f), new Vector2(0.92f, 0.79f), TextAlignmentOptions.Center, true);
            _mensaje.enableAutoSizing = true;
            _mensaje.fontSizeMin = 25f;
            _mensaje.fontSizeMax = 34f;
            _mensaje.textWrappingMode = TextWrappingModes.Normal;

            _botonAudio = CrearBotonTransparente("Botón AUDIO", panel.transform, new Vector2(0.07f, 0.16f), new Vector2(0.48f, 0.32f), "AUDIO  ·  ESCUCHAR", 24f, ReproducirMensajeActual);
            _botonAudio.interactable = _audioBienvenida != null;
            _estadoAudio = CrearEtiqueta("Estado del audio", panel.transform, "Escucha la bienvenida del Guardián", 16f, new Color(0.24f, 0.28f, 0.2f), new Vector2(0.07f, 0.08f), new Vector2(0.48f, 0.15f), TextAlignmentOptions.Center, false);

            _botonComenzar = CrearBotonTransparente("Botón COMENZAR", panel.transform, new Vector2(0.53f, 0.16f), new Vector2(0.93f, 0.32f), "COMENZAR", 27f, ComenzarAventura);
            _botonComenzarTexto = _botonComenzar.GetComponentInChildren<TMP_Text>();
        }

        private void CrearNavegacionInferior(Transform padre)
        {
            CrearBotonTransparente("Botón AYUDA", padre, new Vector2(0.647f, 0.008f), new Vector2(0.746f, 0.09f), "Ayuda", 32f, MostrarAyuda);
            CrearBotonTransparente("Botón SEÑAS", padre, new Vector2(0.75f, 0.008f), new Vector2(0.865f, 0.09f), "Señas", 32f, MostrarSenias);
            CrearBotonTransparente("Botón LECTURA", padre, new Vector2(0.87f, 0.008f), new Vector2(0.981f, 0.09f), "Lectura", 32f, MostrarLectura);
        }

        private static void CrearSelloInferior(Transform padre)
        {
            CrearPanelMarco("Placa de aprendizaje", padre, new Vector2(0.02f, 0.018f), new Vector2(0.24f, 0.09f), new Color32(104, 63, 29, 240));
            CrearEtiqueta("Lema de aprendizaje", padre, "Aprende a tu ritmo", 24f, new Color(1f, 0.96f, 0.82f), new Vector2(0.025f, 0.026f), new Vector2(0.23f, 0.08f), TextAlignmentOptions.Center, true);
        }

        private static void CrearPanelMarco(string nombre, Transform padre, Vector2 anclaMin, Vector2 anclaMax, Color color)
        {
            var panel = CrearImagen(nombre, padre, anclaMin, anclaMax, color);
            AplicarTarjetaRedondeada(panel, color);
            panel.raycastTarget = false;
        }

        private void ReproducirMensajeActual()
        {
            ReproducirClip(ObtenerAudioMensajeActual(), "El audio de este mensaje no está disponible.");
        }

        private AudioClip ObtenerAudioMensajeActual()
        {
            if (!_aventuraIniciada)
                return _audioBienvenida;

            return _audioContinuacion != null ? _audioContinuacion : _audioInicio;
        }

        private void ReproducirClip(AudioClip clip, string mensajeSinAudio)
        {
            if (clip == null)
            {
                if (_estadoAudio != null)
                    _estadoAudio.text = mensajeSinAudio;
                return;
            }

            if (_gestorAudio == null || !_gestorAudio.Reproducir(clip))
            {
                if (_estadoAudio != null)
                    _estadoAudio.text = mensajeSinAudio;
                return;
            }

            if (_estadoAudio != null)
                _estadoAudio.text = "Reproduciendo el mensaje…";
            CancelInvoke(nameof(IndicarFinAudio));
            Invoke(nameof(IndicarFinAudio), clip.length);
        }

        private void IndicarFinAudio()
        {
            if (_estadoAudio != null)
                _estadoAudio.text = "Puedes volver a escuchar el mensaje cuando quieras.";
        }

        private void ComenzarAventura()
        {
            if (_aventuraIniciada)
            {
                NavegacionEscenas.Cargar("IdentificarAlJaguar");
                return;
            }

            _aventuraIniciada = true;
            _controladorProgreso.Iniciar(TotalRetos);

            _mensaje.text = "¡Muy bien, explorador! Ya diste tu primer paso. El Guardián te acompaña por la selva.";
            _botonComenzar.interactable = true;
            _botonComenzarTexto.text = "IR A LA MISIÓN";
            _progreso.text = "1 de 12";
            _estadoAudio.text = "Presiona AUDIO para escuchar el nuevo mensaje.";
            _botonAudio.interactable = ObtenerAudioMensajeActual() != null;
        }

        private void RestaurarProgreso()
        {
            _controladorProgreso = GetComponent<ControladorProgresoNivel>();
            if (_controladorProgreso == null)
                _controladorProgreso = gameObject.AddComponent<ControladorProgresoNivel>();

            var datos = _controladorProgreso.Cargar(TotalRetos);
            _aventuraIniciada = datos.AventuraIniciada;
            _progreso.text = $"{datos.RetosCompletados} de {TotalRetos}";

            if (!_aventuraIniciada)
                return;

            _mensaje.text = "¡Hola de nuevo, explorador! Tu aventura por la selva continúa.";
            _botonComenzar.interactable = true;
            _botonComenzarTexto.text = "IR A LA MISIÓN";
            _botonAudio.interactable = ObtenerAudioMensajeActual() != null;
        }

        private void MostrarAyuda()
        {
            MostrarModal("UNA PISTA", TextoPista, false);
        }

        private void MostrarSenias()
        {
            MostrarSenias(_mensaje.text, "Mensaje del Guardián");
        }

        private void MostrarSenias(string texto, string titulo)
        {
            _textoSeniasActual = texto;
            _tituloSeniasActual = titulo;
            CerrarModal();
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
                return;

            _modalActual = new GameObject("Ventana Lengua de señas", typeof(RectTransform));
            _modalActual.transform.SetParent(canvas.transform, false);
            var oscurecedor = _modalActual.AddComponent<Image>();
            oscurecedor.color = new Color(0.02f, 0.11f, 0.1f, 0.78f);
            Estirar(_modalActual.GetComponent<RectTransform>());

            var panel = new GameObject("Tarjeta de lengua de señas", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(_modalActual.transform, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.15f, 0.1f);
            rect.anchorMax = new Vector2(0.85f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            AplicarTarjetaRedondeada(panel.GetComponent<Image>(), new Color(1f, 0.97f, 0.88f));
            panel.GetComponent<Image>().raycastTarget = false;

            CrearEtiqueta("Título LSEC", panel.transform, "LENGUA DE SEÑAS ECUATORIANA", 34f, new Color(0.09f, 0.28f, 0.14f), new Vector2(0.06f, 0.90f), new Vector2(0.94f, 0.98f), TextAlignmentOptions.Center, true);
            CrearEtiqueta("Alcance de prueba de señas", panel.transform, "ASL en inglés · Demo no validada · No es LSEC", 19f, new Color(0.34f, 0.35f, 0.3f), new Vector2(0.06f, 0.84f), new Vector2(0.94f, 0.90f), TextAlignmentOptions.Center, false);

            var visor = CrearImagen("Respuesta del servicio de señas", panel.transform, new Vector2(0.08f, 0.49f), new Vector2(0.92f, 0.81f), new Color(0.91f, 0.88f, 0.98f));
            AplicarTarjetaRedondeada(visor, new Color(0.91f, 0.88f, 0.98f));
            _estadoApiSenias = CrearEtiqueta("Estado de la API de señas", visor.transform, "Consulta de prueba: hello explorer welcome to the jungle", 22f, new Color(0.24f, 0.12f, 0.46f), new Vector2(0.05f, 0.57f), new Vector2(0.95f, 0.94f), TextAlignmentOptions.Center, true);
            _detalleApiSenias = CrearEtiqueta("Resultado de la API de señas", visor.transform, "El servicio separará las palabras con seña de las que deletrea.", 18f, new Color(0.22f, 0.23f, 0.3f), new Vector2(0.07f, 0.1f), new Vector2(0.93f, 0.60f), TextAlignmentOptions.Center, false);

            CrearEtiqueta("Texto en español para consulta", panel.transform, _textoSeniasActual, 22f, new Color(0.08f, 0.17f, 0.13f), new Vector2(0.08f, 0.39f), new Vector2(0.92f, 0.47f), TextAlignmentOptions.Center, true);

            CrearBotonLsec(panel.transform, "Consultar API", new Vector2(0.07f, 0.23f), new Vector2(0.35f, 0.33f), "CONSULTAR API", new Color(0.55f, 0.82f, 0.98f), true, ConsultarApiSenias);
            _botonAvatarApi = CrearBotonLsec(panel.transform, "Abrir avatar API", new Vector2(0.37f, 0.23f), new Vector2(0.65f, 0.33f), "VER AVATAR", new Color(0.77f, 0.91f, 0.69f), false, AbrirAvatarApi);
            var botonAmpliar = CrearBotonLsec(panel.transform, "Ampliar", new Vector2(0.67f, 0.23f), new Vector2(0.93f, 0.33f), "AMPLIAR", new Color(0.97f, 0.82f, 0.38f), true, AmpliarVisorLsec);
            _etiquetaAmpliarLsec = botonAmpliar.GetComponentInChildren<TMP_Text>();
            _panelSeniasRect = rect;
            _visorLsecAmpliado = false;
            CrearBotonModal("Cerrar señas", panel.transform, "VOLVER A LA ACTIVIDAD", new Vector2(0.25f, 0.07f), new Vector2(0.75f, 0.17f), CerrarModal);
        }

        [Serializable]
        private sealed class RespuestaApiSenias
        {
            public float duration;
            public string[] signed;
            public string[] spelled;
            public string viewer;
        }

        private void ConsultarApiSenias()
        {
            if (_solicitudApiSenias != null)
                return;

            _estadoApiSenias.text = "Conectando con el servicio de señas…";
            _detalleApiSenias.text = "Enviando únicamente una frase genérica de prueba.";
            _botonAvatarApi.interactable = false;
            _solicitudApiSenias = StartCoroutine(ConsultarServicioSenias());
        }

        private IEnumerator ConsultarServicioSenias()
        {
            var url = UrlApiSenias + UnityWebRequest.EscapeURL(FrasePruebaApi);
            using (var solicitud = UnityWebRequest.Get(url))
            {
                solicitud.timeout = 20;
                solicitud.SetRequestHeader("Accept", "application/json");
                yield return solicitud.SendWebRequest();

                if (_estadoApiSenias == null)
                    yield break;

                if (solicitud.result != UnityWebRequest.Result.Success)
                {
                    _estadoApiSenias.text = "No se pudo consultar el servicio.";
                    _detalleApiSenias.text = "Comprueba la conexión e inténtalo de nuevo. El prototipo no envía datos del estudiante.";
                    _solicitudApiSenias = null;
                    yield break;
                }

                var respuesta = JsonUtility.FromJson<RespuestaApiSenias>(solicitud.downloadHandler.text);
                if (respuesta == null)
                {
                    _estadoApiSenias.text = "La respuesta no tiene un formato válido.";
                    _solicitudApiSenias = null;
                    yield break;
                }

                var palabrasConSena = respuesta.signed ?? Array.Empty<string>();
                var palabrasDeletreadas = respuesta.spelled ?? Array.Empty<string>();
                _estadoApiSenias.text = $"API conectada · duración {respuesta.duration:0.0} s";
                _detalleApiSenias.text = $"Con seña ASL: {TextoLista(palabrasConSena)}\nDeletreadas: {TextoLista(palabrasDeletreadas)}\nEl avatar animado se abre en el navegador.";
                _urlAvatarApi = respuesta.viewer;
                _botonAvatarApi.interactable = !string.IsNullOrWhiteSpace(_urlAvatarApi);
                _solicitudApiSenias = null;
            }
        }

        private static string TextoLista(string[] palabras)
        {
            return palabras.Length == 0 ? "ninguna" : string.Join(", ", palabras);
        }

        private void AbrirAvatarApi()
        {
            if (Uri.IsWellFormedUriString(_urlAvatarApi, UriKind.Absolute) && _urlAvatarApi.StartsWith("https://three.ws/", StringComparison.OrdinalIgnoreCase))
                Application.OpenURL(_urlAvatarApi);
        }

        private Button CrearBotonLsec(Transform padre, string nombre, Vector2 anclaMin, Vector2 anclaMax, string texto, Color color, bool activo, UnityEngine.Events.UnityAction accion)
        {
            var imagen = CrearImagen(nombre, padre, anclaMin, anclaMax, color);
            AplicarTarjetaRedondeada(imagen, color);
            var boton = imagen.gameObject.AddComponent<Button>();
            boton.interactable = activo;
            ControladorBotonAccesible.Configurar(boton, nombre, accion);

            var etiqueta = CrearEtiqueta("Texto " + nombre, imagen.transform, texto, 18f, new Color(0.08f, 0.17f, 0.13f), Vector2.zero, Vector2.one, TextAlignmentOptions.Center, true);
            etiqueta.textWrappingMode = TextWrappingModes.NoWrap;
            etiqueta.enableAutoSizing = true;
            etiqueta.fontSizeMin = 14f;
            etiqueta.fontSizeMax = 18f;
            etiqueta.raycastTarget = false;
            return boton;
        }

        private void AmpliarVisorLsec()
        {
            if (_panelSeniasRect == null)
                return;

            _visorLsecAmpliado = !_visorLsecAmpliado;
            _panelSeniasRect.anchorMin = _visorLsecAmpliado ? new Vector2(0.04f, 0.025f) : new Vector2(0.15f, 0.1f);
            _panelSeniasRect.anchorMax = _visorLsecAmpliado ? new Vector2(0.96f, 0.975f) : new Vector2(0.85f, 0.9f);
            _etiquetaAmpliarLsec.text = _visorLsecAmpliado ? "VISTA NORMAL" : "AMPLIAR VISOR";
        }
        private void MostrarLectura()
        {
            MostrarModal("LECTURA", _mensaje.text, true);
        }

        private void MostrarModal(string titulo, string texto, bool incluirAudio)
        {
            _audioModalActual = titulo == "LECTURA" ? ObtenerAudioMensajeActual() :
                titulo == "UNA PISTA" ? _audioPista : _audioBienvenida;
            CerrarModal();
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
                return;

            _modalActual = new GameObject("Ventana " + titulo, typeof(RectTransform));
            _modalActual.transform.SetParent(canvas.transform, false);
            var fondoOscuro = _modalActual.AddComponent<Image>();
            fondoOscuro.color = new Color(0.02f, 0.11f, 0.1f, 0.78f);
            Estirar(_modalActual.GetComponent<RectTransform>());

            var panel = new GameObject("Tarjeta del mensaje", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(_modalActual.transform, false);
            var rectPanel = panel.GetComponent<RectTransform>();
            rectPanel.anchorMin = new Vector2(0.2f, 0.19f);
            rectPanel.anchorMax = new Vector2(0.8f, 0.81f);
            rectPanel.offsetMin = Vector2.zero;
            rectPanel.offsetMax = Vector2.zero;
            AplicarTarjetaRedondeada(panel.GetComponent<Image>(), new Color(1f, 0.97f, 0.88f));

            CrearEtiqueta("Título de ayuda", panel.transform, titulo, 40f, new Color(0.09f, 0.28f, 0.14f), new Vector2(0.08f, 0.79f), new Vector2(0.92f, 0.94f), TextAlignmentOptions.Center, true);
            var cuerpo = CrearEtiqueta("Texto de ayuda", panel.transform, texto, 32f, new Color(0.08f, 0.17f, 0.13f), new Vector2(0.09f, 0.40f), new Vector2(0.91f, 0.76f), TextAlignmentOptions.Center, true);
            cuerpo.enableAutoSizing = true;
            cuerpo.fontSizeMin = 24f;
            cuerpo.fontSizeMax = 34f;
            cuerpo.textWrappingMode = TextWrappingModes.Normal;

            CrearBotonModal("Cerrar ayuda", panel.transform, "VOLVER A LA SELVA", new Vector2(0.22f, 0.08f), new Vector2(0.78f, 0.21f), CerrarModal);
            if (incluirAudio)
                CrearBotonModal("Escuchar lectura", panel.transform, "AUDIO  ·  ESCUCHAR", new Vector2(0.28f, 0.25f), new Vector2(0.72f, 0.36f), () => ReproducirClip(_audioModalActual, "El audio de la lectura no está disponible."));
            else if (titulo == "UNA PISTA")
            {
                CrearBotonModal("Escuchar pista", panel.transform, "ESCUCHAR PISTA", new Vector2(0.08f, 0.25f), new Vector2(0.47f, 0.36f), () => ReproducirClip(_audioModalActual, "El audio de la pista no está disponible."));
                CrearBotonModal("Señas de la pista", panel.transform, "VER DEMO DE SEÑAS", new Vector2(0.53f, 0.25f), new Vector2(0.92f, 0.36f), () => MostrarSenias(TextoPista, "Pista de la actividad"));
            }
        }

        private void CerrarModal()
        {
            if (_solicitudApiSenias != null)
            {
                StopCoroutine(_solicitudApiSenias);
                _solicitudApiSenias = null;
            }

            if (_modalActual != null)
                Destroy(_modalActual);
            _modalActual = null;
            _estadoApiSenias = null;
            _detalleApiSenias = null;
            _botonAvatarApi = null;
            _panelSeniasRect = null;
        }

        private Button CrearBotonModal(string nombre, Transform padre, string texto, Vector2 anclaMin, Vector2 anclaMax, UnityEngine.Events.UnityAction accion)
        {
            var fondo = CrearImagen(nombre, padre, anclaMin, anclaMax, new Color(0.98f, 0.68f, 0.08f));
            AplicarTarjetaRedondeada(fondo, new Color(0.98f, 0.68f, 0.08f));
            var boton = fondo.gameObject.AddComponent<Button>();
            ControladorBotonAccesible.Configurar(boton, nombre, accion);
            var etiqueta = CrearEtiqueta("Texto " + nombre, fondo.transform, texto, 25f, new Color(0.1f, 0.17f, 0.1f), Vector2.zero, Vector2.one, TextAlignmentOptions.Center, true);
            etiqueta.raycastTarget = false;
            return boton;
        }

        private Button CrearBotonTransparente(string nombre, Transform padre, Vector2 anclaMin, Vector2 anclaMax, string texto, float tamano, UnityEngine.Events.UnityAction accion)
        {
            var colorFondo = nombre.Contains("AUDIO") ? new Color32(167, 221, 247, 255) :
                nombre.Contains("SEÑAS") ? new Color32(179, 136, 255, 255) :
                nombre.Contains("LECTURA") ? new Color32(167, 221, 247, 255) :
                nombre.Contains("AYUDA") ? new Color32(255, 248, 232, 255) :
                new Color32(246, 196, 69, 255);
            var imagen = CrearImagen(nombre, padre, anclaMin, anclaMax, colorFondo);
            AplicarTarjetaRedondeada(imagen, colorFondo);
            var boton = imagen.gameObject.AddComponent<Button>();
            boton.transition = Selectable.Transition.ColorTint;
            boton.targetGraphic = imagen;
            ColorBlock colores = boton.colors;
            colores.normalColor = Color.white;
            colores.highlightedColor = new Color32(232, 255, 220, 255);
            colores.pressedColor = new Color32(211, 235, 197, 255);
            colores.selectedColor = colores.highlightedColor;
            colores.fadeDuration = 0.12f;
            boton.colors = colores;
            ControladorBotonAccesible.Configurar(boton, nombre, accion);
            var etiqueta = CrearEtiqueta("Texto " + nombre, imagen.transform, texto, tamano, new Color(0.07f, 0.15f, 0.1f), Vector2.zero, Vector2.one, TextAlignmentOptions.Center, true);
            etiqueta.enableAutoSizing = true;
            etiqueta.fontSizeMin = Mathf.Max(16f, tamano * 0.72f);
            etiqueta.fontSizeMax = tamano;
            etiqueta.textWrappingMode = TextWrappingModes.NoWrap;
            etiqueta.overflowMode = TextOverflowModes.Truncate;
            etiqueta.raycastTarget = false;
            return boton;
        }

        private static Image CrearImagen(string nombre, Transform padre, Vector2 anclaMin, Vector2 anclaMax, Color color)
        {
            var objeto = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            objeto.transform.SetParent(padre, false);
            var rect = objeto.GetComponent<RectTransform>();
            rect.anchorMin = anclaMin;
            rect.anchorMax = anclaMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var imagen = objeto.GetComponent<Image>();
            imagen.color = color;
            return imagen;
        }

        private static TMP_Text CrearEtiqueta(string nombre, Transform padre, string texto, float tamano, Color color, Vector2 anclaMin, Vector2 anclaMax, TextAlignmentOptions alineacion, bool negrita)
        {
            var objeto = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            objeto.transform.SetParent(padre, false);
            var rect = objeto.GetComponent<RectTransform>();
            rect.anchorMin = anclaMin;
            rect.anchorMax = anclaMax;
            rect.offsetMin = new Vector2(7f, 3f);
            rect.offsetMax = new Vector2(-7f, -3f);
            var etiqueta = objeto.GetComponent<TextMeshProUGUI>();
            etiqueta.overflowMode = TextOverflowModes.Truncate;
            etiqueta.text = texto;
            etiqueta.font = _tipografiaCompartida != null ? _tipografiaCompartida : TMP_Settings.defaultFontAsset;
            etiqueta.fontSize = tamano;
            etiqueta.color = color;
            etiqueta.alignment = alineacion;
            etiqueta.characterSpacing = 0f;
            etiqueta.wordSpacing = 0f;
            etiqueta.textWrappingMode = TextWrappingModes.Normal;
            etiqueta.enableAutoSizing = true;
            etiqueta.fontSizeMin = Mathf.Max(12f, tamano * 0.68f);
            etiqueta.fontSizeMax = tamano;
            etiqueta.overflowMode = TextOverflowModes.Truncate;
            if (negrita)
                etiqueta.fontStyle = FontStyles.Bold;
            return etiqueta;
        }

        private static void AplicarTarjetaRedondeada(Image imagen, Color tinte)
        {
            if (_spriteTarjetaRedondeada == null)
            {
                const int size = 96;
                const float radius = 23f;
                const float border = 5f;
                var textura = new Texture2D(size, size, TextureFormat.RGBA32, false);
                textura.name = "Sprite 9-slice adaptable";
                textura.filterMode = FilterMode.Bilinear;
                textura.wrapMode = TextureWrapMode.Clamp;

                for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var px = x + 0.5f;
                    var py = y + 0.5f;
                    var dentroExterior = DentroRectanguloRedondeado(px, py, 0f, radius, size);
                    var dentroInterior = DentroRectanguloRedondeado(px, py, border, radius - border, size);
                    textura.SetPixel(x, y, !dentroExterior ? new Color(0f, 0f, 0f, 0f) :
                        dentroInterior ? new Color(1f, 0.985f, 0.91f, 1f) : new Color(0.84f, 0.52f, 0.16f, 1f));
                }

                textura.Apply(false, true);
                _spriteTarjetaRedondeada = Sprite.Create(textura, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0u, SpriteMeshType.FullRect, new Vector4(27f, 27f, 27f, 27f));
                _spriteTarjetaRedondeada.name = "Marco redondeado escalable 9-slice";
            }

            imagen.sprite = _spriteTarjetaRedondeada;
            imagen.type = Image.Type.Sliced;
            imagen.color = tinte;
        }

        private static bool DentroRectanguloRedondeado(float x, float y, float inset, float radius, float size)
        {
            var centro = size * 0.5f;
            var mitad = centro - inset;
            var qx = Mathf.Abs(x - centro) - (mitad - radius);
            var qy = Mathf.Abs(y - centro) - (mitad - radius);
            var exterior = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
            var distancia = exterior + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
            return distancia <= 0f;
        }

        private static void Estirar(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}



