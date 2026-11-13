using System;
using System.Collections.Generic;
using System.Linq;
using GuardianesAmazonicos.Models;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

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
                Felicitacion = "¡Muy bien! Reconociste al mono."
            },
            new Reto
            {
                Nombre = "HU-03 Guacamayo",
                RutaConfiguracion = "Assets/Scripts/Models/NivelesData/RetoGuacamayo.asset",
                RutaEscena = "Assets/Scenes/Niveles/Nivel01/Escena03_HU03_Guacamayo.unity",
                Nivel = 1,
                NumeroEscena = 3,
                Felicitacion = "¡Muy bien! Reconociste al guacamayo.",
                Explicaciones = new[]
                {
                    "El mono trepa los árboles y tiene cola larga. El guacamayo es un ave de plumas de colores.",
                    string.Empty,
                    "La tortuga camina despacio y lleva un caparazón duro. El guacamayo es un ave de plumas de colores."
                }
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
                Felicitacion = "¡Muy bien! El agua es importante para los seres vivos."
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
            NormalizarDatos();
            ReconstruirEscenaMono();
            IntegrarNivel01.AplicarOrdenDeEscenas();
            IntegrarNivel01.AplicarEnlacesSiguiente();
            IntegrarNivel01.AplicarFuenteAccesible();

            AssetDatabase.SaveAssets();
            Debug.Log("EstandarizarEscenas: datos, escena de HU-02 y tipografía estandarizados.");
            PruebasEstandarEscenas.EjecutarPruebas();
        }

        /// <summary>Punto de entrada para Unity en modo batch: -executeMethod.</summary>
        public static void EstandarizarPorLote()
        {
            try
            {
                Estandarizar();
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

                if (reto.Explicaciones != null && reto.Explicaciones.Length == RetoSeleccionConfiguracion.CANTIDAD_OPCIONES
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

                if (cambios.Count > 0)
                {
                    EditorUtility.SetDirty(configuracion);
                    Debug.Log("EstandarizarEscenas: " + configuracion.name + " -> " + string.Join(", ", cambios));
                }
            }

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Reconstruye HU-02 con la plantilla serializada. Su escena anterior generaba
        /// toda la interfaz en tiempo de ejecución y no tenía animaciones ni paneles
        /// equivalentes a los del resto de actividades.
        /// </summary>
        public static void ReconstruirEscenaMono()
        {
            Reto mono = Retos.First(reto => reto.Nombre == "HU-02 Mono");
            RetoSeleccionConfiguracion configuracion =
                AssetDatabase.LoadAssetAtPath<RetoSeleccionConfiguracion>(mono.RutaConfiguracion);
            if (configuracion == null)
                throw new InvalidOperationException("No existe la configuración: " + mono.RutaConfiguracion);

            BuildAssignedScenes.PrepararRecursos();
            BuildAssignedScenes.CrearEscena(mono.RutaEscena, configuracion);
            Debug.Log("EstandarizarEscenas: HU-02 reconstruida con la plantilla aprobada.");
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
    }
}
