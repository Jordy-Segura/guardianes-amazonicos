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
    /// Comprueba la integración del Nivel 1 implementado: orden en Build Profiles,
    /// cadena de navegación entre retos y tipografía accesible OpenDyslexic (RNF-01).
    /// Se ejecuta desde el menú o con -executeMethod en modo batch.
    /// </summary>
    public static class PruebasIntegracionNivel1
    {
        [MenuItem("Guardianes/Sahid/Verificar Integración Nivel 1")]
        public static void EjecutarPruebas()
        {
            Debug.Log("================ INICIANDO PRUEBAS DE INTEGRACIÓN DEL NIVEL 1 ================");

            ProbarOrdenDeEscenas();
            ProbarCadenaDeNavegacion();
            ProbarFuenteAccesible();

            Debug.Log("================ INTEGRACIÓN DEL NIVEL 1: TODAS LAS PRUEBAS PASARON ================");
        }

        /// <summary>Punto de entrada para Unity en modo batch: -executeMethod.</summary>
        public static void EjecutarPorLote()
        {
            try
            {
                EjecutarPruebas();
                EditorApplication.Exit(0);
            }
            catch (Exception excepcion)
            {
                Debug.LogError("PruebasIntegracionNivel1: falló la verificación. " + excepcion);
                EditorApplication.Exit(1);
            }
        }

        private static void ProbarOrdenDeEscenas()
        {
            List<string> rutas = EditorBuildSettings.scenes
                .Where(escena => escena.enabled)
                .Select(escena => escena.path)
                .ToList();

            Assert(rutas.Count == rutas.Distinct().Count(), "No hay escenas duplicadas en Build Profiles");

            int posicionAnterior = -1;
            foreach (string ruta in IntegrarNivel01.OrdenEscenas)
            {
                int posicion = rutas.IndexOf(ruta);
                Assert(posicion >= 0, "La escena está habilitada en Build Profiles: " + ruta);
                Assert(posicion > posicionAnterior, "El orden de la aventura es creciente: " + ruta);
                posicionAnterior = posicion;
            }

            int jaguar = rutas.IndexOf("Assets/Scenes/Niveles/Nivel01/Escena05_IdentificarJaguar.unity");
            int tortuga = rutas.IndexOf("Assets/Scenes/Niveles/Nivel01/Escena06_HU06_TortugaAgua.unity");
            int platano = rutas.IndexOf("Assets/Scenes/Niveles/Nivel01/Escena09_IdentificarPlatano.unity");
            Assert(tortuga == jaguar + 1, "La tortuga (HU-06) va inmediatamente después del jaguar (HU-05)");
            Assert(platano == tortuga + 1, "El plátano (HU-09) va inmediatamente después de la tortuga (HU-06)");

            Debug.Log("Integración: orden de escenas verificado (07 escenas del bloque implementado).");
        }

        private static void ProbarCadenaDeNavegacion()
        {
            RetoSeleccionConfiguracion mono = Cargar("Assets/Scripts/Models/NivelesData/RetoMono.asset");
            RetoSeleccionConfiguracion guacamayo = Cargar("Assets/Scripts/Models/NivelesData/RetoGuacamayo.asset");
            RetoSeleccionConfiguracion jaguar = Cargar("Assets/Scripts/Models/NivelesData/RetoJaguar.asset");
            RetoSeleccionConfiguracion tortuga = Cargar("Assets/Scripts/Models/NivelesData/RetoTortuga.asset");
            RetoSeleccionConfiguracion platano = Cargar("Assets/Scripts/Models/NivelesData/RetoPlatano.asset");

            Assert(mono.SiguienteEscena == IntegrarNivel01.EscenaGuacamayo, "HU-02 enlaza con HU-03");
            Assert(guacamayo.SiguienteEscena == IntegrarNivel01.EscenaPlantas, "HU-03 conserva el destino previsto HU-04");
            Assert(jaguar.SiguienteEscena == IntegrarNivel01.EscenaTortuga, "HU-05 enlaza con HU-06");
            Assert(tortuga.SiguienteEscena == IntegrarNivel01.EscenaPlatano, "HU-06 enlaza con HU-09");
            Assert(string.IsNullOrEmpty(platano.SiguienteEscena), "HU-09 usa el orden de Build Profiles como respaldo");

            Assert(EsCargable(IntegrarNivel01.EscenaGuacamayo), "HU-03 es cargable");
            Assert(EsCargable(IntegrarNivel01.EscenaTortuga), "HU-06 es cargable");
            Assert(EsCargable(IntegrarNivel01.EscenaPlatano), "HU-09 es cargable");
            Assert(!EsCargable(IntegrarNivel01.EscenaPlantas),
                "HU-04 aún no existe: HU-03 avanzará a la escena siguiente de Build Profiles");

            Assert(tortuga.NumeroEscena == 6 && tortuga.TotalEscenas == 12, "HU-06 se muestra como 6 de 12");

            Debug.Log("Integración: cadena HU-02 -> HU-03 -> HU-05 -> HU-06 -> HU-09 -> Nivel 2 verificada.");
        }

        private static void ProbarFuenteAccesible()
        {
            TMP_FontAsset fuente = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(IntegrarNivel01.FuenteAccesible);
            Assert(fuente != null, "Existe la fuente accesible OpenDyslexic SDF");
            Assert(PlantillaUIBuilder.FUENTE == IntegrarNivel01.FuenteAccesible,
                "La plantilla genera escenas nuevas con OpenDyslexic");

            foreach (string ruta in new[]
            {
                "Assets/Scripts/Models/NivelesData/RetoMono.asset",
                "Assets/Scripts/Models/NivelesData/RetoGuacamayo.asset",
                "Assets/Scripts/Models/NivelesData/RetoJaguar.asset",
                "Assets/Scripts/Models/NivelesData/RetoTortuga.asset",
                "Assets/Scripts/Models/NivelesData/RetoPlatano.asset",
                "Assets/Scripts/Models/NivelesData/ProtegerHabitatMono.asset"
            })
            {
                Assert(Cargar(ruta).Fuente == fuente, "La configuración usa OpenDyslexic: " + ruta);
            }

            foreach (string ruta in IntegrarNivel01.OrdenEscenas)
            {
                Scene escena = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
                TMP_Text[] textos = escena.GetRootGameObjects()
                    .SelectMany(objeto => objeto.GetComponentsInChildren<TMP_Text>(true))
                    .ToArray();

                foreach (TMP_Text texto in textos)
                {
                    Assert(texto.font == fuente,
                        "Texto con OpenDyslexic en " + System.IO.Path.GetFileName(ruta) + ": " + texto.name);
                }
            }

            Debug.Log("Integración: tipografía accesible verificada en configuraciones y escenas.");
        }

        /// <summary>Comprueba el nombre de escena dentro de Build Profiles habilitadas.</summary>
        private static bool EsCargable(string nombreEscena)
        {
            return EditorBuildSettings.scenes.Any(escena => escena.enabled
                && System.IO.Path.GetFileNameWithoutExtension(escena.path) == nombreEscena);
        }

        private static RetoSeleccionConfiguracion Cargar(string ruta)
        {
            RetoSeleccionConfiguracion configuracion =
                AssetDatabase.LoadAssetAtPath<RetoSeleccionConfiguracion>(ruta);
            Assert(configuracion != null, "Existe la configuración del reto: " + ruta);
            return configuracion;
        }

        private static void Assert(bool condicion, string mensaje)
        {
            if (!condicion)
            {
                throw new Exception("FALLÓ LA INTEGRACIÓN DEL NIVEL 1: " + mensaje);
            }
        }
    }
}
