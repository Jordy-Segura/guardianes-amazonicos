using System;
using System.IO;
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
    public static class PruebasHU06
    {
        [MenuItem("Guardianes/Sahid/Verificar HU-06 (Pruebas Automatizadas)")]
        public static void EjecutarPruebas()
        {
            Debug.Log("================ INICIANDO PRUEBAS HU-06 ================");

            // 1. Prueba de lógica del Modelo
            ProbarModelo();

            // 2. Prueba del ScriptableObject de Configuración
            ProbarConfiguracion();

            // 3. Prueba de la Escena Serializada
            ProbarEscena();

            Debug.Log("================ HU-06: TODAS LAS PRUEBAS PASARON EXITOSAMENTE ================");
        }

        private static void ProbarModelo()
        {
            // HU-06: 3 opciones, índice correcto 0 (Agua)
            RetoSeleccionModel modelo = new RetoSeleccionModel(0, 3);
            Assert(modelo.IndiceSeleccionado == RetoSeleccionModel.SIN_SELECCION, "Modelo inicia sin selección");
            Assert(!modelo.HaySeleccion, "HaySeleccion es false al inicio");

            // Seleccionar Fruta (distractor 1)
            Assert(modelo.Seleccionar(1), "Permite seleccionar distractor 1 (Fruta)");
            Assert(modelo.IndiceSeleccionado == 1, "Índice seleccionado es 1");
            Assert(!modelo.Confirmar(), "Confirmar distractor Fruta devuelve false (incorrecto)");
            Assert(!modelo.Resuelto, "Resuelto es false tras fallo");

            // Reintentar sin penalización
            modelo.Reintentar();
            Assert(modelo.IndiceSeleccionado == RetoSeleccionModel.SIN_SELECCION, "Reintentar limpia la selección");

            // Seleccionar Árbol (distractor 2)
            Assert(modelo.Seleccionar(2), "Permite seleccionar distractor 2 (Árbol)");
            Assert(!modelo.Confirmar(), "Confirmar distractor Árbol devuelve false (incorrecto)");

            // Reintentar y elegir Agua (correcta)
            modelo.Reintentar();
            Assert(modelo.Seleccionar(0), "Permite seleccionar opción correcta 0 (Agua)");
            Assert(modelo.Confirmar(), "Confirmar opción Agua devuelve true (correcto)");
            Assert(modelo.Resuelto, "Resuelto es true tras acierto");

            Debug.Log("HU-06: Pruebas unitarias de RetoSeleccionModel superadas.");
        }

        private static void ProbarConfiguracion()
        {
            RetoSeleccionConfiguracion config = AssetDatabase.LoadAssetAtPath<RetoSeleccionConfiguracion>(BuildSceneHU06.ConfigTortugaPath);
            Assert(config != null, "Existe RetoTortuga.asset");
            Assert(config.Nivel == 1, "Nivel es 1");
            Assert(config.NumeroEscena == 6, "Número de escena es 6");
            Assert(config.TotalEscenas == 12, "Total de escenas es 12");
            Assert(config.Pregunta.Contains("necesita para estar bien"), "Pregunta coincide con la HU-06");
            Assert(config.Opciones.Length == 3, "Tiene 3 opciones");
            Assert(config.Opciones[0] == "Agua", "Opción 0 es Agua");
            Assert(config.Opciones[1] == "Fruta", "Opción 1 es Fruta");
            Assert(config.Opciones[2] == "Árbol", "Opción 2 es Árbol");
            Assert(config.IndiceCorrecto == 0, "Índice correcto es 0");
            Assert(config.Felicitacion.Contains("agua es importante"), "Felicitación menciona la importancia del agua");
            Assert(!string.IsNullOrEmpty(config.Pista), "Tiene pista definida");
            Assert(config.ImagenesOpciones.Length == 3, "Tiene 3 imágenes de opciones asignadas");
            Assert(config.ImagenesOpciones[0] != null, "Imagen Agua asignada");
            Assert(config.ImagenesOpciones[1] != null, "Imagen Fruta asignada");
            Assert(config.ImagenesOpciones[2] != null, "Imagen Árbol asignada");
            Assert(config.ImagenEscena != null, "Imagen principal de la escena asignada");
            Assert(config.Personaje != null, "Personaje guía (Tortuga) asignado");

            Debug.Log("HU-06: Pruebas de configuración de datos RetoTortuga superadas.");
        }

        private static void ProbarEscena()
        {
            Assert(File.Exists(BuildSceneHU06.SceneTortugaPath), "Existe el archivo de escena: " + BuildSceneHU06.SceneTortugaPath);

            Scene escena = EditorSceneManager.OpenScene(BuildSceneHU06.SceneTortugaPath, OpenSceneMode.Single);
            Assert(escena.IsValid(), "La escena es válida");

            RetoSeleccionPresenter presenter = escena.GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<RetoSeleccionPresenter>(true))
                .FirstOrDefault();
            Assert(presenter != null, "Escena contiene RetoSeleccionPresenter");

            RetoSeleccionView vista = escena.GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<RetoSeleccionView>(true))
                .FirstOrDefault();
            Assert(vista != null, "Escena contiene RetoSeleccionView");

            SerializedObject soPresenter = new SerializedObject(presenter);
            var propConfig = soPresenter.FindProperty("_configuracion");
            var propVista = soPresenter.FindProperty("_vista");
            Assert(propConfig.objectReferenceValue != null, "Presenter tiene asignada la configuración");
            Assert(propVista.objectReferenceValue != null, "Presenter tiene asignada la vista");

            Button[] botonesRespuesta = escena.GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<Button>(true))
                .Where(b => b.name.StartsWith("Respuesta"))
                .ToArray();
            Assert(botonesRespuesta.Length == 3, "La escena contiene 3 botones de respuesta interactivos");

            Debug.Log("HU-06: Pruebas estructurales de la escena Escena06_HU06_TortugaAgua superadas.");
        }

        private static void Assert(bool condicion, string mensaje)
        {
            if (!condicion)
            {
                throw new Exception("FALLÓ PRUEBA HU-06: " + mensaje);
            }
        }
    }
}
