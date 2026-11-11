using GuardianesAmazonicos.Presenters;
using GuardianesAmazonicos.Views;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GuardianesAmazonicos.Editor
{
    /// <summary>
    /// Construye «Bienvenida e inicio de cada nivel» (Nivel 1, Escena 1) con la plantilla
    /// de la guía: cabecera con Lectura y Señas, tarjeta de presentación, personaje guía,
    /// botón «¡Comenzar!» y ventana de señas.
    /// </summary>
    public static class BuildWelcomeScene
    {
        private const string ScenePath = "Assets/Scenes/Niveles/Nivel01/BienvenidaDelGuardian.unity";
        private const string BackgroundPath = "Assets/Art/Backgrounds/Niveles/N1_Conozco_la_selva.png";
        private const string ImagePath = "Assets/Art/Escenas/bienvenida_nivel_01.png";
        private const string BoyPath = "Assets/Art/Characters/Explorador.png";
        private const string AudioClipPath = "Assets/Audio/Voice/Lectura/lectura_01_01_bienvenida.wav";

        [MenuItem("Guardianes/Construir bienvenida según plantilla")]
        public static void Build()
        {
            BuildAssignedScenes.PrepararRecursos();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            PlantillaUIBuilder.CrearCamaraYEventos();
            Canvas canvas = PlantillaUIBuilder.CrearCanvas("CanvasBienvenida");

            RectTransform capaBase = PlantillaUIBuilder.Capa("CapaBase", canvas.transform);
            CanvasGroup grupoBase = capaBase.gameObject.AddComponent<CanvasGroup>();
            Sprite[] saludo = BuildAssignedScenes.CargarFotogramasExplorador();
            PlantillaUIBuilder.Bienvenida bienvenida = PlantillaUIBuilder.ConstruirBienvenida(capaBase, new PlantillaUIBuilder.DatosBienvenida
            {
                etiquetaNivel = "Nivel 1",
                nombreNivel = "¡Conozco la selva!",
                saludo = "¡Hola, guardián!",
                descripcion = "En este nivel conocerás los animales, las plantas y los ríos de la selva amazónica.",
                fondo = PlantillaUIBuilder.S(BackgroundPath),
                imagen = PlantillaUIBuilder.S(ImagePath),
                personaje = BuildAssignedScenes.FotogramaSaludo(saludo, PlantillaUIBuilder.S(BoyPath)),
                fotogramas = saludo
            });

            // La bienvenida no tiene pista: la cabecera muestra Lectura y Señas, como en la captura aprobada.
            PlantillaUIBuilder.Cabecera cabecera = PlantillaUIBuilder.ConstruirCabecera(capaBase,
                "Nivel 1 • Conozco la selva", 1, 12, false);
            PlantillaUIBuilder.PanelSenas senas = PlantillaUIBuilder.ConstruirPanelSenas(canvas.transform);

            GameObject controlador = new GameObject("ControladorBienvenida");
            AudioSource audio = controlador.AddComponent<AudioSource>();
            audio.clip = PlantillaUIBuilder.Cargar<AudioClip>(AudioClipPath);
            audio.playOnAwake = false;
            audio.loop = false;

            BienvenidaNivelView view = controlador.AddComponent<BienvenidaNivelView>();
            SetReference(view, "_textoProgreso", cabecera.progreso);
            SetReference(view, "_textoEstadoSenas", senas.estado);
            SetReference(view, "_botonComenzar", bienvenida.comenzar);
            SetReference(view, "_botonAudio", cabecera.lectura);
            SetReference(view, "_botonSenas", cabecera.senas);
            SetReference(view, "_botonCerrarSenas", senas.cerrar);
            SetReference(view, "_botonReproducirSenas", senas.reproducir);
            SetReference(view, "_botonRegresar", cabecera.regresar);
            SetReference(view, "_botonAvanzar", cabecera.avanzar);
            SetReference(view, "_botonInicio", cabecera.inicio);
            SetReference(view, "_botonConfiguracion", cabecera.configuracion);
            SetReference(view, "_fuenteAudio", audio);
            SetReference(view, "_reproductorSenas", senas.reproductor);
            SetReference(view, "_pantallaVideo", senas.pantalla);
            SetReference(view, "_panelSenas", senas.raiz);
            SetReference(view, "_capaBase", grupoBase);
            SetReference(view, "_animacionPersonaje", bienvenida.animacion);

            BienvenidaNivelPresenter presenter = controlador.AddComponent<BienvenidaNivelPresenter>();
            SetReference(presenter, "_vista", view);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            BuildAssignedScenes.AgregarEscenasBuild(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("Bienvenida del Nivel 1 reconstruida con la plantilla: " + ScenePath);
        }

        private static void SetReference(Object target, string propertyName, Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
                throw new System.InvalidOperationException("No existe la propiedad " + propertyName + " en " + target.GetType().Name);
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
