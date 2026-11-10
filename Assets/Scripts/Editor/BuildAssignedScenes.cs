using System.Collections.Generic;
using System.Linq;
using GuardianesAmazonicos.Models;
using GuardianesAmazonicos.Presenters;
using GuardianesAmazonicos.Views;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace GuardianesAmazonicos.Editor
{
    public static class BuildAssignedScenes
    {
        private const string ConfigMonoPath = "Assets/Scripts/Models/NivelesData/RetoMono.asset";
        private const string SceneMonoPath = "Assets/Scenes/Niveles/Nivel01/Escena02_HU02_Mono.unity";

        [MenuItem("Guardianes/Generar actividad del Mono (HU-02)")]
        public static void GenerarActividadMonoHU02()
        {
            ConfigurarImportadores();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            TMP_FontAsset fuente = Cargar<TMP_FontAsset>("Assets/Fonts/OpenDyslexic3-Regular SDF.asset");
            Sprite fondo = Cargar<Sprite>("Assets/Art/Backgrounds/N1_ConozcoLaSelva.png");
            Sprite explorador = Cargar<Sprite>("Assets/Art/Characters/Explorador.png");

            RetoSeleccionConfiguracion mono = CrearConfiguracion(ConfigMonoPath);
            mono.Nivel = 1;
            mono.numeroEscena = 2;
            mono.titulo = "Nivel 1. Conozco la Selva";
            mono.textoProgreso = "2 de 2";
            mono.pregunta = "¿Cual de estos animales es el mono?";
            mono.instruccion = "Toca la imagen del mono.";
            mono.opciones = new[] { "Mono", "Guacamayo", "Tortuga" };
            mono.imagenes = new[]
            {
                Cargar<Sprite>("Assets/Art/Characters/Mono.png"),
                Cargar<Sprite>("Assets/Art/Characters/Guacamayo.png"),
                Cargar<Sprite>("Assets/Art/Characters/Tortuga.png")
            };
            mono.atlasDosPorDos = null;
            mono.AtlasOpciones = null;
            mono.ColumnasAtlas = 3;
            mono.FilasAtlas = 1;
            mono.indiceCorrecto = 0;
            mono.pistaIncorrecta = "Busca al animal de cola larga que trepa y salta entre las ramas.";
            mono.mensajeCorrecto = "¡Muy bien! Reconociste al mono.";
            mono.pronunciaciones = null;
            mono.siguienteEscena = "Escena03_HU03_Guacamayo";
            mono.textoContinuar = "SIGUIENTE RETO";
            mono.fondo = fondo;
            mono.imagenEscena = Cargar<Sprite>("Assets/Art/Backgrounds/EscenaSelvaHabitat.png");
            mono.explorador = explorador;
            mono.fuente = fuente;
            EditorUtility.SetDirty(mono);

            CrearEscena(SceneMonoPath, mono);
            List<EditorBuildSettingsScene> escenas = new List<EditorBuildSettingsScene>();
            AgregarEscenaBuild(escenas, SceneMonoPath);
            EditorBuildSettings.scenes = escenas.ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Actividad del Mono (HU-02) generada en Nivel 1.");
        }

        private static void ConfigurarImportadores()
        {
            string[] sprites =
            {
                "Assets/Art/Backgrounds/N1_ConozcoLaSelva.png",
                "Assets/Art/Characters/Explorador.png",
                "Assets/Art/Characters/Guacamayo.png",
                "Assets/Art/Characters/Mono.png",
                "Assets/Art/Characters/Tortuga.png"
            };
            foreach (string ruta in sprites)
            {
                TextureImporter importer = AssetImporter.GetAtPath(ruta) as TextureImporter;
                if (importer == null) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 2048;
                importer.SaveAndReimport();
            }

            string[] carpetasUI = { "Assets/Art/UI/Sprites", "Assets/Resources/UI/Sprites" };
            foreach (string carpeta in carpetasUI)
            {
                if (!AssetDatabase.IsValidFolder(carpeta)) continue;
                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { carpeta });
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    TextureImporter uiImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (uiImporter != null && uiImporter.textureType != TextureImporterType.Sprite)
                    {
                        uiImporter.textureType = TextureImporterType.Sprite;
                        uiImporter.spriteImportMode = SpriteImportMode.Single;
                        uiImporter.alphaIsTransparency = true;
                        uiImporter.mipmapEnabled = false;
                        uiImporter.textureCompression = TextureImporterCompression.Uncompressed;
                        uiImporter.maxTextureSize = 2048;
                        uiImporter.SaveAndReimport();
                    }
                }
            }
        }

        private static RetoSeleccionConfiguracion CrearConfiguracion(string ruta)
        {
            RetoSeleccionConfiguracion configuracion = AssetDatabase.LoadAssetAtPath<RetoSeleccionConfiguracion>(ruta);
            if (configuracion == null)
            {
                configuracion = ScriptableObject.CreateInstance<RetoSeleccionConfiguracion>();
                AssetDatabase.CreateAsset(configuracion, ruta);
            }
            return configuracion;
        }

        private static void CrearEscena(string ruta, RetoSeleccionConfiguracion configuracion)
        {
            Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CrearCamara();
            CrearEventSystem();

            GameObject actividad = new GameObject("RetoSeleccion");
            RetoSeleccionView vista = actividad.AddComponent<RetoSeleccionView>();
            RetoSeleccionPresenter presenter = actividad.AddComponent<RetoSeleccionPresenter>();
            presenter.Configurar(configuracion, vista);
            EditorUtility.SetDirty(presenter);

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena, ruta);
            VerificarReferenciasGuardadas(ruta);
        }

        private static void VerificarReferenciasGuardadas(string ruta)
        {
            Scene escena = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            RetoSeleccionPresenter presenter = escena.GetRootGameObjects()
                .SelectMany(objeto => objeto.GetComponentsInChildren<RetoSeleccionPresenter>(true))
                .FirstOrDefault();
            if (presenter == null)
                throw new System.InvalidOperationException("No se guardó el presenter en la escena: " + ruta);

            SerializedObject serialized = new SerializedObject(presenter);
            serialized.Update();
            if (serialized.FindProperty("_configuracion").objectReferenceValue == null ||
                serialized.FindProperty("_vista").objectReferenceValue == null)
                throw new System.InvalidOperationException("La escena quedó sin referencias del reto: " + ruta);
        }

        private static void CrearCamara()
        {
            GameObject objeto = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            objeto.tag = "MainCamera";
            objeto.transform.position = new Vector3(0f, 0f, -10f);
            Camera camara = objeto.GetComponent<Camera>();
            camara.clearFlags = CameraClearFlags.SolidColor;
            camara.backgroundColor = new Color(0.04f, 0.16f, 0.1f, 1f);
            camara.orthographic = true;
        }

        private static void CrearEventSystem()
        {
            GameObject objeto = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            objeto.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static void AgregarEscenaBuild(List<EditorBuildSettingsScene> escenas, string ruta)
        {
            if (escenas.Any(escena => escena.path == ruta)) return;
            escenas.Add(new EditorBuildSettingsScene(ruta, true));
        }

        private static T Cargar<T>(string ruta) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(ruta);
            if (asset == null) throw new System.InvalidOperationException("No se encontró el recurso: " + ruta);
            return asset;
        }
    }
}
