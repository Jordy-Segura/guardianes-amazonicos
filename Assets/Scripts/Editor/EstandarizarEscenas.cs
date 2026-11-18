using System;
using System.Collections.Generic;
using System.Linq;
using GuardianesAmazonicos.Models;
using GuardianesAmazonicos.Views;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GuardianesAmazonicos.Editor
{
    /// <summary>
    /// Deja todas las escenas de actividad con el mismo estándar visual y de datos:
    /// plantilla serializada, tipografía OpenDyslexic, animación del personaje guía,
    /// de la celebración y de la pista, audio ambiente, paneles de Ayuda y Señas, y
    /// datos completos en cada RetoSeleccionConfiguracion.
    /// No cambia el arte, las coordenadas aprobadas ni la arquitectura MVP.
    /// </summary>
    public static class EstandarizarEscenas
    {
        /// <summary>Datos mínimos y destino de una actividad estandarizada.</summary>
        public sealed class Reto
        {
            public string RutaConfiguracion;
            public string RutaEscena;
            public string Nombre;
            public int Nivel;
            public int NumeroEscena;
            public string Felicitacion;
            public string[] Explicaciones;
            public string RutaAudioLectura;
            public string RutaAudioPista;
            public string[] RutasAudioCorrecciones;
            public string RutaAudioFelicitacion;
            public string RutaFondo;
            public int Tarjetas = 3;
            public bool Reconstruir;
        }

        private const string ExploradorPreocupado = "Assets/Art/Characters/ExploradorPreocupado.png";

        public static readonly Reto[] Retos =
        {
            new Reto
            {
                Nombre = "HU-02 Mono",
                RutaConfiguracion = "Assets/Scripts/Models/NivelesData/RetoMono.asset",
                RutaEscena = "Assets/Scenes/Niveles/Nivel01/Escena02_HU02_Mono.unity",
                Nivel = 1,
                NumeroEscena = 2,
                Felicitacion = "¡Muy bien! Reconociste al mono.",
                Reconstruir = true
            },
            new Reto
            {
                Nombre = "HU-03 Guacamayo",
                RutaConfiguracion = "Assets/Scripts/Models/NivelesData/RetoGuacamayo.asset",
                RutaEscena = "Assets/Scenes/Niveles/Nivel01/Escena03_HU03_Guacamayo.unity",
                Nivel = 1,
                NumeroEscena = 3,
                Felicitacion = "¡Muy bien! Reconociste al guacamayo.",
                RutaAudioLectura = "Assets/Audio/Voice/Lectura/lectura_01_03.wav",
                RutaFondo = "Assets/Art/Backgrounds/Niveles/N1_Conozco_la_selva.png",
                Reconstruir = true
            },
            new Reto
            {
                Nombre = "HU-05 Jaguar",
                RutaConfiguracion = "Assets/Scripts/Models/NivelesData/RetoJaguar.asset",
                RutaEscena = "Assets/Scenes/Niveles/Nivel01/Escena05_IdentificarJaguar.unity",
                Nivel = 1,
                NumeroEscena = 5,
                Felicitacion = "¡Lo lograste! Encontraste al jaguar."
            },
            new Reto
            {
                Nombre = "HU-06 Tortuga",
                RutaConfiguracion = "Assets/Scripts/Models/NivelesData/RetoTortuga.asset",
                RutaEscena = "Assets/Scenes/Niveles/Nivel01/Escena06_HU06_TortugaAgua.unity",
                Nivel = 1,
                NumeroEscena = 6,
                Felicitacion = "¡Muy bien! El agua es importante para los seres vivos.",
                RutaAudioLectura = "Assets/Audio/Voice/Lectura/lectura_01_06.wav",
                RutaAudioPista = "Assets/Audio/Voice/Lectura/pista_01_06.wav",
                RutaAudioFelicitacion = "Assets/Audio/Voice/Lectura/acierto_01_06.wav",
                RutasAudioCorrecciones = new[]
                {
                    null,
                    "Assets/Audio/Voice/Lectura/correccion_01_06_fruta.wav",
                    "Assets/Audio/Voice/Lectura/correccion_01_06_arbol.wav"
                },
                Reconstruir = true
            },
            new Reto
            {
                Nombre = "HU-09 Plátano",
                RutaConfiguracion = "Assets/Scripts/Models/NivelesData/RetoPlatano.asset",
                RutaEscena = "Assets/Scenes/Niveles/Nivel01/Escena09_IdentificarPlatano.unity",
                Nivel = 1,
                NumeroEscena = 9,
                Felicitacion = "¡Lo lograste! Identificaste el plátano."
            },
            new Reto
            {
                Nombre = "HU-10 Loro",
                RutaConfiguracion = "Assets/Scripts/Models/NivelesData/RetoLoro.asset",
                RutaEscena = "Assets/Scenes/Niveles/Nivel01/Escena10_HU10_Loro.unity",
                Nivel = 1,
                NumeroEscena = 10,
                Felicitacion = "¡Muy bien! Ese es el loro.",
                Tarjetas = 3,
                RutaAudioLectura = "Assets/Audio/Voice/HU10/lectura_01_10.wav",
                RutaAudioPista = "Assets/Audio/Voice/HU10/pista_01_10.wav",
                RutaAudioFelicitacion = "Assets/Audio/Voice/HU10/acierto_01_10.wav",
                RutasAudioCorrecciones = new[]
                {
                    null,
                    "Assets/Audio/Voice/HU10/correccion_guacamayo_01_10.wav",
                    "Assets/Audio/Voice/HU10/correccion_jaguar_01_10.wav"
                },
                Reconstruir = true
            },
            new Reto
            {
                Nombre = "Nivel 2 · Mono Curioso",
                RutaConfiguracion = "Assets/Scripts/Models/NivelesData/ProtegerHabitatMono.asset",
                RutaEscena = "Assets/Scenes/Niveles/Nivel02/Escena01_ProtegerHabitatMono.unity",
                Nivel = 2,
                NumeroEscena = 1,
                Felicitacion = "¡Lo lograste! Cuidar el bosque protege su hogar."
            }
        };

        [MenuItem("Guardianes/Sahid/Estandarizar escenas de actividad")]
        public static void Estandarizar()
        {
            Estandarizar(false);
        }

        /// <summary>
        /// Aplica el estándar. Con <paramref name="forzarReconstruccion"/> vuelve a
        /// generar las escenas de plantilla aunque ya existan, para incorporar los
        /// cambios de la plantilla común.
        /// </summary>
        public static void Estandarizar(bool forzarReconstruccion)
        {
            NormalizarDatos();
            ReconstruirEscenasDePlantilla(forzarReconstruccion);
            IntegrarNivel01.AplicarOrdenDeEscenas();
            IntegrarNivel01.AplicarEnlacesSiguiente();
            IntegrarNivel01.AplicarFuenteAccesible();

            AssetDatabase.SaveAssets();
            Debug.Log("EstandarizarEscenas: datos, escenas de plantilla y tipografía estandarizados.");
            PruebasEstandarEscenas.EjecutarPruebas();
        }

        /// <summary>Punto de entrada para Unity en modo batch: -executeMethod.</summary>
        public static void EstandarizarPorLote()
        {
            EstandarizarPorLote(false);
        }

        /// <summary>Reconstruye las escenas de plantilla y luego verifica todo el estándar.</summary>
        public static void EstandarizarReconstruyendoPorLote()
        {
            EstandarizarPorLote(true);
        }

        /// <summary>
        /// Genera la actividad HU-10 (loro) si todavía no existe y después aplica
        /// el estándar y las pruebas completas del Nivel 1.
        /// </summary>
        public static void EstandarizarConLoroPorLote()
        {
            try
            {
                BuildSceneHU10.Generar();
                Estandarizar(false);
                PruebasIntegracionNivel1.EjecutarPruebas();
                PruebasHU06.EjecutarPruebas();
                EditorApplication.Exit(0);
            }
            catch (Exception excepcion)
            {
                Debug.LogError("EstandarizarEscenas: la generación de HU-10 falló. " + excepcion);
                EditorApplication.Exit(1);
            }
        }

        private static void EstandarizarPorLote(bool forzarReconstruccion)
        {
            try
            {
                Estandarizar(forzarReconstruccion);
                PruebasIntegracionNivel1.EjecutarPruebas();
                PruebasHU06.EjecutarPruebas();
                EditorApplication.Exit(0);
            }
            catch (Exception excepcion)
            {
                Debug.LogError("EstandarizarEscenas: la estandarización falló. " + excepcion);
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// Completa los datos que cada actividad necesita para comportarse igual que
        /// las demás: título, personajes, fotogramas, video de señas, explicaciones y
        /// progreso. No sobrescribe contenido propio ya revisado.
        /// </summary>
        public static void NormalizarDatos()
        {
            TMP_FontAsset fuente = PlantillaUIBuilder.Cargar<TMP_FontAsset>(PlantillaUIBuilder.FUENTE);
            Sprite preocupado = PlantillaUIBuilder.S(ExploradorPreocupado);
            Sprite[] celebracion = BuildAssignedScenes.CargarFotogramasExplorador();

            if (fuente == null) throw new InvalidOperationException("No se encontró la fuente accesible.");
            if (preocupado == null) throw new InvalidOperationException("No se encontró ExploradorPreocupado.");
            if (celebracion == null || celebracion.Length == 0)
                throw new InvalidOperationException("No se encontraron los fotogramas del Explorador.");

            foreach (Reto reto in Retos)
            {
                RetoSeleccionConfiguracion configuracion =
                    AssetDatabase.LoadAssetAtPath<RetoSeleccionConfiguracion>(reto.RutaConfiguracion);
                if (configuracion == null)
                    throw new InvalidOperationException("No existe la configuración: " + reto.RutaConfiguracion);

                List<string> cambios = new List<string>();

                string titulo = reto.Nivel == 1 ? "Nivel 1 • Conozco la selva" : "Nivel 2 • Cuido los animales";
                if (configuracion.TituloNivel != titulo) { configuracion.TituloNivel = titulo; cambios.Add("título"); }
                if (configuracion.Nivel != reto.Nivel) { configuracion.Nivel = reto.Nivel; cambios.Add("nivel"); }
                if (configuracion.NumeroEscena != reto.NumeroEscena)
                {
                    configuracion.NumeroEscena = reto.NumeroEscena;
                    cambios.Add("número de escena");
                }
                if (configuracion.TotalEscenas != 12) { configuracion.TotalEscenas = 12; cambios.Add("total de escenas"); }

                if (configuracion.Personaje == null)
                {
                    configuracion.Personaje = PlantillaUIBuilder.S("Assets/Art/Characters/Explorador.png");
                    cambios.Add("personaje guía");
                }
                if (configuracion.PersonajeIncorrecto == null)
                {
                    configuracion.PersonajeIncorrecto = preocupado;
                    cambios.Add("personaje de error");
                }
                if (configuracion.FotogramasCelebracion == null || configuracion.FotogramasCelebracion.Length == 0)
                {
                    configuracion.FotogramasCelebracion = celebracion;
                    cambios.Add("fotogramas de celebración");
                }

                string video = string.Format("senas_nivel_{0:00}_pregunta_{1:00}.mp4", reto.Nivel, reto.NumeroEscena);
                if (configuracion.VideoSenas != video) { configuracion.VideoSenas = video; cambios.Add("video de señas"); }

                if (string.IsNullOrWhiteSpace(configuracion.Felicitacion) || configuracion.Felicitacion == "¡Lo lograste!")
                {
                    configuracion.Felicitacion = reto.Felicitacion;
                    cambios.Add("felicitación");
                }

                if (reto.Explicaciones != null && reto.Explicaciones.Length == reto.Tarjetas
                    && !MismasExplicaciones(configuracion.ExplicacionesIncorrectas, reto.Explicaciones))
                {
                    configuracion.ExplicacionesIncorrectas = reto.Explicaciones;
                    cambios.Add("explicaciones");
                }

                if (configuracion.Fuente != fuente) { configuracion.Fuente = fuente; cambios.Add("tipografía"); }
                if (configuracion.textoProgreso != reto.NumeroEscena + " de 12")
                {
                    configuracion.textoProgreso = reto.NumeroEscena + " de 12";
                    cambios.Add("progreso heredado");
                }

                if (!string.IsNullOrEmpty(reto.RutaAudioLectura))
                {
                    AudioClip locucion = PlantillaUIBuilder.Cargar<AudioClip>(reto.RutaAudioLectura);
                    if (locucion == null)
                        throw new InvalidOperationException("No existe la locución: " + reto.RutaAudioLectura);
                    if (configuracion.AudioLectura != locucion)
                    {
                        configuracion.AudioLectura = locucion;
                        cambios.Add("locución");
                    }
                }

                if (!string.IsNullOrEmpty(reto.RutaAudioPista))
                {
                    AudioClip pista = PlantillaUIBuilder.Cargar<AudioClip>(reto.RutaAudioPista);
                    if (pista == null)
                        throw new InvalidOperationException("No existe el audio de pista: " + reto.RutaAudioPista);
                    if (configuracion.AudioPista != pista)
                    {
                        configuracion.AudioPista = pista;
                        cambios.Add("audio de pista");
                    }
                }

                if (reto.RutasAudioCorrecciones != null && reto.RutasAudioCorrecciones.Length == reto.Tarjetas)
                {
                    AudioClip[] correcciones = new AudioClip[reto.RutasAudioCorrecciones.Length];
                    for (int indice = 0; indice < correcciones.Length; indice++)
                    {
                        string ruta = reto.RutasAudioCorrecciones[indice];
                        if (string.IsNullOrEmpty(ruta)) continue;
                        AudioClip clip = PlantillaUIBuilder.Cargar<AudioClip>(ruta);
                        if (clip == null)
                            throw new InvalidOperationException("No existe el audio de corrección: " + ruta);
                        correcciones[indice] = clip;
                    }

                    if (!MismosAudios(configuracion.AudioCorrecciones, correcciones))
                    {
                        configuracion.AudioCorrecciones = correcciones;
                        cambios.Add("audios de corrección");
                    }
                }

                if (!string.IsNullOrEmpty(reto.RutaAudioFelicitacion))
                {
                    AudioClip felicitacion = PlantillaUIBuilder.Cargar<AudioClip>(reto.RutaAudioFelicitacion);
                    if (felicitacion == null)
                        throw new InvalidOperationException("No existe el audio de felicitación: " + reto.RutaAudioFelicitacion);
                    if (configuracion.AudioFelicitacion != felicitacion)
                    {
                        configuracion.AudioFelicitacion = felicitacion;
                        cambios.Add("audio de felicitación");
                    }
                }

                if (!string.IsNullOrEmpty(reto.RutaFondo))
                {
                    Sprite fondo = PlantillaUIBuilder.S(reto.RutaFondo);
                    if (fondo == null)
                        throw new InvalidOperationException("No existe el fondo: " + reto.RutaFondo);
                    if (configuracion.Fondo != fondo)
                    {
                        configuracion.Fondo = fondo;
                        cambios.Add("fondo");
                    }
                }

                if (cambios.Count > 0)
                {
                    EditorUtility.SetDirty(configuracion);
                    Debug.Log("EstandarizarEscenas: " + configuracion.name + " -> " + string.Join(", ", cambios));
                }
            }

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Reconstruye con la plantilla serializada las escenas que todavía generaban
        /// su interfaz en tiempo de ejecución (HU-02 y HU-03), para que tengan las
        /// mismas animaciones, paneles y referencias que el resto de actividades.
        /// </summary>
        public static void ReconstruirEscenasDePlantilla(bool forzar = false)
        {
            BuildAssignedScenes.PrepararRecursos();
            IEnumerable<Reto> pendientes = Retos.Where(reto => reto.Reconstruir);

            foreach (Reto reto in pendientes)
            {
                if (!forzar && YaUsaLaPlantilla(reto.RutaEscena))
                {
                    Debug.Log("EstandarizarEscenas: " + reto.Nombre + " ya usa la plantilla serializada.");
                    continue;
                }

                RetoSeleccionConfiguracion configuracion =
                    AssetDatabase.LoadAssetAtPath<RetoSeleccionConfiguracion>(reto.RutaConfiguracion);
                if (configuracion == null)
                    throw new InvalidOperationException("No existe la configuración: " + reto.RutaConfiguracion);

                BuildAssignedScenes.CrearEscena(reto.RutaEscena, configuracion);
                Debug.Log("EstandarizarEscenas: " + reto.Nombre + " reconstruida con la plantilla aprobada.");
            }
        }

        /// <summary>
        /// Evita regenerar una escena que ya está en la plantilla: así el proceso es
        /// idempotente y no reescribe los identificadores internos de la escena.
        /// </summary>
        private static bool YaUsaLaPlantilla(string ruta)
        {
            if (!System.IO.File.Exists(ruta)) return false;

            Scene escena = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            GameObject[] raices = escena.GetRootGameObjects();

            bool tieneVista = raices
                .SelectMany(objeto => objeto.GetComponentsInChildren<RetoSeleccionView>(true))
                .Any();
            bool tieneCanvas = raices
                .SelectMany(objeto => objeto.GetComponentsInChildren<Canvas>(true))
                .Any(canvas => canvas.name == "CanvasActividad");
            int animaciones = raices
                .SelectMany(objeto => objeto.GetComponentsInChildren<AnimacionPersonajePNG>(true))
                .Count();

            return tieneVista && tieneCanvas && animaciones >= 3;
        }

        private static bool MismasExplicaciones(string[] actuales, string[] esperadas)
        {
            if (actuales == null || actuales.Length != esperadas.Length) return false;
            for (int indice = 0; indice < esperadas.Length; indice++)
            {
                string actual = actuales[indice] ?? string.Empty;
                string esperada = esperadas[indice] ?? string.Empty;
                if (actual.Trim() != esperada.Trim()) return false;
            }

            return true;
        }

        private static bool MismosAudios(AudioClip[] actuales, AudioClip[] esperados)
        {
            if (actuales == null || actuales.Length != esperados.Length) return false;
            for (int indice = 0; indice < esperados.Length; indice++)
            {
                if (actuales[indice] != esperados[indice]) return false;
            }

            return true;
        }
    }
}
