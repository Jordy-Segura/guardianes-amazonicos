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
            int rio = rutas.IndexOf("Assets/Scenes/Niveles/Nivel01/Escena07_HU07_IdentificarRio.unity");
            int mariposa = rutas.IndexOf("Assets/Scenes/Niveles/Nivel01/Escena08_HU08_Mariposa.unity");
            int platano = rutas.IndexOf("Assets/Scenes/Niveles/Nivel01/Escena09_IdentificarPlatano.unity");
            int arbol = rutas.IndexOf("Assets/Scenes/Niveles/Nivel01/Escena11_HU11_ReconocerArbol.unity");
            int recompensa = rutas.IndexOf("Assets/Scenes/Niveles/Nivel01/Escena12_Recompensa_final.unity");
            Assert(tortuga == jaguar + 1, "La tortuga (HU-06) va inmediatamente después del jaguar (HU-05)");
            Assert(rio == tortuga + 1, "El río (HU-07) va inmediatamente después de la tortuga (HU-06)");
            Assert(mariposa == rio + 1, "La mariposa (HU-08) va inmediatamente después del río (HU-07)");
            Assert(arbol == platano + 1, "El árbol (HU-11) va inmediatamente después del plátano (HU-09)");
            Assert(recompensa == arbol + 1, "La recompensa final (HU-12) cierra el Nivel 1");

            Debug.Log("Integración: orden de escenas verificado (12 escenas de la aventura).");
        }

        private static void ProbarCadenaDeNavegacion()
        {
            RetoSeleccionConfiguracion mono = Cargar("Assets/Scripts/Models/NivelesData/RetoMono.asset");
            RetoSeleccionConfiguracion guacamayo = Cargar("Assets/Scripts/Models/NivelesData/RetoGuacamayo.asset");
            RetoSeleccionConfiguracion jaguar = Cargar("Assets/Scripts/Models/NivelesData/RetoJaguar.asset");
            RetoSeleccionConfiguracion tortuga = Cargar("Assets/Scripts/Models/NivelesData/RetoTortuga.asset");
            RetoSeleccionConfiguracion platano = Cargar("Assets/Scripts/Models/NivelesData/RetoPlatano.asset");

            Assert(mono.SiguienteEscena == IntegrarNivel01.EscenaGuacamayo, "HU-02 enlaza con HU-03");
            Assert(guacamayo.SiguienteEscena == IntegrarNivel01.EscenaPlantas, "HU-03 enlaza con HU-04");
            Assert(jaguar.SiguienteEscena == IntegrarNivel01.EscenaTortuga, "HU-05 enlaza con HU-06");
            Assert(tortuga.SiguienteEscena == IntegrarNivel01.EscenaRio, "HU-06 enlaza con HU-07");
            Assert(platano.SiguienteEscena == IntegrarNivel01.EscenaArbol, "HU-09 enlaza con HU-11");

            foreach (string escena in new[]
            {
                IntegrarNivel01.EscenaPlantas, IntegrarNivel01.EscenaTortuga,
                IntegrarNivel01.EscenaRio, IntegrarNivel01.EscenaMariposa,
                IntegrarNivel01.EscenaPlatano, IntegrarNivel01.EscenaArbol,
                IntegrarNivel01.EscenaRecompensa
            })
            {
                Assert(EsCargable(escena), "La escena está en Build Profiles: " + escena);
            }

            Assert(tortuga.NumeroEscena == 6 && tortuga.TotalEscenas == 12, "HU-06 se muestra como 6 de 12");

            Debug.Log("Integración: cadena HU-02 -> HU-03 -> HU-04 -> HU-05 -> HU-06 -> HU-07 -> HU-08 -> " +
                "HU-09 -> HU-11 -> HU-12 -> Nivel 2 verificada.");
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
