using System;
using System.Linq;
using GuardianesAmazonicos.Models;
using GuardianesAmazonicos.Presenters;
using GuardianesAmazonicos.Views;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GuardianesAmazonicos.Editor
{
    /// <summary>
    /// Genera solo la escena HU-03 con los bloques y coordenadas de la plantilla
    /// que ya usan las otras escenas. Los componentes compartidos no se modifican.
    /// </summary>
    public static class VerificarGuacamayo
    {
        public const string RutaEscena = "Assets/Scenes/Niveles/Nivel01/Escena03_HU03_Guacamayo.unity";
        private const string RutaConfiguracion = "Assets/Scripts/Models/NivelesData/RetoGuacamayo.asset";
        private const string RutaFauna = "Assets/Art/Backgrounds/EscenaFaunaHU03.png";
        private const string RutaPreocupado = "Assets/Art/Characters/ExploradorPreocupado.png";

        [MenuItem("Guardianes/HU-03/Generar escena")]
        public static void Generar()
        {
            PlantillaUIBuilder.Preparar();
            foreach (string ruta in new[] { RutaFauna, RutaPreocupado })
                PlantillaUIBuilder.ImportarComoSprite(ruta);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            RetoSeleccionConfiguracion configuracion = CrearConfiguracion();
            Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            PlantillaUIBuilder.CrearCamaraYEventos();
            Canvas canvas = PlantillaUIBuilder.CrearCanvas("CanvasActividad");
            RetoSeleccionView.ReferenciasActividad referencias = new RetoSeleccionView.ReferenciasActividad();

            RectTransform capaBase = PlantillaUIBuilder.Capa("CapaBase", canvas.transform);
            referencias.capaBase = capaBase.gameObject.AddComponent<CanvasGroup>();
            PlantillaUIBuilder.Imagen("FondoNivel", capaBase, configuracion.Fondo, 0f, 0f,
                PlantillaUIBuilder.ANCHO, PlantillaUIBuilder.ALTO);
            PlantillaUIBuilder.ConstruirActividad(capaBase, configuracion.Pregunta, configuracion.Opciones,
                configuracion.ImagenesOpciones, configuracion.Personaje, configuracion.PersonajeIncorrecto,
                configuracion.FotogramasCelebracion, configuracion.ImagenEscena, null, default, referencias);

            // En HU-03 las tarjetas se reconocen por la imagen. Ocultamos los nombres
            // y ampliamos los animales dentro del mismo marco dorado de la plantilla.
            foreach (RetoSeleccionView.TarjetaRespuestaVista tarjeta in referencias.tarjetas)
            {
                tarjeta.nombre.gameObject.SetActive(false);
                RectTransform ilustracion = (RectTransform)tarjeta.ilustracion.transform;
                PlantillaUIBuilder.Colocar(ilustracion, 12f, -12f, 232f, 278f);
            }

            PlantillaUIBuilder.Cabecera cabecera = PlantillaUIBuilder.ConstruirCabecera(capaBase,
                configuracion.TituloNivel, configuracion.NumeroEscena, configuracion.TotalEscenas, true);
            referencias.botonLectura = cabecera.lectura;
            referencias.botonSenas = cabecera.senas;
            referencias.botonAyuda = cabecera.ayuda;
            referencias.botonRegresar = cabecera.regresar;
            referencias.botonAvanzar = cabecera.avanzar;
            referencias.botonInicio = cabecera.inicio;
            referencias.botonConfiguracion = cabecera.configuracion;
            referencias.textoProgreso = cabecera.progreso;

            PlantillaUIBuilder.ConstruirEstadoCorrecto(canvas.transform, configuracion.Personaje,
                configuracion.FotogramasCelebracion, configuracion.Felicitacion, referencias);
            PlantillaUIBuilder.ConstruirPanelPista(canvas.transform, configuracion.Pista,
                configuracion.Personaje, configuracion.FotogramasCelebracion, referencias);
            PlantillaUIBuilder.PanelSenas panelSenas = PlantillaUIBuilder.ConstruirPanelSenas(canvas.transform);
            referencias.panelSenas = panelSenas.raiz;
            referencias.botonReproducirSenas = panelSenas.reproducir;
            referencias.botonCerrarSenas = panelSenas.cerrar;
            referencias.textoEstadoSenas = panelSenas.estado;
            referencias.pantallaVideo = panelSenas.pantalla;
            referencias.reproductorSenas = panelSenas.reproductor;

            GameObject controlador = new GameObject("HU03_Guacamayo");
            AudioSource audio = controlador.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.loop = false;
            referencias.fuenteAudio = audio;
            RetoSeleccionView vista = controlador.AddComponent<RetoSeleccionView>();
            vista.Enlazar(referencias);
            GuacamayoPresenter presenter = controlador.AddComponent<GuacamayoPresenter>();
            presenter.Configurar(configuracion, vista, referencias.personajeGuia.GetComponent<Image>(),
                Cargar<Sprite>(RutaPreocupado));

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena, RutaEscena);
            AssetDatabase.SaveAssets();
            VerificarEscena();
            Debug.Log("HU03_GENERACION_OK: escena construida con plantilla común, tarjetas de solo imagen y fauna enmarcada.");
        }

        private static RetoSeleccionConfiguracion CrearConfiguracion()
        {
            RetoSeleccionConfiguracion c = AssetDatabase.LoadAssetAtPath<RetoSeleccionConfiguracion>(RutaConfiguracion);
            if (c == null)
            {
                c = ScriptableObject.CreateInstance<RetoSeleccionConfiguracion>();
                AssetDatabase.CreateAsset(c, RutaConfiguracion);
            }

            c.Nivel = 1;
            c.NumeroEscena = 3;
            c.TotalEscenas = 12;
            c.TituloNivel = "Nivel 1 · Conozco la selva";
            c.Pregunta = "En la selva viven aves de muchos colores.\n¿Cuál de estos animales es el guacamayo?";
            c.Opciones = new[] { "", "", "" };
            c.ImagenesOpciones = new[]
            {
                Cargar<Sprite>("Assets/Art/Characters/Mono.png"),
                Cargar<Sprite>("Assets/Art/Characters/Guacamayo.png"),
                Cargar<Sprite>("Assets/Art/Characters/Tortuga.png")
            };
            c.IndiceCorrecto = 1;
            c.ExplicacionesIncorrectas = new[]
            {
                "¡Buen intento! El mono vive en los árboles. Busca el ave de colores y vuelve a intentarlo.",
                "¡Muy bien! Identificaste el guacamayo.",
                "¡Buen intento! La tortuga tiene caparazón. Busca el ave de colores y vuelve a intentarlo."
            };
            c.ImagenEscena = Cargar<Sprite>(RutaFauna);
            c.Pista = "Busca el ave con plumas rojas, amarillas y azules y un pico curvo.";
            c.Felicitacion = "¡Lo lograste!";
            c.AudioLectura = Cargar<AudioClip>("Assets/Audio/Voice/Guacamayo.wav");
            c.AudioPista = Cargar<AudioClip>("Assets/Audio/Voice/HU03/pista_01_03.wav");
            c.AudioCorrecciones = new[]
            {
                Cargar<AudioClip>("Assets/Audio/Voice/HU03/correccion_mono_01_03.wav"),
                null,
                Cargar<AudioClip>("Assets/Audio/Voice/HU03/correccion_tortuga_01_03.wav")
            };
            c.AudioFelicitacion = Cargar<AudioClip>("Assets/Audio/Voice/HU03/acierto_01_03.wav");
            c.Fondo = Cargar<Sprite>("Assets/Art/Backgrounds/Niveles/N1_Conozco_la_selva.png");
            c.Personaje = Cargar<Sprite>("Assets/Art/Characters/Explorador.png");
            c.PersonajeIncorrecto = Cargar<Sprite>(RutaPreocupado);
            c.FotogramasCelebracion = BuildAssignedScenes.CargarFotogramasExplorador();
            c.Fuente = Cargar<TMP_FontAsset>("Assets/Fonts/OpenDyslexic3-Regular SDF.asset");
            c.SiguienteEscena = "Escena04_HU04_Plantas";
            c.VideoSenas = string.Empty;
            EditorUtility.SetDirty(c);
            AssetDatabase.SaveAssets();
            return c;
        }

        private static void VerificarEscena()
        {
            Scene escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);
            RetoSeleccionView vista = escena.GetRootGameObjects()
                .SelectMany(objeto => objeto.GetComponentsInChildren<RetoSeleccionView>(true)).FirstOrDefault();
            GuacamayoPresenter presenter = escena.GetRootGameObjects()
                .SelectMany(objeto => objeto.GetComponentsInChildren<GuacamayoPresenter>(true)).FirstOrDefault();
            if (vista == null || presenter == null || vista.GetComponent<AudioSource>() == null)
                throw new InvalidOperationException("La escena HU-03 no guardó sus componentes principales.");
            SerializedProperty referenciaAudio = new SerializedObject(vista)
                .FindProperty("_ref").FindPropertyRelative("fuenteAudio");
            if (referenciaAudio == null || referenciaAudio.objectReferenceValue != vista.GetComponent<AudioSource>())
                throw new InvalidOperationException("La escena HU-03 no enlazó la fuente de audio del botón Lectura.");
            if (escena.GetRootGameObjects().SelectMany(objeto => objeto.GetComponentsInChildren<Button>(true)).Count() < 12)
                throw new InvalidOperationException("La escena HU-03 no guardó los controles de la plantilla.");
        }

        private static T Cargar<T>(string ruta) where T : UnityEngine.Object
        {
            T recurso = AssetDatabase.LoadAssetAtPath<T>(ruta);
            if (recurso == null) throw new InvalidOperationException("Falta recurso de HU-03: " + ruta);
            return recurso;
        }
    }
}
