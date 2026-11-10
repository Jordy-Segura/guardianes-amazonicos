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
        [SerializeField] private Sprite _elementoEscena;
        [Tooltip("x, y, ancho y alto dentro de la imagen principal (origen arriba a la izquierda, Y negativa).")]
        [SerializeField] private Rect _rectElementoEscena;

        [Header("Fondo y personaje guía")]
        [SerializeField] private Sprite _fondo;
        [SerializeField] private Sprite _personaje;
        [SerializeField] private Sprite[] _fotogramasCelebracion;

        [Header("Accesibilidad")]
        [SerializeField] private AudioClip _audioLectura;
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

        public int Nivel { get => _nivel; set => _nivel = value; }
        public int NumeroEscena { get => _numeroEscena; set => _numeroEscena = value; }
        public int TotalEscenas { get => _totalEscenas; set => _totalEscenas = value; }
        public string TituloNivel { get => _tituloNivel; set => _tituloNivel = value; }
        public string SiguienteEscena { get => _siguienteEscena; set => _siguienteEscena = value; }
        public string Pregunta { get => _pregunta; set => _pregunta = value; }
        public string[] Opciones { get => _opciones; set => _opciones = value; }
        public Sprite[] ImagenesOpciones { get => _imagenesOpciones; set => _imagenesOpciones = value; }
        public int IndiceCorrecto { get => _indiceCorrecto; set => _indiceCorrecto = value; }
        public string[] ExplicacionesIncorrectas { get => _explicacionesIncorrectas; set => _explicacionesIncorrectas = value; }
        public string Pista { get => _pista; set => _pista = value; }
        public string Felicitacion { get => _felicitacion; set => _felicitacion = value; }
        public Sprite ImagenEscena { get => _imagenEscena; set => _imagenEscena = value; }
        public Sprite ElementoEscena { get => _elementoEscena; set => _elementoEscena = value; }
        public Rect RectElementoEscena { get => _rectElementoEscena; set => _rectElementoEscena = value; }
        public Sprite Fondo { get => _fondo; set => _fondo = value; }
        public Sprite Personaje { get => _personaje; set => _personaje = value; }
        public Sprite[] FotogramasCelebracion { get => _fotogramasCelebracion; set => _fotogramasCelebracion = value; }
        public AudioClip AudioLectura { get => _audioLectura; set => _audioLectura = value; }
        public string VideoSenas { get => _videoSenas; set => _videoSenas = value; }
        public bool MostrarBienvenida { get => _mostrarBienvenida; set => _mostrarBienvenida = value; }
        public string BienvenidaEtiquetaNivel { get => _bienvenidaEtiquetaNivel; set => _bienvenidaEtiquetaNivel = value; }
        public string BienvenidaNombreNivel { get => _bienvenidaNombreNivel; set => _bienvenidaNombreNivel = value; }
        public string BienvenidaSaludo { get => _bienvenidaSaludo; set => _bienvenidaSaludo = value; }
        public string BienvenidaDescripcion { get => _bienvenidaDescripcion; set => _bienvenidaDescripcion = value; }
        public Sprite BienvenidaImagen { get => _bienvenidaImagen; set => _bienvenidaImagen = value; }
        public AudioClip BienvenidaAudio { get => _bienvenidaAudio; set => _bienvenidaAudio = value; }
        public string BienvenidaVideoSenas { get => _bienvenidaVideoSenas; set => _bienvenidaVideoSenas = value; }
        public TMP_FontAsset Fuente { get => _fuente; set => _fuente = value; }

        /// <summary>Explicación breve y positiva para la opción incorrecta elegida.</summary>
        public string ObtenerExplicacion(int indice)
        {
            if (_explicacionesIncorrectas != null && indice >= 0 && indice < _explicacionesIncorrectas.Length
                && !string.IsNullOrWhiteSpace(_explicacionesIncorrectas[indice]))
            {
                return _explicacionesIncorrectas[indice];
            }

            return "Esa no es la respuesta. Observa otra vez y vuelve a intentarlo.";
        }
    }
}
