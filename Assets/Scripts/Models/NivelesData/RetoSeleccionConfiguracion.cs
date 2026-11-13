using TMPro;
using UnityEngine;

namespace GuardianesAmazonicos.Models
{
    /// <summary>
    /// Contenido variable de una actividad que usa la plantilla base aprobada:
    /// enunciado, escena de la pregunta, tres tarjetas, explicaciones, pista y
    /// recursos de accesibilidad. Los marcos, botones y coordenadas no se guardan
    /// aquí porque son fijos para todos los niveles.
    /// </summary>
    [CreateAssetMenu(fileName = "RetoSeleccion", menuName = "Guardianes/Reto de selección")]
    public sealed class RetoSeleccionConfiguracion : ScriptableObject
    {
        public const int CANTIDAD_OPCIONES = 3;

        [Header("Ubicación en la aventura")]
        [SerializeField, Min(1)] private int _nivel = 1;
        [SerializeField, Min(1)] private int _numeroEscena = 1;
        [SerializeField, Min(1)] private int _totalEscenas = 12;
        [SerializeField] private string _tituloNivel = "Nivel 1 • Conozco la selva";
        [Tooltip("Vacío: se carga la siguiente escena de Build Profiles.")]
        [SerializeField] private string _siguienteEscena;

        [Header("Pregunta y respuestas")]
        [SerializeField, TextArea(2, 4)] private string _pregunta;
        [SerializeField] private string[] _opciones = new string[CANTIDAD_OPCIONES];
        [SerializeField] private Sprite[] _imagenesOpciones = new Sprite[CANTIDAD_OPCIONES];
        [SerializeField, Range(0, CANTIDAD_OPCIONES - 1)] private int _indiceCorrecto;
        [SerializeField, TextArea(2, 4)] private string[] _explicacionesIncorrectas = new string[CANTIDAD_OPCIONES];
        [SerializeField, TextArea(2, 4)] private string _pista;
        [SerializeField] private string _felicitacion = "¡Lo lograste!";

        [Header("Escena de la pregunta (PNG horizontal sin texto)")]
        [SerializeField] private Sprite _imagenEscena;
        [SerializeField] private bool _mostrarImagenEscena = true;
        [SerializeField] private Sprite _elementoEscena;
        [Tooltip("x, y, ancho y alto dentro de la imagen principal (origen arriba a la izquierda, Y negativa).")]
        [SerializeField] private Rect _rectElementoEscena;

        [Header("Fondo y personaje guía")]
        [SerializeField] private Sprite _fondo;
        [SerializeField] private Sprite _personaje;
        [SerializeField] private Sprite _personajeIncorrecto;
        [SerializeField] private Sprite[] _fotogramasCelebracion;

        [Header("Accesibilidad")]
        [SerializeField] private AudioClip _audioLectura;
        [SerializeField] private AudioClip _audioPista;
        [SerializeField] private AudioClip[] _audioCorrecciones = new AudioClip[CANTIDAD_OPCIONES];
        [SerializeField] private AudioClip _audioFelicitacion;
        [Tooltip("Archivo en StreamingAssets/LSEC grabado con la intérprete aprobada.")]
        [SerializeField] private string _videoSenas;

        [Header("Bienvenida del nivel (solo en la primera escena del nivel)")]
        [SerializeField] private bool _mostrarBienvenida;
        [SerializeField] private string _bienvenidaEtiquetaNivel;
        [SerializeField] private string _bienvenidaNombreNivel;
        [SerializeField] private string _bienvenidaSaludo = "¡Hola, guardián!";
        [SerializeField, TextArea(2, 4)] private string _bienvenidaDescripcion;
        [SerializeField] private Sprite _bienvenidaImagen;
        [SerializeField] private AudioClip _bienvenidaAudio;
        [SerializeField] private string _bienvenidaVideoSenas;

        [Header("Texto")]
        [SerializeField] private TMP_FontAsset _fuente;

        // Campos compatibles con serialización directa de retos (HU-02)
        [Min(1)] public int numeroEscena = 1;
        public string titulo = "Nivel 1. Conozco la Selva";
        public string textoProgreso = "2 de 2";
        [TextArea(1, 2)] public string pregunta;
        [TextArea(1, 2)] public string instruccion;
        public string[] opciones;
        public Sprite[] imagenes;
        public Texture2D atlasDosPorDos;
        [Range(0, 3)] public int indiceCorrecto;
        [TextArea(1, 2)] public string pistaIncorrecta;
        [TextArea(1, 2)] public string mensajeCorrecto;
        public AudioClip[] pronunciaciones;
        public string siguienteEscena;
        public string textoContinuar = "SIGUIENTE RETO";
        public Sprite fondo;
        public Sprite imagenEscena;
        public Sprite explorador;
        public TMP_FontAsset fuente;

        public int Nivel { get => _nivel; set => _nivel = value; }
        public int NumeroEscena
        {
            get => _numeroEscena > 1 ? _numeroEscena : (numeroEscena > 1 ? numeroEscena : _numeroEscena);
            set { _numeroEscena = value; numeroEscena = value; }
        }
        public int TotalEscenas { get => _totalEscenas; set => _totalEscenas = value; }
        public string TituloNivel
        {
            get => !string.IsNullOrEmpty(_tituloNivel) && _tituloNivel != "Nivel 1 • Conozco la selva" ? _tituloNivel : (!string.IsNullOrEmpty(titulo) ? titulo : _tituloNivel);
            set { _tituloNivel = value; titulo = value; }
        }
        public string SiguienteEscena
        {
            get => !string.IsNullOrEmpty(_siguienteEscena) ? _siguienteEscena : siguienteEscena;
            set { _siguienteEscena = value; siguienteEscena = value; }
        }
        public string Pregunta
        {
            get => !string.IsNullOrEmpty(_pregunta) ? _pregunta : pregunta;
            set { _pregunta = value; pregunta = value; }
        }
        public string[] Opciones
        {
            get => (_opciones != null && _opciones.Length > 0 && !string.IsNullOrEmpty(_opciones[0])) ? _opciones : opciones;
            set { _opciones = value; opciones = value; }
        }
        public Sprite[] ImagenesOpciones
        {
            get => (_imagenesOpciones != null && _imagenesOpciones.Length > 0 && _imagenesOpciones[0] != null) ? _imagenesOpciones : imagenes;
            set { _imagenesOpciones = value; imagenes = value; }
        }
        public int IndiceCorrecto
        {
            get => _indiceCorrecto != 0 ? _indiceCorrecto : indiceCorrecto;
            set { _indiceCorrecto = value; indiceCorrecto = value; }
        }
        public string[] ExplicacionesIncorrectas { get => _explicacionesIncorrectas; set => _explicacionesIncorrectas = value; }
        public string Pista
        {
            get => !string.IsNullOrEmpty(_pista) ? _pista : pistaIncorrecta;
            set { _pista = value; pistaIncorrecta = value; }
        }
        public string Felicitacion
        {
            get => !string.IsNullOrEmpty(_felicitacion) && _felicitacion != "¡Lo lograste!" ? _felicitacion : (!string.IsNullOrEmpty(mensajeCorrecto) ? mensajeCorrecto : _felicitacion);
            set { _felicitacion = value; mensajeCorrecto = value; }
        }
        public Sprite ImagenEscena
        {
            get => _imagenEscena != null ? _imagenEscena : imagenEscena;
            set { _imagenEscena = value; imagenEscena = value; }
        }
        public bool MostrarImagenEscena { get => _mostrarImagenEscena; set => _mostrarImagenEscena = value; }
        public Sprite ElementoEscena { get => _elementoEscena; set => _elementoEscena = value; }
        public Rect RectElementoEscena { get => _rectElementoEscena; set => _rectElementoEscena = value; }
        public Sprite Fondo
        {
            get => _fondo != null ? _fondo : fondo;
            set { _fondo = value; fondo = value; }
        }
        public Sprite Personaje
        {
            get => _personaje != null ? _personaje : explorador;
            set { _personaje = value; explorador = value; }
        }
        public Sprite PersonajeIncorrecto { get => _personajeIncorrecto; set => _personajeIncorrecto = value; }
        public Sprite[] FotogramasCelebracion { get => _fotogramasCelebracion; set => _fotogramasCelebracion = value; }
        public AudioClip AudioLectura { get => _audioLectura; set => _audioLectura = value; }
        public AudioClip AudioPista { get => _audioPista; set => _audioPista = value; }
        public AudioClip AudioFelicitacion { get => _audioFelicitacion; set => _audioFelicitacion = value; }
        public AudioClip[] AudioCorrecciones { get => _audioCorrecciones; set => _audioCorrecciones = value; }
        public AudioClip ObtenerAudioCorreccion(int indice)
        {
            return _audioCorrecciones != null && indice >= 0 && indice < _audioCorrecciones.Length
                ? _audioCorrecciones[indice]
                : null;
        }
        public string VideoSenas { get => _videoSenas; set => _videoSenas = value; }
        public bool MostrarBienvenida { get => _mostrarBienvenida; set => _mostrarBienvenida = value; }
        public string BienvenidaEtiquetaNivel { get => _bienvenidaEtiquetaNivel; set => _bienvenidaEtiquetaNivel = value; }
        public string BienvenidaNombreNivel { get => _bienvenidaNombreNivel; set => _bienvenidaNombreNivel = value; }
        public string BienvenidaSaludo { get => _bienvenidaSaludo; set => _bienvenidaSaludo = value; }
        public string BienvenidaDescripcion { get => _bienvenidaDescripcion; set => _bienvenidaDescripcion = value; }
        public Sprite BienvenidaImagen { get => _bienvenidaImagen; set => _bienvenidaImagen = value; }
        public AudioClip BienvenidaAudio { get => _bienvenidaAudio; set => _bienvenidaAudio = value; }
        public string BienvenidaVideoSenas { get => _bienvenidaVideoSenas; set => _bienvenidaVideoSenas = value; }
        public TMP_FontAsset Fuente
        {
            get => _fuente != null ? _fuente : fuente;
            set { _fuente = value; fuente = value; }
        }

        /// <summary>Explicación breve y positiva para la opción incorrecta elegida.</summary>
        public string ObtenerExplicacion(int indice)
        {
            if (_explicacionesIncorrectas != null && indice >= 0 && indice < _explicacionesIncorrectas.Length
                && !string.IsNullOrWhiteSpace(_explicacionesIncorrectas[indice]))
            {
                return _explicacionesIncorrectas[indice];
            }

            if (!string.IsNullOrWhiteSpace(pistaIncorrecta))
            {
                return pistaIncorrecta;
            }

            return "Esa no es la respuesta. Observa otra vez y vuelve a intentarlo.";
        }
    }
}
