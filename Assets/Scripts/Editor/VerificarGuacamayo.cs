using System;
using GuardianesAmazonicos.Models;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GuardianesAmazonicos.Editor
{
    /// <summary>
    /// HU-03 se construye con la plantilla común desde la estandarización de escenas.
    /// Se conservan el punto de entrada y la compilación de verificación originales,
    /// pero la escena ya no usa la vista propia GuacamayoView: comparte animaciones,
    /// paneles, cabecera y tipografía con el resto de actividades.
    /// </summary>
    public static class VerificarGuacamayo
    {
        public const string RutaEscena = "Assets/Scenes/Niveles/Nivel01/Escena03_HU03_Guacamayo.unity";
        private const string RutaConfiguracion = "Assets/Scripts/Models/NivelesData/RetoGuacamayo.asset";

        [MenuItem("Guardianes/HU-03/Generar escena")]
        public static void Generar()
        {
            RetoSeleccionConfiguracion configuracion =
                AssetDatabase.LoadAssetAtPath<RetoSeleccionConfiguracion>(RutaConfiguracion);
            if (configuracion == null)
                throw new InvalidOperationException("Falta la configuración: " + RutaConfiguracion);

            BuildAssignedScenes.PrepararRecursos();
            BuildAssignedScenes.CrearEscena(RutaEscena, configuracion);
            Debug.Log("HU-03: escena regenerada con la plantilla aprobada.");
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
            if (informe.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Compilación fallida: " + informe.summary.result);
            Debug.Log("HU03_BUILD_OK: " + informe.summary.totalSize + " bytes.");
        }
    }
}
