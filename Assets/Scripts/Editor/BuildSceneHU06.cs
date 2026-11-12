using System.Collections.Generic;
using System.Linq;
using GuardianesAmazonicos.Models;
using GuardianesAmazonicos.Presenters;
using GuardianesAmazonicos.Views;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GuardianesAmazonicos.Editor
{
    /// <summary>
    /// Genera la actividad HU-06 asignada a Sahid:
    /// Nivel 1 – Escena 6 (¿Qué necesita la tortuga para estar bien? -> Agua).
    /// Utiliza la plantilla base aprobada y el patrón MVP + Observer.
    /// </summary>
    public static class BuildSceneHU06
    {
        public const string ConfigTortugaPath = "Assets/Scripts/Models/NivelesData/RetoTortuga.asset";
        public const string SceneTortugaPath = "Assets/Scenes/Niveles/Nivel01/Escena06_HU06_TortugaAgua.unity";

        private const string FondoNivel1 = "Assets/Art/Backgrounds/Niveles/N1_Conozco_la_selva.png";
        private const string TortugaPersonaje = "Assets/Art/Characters/Tortuga.png";
        private const string Explorador = "Assets/Art/Characters/Explorador.png";
        private const string ExploradorPreocupado = "Assets/Art/Characters/ExploradorPreocupado.png";
        private const string Escenas = "Assets/Art/Escenas/";
        private const string Respuestas = "Assets/Art/Items/Respuestas/";
        private const string Lectura = "Assets/Audio/Voice/Lectura/";

        private static readonly string[] RecursosHU06 =
        {
            Escenas + "pregunta_01_06_escena.png",
            Respuestas + "respuesta_01_06_a.png",
            Respuestas + "respuesta_01_06_b.png",
            Respuestas + "respuesta_01_06_c.png"
        };

        [MenuItem("Guardianes/Sahid/Generar Escena 06 (HU-06 Tortuga - Agua)")]
        public static void Generar()
        {
            PrepararRecursos();

            RetoSeleccionConfiguracion tortuga = CrearConfiguracion(ConfigTortugaPath);
            tortuga.Nivel = 1;
            tortuga.NumeroEscena = 6;
            tortuga.TotalEscenas = 12;
            tortuga.TituloNivel = "Nivel 1 • Conozco la selva";
            tortuga.Pregunta = "¿Qué necesita para estar bien?";
            tortuga.Opciones = new[] { "Agua", "Fruta", "Árbol" };
            tortuga.ImagenesOpciones = new[]
            {
                PlantillaUIBuilder.S(Respuestas + "respuesta_01_06_a.png"),
                PlantillaUIBuilder.S(Respuestas + "respuesta_01_06_b.png"),
                PlantillaUIBuilder.S(Respuestas + "respuesta_01_06_c.png")
            };
            tortuga.IndiceCorrecto = 0; // Agua es la respuesta correcta
            tortuga.ExplicacionesIncorrectas = new[]
            {
                string.Empty,
                "La fruta es un rico alimento, pero todos los seres vivos necesitan agua para calmar su sed y vivir bien.",
                "El árbol da sombra y un hogar, pero la tortuga necesita agua fresca para hidratarse y nadar."
            };
            tortuga.Pista = "Busca el elemento líquido y refrescante que la tortuga y todos los seres vivos necesitan beber para vivir.";
            tortuga.Felicitacion = "¡Muy bien! El agua es importante para los seres vivos.";
            tortuga.ImagenEscena = PlantillaUIBuilder.S(Escenas + "pregunta_01_06_escena.png");
            tortuga.ElementoEscena = null;
            tortuga.RectElementoEscena = Rect.zero;
            tortuga.Fondo = PlantillaUIBuilder.S(FondoNivel1);
            tortuga.Personaje = PlantillaUIBuilder.S(TortugaPersonaje);
            tortuga.PersonajeIncorrecto = PlantillaUIBuilder.S(ExploradorPreocupado);
            tortuga.FotogramasCelebracion = BuildAssignedScenes.CargarFotogramasExplorador();
            tortuga.AudioLectura = PlantillaUIBuilder.Cargar<AudioClip>(Lectura + "lectura_01_06.wav");
            tortuga.VideoSenas = "senas_nivel_01_pregunta_06.mp4";
            tortuga.MostrarBienvenida = false;
            tortuga.SiguienteEscena = string.Empty;
            tortuga.Fuente = PlantillaUIBuilder.Cargar<TMPro.TMP_FontAsset>(PlantillaUIBuilder.FUENTE);
            EditorUtility.SetDirty(tortuga);

            AssetDatabase.SaveAssets();
            CrearEscena(SceneTortugaPath, tortuga);
            AgregarEscenasBuild(SceneTortugaPath);
            Debug.Log("HU-06: Escena 6 (Tortuga - Agua) generada exitosamente en " + SceneTortugaPath);
        }

        public static void PrepararRecursos()
        {
            PlantillaUIBuilder.Preparar();
            foreach (string ruta in RecursosHU06)
            {
                if (System.IO.File.Exists(ruta))
                {
                    PlantillaUIBuilder.ImportarComoSprite(ruta);
                }
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
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

        private static void CrearEscena(string ruta, RetoSeleccionConfiguracion c)
        {
            Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            PlantillaUIBuilder.CrearCamaraYEventos();

            Canvas canvas = PlantillaUIBuilder.CrearCanvas("CanvasActividad");
            RetoSeleccionView.ReferenciasActividad r = new RetoSeleccionView.ReferenciasActividad();

            // Capa base
            RectTransform capaBase = PlantillaUIBuilder.Capa("CapaBase", canvas.transform);
            r.capaBase = capaBase.gameObject.AddComponent<CanvasGroup>();
            PlantillaUIBuilder.Imagen("FondoNivel", capaBase, c.Fondo, 0f, 0f, PlantillaUIBuilder.ANCHO, PlantillaUIBuilder.ALTO);
            PlantillaUIBuilder.ConstruirActividad(capaBase, c.Pregunta, c.Opciones, c.ImagenesOpciones, c.Personaje,
                c.PersonajeIncorrecto, c.FotogramasCelebracion, c.ImagenEscena, c.ElementoEscena, c.RectElementoEscena, r);

            // Cabecera
            PlantillaUIBuilder.Cabecera cabecera = PlantillaUIBuilder.ConstruirCabecera(capaBase, c.TituloNivel, c.NumeroEscena, c.TotalEscenas, true);
            r.botonLectura = cabecera.lectura;
            r.botonSenas = cabecera.senas;
            r.botonAyuda = cabecera.ayuda;
            r.botonRegresar = cabecera.regresar;
            r.botonAvanzar = cabecera.avanzar;
            r.botonInicio = cabecera.inicio;
            r.botonConfiguracion = cabecera.configuracion;
            r.textoProgreso = cabecera.progreso;

            // Ventanas modales
            PlantillaUIBuilder.ConstruirEstadoCorrecto(canvas.transform, c.Personaje, c.FotogramasCelebracion, c.Felicitacion, r);
            PlantillaUIBuilder.ConstruirPanelPista(canvas.transform, c.Pista, c.Personaje, c.FotogramasCelebracion, r);
            PlantillaUIBuilder.PanelSenas senas = PlantillaUIBuilder.ConstruirPanelSenas(canvas.transform);
            r.panelSenas = senas.raiz;
            r.botonReproducirSenas = senas.reproducir;
            r.botonCerrarSenas = senas.cerrar;
            r.textoEstadoSenas = senas.estado;
            r.pantallaVideo = senas.pantalla;
            r.reproductorSenas = senas.reproductor;

            // Controlador MVP
            GameObject controlador = new GameObject("RetoSeleccion");
            r.fuenteAudio = controlador.AddComponent<AudioSource>();
            r.fuenteAudio.playOnAwake = false;
            r.fuenteAudio.loop = false;
            RetoSeleccionView vista = controlador.AddComponent<RetoSeleccionView>();
            vista.Enlazar(r);
            RetoSeleccionPresenter presenter = controlador.AddComponent<RetoSeleccionPresenter>();
            presenter.Configurar(c, vista);
            EditorUtility.SetDirty(vista);
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

            SerializedObject datos = new SerializedObject(presenter);
            if (datos.FindProperty("_configuracion").objectReferenceValue == null ||
                datos.FindProperty("_vista").objectReferenceValue == null)
                throw new System.InvalidOperationException("La escena quedó sin referencias del reto: " + ruta);

            int tarjetas = escena.GetRootGameObjects()
                .SelectMany(objeto => objeto.GetComponentsInChildren<Button>(true))
                .Count(boton => boton.name.StartsWith("Respuesta"));
            if (tarjetas != RetoSeleccionConfiguracion.CANTIDAD_OPCIONES)
                throw new System.InvalidOperationException("La plantilla debe tener tres tarjetas de respuesta: " + ruta);
        }

        private static void AgregarEscenasBuild(params string[] rutas)
        {
            List<EditorBuildSettingsScene> escenas = EditorBuildSettings.scenes
                .Where(escena => System.IO.File.Exists(escena.path))
                .ToList();
            bool cambio = escenas.Count != EditorBuildSettings.scenes.Length;
            foreach (string ruta in rutas)
            {
                if (escenas.Any(escena => escena.path == ruta)) continue;
                escenas.Add(new EditorBuildSettingsScene(ruta, true));
                cambio = true;
            }

            if (cambio) EditorBuildSettings.scenes = escenas.ToArray();
        }
    }
}
