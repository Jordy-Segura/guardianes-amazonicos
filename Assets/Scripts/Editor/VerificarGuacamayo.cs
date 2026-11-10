using System;
using System.IO;
using System.Linq;
using GuardianesAmazonicos.Models;
using GuardianesAmazonicos.Presenters;
using GuardianesAmazonicos.Views;
using TMPro;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GuardianesAmazonicos.Editor
{
    public static class VerificarGuacamayo
    {
        public const string RutaEscena = "Assets/Scenes/Niveles/Nivel01/Escena03_HU03_Guacamayo.unity";
        private const string RutaConfiguracion = "Assets/Scripts/Models/NivelesData/RetoGuacamayo.asset";

        [MenuItem("Guardianes/HU-03/Generar escena")]
        public static void Generar()
        {
            AssetDatabase.Refresh();
            string[] animales = { "Mono", "Guacamayo", "Tortuga", "Explorador" };
            TextureImporter importadorFondo = AssetImporter.GetAtPath("Assets/Art/Backgrounds/N1_Conozco_la_selva.png") as TextureImporter;
            if (importadorFondo == null) throw new InvalidOperationException("Falta el fondo de Nivel 1.");
            ConfigurarSpriteAltaCalidad(importadorFondo);
            foreach (string animal in animales)
            {
                string ruta = "Assets/Art/Characters/" + animal + ".png";
                TextureImporter importador = AssetImporter.GetAtPath(ruta) as TextureImporter;
                if (importador == null) throw new InvalidOperationException("Falta imagen: " + ruta);
                ConfigurarSpriteAltaCalidad(importador);
            }
            foreach (string ruta in new[] {
                "Assets/Art/Backgrounds/EscenaFaunaHU03.png",
                "Assets/Art/Characters/ExploradorPreocupado.png",
                "Assets/Art/Characters/ExploradorCelebrando.png",
                "Assets/Art/UI/HU03/InsigniaProgreso.png",
                "Assets/Art/UI/HU03/FondoAciertoVerde.png",
                "Assets/Art/UI/HU03/IconoReintentar.png" })
            {
                TextureImporter importador = AssetImporter.GetAtPath(ruta) as TextureImporter;
                if (importador == null) throw new InvalidOperationException("Falta recurso visual HU-03: " + ruta);
                ConfigurarSpriteAltaCalidad(importador);
            }
            foreach (string nombre in new[] { "TarjetaRespuesta", "MarcoPregunta", "GloboPregunta" })
            {
                TextureImporter importador = AssetImporter.GetAtPath("Assets/Art/UI/HU03/" + nombre + ".png") as TextureImporter;
                if (importador == null) throw new InvalidOperationException("Falta UI: " + nombre);
                ConfigurarSpriteAltaCalidad(importador);
            }
            foreach (string nombre in new[] { "BannerNivel", "InsigniaProgreso", "CartaPista", "TarjetaSenas", "boton_cerrar", "boton_reproducir" })
            {
                string ruta = "Assets/Art/UI/HU03/" + nombre + ".png";
                TextureImporter importador = AssetImporter.GetAtPath(ruta) as TextureImporter;
                if (importador == null) throw new InvalidOperationException("Falta componente visual HU-03: " + ruta);
                ConfigurarSpriteAltaCalidad(importador);
            }
            foreach (string nombre in new[] { "BotonLectura", "BotonSenas", "BotonAyuda" })
            {
                TextureImporter importador = AssetImporter.GetAtPath("Assets/Art/UI/HU03/" + nombre + ".png") as TextureImporter;
                if (importador == null) throw new InvalidOperationException("Falta control aprobado: " + nombre);
                ConfigurarSpriteAltaCalidad(importador);
            }
            string[] componentes = { "1.7 Regresar", "1.7 Avanzar", "1.7 Inicio", "1.7 Configuración",
                "boton_aceptar", "BotonVoyIntentarlo", "boton_siguiente", "letrero_muy_bien", "brillo_respuesta_seleccionada" };
            foreach (string nombre in componentes)
            {
                string ruta = "Assets/Art/UI/HU03/" + nombre + ".png";
                TextureImporter importador = AssetImporter.GetAtPath(ruta) as TextureImporter;
                if (importador == null) throw new InvalidOperationException("Falta control aprobado: " + ruta);
                ConfigurarSpriteAltaCalidad(importador);
                if (nombre == "brillo_respuesta_seleccionada")
                {
                    importador.spriteBorder = new Vector4(155f, 155f, 155f, 155f);
                    importador.SaveAndReimport();
                }
            }
            foreach (string carpeta in new[] { "Assets/Resources/HU03/Animaciones/Explorador", "Assets/Resources/HU03/Animaciones/Guacamayo" })
            {
                if (!Directory.Exists(carpeta)) throw new InvalidOperationException("Faltan fotogramas aprobados: " + carpeta);
                foreach (string ruta in Directory.GetFiles(carpeta, "*.png"))
                {
                    TextureImporter importador = AssetImporter.GetAtPath(ruta) as TextureImporter;
                    if (importador == null) continue;
                    ConfigurarSpriteAltaCalidad(importador, false);
                }
            }
            AssetDatabase.Refresh();
            RetoSeleccionConfiguracion configuracion = AssetDatabase.LoadAssetAtPath<RetoSeleccionConfiguracion>(RutaConfiguracion);
            if (configuracion == null)
            {
                configuracion = ScriptableObject.CreateInstance<RetoSeleccionConfiguracion>();
                AssetDatabase.CreateAsset(configuracion, RutaConfiguracion);
            }
            configuracion.numeroEscena = 3;
            configuracion.totalRetos = 12;
            configuracion.titulo = "Nivel 1 · Conozco la selva";
            configuracion.pregunta = "Escucha: guacamayo.\n¿Cuál es el guacamayo?";
            configuracion.instruccion = "Escucha, selecciona y pulsa Aceptar.";
            configuracion.opciones = new[] { "Mono", "Guacamayo", "Tortuga" };
            configuracion.imagenes = animales.Take(3).Select(animal => Cargar<Sprite>("Assets/Art/Characters/" + animal + ".png")).ToArray();
            configuracion.imagenEscena = Cargar<Sprite>("Assets/Art/Backgrounds/EscenaFaunaHU03.png");
            configuracion.insigniaProgreso = Cargar<Sprite>("Assets/Art/UI/HU03/InsigniaProgreso.png");
            configuracion.indiceCorrecto = 1;
            configuracion.pistaIncorrecta = "Busca el ave de plumas rojas, amarillas y azules y pico curvo.";
            configuracion.mensajeCorrecto = "¡Muy bien! Identificaste al guacamayo.";
            configuracion.pronunciaciones = new[] { Cargar<AudioClip>("Assets/Audio/Voice/Guacamayo.wav") };
            configuracion.fondo = Cargar<Sprite>("Assets/Art/Backgrounds/N1_Conozco_la_selva.png");
            configuracion.explorador = Cargar<Sprite>("Assets/Art/Characters/Explorador.png");
            configuracion.fuente = Cargar<TMP_FontAsset>("Assets/Fonts/OpenDyslexic3-Regular SDF.asset");
            configuracion.siguienteEscena = "Escena04_HU04_Plantas";
            EditorUtility.SetDirty(configuracion);
            AssetDatabase.SaveAssets();
            Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject camara = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camara.tag = "MainCamera";
            camara.transform.position = new Vector3(0, 0, -10);
            camara.GetComponent<Camera>().orthographic = true;
            GameObject actividad = new GameObject("HU03_Guacamayo");
            GuacamayoView vista = actividad.AddComponent<GuacamayoView>();
            vista.ConfigurarArte(Cargar<Sprite>("Assets/Art/UI/HU03/TarjetaRespuesta.png"),
                Cargar<Sprite>("Assets/Art/UI/HU03/MarcoPregunta.png"), Cargar<Sprite>("Assets/Art/UI/HU03/GloboPregunta.png"),
                new[] { Cargar<Sprite>("Assets/Art/UI/HU03/BotonLectura.png"), Cargar<Sprite>("Assets/Art/UI/HU03/BotonSenas.png"),
                    Cargar<Sprite>("Assets/Art/UI/HU03/BotonAyuda.png") },
                new[] { Cargar<Sprite>("Assets/Art/UI/HU03/1.7 Regresar.png"), Cargar<Sprite>("Assets/Art/UI/HU03/1.7 Avanzar.png"),
                    Cargar<Sprite>("Assets/Art/UI/HU03/1.7 Inicio.png"), Cargar<Sprite>("Assets/Art/UI/HU03/1.7 Configuración.png") },
                Cargar<Sprite>("Assets/Art/UI/HU03/boton_aceptar.png"), Cargar<Sprite>("Assets/Art/UI/HU03/BotonVoyIntentarlo.png"),
                Cargar<Sprite>("Assets/Art/UI/HU03/boton_siguiente.png"), Cargar<Sprite>("Assets/Art/UI/HU03/letrero_muy_bien.png"),
                Cargar<Sprite>("Assets/Art/UI/HU03/brillo_respuesta_seleccionada.png"),
                Cargar<Sprite>("Assets/Art/UI/HU03/BannerNivel.png"), Cargar<Sprite>("Assets/Art/UI/HU03/CartaPista.png"),
                Cargar<Sprite>("Assets/Art/UI/HU03/TarjetaSenas.png"), Cargar<Sprite>("Assets/Art/UI/HU03/boton_cerrar.png"),
                Cargar<Sprite>("Assets/Art/UI/HU03/boton_reproducir.png"),
                Cargar<Sprite>("Assets/Art/Characters/ExploradorPreocupado.png"),
                Cargar<Sprite>("Assets/Art/Characters/ExploradorCelebrando.png"),
                Cargar<Sprite>("Assets/Art/UI/HU03/FondoAciertoVerde.png"),
                Cargar<Sprite>("Assets/Art/UI/HU03/IconoReintentar.png"));
            actividad.AddComponent<GuacamayoPresenter>().Configurar(configuracion, vista);
            EditorSceneManager.SaveScene(escena, RutaEscena);
            AssetDatabase.SaveAssets();
            Debug.Log("HU03_GENERACION_OK: escena y configuración con referencias guardadas.");
        }

        public static void Compilar()
        {
            string salida = Environment.GetEnvironmentVariable("HU03_BUILD");
            if (string.IsNullOrWhiteSpace(salida)) throw new InvalidOperationException("Falta HU03_BUILD.");
            BuildReport informe = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { RutaEscena },
                locationPathName = salida,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            if (informe.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Compilación fallida: " + informe.summary.result);
            Debug.Log("HU03_BUILD_OK: " + informe.summary.totalSize + " bytes.");
        }

        private static T Cargar<T>(string ruta) where T : UnityEngine.Object
        {
            T recurso = AssetDatabase.LoadAssetAtPath<T>(ruta);
            if (recurso == null) throw new InvalidOperationException("Falta recurso: " + ruta);
            return recurso;
        }

        private static void ConfigurarSpriteAltaCalidad(TextureImporter importador, bool conservarSinPerdida = true)
        {
            TextureImporterCompression compresion = conservarSinPerdida
                ? TextureImporterCompression.Uncompressed
                : TextureImporterCompression.CompressedHQ;
            importador.textureType = TextureImporterType.Sprite;
            importador.spriteImportMode = SpriteImportMode.Single;
            importador.alphaIsTransparency = true;
            importador.mipmapEnabled = false;
            importador.filterMode = FilterMode.Bilinear;
            importador.anisoLevel = 4;
            importador.maxTextureSize = 4096;
            importador.textureCompression = compresion;
            importador.compressionQuality = 100;

            foreach (string plataforma in new[] { "Standalone", "Android", "iPhone" })
            {
                TextureImporterPlatformSettings ajustes = importador.GetPlatformTextureSettings(plataforma);
                ajustes.name = plataforma;
                ajustes.overridden = true;
                ajustes.maxTextureSize = 4096;
                ajustes.format = TextureImporterFormat.Automatic;
                ajustes.textureCompression = compresion;
                ajustes.compressionQuality = 100;
                importador.SetPlatformTextureSettings(ajustes);
            }

            importador.SaveAndReimport();
        }
    }
}
