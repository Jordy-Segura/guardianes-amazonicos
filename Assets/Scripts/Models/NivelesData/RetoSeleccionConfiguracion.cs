using TMPro;
using UnityEngine;

namespace GuardianesAmazonicos.Models
{
    [CreateAssetMenu(fileName = "RetoSeleccion", menuName = "Guardianes/Reto de selección")]
    public sealed class RetoSeleccionConfiguracion : ScriptableObject
    {
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
        public Sprite explorador;
        public TMP_FontAsset fuente;
    }
}
