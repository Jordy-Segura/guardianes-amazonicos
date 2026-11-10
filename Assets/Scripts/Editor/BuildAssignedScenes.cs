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
        private const string ConfigJaguarPath = "Assets/Scripts/Models/NivelesData/RetoJaguar.asset";
        private const string ConfigPlatanoPath = "Assets/Scripts/Models/NivelesData/RetoPlatano.asset";
        private const string ConfigMonoCuriosoPath = "Assets/Scripts/Models/NivelesData/ProtegerHabitatMono.asset";
        private const string SceneMonoPath = "Assets/Scenes/Niveles/Nivel01/Escena02_HU02_Mono.unity";
        private const string SceneJaguarPath = "Assets/Scenes/Niveles/Nivel01/Escena05_HU05_Jaguar.unity";
        private const string ScenePlatanoPath = "Assets/Scenes/Niveles/Nivel01/Escena09_HU09_Platano.unity";
        private const string SceneMonoCuriosoPath = "Assets/Scenes/Niveles/Nivel02/Escena01_ProtegerHabitatMono.unity";

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
            mono.titulo = "Nivel 1 · Conozco la selva";
            mono.pregunta = "¿Cuál de estos animales es el mono?";
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
            mono.siguienteEscena = "Escena05_HU05_Jaguar";
            mono.textoContinuar = "SIGUIENTE RETO";
            mono.fondo = fondo;
            mono.explorador = explorador;
            mono.fuente = fuente;
            EditorUtility.SetDirty(mono);

            CrearEscena(SceneMonoPath, mono);
            List<EditorBuildSettingsScene> escenas = EditorBuildSettings.scenes.ToList();
            AgregarEscenaBuild(escenas, SceneMonoPath);
            EditorBuildSettings.scenes = escenas.ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Actividad del Mono (HU-02) generada en Nivel 1.");
        }

        [MenuItem("Guardianes/Generar actividad del Mono Curioso")]
        public static void GenerarActividadMonoCurioso()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ConfigurarImportadoresNivelDos();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            RetoSeleccionConfiguracion configuracion = CrearConfiguracion(ConfigMonoCuriosoPath);
            configuracion.Nivel = 2;
            configuracion.numeroEscena = 1;
            configuracion.titulo = "Nivel 2 · Cuido a los animales";
            configuracion.pregunta = "¿Qué acción ayuda a proteger el hogar del Mono Curioso?";
            configuracion.instruccion = "Toca la imagen que cuida el bosque.";
            configuracion.opciones = new[] { "Cuidar el bosque", "Tirar basura", "Cortar árboles" };
            configuracion.imagenes = null;
            configuracion.atlasDosPorDos = null;
            configuracion.AtlasOpciones = Cargar<Texture2D>("Assets/Art/Items/AccionesProteccionMono.png");
            configuracion.ColumnasAtlas = 3;
            configuracion.FilasAtlas = 1;
            configuracion.indiceCorrecto = 0;
            configuracion.pistaIncorrecta = "Esa acción puede dañar el hogar del mono. Busca la que ayuda a cuidar las plantas y los árboles.";
            configuracion.mensajeCorrecto = "¡Muy bien! Cuidar el bosque protege el hogar del Mono Curioso.";
            configuracion.pronunciaciones = null;
            configuracion.siguienteEscena = string.Empty;
            configuracion.textoContinuar = "SIGUIENTE ACTIVIDAD";
            configuracion.fondo = Cargar<Sprite>("Assets/Art/Backgrounds/N2_CuidoLosAnimales.png");
            configuracion.explorador = Cargar<Sprite>("Assets/Art/Characters/Mono.png");
            configuracion.fuente = Cargar<TMP_FontAsset>("Assets/Fonts/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            EditorUtility.SetDirty(configuracion);

            CrearEscena(SceneMonoCuriosoPath, configuracion);
            List<EditorBuildSettingsScene> escenas = EditorBuildSettings.scenes.ToList();
            AgregarEscenaBuild(escenas, SceneMonoCuriosoPath);
            EditorBuildSettings.scenes = escenas.ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Actividad del Mono Curioso generada en el segundo nivel.");
        }

        private static void ConfigurarImportadoresNivelDos()
        {
            TextureImporter fondo = AssetImporter.GetAtPath("Assets/Art/Backgrounds/N2_CuidoLosAnimales.png") as TextureImporter;
            if (fondo != null)
            {
                fondo.textureType = TextureImporterType.Sprite;
                fondo.spriteImportMode = SpriteImportMode.Single;
                fondo.alphaIsTransparency = true;
                fondo.mipmapEnabled = false;
                fondo.textureCompression = TextureImporterCompression.Uncompressed;
                fondo.maxTextureSize = 2048;
                fondo.SaveAndReimport();
            }

            TextureImporter atlas = AssetImporter.GetAtPath("Assets/Art/Items/AccionesProteccionMono.png") as TextureImporter;
            if (atlas != null)
            {
                atlas.textureType = TextureImporterType.Default;
                atlas.isReadable = true;
                atlas.alphaIsTransparency = true;
                atlas.mipmapEnabled = false;
                atlas.textureCompression = TextureImporterCompression.Uncompressed;
                atlas.maxTextureSize = 4096;
                atlas.SaveAndReimport();
            }
        }

        public static void Generar()
        {
            ConfigurarImportadores();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            TMP_FontAsset fuente = Cargar<TMP_FontAsset>("Assets/Fonts/OpenDyslexic3-Regular SDF.asset");
            Sprite fondo = Cargar<Sprite>("Assets/Art/Backgrounds/N1_ConozcoLaSelva.png");
            Sprite explorador = Cargar<Sprite>("Assets/Art/Characters/Explorador.png");

            RetoSeleccionConfiguracion jaguar = CrearConfiguracion(ConfigJaguarPath);
            jaguar.numeroEscena = 5;
            jaguar.titulo = "Nivel 1 · Conozco la selva";
            jaguar.pregunta = "¿Cuál de estos animales es el jaguar?";
            jaguar.instruccion = "Toca la imagen del jaguar.";
            jaguar.opciones = new[] { "Jaguar", "Guacamayo", "Mono", "Tortuga" };
            jaguar.imagenes = new[]
            {
                Cargar<Sprite>("Assets/Art/Characters/Jaguar.png"),
                Cargar<Sprite>("Assets/Art/Characters/Guacamayo.png"),
                Cargar<Sprite>("Assets/Art/Characters/Mono.png"),
                Cargar<Sprite>("Assets/Art/Characters/Tortuga.png")
            };
            jaguar.atlasDosPorDos = null;
            jaguar.indiceCorrecto = 0;
            jaguar.pistaIncorrecta = "Busca un felino amarillo con manchas oscuras.";
            jaguar.mensajeCorrecto = "¡Muy bien! Encontraste al jaguar.";
            jaguar.pronunciaciones = null;
            jaguar.siguienteEscena = string.Empty;
            jaguar.textoContinuar = "SIGUIENTE RETO";
            jaguar.fondo = fondo;
            jaguar.explorador = explorador;
            jaguar.fuente = fuente;
            EditorUtility.SetDirty(jaguar);

            RetoSeleccionConfiguracion platano = CrearConfiguracion(ConfigPlatanoPath);
            platano.numeroEscena = 9;
            platano.titulo = "Nivel 1 · Conozco la selva";
            platano.pregunta = "¿Cuál de estos elementos es un plátano?";
            platano.instruccion = "Toca una imagen. Después puedes escuchar su nombre.";
            platano.opciones = new[] { "Plátano", "Manzana", "Cacao", "Hoja" };
            platano.imagenes = null;
            platano.atlasDosPorDos = Cargar<Texture2D>("Assets/Art/Items/FrutasJungla.png");
            platano.indiceCorrecto = 0;
            platano.pistaIncorrecta = "Busca la fruta amarilla y curva.";
            platano.mensajeCorrecto = "¡Excelente! Identificaste el plátano.";
            platano.pronunciaciones = new[]
            {
                Cargar<AudioClip>("Assets/Audio/Voice/Retos/Pronunciacion_Platano.wav"),
                Cargar<AudioClip>("Assets/Audio/Voice/Retos/Pronunciacion_Manzana.wav"),
                Cargar<AudioClip>("Assets/Audio/Voice/Retos/Pronunciacion_Cacao.wav"),
                Cargar<AudioClip>("Assets/Audio/Voice/Retos/Pronunciacion_Hoja.wav")
            };
            platano.siguienteEscena = string.Empty;
            platano.textoContinuar = "CONTINUAR";
            platano.fondo = fondo;
            platano.explorador = explorador;
            platano.fuente = fuente;
            EditorUtility.SetDirty(platano);

            AssetDatabase.SaveAssets();
            CrearEscena(SceneJaguarPath, jaguar);
            CrearEscena(ScenePlatanoPath, platano);
            ActualizarBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Escenas de selección HU-05 y HU-09 generadas y añadidas a Build Settings.");
        }

        private static void ConfigurarImportadores()
        {
            string[] sprites =
            {
                "Assets/Art/Backgrounds/N1_ConozcoLaSelva.png",
                "Assets/Art/Characters/Explorador.png",
                "Assets/Art/Characters/Guacamayo.png",
                "Assets/Art/Characters/Mono.png",
                "Assets/Art/Characters/Tortuga.png",
                "Assets/Art/Characters/Jaguar.png"
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

            TextureImporter atlas = AssetImporter.GetAtPath("Assets/Art/Items/FrutasJungla.png") as TextureImporter;
            if (atlas != null)
            {
                atlas.textureType = TextureImporterType.Default;
                atlas.isReadable = true;
                atlas.alphaIsTransparency = true;
                atlas.mipmapEnabled = false;
                atlas.textureCompression = TextureImporterCompression.Uncompressed;
                atlas.maxTextureSize = 2048;
                atlas.SaveAndReimport();
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

        private static void ActualizarBuildSettings()
        {
            List<EditorBuildSettingsScene> escenas = EditorBuildSettings.scenes.ToList();
            AgregarEscenaBuild(escenas, "Assets/Scenes/Niveles/Nivel01/Nivel1_ConozcoLaSelva.unity");
            AgregarEscenaBuild(escenas, SceneMonoPath);
            AgregarEscenaBuild(escenas, SceneJaguarPath);
            AgregarEscenaBuild(escenas, ScenePlatanoPath);
            EditorBuildSettings.scenes = escenas.ToArray();
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
