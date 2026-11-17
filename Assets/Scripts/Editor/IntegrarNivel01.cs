using System;
using System.Collections.Generic;
using System.Linq;
using GuardianesAmazonicos.Models;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GuardianesAmazonicos.Editor
{
    /// <summary>
    /// Integra el Nivel 1 ya implementado sin reconstruir escenas ni alterar la
    /// plantilla aprobada: ordena el Nivel 1 y el inicio del Nivel 2 en Build
    /// Profiles, enlaza el botón Siguiente de cada reto y unifica la tipografía
    /// accesible OpenDyslexic (RNF-01). Se puede ejecutar desde el menú o en lote.
    /// </summary>
    public static class IntegrarNivel01
    {
        public const string FuenteAccesible = "Assets/Fonts/OpenDyslexic3-Regular SDF.asset";

        /// <summary>Orden real de la aventura: Nivel 1 completo y luego Nivel 2.</summary>
        public static readonly string[] OrdenEscenas =
        {
            "Assets/Scenes/Niveles/Nivel01/BienvenidaDelGuardian.unity",
            "Assets/Scenes/Niveles/Nivel01/Escena02_HU02_Mono.unity",
            "Assets/Scenes/Niveles/Nivel01/Escena03_HU03_Guacamayo.unity",
            "Assets/Scenes/Niveles/Nivel01/Escena04_HU04_Plantas.unity",
            "Assets/Scenes/Niveles/Nivel01/Escena05_IdentificarJaguar.unity",
            "Assets/Scenes/Niveles/Nivel01/Escena06_HU06_TortugaAgua.unity",
            "Assets/Scenes/Niveles/Nivel01/Escena07_HU07_IdentificarRio.unity",
            "Assets/Scenes/Niveles/Nivel01/Escena08_HU08_Mariposa.unity",
            "Assets/Scenes/Niveles/Nivel01/Escena09_IdentificarPlatano.unity",
            "Assets/Scenes/Niveles/Nivel01/Escena11_HU11_ReconocerArbol.unity",
            "Assets/Scenes/Niveles/Nivel01/Escena12_Recompensa_final.unity",
            "Assets/Scenes/Niveles/Nivel02/Escena01_ProtegerHabitatMono.unity"
        };

        // Nombres de escena que se guardan en cada RetoSeleccionConfiguracion.
        public const string EscenaMono = "Escena02_HU02_Mono";
        public const string EscenaGuacamayo = "Escena03_HU03_Guacamayo";
        public const string EscenaPlantas = "Escena04_HU04_Plantas";
        public const string EscenaJaguar = "Escena05_IdentificarJaguar";
        public const string EscenaTortuga = "Escena06_HU06_TortugaAgua";
        public const string EscenaRio = "Escena07_HU07_IdentificarRio";
        public const string EscenaMariposa = "Escena08_HU08_Mariposa";
        public const string EscenaPlatano = "Escena09_IdentificarPlatano";
        public const string EscenaArbol = "Escena11_HU11_ReconocerArbol";
        public const string EscenaRecompensa = "Escena12_Recompensa_final";

        private const string ConfigMono = "Assets/Scripts/Models/NivelesData/RetoMono.asset";
        private const string ConfigGuacamayo = "Assets/Scripts/Models/NivelesData/RetoGuacamayo.asset";
        private const string ConfigJaguar = "Assets/Scripts/Models/NivelesData/RetoJaguar.asset";
        private const string ConfigTortuga = "Assets/Scripts/Models/NivelesData/RetoTortuga.asset";
        private const string ConfigPlatano = "Assets/Scripts/Models/NivelesData/RetoPlatano.asset";
        private const string ConfigNivel2 = "Assets/Scripts/Models/NivelesData/ProtegerHabitatMono.asset";

        [MenuItem("Guardianes/Sahid/Integrar Nivel 1 (orden, enlaces y tipografía)")]
        public static void Integrar()
        {
            AplicarOrdenDeEscenas();
            AplicarEnlacesSiguiente();
            AplicarFuenteAccesible();

            AssetDatabase.SaveAssets();
            Debug.Log("IntegrarNivel01: orden de escenas, enlaces Siguiente y tipografía OpenDyslexic aplicados.");
        }

        /// <summary>Punto de entrada para Unity en modo batch: -executeMethod.</summary>
        public static void IntegrarPorLote()
        {
            try
            {
                Integrar();
                PruebasIntegracionNivel1.EjecutarPruebas();
                PruebasHU06.EjecutarPruebas();
                EditorApplication.Exit(0);
            }
            catch (Exception excepcion)
            {
                Debug.LogError("IntegrarNivel01: la integración falló. " + excepcion);
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// Reordena las escenas en Build Profiles conservando las que no pertenecen
        /// a esta integración y su estado habilitado.
        /// </summary>
        public static void AplicarOrdenDeEscenas()
        {
            List<EditorBuildSettingsScene> actuales = EditorBuildSettings.scenes.ToList();
            List<EditorBuildSettingsScene> ordenadas = new List<EditorBuildSettingsScene>();

            foreach (string ruta in OrdenEscenas)
            {
                EditorBuildSettingsScene escena = actuales.FirstOrDefault(e => e.path == ruta);
                if (escena == null)
                {
                    Debug.LogWarning("IntegrarNivel01: no se encontró la escena " + ruta + " en Build Profiles.");
                    continue;
                }

                escena.enabled = true;
                ordenadas.Add(escena);
            }

            foreach (EditorBuildSettingsScene escena in actuales)
            {
                if (!ordenadas.Contains(escena)) ordenadas.Add(escena);
            }

            if (!actuales.SequenceEqual(ordenadas))
            {
                EditorBuildSettings.scenes = ordenadas.ToArray();
                Debug.Log("IntegrarNivel01: Build Profiles reordenado (" + ordenadas.Count + " escenas).");
            }
        }

        /// <summary>
        /// Enlaza explícitamente el reto que sigue dentro del Nivel 1 completo.
        /// Las escenas sin enlace usan el orden de Build Profiles como respaldo.
        /// </summary>
        public static void AplicarEnlacesSiguiente()
        {
            Configurar(ConfigMono, 1, 2, EscenaGuacamayo);
            Configurar(ConfigGuacamayo, 1, 3, EscenaPlantas);
            Configurar(ConfigJaguar, 1, 5, EscenaTortuga);
            Configurar(ConfigTortuga, 1, 6, EscenaRio);
            Configurar(ConfigPlatano, 1, 9, EscenaArbol);
            Configurar(ConfigNivel2, 2, 1, string.Empty);
        }

        private static void Configurar(string ruta, int nivel, int escena, string siguiente)
        {
            RetoSeleccionConfiguracion configuracion =
                AssetDatabase.LoadAssetAtPath<RetoSeleccionConfiguracion>(ruta);
            if (configuracion == null)
            {
                throw new InvalidOperationException("No existe la configuración del reto: " + ruta);
            }

            bool cambio = false;
            if (configuracion.Nivel != nivel) { configuracion.Nivel = nivel; cambio = true; }
            if (configuracion.NumeroEscena != escena) { configuracion.NumeroEscena = escena; cambio = true; }
            if (configuracion.SiguienteEscena != siguiente) { configuracion.SiguienteEscena = siguiente; cambio = true; }

            if (cambio)
            {
                EditorUtility.SetDirty(configuracion);
                Debug.Log("IntegrarNivel01: " + configuracion.name + " -> " +
                    (string.IsNullOrEmpty(siguiente) ? "fin del bloque implementado" : siguiente));
            }
        }

        /// <summary>
        /// Aplica OpenDyslexic a las configuraciones y a los textos ya serializados
        /// en cada escena, conservando el contorno de los textos que lo tenían.
        /// </summary>
        public static void AplicarFuenteAccesible()
        {
            TMP_FontAsset fuente = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FuenteAccesible);
            if (fuente == null)
            {
                throw new InvalidOperationException("No se encontró la fuente accesible: " + FuenteAccesible);
            }

            foreach (string ruta in new[] { ConfigMono, ConfigGuacamayo, ConfigJaguar, ConfigTortuga, ConfigPlatano, ConfigNivel2 })
            {
                RetoSeleccionConfiguracion configuracion =
                    AssetDatabase.LoadAssetAtPath<RetoSeleccionConfiguracion>(ruta);
                if (configuracion == null) continue;
                if (configuracion.Fuente == fuente) continue;

                configuracion.Fuente = fuente;
                EditorUtility.SetDirty(configuracion);
                Debug.Log("IntegrarNivel01: tipografía accesible aplicada a " + configuracion.name);
            }

            foreach (string ruta in OrdenEscenas)
            {
                Scene escena = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
                TMP_Text[] textos = escena.GetRootGameObjects()
                    .SelectMany(objeto => objeto.GetComponentsInChildren<TMP_Text>(true))
                    .ToArray();

                int cambios = 0;
                foreach (TMP_Text texto in textos)
                {
                    bool teniaContorno = TieneContorno(texto);
                    Material contorno = teniaContorno ? PlantillaUIBuilder.ContornoPara(fuente) : null;
                    if (texto.font == fuente && (!teniaContorno || texto.fontSharedMaterial == contorno)) continue;

                    texto.font = fuente;
                    if (contorno != null) texto.fontSharedMaterial = contorno;

                    EditorUtility.SetDirty(texto);
                    cambios++;
                }

                if (cambios > 0)
                {
                    EditorSceneManager.MarkSceneDirty(escena);
                    EditorSceneManager.SaveScene(escena);
                    Debug.Log("IntegrarNivel01: " + cambios + " textos actualizados en " +
                        System.IO.Path.GetFileName(ruta));
                }
            }
        }

        public static bool TieneContorno(TMP_Text texto)
        {
            return texto != null && texto.fontSharedMaterial != null
                && texto.fontSharedMaterial.name.Contains("Outline");
        }
    }
}
