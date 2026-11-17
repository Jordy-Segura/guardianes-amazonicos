using GuardianesAmazonicos.Models;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace GuardianesAmazonicos.Editor
{
    /// <summary>
    /// Genera la actividad HU-10 (Nivel 1 – Escena 10): reconocer al loro entre
    /// cuatro animales. Usa la plantilla común, que ya contempla cuatro tarjetas
    /// en dos filas, el audio por estado y el panel de configuración.
    /// </summary>
    public static class BuildSceneHU10
    {
        public const string ConfigLoroPath = "Assets/Scripts/Models/NivelesData/RetoLoro.asset";
        public const string EscenaLoroPath = "Assets/Scenes/Niveles/Nivel01/Escena10_HU10_Loro.unity";

        private const string FondoNivel1 = "Assets/Art/Backgrounds/Niveles/N1_Conozco_la_selva.png";
        private const string Explorador = "Assets/Art/Characters/Explorador.png";
        private const string ExploradorPreocupado = "Assets/Art/Characters/ExploradorPreocupado.png";
        private const string Loro = "Assets/Art/Characters/Loro.png";
        private const string Guacamayo = "Assets/Art/Characters/Guacamayo.png";
        private const string Jaguar = "Assets/Art/Characters/Jaguar.png";
        private const string Rana = "Assets/Art/Characters/Rana.png";
        private const string Audios = "Assets/Audio/Voice/HU10/";

        [MenuItem("Guardianes/Sahid/Generar Escena 10 (HU-10 Loro)")]
        public static void Generar()
        {
            BuildAssignedScenes.PrepararRecursos();
            PlantillaUIBuilder.ImportarComoSprite(Loro);
            PlantillaUIBuilder.ImportarComoSprite(Rana);

            RetoSeleccionConfiguracion configuracion =
                AssetDatabase.LoadAssetAtPath<RetoSeleccionConfiguracion>(ConfigLoroPath);
            if (configuracion == null)
            {
                configuracion = ScriptableObject.CreateInstance<RetoSeleccionConfiguracion>();
                AssetDatabase.CreateAsset(configuracion, ConfigLoroPath);
            }

            configuracion.Nivel = 1;
            configuracion.NumeroEscena = 10;
            configuracion.TotalEscenas = 12;
            configuracion.TituloNivel = "Nivel 1 • Conozco la selva";
            configuracion.Pregunta = "Observa los animales de la selva. ¿Cuál de estos animales es el loro?";
            configuracion.Opciones = new[] { "Loro", "Guacamayo", "Jaguar", "Rana" };
            configuracion.ImagenesOpciones = new[]
            {
                PlantillaUIBuilder.S(Loro),
                PlantillaUIBuilder.S(Guacamayo),
                PlantillaUIBuilder.S(Jaguar),
                PlantillaUIBuilder.S(Rana)
            };
            configuracion.IndiceCorrecto = 0;
            configuracion.ExplicacionesIncorrectas = new[]
            {
                string.Empty,
                "Ese es el guacamayo, de plumas rojas y azules. Busca el ave de plumas verdes.",
                "Ese es el jaguar, el gran felino con manchas de la selva. Busca el ave de plumas verdes.",
                "Esa es la rana, que salta cerca del agua. Busca el ave de plumas verdes."
            };
            configuracion.Pista = "Busca el ave de plumas verdes y pico curvo que repite palabras.";
            configuracion.Felicitacion = "¡Muy bien! Ese es el loro.";
            configuracion.ImagenEscena = null;
            configuracion.ElementoEscena = null;
            configuracion.RectElementoEscena = Rect.zero;
            configuracion.Fondo = PlantillaUIBuilder.S(FondoNivel1);
            configuracion.Personaje = PlantillaUIBuilder.S(Explorador);
            configuracion.PersonajeIncorrecto = PlantillaUIBuilder.S(ExploradorPreocupado);
            configuracion.FotogramasCelebracion = BuildAssignedScenes.CargarFotogramasExplorador();
            configuracion.AudioLectura = PlantillaUIBuilder.Cargar<AudioClip>(Audios + "lectura_01_10.wav");
            configuracion.AudioPista = PlantillaUIBuilder.Cargar<AudioClip>(Audios + "pista_01_10.wav");
            configuracion.AudioFelicitacion = PlantillaUIBuilder.Cargar<AudioClip>(Audios + "acierto_01_10.wav");
            configuracion.AudioCorrecciones = new[]
            {
                null,
                PlantillaUIBuilder.Cargar<AudioClip>(Audios + "correccion_guacamayo_01_10.wav"),
                PlantillaUIBuilder.Cargar<AudioClip>(Audios + "correccion_jaguar_01_10.wav"),
                PlantillaUIBuilder.Cargar<AudioClip>(Audios + "correccion_rana_01_10.wav")
            };
            configuracion.VideoSenas = "senas_nivel_01_pregunta_10.mp4";
            configuracion.MostrarBienvenida = false;
            configuracion.SiguienteEscena = "Escena11_HU11_ReconocerArbol";
            configuracion.Fuente = PlantillaUIBuilder.Cargar<TMP_FontAsset>(PlantillaUIBuilder.FUENTE);
            configuracion.textoProgreso = "10 de 12";
            EditorUtility.SetDirty(configuracion);

            AssetDatabase.SaveAssets();
            BuildAssignedScenes.CrearEscena(EscenaLoroPath, configuracion);
            IntegrarNivel01.AplicarOrdenDeEscenas();
            Debug.Log("HU-10: escena del loro generada en " + EscenaLoroPath);
        }
    }
}
