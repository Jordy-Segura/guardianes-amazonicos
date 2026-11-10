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

namespace GuardianesAmazonicos.Editor
{
    /// <summary>
    /// Renderiza cada estado de la plantilla (bienvenida, principal, seleccionada, incorrecta,
    /// pista, señas y correcta) a PNG 1920 × 1080 para comparar con las capturas de la guía.
    /// No guarda cambios en las escenas.
    /// Uso por lotes: -executeMethod GuardianesAmazonicos.Editor.CapturaPlantilla.GenerarYCapturar -capturas "carpeta"
    /// </summary>
    public static class CapturaPlantilla
    {
        private const string Bienvenida = "Assets/Scenes/Niveles/Nivel01/BienvenidaDelGuardian.unity";

        [MenuItem("Guardianes/Capturar estados de mis escenas")]
        public static void Capturar()
        {
            string carpeta = Argumento("-capturas") ?? Path.Combine(Directory.GetCurrentDirectory(), "Temp", "CapturasPlantilla");
            Directory.CreateDirectory(carpeta);

            CapturarBienvenida(carpeta);
            CapturarActividad(BuildAssignedScenes.SceneJaguarPath, "n1_e05_jaguar", carpeta);
            CapturarActividad(BuildAssignedScenes.ScenePlatanoPath, "n1_e09_platano", carpeta);
            CapturarActividad(BuildAssignedScenes.SceneMonoCuriosoPath, "n2_e01_mono", carpeta);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Debug.Log("Capturas de la plantilla guardadas en: " + carpeta);
        }

        public static void GenerarYCapturar()
        {
            BuildAssignedScenes.GenerarTodas();
            Capturar();
        }

        private static void CapturarBienvenida(string carpeta)
        {
            EditorSceneManager.OpenScene(Bienvenida, OpenSceneMode.Single);
            BienvenidaNivelView vista = UnityEngine.Object.FindFirstObjectByType<BienvenidaNivelView>();
            using (Capturador captura = new Capturador(UnityEngine.Object.FindFirstObjectByType<Canvas>(), carpeta, "n1_e01_bienvenida"))
            {
                captura.Guardar("1_bienvenida");
                vista.MostrarPanelSenas();
                captura.Guardar("2_senas");
            }
        }

        private static void CapturarActividad(string ruta, string prefijo, string carpeta)
        {
            EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            RetoSeleccionView vista = UnityEngine.Object.FindFirstObjectByType<RetoSeleccionView>();
            RetoSeleccionPresenter presenter = UnityEngine.Object.FindFirstObjectByType<RetoSeleccionPresenter>();
            RetoSeleccionConfiguracion c = (RetoSeleccionConfiguracion)new SerializedObject(presenter)
                .FindProperty("_configuracion").objectReferenceValue;

            using (Capturador captura = new Capturador(UnityEngine.Object.FindFirstObjectByType<Canvas>(), carpeta, prefijo))
            {
                if (c.MostrarBienvenida)
                {
                    vista.MostrarBienvenida(true);
                    captura.Guardar("0_bienvenida");
                    vista.MostrarBienvenida(false);
                }

                vista.MostrarSinSeleccion();
                captura.Guardar("1_principal");

                vista.MostrarSeleccion(c.IndiceCorrecto);
                captura.Guardar("2_seleccionada");

                int incorrecta = (c.IndiceCorrecto + 1) % RetoSeleccionConfiguracion.CANTIDAD_OPCIONES;
                vista.MostrarIncorrecta(incorrecta, c.ObtenerExplicacion(incorrecta));
                captura.Guardar("3_incorrecta");

                vista.MostrarSinSeleccion();
                vista.MostrarPista(true);
                captura.Guardar("4_pista");
                vista.MostrarPista(false);

                vista.MostrarSenas(true, c.VideoSenas);
                captura.Guardar("5_senas");
                vista.MostrarSenas(false, null);

                vista.MostrarCorrecta(c.IndiceCorrecto, c.Felicitacion);
                captura.Guardar("6_correcta");
            }
        }

        private static string Argumento(string nombre)
        {
            string[] argumentos = Environment.GetCommandLineArgs();
            int indice = Array.IndexOf(argumentos, nombre);
            return indice >= 0 && indice + 1 < argumentos.Length ? argumentos[indice + 1] : null;
        }

        private sealed class Capturador : IDisposable
        {
            private readonly Canvas _canvas;
            private readonly string _carpeta;
            private readonly string _prefijo;
            private readonly GameObject _camaraObjeto;
            private readonly Camera _camara;
            private readonly RenderTexture _destino;

            public Capturador(Canvas canvas, string carpeta, string prefijo)
            {
                _canvas = canvas;
                _carpeta = carpeta;
                _prefijo = prefijo;
                _destino = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
                _camaraObjeto = new GameObject("CamaraCaptura");
                _camara = _camaraObjeto.AddComponent<Camera>();
                _camara.clearFlags = CameraClearFlags.SolidColor;
                _camara.backgroundColor = Color.black;
                _camara.orthographic = true;
                _camara.targetTexture = _destino;
                _canvas.renderMode = RenderMode.ScreenSpaceCamera;
                _canvas.worldCamera = _camara;
                _canvas.planeDistance = 10f;
            }

            public void Guardar(string estado)
            {
                Canvas.ForceUpdateCanvases();
                _camara.Render();
                Canvas.ForceUpdateCanvases();
                _camara.Render();

                RenderTexture anterior = RenderTexture.active;
                RenderTexture.active = _destino;
                Texture2D imagen = new Texture2D(_destino.width, _destino.height, TextureFormat.RGB24, false);
                imagen.ReadPixels(new Rect(0, 0, _destino.width, _destino.height), 0, 0);
                imagen.Apply();
                RenderTexture.active = anterior;

                File.WriteAllBytes(Path.Combine(_carpeta, _prefijo + "_" + estado + ".png"), imagen.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(imagen);
            }

            public void Dispose()
            {
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _camara.targetTexture = null;
                UnityEngine.Object.DestroyImmediate(_camaraObjeto);
                _destino.Release();
                UnityEngine.Object.DestroyImmediate(_destino);
            }
        }
    }
}
