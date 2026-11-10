using TMPro;
using UnityEngine;

namespace GuardianesAmazonicos.Models
{
    [CreateAssetMenu(fileName = "RetoSeleccion", menuName = "Guardianes/Reto de selección")]
    public sealed class RetoSeleccionConfiguracion : ScriptableObject
    {
        [SerializeField, Min(1)] private int _nivel = 1;
        [SerializeField] private Texture2D _atlasOpciones;
        [SerializeField, Range(1, 4)] private int _columnasAtlas = 2;
        [SerializeField, Range(1, 4)] private int _filasAtlas = 2;

        public int Nivel { get => _nivel; set => _nivel = value; }
        public Texture2D AtlasOpciones { get => _atlasOpciones; set => _atlasOpciones = value; }
        public int ColumnasAtlas { get => _columnasAtlas; set => _columnasAtlas = value; }
        public int FilasAtlas { get => _filasAtlas; set => _filasAtlas = value; }

        [Min(1)] public int numeroEscena = 1;
        public string titulo = "Reto de la selva";
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
    }
}
