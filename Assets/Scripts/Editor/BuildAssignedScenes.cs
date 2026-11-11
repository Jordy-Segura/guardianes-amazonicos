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
    /// Genera SOLO las actividades asignadas a Jordy con la plantilla base aprobada:
    /// Nivel 1 – Escena 5 (jaguar), Nivel 1 – Escena 9 (plátano) y
    /// Nivel 2 – Escena 1 (bienvenida del nivel + Mono Curioso).
    /// No abre ni modifica las escenas de otros integrantes.
    /// </summary>
    public static class BuildAssignedScenes
    {
        private const string ConfigJaguarPath = "Assets/Scripts/Models/NivelesData/RetoJaguar.asset";
        private const string ConfigPlatanoPath = "Assets/Scripts/Models/NivelesData/RetoPlatano.asset";
        private const string ConfigMonoCuriosoPath = "Assets/Scripts/Models/NivelesData/ProtegerHabitatMono.asset";
        public const string SceneJaguarPath = "Assets/Scenes/Niveles/Nivel01/Escena05_IdentificarJaguar.unity";
        public const string ScenePlatanoPath = "Assets/Scenes/Niveles/Nivel01/Escena09_IdentificarPlatano.unity";
        public const string SceneMonoCuriosoPath = "Assets/Scenes/Niveles/Nivel02/Escena01_ProtegerHabitatMono.unity";

        private const string FondoNivel1 = "Assets/Art/Backgrounds/Niveles/N1_Conozco_la_selva.png";
        private const string FondoNivel2 = "Assets/Art/Backgrounds/Niveles/N2_Cuido_los_animales.png";
        private const string Explorador = "Assets/Art/Characters/Explorador.png";
        private const string ExploradorPreocupado = "Assets/Art/Characters/ExploradorPreocupado.png";
        private const string CarpetaFotogramasExplorador = "Assets/Art/Animaciones/Explorador/Fotogramas";
        private const string Escenas = "Assets/Art/Escenas/";
        private const string Respuestas = "Assets/Art/Items/Respuestas/";
        private const string Lectura = "Assets/Audio/Voice/Lectura/";

        private static readonly string[] RecursosNuevos =
        {
            ExploradorPreocupado,
            Escenas + "pregunta_01_05_escena.png", Escenas + "pregunta_01_09_escena.png",
            Escenas + "pregunta_02_01_escena.png", Escenas + "bienvenida_nivel_01.png", Escenas + "bienvenida_nivel_02.png",
            Respuestas + "respuesta_01_09_a.png", Respuestas + "respuesta_01_09_b.png", Respuestas + "respuesta_01_09_c.png",
            Respuestas + "respuesta_02_01_a.png", Respuestas + "respuesta_02_01_b.png", Respuestas + "respuesta_02_01_c.png"
        };

        [MenuItem("Guardianes/Generar mis escenas con la plantilla (Bienvenida N1, Escenas 5 y 9, Nivel 2 Escena 1)")]
        public static void GenerarTodas()
        {
            BuildWelcomeScene.Build();
            Generar();
            GenerarActividadMonoCurioso();
        }

        [MenuItem("Guardianes/Generar retos del jaguar y del plátano")]
        public static void Generar()
        {
            PrepararRecursos();
            RetoSeleccionConfiguracion jaguar = CrearConfiguracion(ConfigJaguarPath);
            jaguar.Nivel = 1;
            jaguar.NumeroEscena = 5;
            jaguar.TituloNivel = "Nivel 1 • Conozco la selva";
            jaguar.Pregunta = "Observa la selva. ¿Cuál de estos animales es el jaguar?";
            jaguar.Opciones = new[] { "Mono", "Jaguar", "Guacamayo" };
            jaguar.ImagenesOpciones = new[]
            {
                PlantillaUIBuilder.S("Assets/Art/Characters/Mono.png"),
                PlantillaUIBuilder.S("Assets/Art/Characters/Jaguar.png"),
                PlantillaUIBuilder.S("Assets/Art/Characters/Guacamayo.png")
            };
            jaguar.IndiceCorrecto = 1;
            jaguar.ExplicacionesIncorrectas = new[]
            {
                "El mono trepa los árboles y tiene una cola larga. El jaguar es un felino con manchas.",
                string.Empty,
                "El guacamayo es un ave de plumas de colores. El jaguar es un felino con manchas."
            };
            jaguar.Pista = "Busca un animal parecido a un gato grande, de pelaje amarillo con manchas oscuras.";
            jaguar.Felicitacion = "¡Lo lograste! Encontraste al jaguar.";
            jaguar.ImagenEscena = PlantillaUIBuilder.S(Escenas + "pregunta_01_05_escena.png");
            jaguar.ElementoEscena = null;
            jaguar.Fondo = PlantillaUIBuilder.S(FondoNivel1);
            jaguar.Personaje = PlantillaUIBuilder.S(Explorador);
            jaguar.PersonajeIncorrecto = PlantillaUIBuilder.S(ExploradorPreocupado);
            jaguar.FotogramasCelebracion = CargarFotogramasExplorador();
            jaguar.AudioLectura = PlantillaUIBuilder.Cargar<AudioClip>(Lectura + "lectura_01_05.wav");
            jaguar.VideoSenas = "senas_nivel_01_pregunta_05.mp4";
            jaguar.MostrarBienvenida = false;
            jaguar.SiguienteEscena = string.Empty;
            jaguar.Fuente = PlantillaUIBuilder.Cargar<TMPro.TMP_FontAsset>(PlantillaUIBuilder.FUENTE);
            EditorUtility.SetDirty(jaguar);

            RetoSeleccionConfiguracion platano = CrearConfiguracion(ConfigPlatanoPath);
            platano.Nivel = 1;
            platano.NumeroEscena = 9;
            platano.TituloNivel = "Nivel 1 • Conozco la selva";
            platano.Pregunta = "Observa la selva. ¿Cuál de estos frutos es el plátano?";
            platano.Opciones = new[] { "Manzana", "Cacao", "Plátano" };
            platano.ImagenesOpciones = new[]
            {
                PlantillaUIBuilder.S(Respuestas + "respuesta_01_09_a.png"),
                PlantillaUIBuilder.S(Respuestas + "respuesta_01_09_b.png"),
                PlantillaUIBuilder.S(Respuestas + "respuesta_01_09_c.png")
            };
            platano.IndiceCorrecto = 2;
            platano.ExplicacionesIncorrectas = new[]
            {
                "La manzana es redonda y roja. El plátano es largo, curvo y amarillo.",
                "El cacao es un fruto morado con semillas blancas. El plátano es largo, curvo y amarillo.",
                string.Empty
            };
            platano.Pista = "Busca una fruta larga y curva que se vuelve amarilla cuando está madura.";
            platano.Felicitacion = "¡Lo lograste! Identificaste el plátano.";
            platano.ImagenEscena = PlantillaUIBuilder.S(Escenas + "pregunta_01_09_escena.png");
            platano.ElementoEscena = null;
            platano.Fondo = PlantillaUIBuilder.S(FondoNivel1);
            platano.Personaje = PlantillaUIBuilder.S(Explorador);
            platano.PersonajeIncorrecto = PlantillaUIBuilder.S(ExploradorPreocupado);
            platano.FotogramasCelebracion = CargarFotogramasExplorador();
            platano.AudioLectura = PlantillaUIBuilder.Cargar<AudioClip>(Lectura + "lectura_01_09.wav");
            platano.VideoSenas = "senas_nivel_01_pregunta_09.mp4";
            platano.MostrarBienvenida = false;
            platano.SiguienteEscena = string.Empty;
            platano.Fuente = jaguar.Fuente;
            EditorUtility.SetDirty(platano);

            AssetDatabase.SaveAssets();
            CrearEscena(SceneJaguarPath, jaguar);
            CrearEscena(ScenePlatanoPath, platano);
            AgregarEscenasBuild(SceneJaguarPath, ScenePlatanoPath);
            Debug.Log("Escenas 5 y 9 del Nivel 1 generadas con la plantilla base.");
        }

        [MenuItem("Guardianes/Generar actividad del Mono Curioso")]
        public static void GenerarActividadMonoCurioso()
        {
            PrepararRecursos();
            RetoSeleccionConfiguracion mono = CrearConfiguracion(ConfigMonoCuriosoPath);
            mono.Nivel = 2;
            mono.NumeroEscena = 1;
            mono.TituloNivel = "Nivel 2 • Cuido los animales";
            mono.Pregunta = "¿Qué acción ayuda a proteger el hogar del Mono Curioso?";
            mono.Opciones = new[] { "Tirar basura", "Cuidar el bosque", "Cortar árboles" };
            mono.ImagenesOpciones = new[]
            {
                PlantillaUIBuilder.S(Respuestas + "respuesta_02_01_a.png"),
                PlantillaUIBuilder.S(Respuestas + "respuesta_02_01_b.png"),
                PlantillaUIBuilder.S(Respuestas + "respuesta_02_01_c.png")
            };
            mono.IndiceCorrecto = 1;
            mono.ExplicacionesIncorrectas = new[]
            {
                "La basura ensucia el bosque y puede lastimar a los animales. Busca la acción que cuida su hogar.",
                string.Empty,
                "Si cortamos los árboles, el mono pierde su casa. Busca la acción que cuida el bosque."
            };
            mono.Pista = "Piensa en lo que necesita el mono para vivir: árboles sanos y un bosque limpio.";
            mono.Felicitacion = "¡Lo lograste! Cuidar el bosque protege su hogar.";
            mono.ImagenEscena = PlantillaUIBuilder.S(Escenas + "pregunta_02_01_escena.png");
            mono.ElementoEscena = null;
            mono.RectElementoEscena = Rect.zero;
            mono.Fondo = PlantillaUIBuilder.S(FondoNivel2);
            mono.Personaje = PlantillaUIBuilder.S(Explorador);
            mono.PersonajeIncorrecto = PlantillaUIBuilder.S(ExploradorPreocupado);
            mono.FotogramasCelebracion = CargarFotogramasExplorador();
            mono.AudioLectura = PlantillaUIBuilder.Cargar<AudioClip>(Lectura + "lectura_02_01.wav");
            mono.VideoSenas = "senas_nivel_02_pregunta_01.mp4";
            mono.MostrarBienvenida = true;
            mono.BienvenidaEtiquetaNivel = "Nivel 2";
            mono.BienvenidaNombreNivel = "¡Cuido los animales!";
            mono.BienvenidaSaludo = "¡Hola, guardián!";
            mono.BienvenidaDescripcion = "En este nivel aprenderás a cuidar a los animales de la selva y a proteger su hogar.";
            mono.BienvenidaImagen = PlantillaUIBuilder.S(Escenas + "bienvenida_nivel_02.png");
            mono.BienvenidaAudio = PlantillaUIBuilder.Cargar<AudioClip>(Lectura + "lectura_02_00_bienvenida.wav");
            mono.BienvenidaVideoSenas = "senas_nivel_02_bienvenida.mp4";
            mono.SiguienteEscena = string.Empty;
            mono.Fuente = PlantillaUIBuilder.Cargar<TMPro.TMP_FontAsset>(PlantillaUIBuilder.FUENTE);
            EditorUtility.SetDirty(mono);

            AssetDatabase.SaveAssets();
            CrearEscena(SceneMonoCuriosoPath, mono);
            AgregarEscenasBuild(SceneMonoCuriosoPath);
            Debug.Log("Escena 1 del Nivel 2 generada: bienvenida del nivel y actividad del Mono Curioso.");
        }

        public static void PrepararRecursos()
        {
            PlantillaUIBuilder.Preparar();
            foreach (string ruta in RecursosNuevos) PlantillaUIBuilder.ImportarComoSprite(ruta);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        /// <summary>Fotograma central del saludo (brazo levantado) para la bienvenida.</summary>
        public static Sprite FotogramaSaludo(Sprite[] fotogramas, Sprite respaldo)
        {
            return fotogramas != null && fotogramas.Length > 0 ? fotogramas[fotogramas.Length / 2] : respaldo;
        }

        public static Sprite[] CargarFotogramasExplorador()
        {
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { CarpetaFotogramasExplorador });
            return guids.Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(ruta => ruta, System.StringComparer.Ordinal)
                .Select(AssetDatabase.LoadAssetAtPath<Sprite>)
                .Where(sprite => sprite != null)
                .ToArray();
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

            // Capa base: fondo → actividad → bienvenida (si corresponde) → cabecera.
            RectTransform capaBase = PlantillaUIBuilder.Capa("CapaBase", canvas.transform);
            r.capaBase = capaBase.gameObject.AddComponent<CanvasGroup>();
            PlantillaUIBuilder.Imagen("FondoNivel", capaBase, c.Fondo, 0f, 0f, PlantillaUIBuilder.ANCHO, PlantillaUIBuilder.ALTO);
            PlantillaUIBuilder.ConstruirActividad(capaBase, c.Pregunta, c.Opciones, c.ImagenesOpciones, c.Personaje,
                c.PersonajeIncorrecto, c.FotogramasCelebracion, c.ImagenEscena, c.ElementoEscena, c.RectElementoEscena, r);

            if (c.MostrarBienvenida)
            {
                PlantillaUIBuilder.Bienvenida b = PlantillaUIBuilder.ConstruirBienvenida(capaBase, new PlantillaUIBuilder.DatosBienvenida
                {
                    etiquetaNivel = c.BienvenidaEtiquetaNivel,
                    nombreNivel = c.BienvenidaNombreNivel,
                    saludo = c.BienvenidaSaludo,
                    descripcion = c.BienvenidaDescripcion,
                    fondo = c.Fondo,
                    imagen = c.BienvenidaImagen,
                    personaje = FotogramaSaludo(c.FotogramasCelebracion, c.Personaje),
                    fotogramas = c.FotogramasCelebracion
                });
                r.estadoBienvenida = b.raiz;
                r.botonComenzar = b.comenzar;
                r.animacionBienvenida = b.animacion;
            }

            PlantillaUIBuilder.Cabecera cabecera = PlantillaUIBuilder.ConstruirCabecera(capaBase, c.TituloNivel, c.NumeroEscena, c.TotalEscenas, true);
            r.botonLectura = cabecera.lectura;
            r.botonSenas = cabecera.senas;
            r.botonAyuda = cabecera.ayuda;
            r.botonRegresar = cabecera.regresar;
            r.botonAvanzar = cabecera.avanzar;
            r.botonInicio = cabecera.inicio;
            r.botonConfiguracion = cabecera.configuracion;
            r.textoProgreso = cabecera.progreso;
            if (c.MostrarBienvenida && r.botonAyuda != null) r.botonAyuda.gameObject.SetActive(false);

            // Ventanas independientes: nunca se muestran dos a la vez.
            PlantillaUIBuilder.ConstruirEstadoCorrecto(canvas.transform, c.Personaje, c.FotogramasCelebracion, c.Felicitacion, r);
            PlantillaUIBuilder.ConstruirPanelPista(canvas.transform, c.Pista, c.Personaje, c.FotogramasCelebracion, r);
            PlantillaUIBuilder.PanelSenas senas = PlantillaUIBuilder.ConstruirPanelSenas(canvas.transform);
            r.panelSenas = senas.raiz;
            r.botonReproducirSenas = senas.reproducir;
            r.botonCerrarSenas = senas.cerrar;
            r.textoEstadoSenas = senas.estado;
            r.pantallaVideo = senas.pantalla;
            r.reproductorSenas = senas.reproductor;

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

        public static void AgregarEscenasBuild(params string[] rutas)
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
