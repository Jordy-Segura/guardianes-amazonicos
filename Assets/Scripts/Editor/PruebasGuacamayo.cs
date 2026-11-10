using System;
using System.IO;
using System.Reflection;
using GuardianesAmazonicos.Models;
using GuardianesAmazonicos.Persistence;
using GuardianesAmazonicos.Presenters;
using GuardianesAmazonicos.Views;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GuardianesAmazonicos.Editor
{
    [InitializeOnLoad]
    public static class PruebasGuacamayo
    {
        private static double _siguienteInstante;
        private static int _paso;
        private static GuacamayoPresenter _presenter;
        private static GuacamayoView _vista;
        private static RepositorioMemoria _repositorio;
        private static string Carpeta => Environment.GetEnvironmentVariable("HU03_PRUEBAS");

        static PruebasGuacamayo()
        {
            EditorApplication.playModeStateChanged += CambiarEstado;
            if (SessionState.GetBool("HU03_Pruebas", false)) EditorApplication.update += EjecutarPaso;
        }

        public static void Iniciar()
        {
            if (string.IsNullOrWhiteSpace(Carpeta)) throw new InvalidOperationException("Falta HU03_PRUEBAS.");
            Directory.CreateDirectory(Carpeta);
            RetoSeleccionModel modelo = new RetoSeleccionModel(1, 3);
            Comprobar(modelo.Seleccionar(0), "Modelo permite seleccionar distractor");
            Comprobar(!modelo.Confirmar(), "Modelo rechaza distractor");
            modelo.Reintentar();
            Comprobar(modelo.Seleccionar(1), "Modelo permite elegir guacamayo tras error");
            Comprobar(modelo.Confirmar(), "Modelo acepta guacamayo tras error");
            EditorSceneManager.OpenScene(VerificarGuacamayo.RutaEscena);
            SessionState.SetBool("HU03_Pruebas", true);
            EditorApplication.EnterPlaymode();
        }

        private static void CambiarEstado(PlayModeStateChange estado)
        {
            if (!SessionState.GetBool("HU03_Pruebas", false)) return;
            if (estado == PlayModeStateChange.EnteredPlayMode)
            {
                _paso = 0;
                _siguienteInstante = EditorApplication.timeSinceStartup + 2;
                EditorApplication.update -= EjecutarPaso;
                EditorApplication.update += EjecutarPaso;
            }
        }

        private static void EjecutarPaso()
        {
            if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < _siguienteInstante) return;
            _siguienteInstante = EditorApplication.timeSinceStartup + 1;
            try
            {
                switch (_paso++)
                {
                    case 0:
                        _presenter = UnityEngine.Object.FindFirstObjectByType<GuacamayoPresenter>();
                        _vista = UnityEngine.Object.FindFirstObjectByType<GuacamayoView>();
                        Comprobar(_presenter != null && _presenter.enabled, "Presenter y View activos en Play Mode");
                        _repositorio = new RepositorioMemoria();
                        typeof(GuacamayoPresenter).GetField("_repositorio", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_presenter, _repositorio);
                        Comprobar(!Boton("Aceptar").interactable, "Aceptar deshabilitado sin selección");
                        Pulsar("Audio");
                        Comprobar(_vista.GetComponent<AudioSource>().isPlaying, "Audio iniciado");
                        Pulsar("Audio");
                        Comprobar(_vista.GetComponent<AudioSource>().time < 0.2f, "Repetición reinicia audio");
                        Capturar("01-inicial.png");
                        break;
                    case 1:
                        Pulsar("Opcion0");
                        Comprobar(Boton("Aceptar").interactable, "Seleccionar habilita Aceptar");
                        Comprobar(_repositorio.Guardados == 0, "Seleccionar no guarda avance");
                        Pulsar("Aceptar");
                        Comprobar(GameObject.Find("Reintentar") != null && GameObject.Find("Siguiente") == null, "Error permite reintento y bloquea avance");
                        Comprobar(_repositorio.Guardados == 0, "Error no penaliza ni guarda avance");
                        Comprobar(GameObject.Find("ModalAyuda") == null, "Ayuda no aparece automáticamente");
                        Capturar("02-error.png");
                        break;
                    case 2:
                        Pulsar("Ayuda");
                        Comprobar(GameObject.Find("PistaVisual") != null, "Ayuda contiene imagen visual");
                        Comprobar(!Boton("Reintentar").interactable, "Modal bloquea controles del juego");
                        Capturar("03-ayuda.png");
                        break;
                    case 3:
                        Pulsar("CerrarAyuda");
                        Pulsar("Reintentar");
                        Comprobar(!Boton("Aceptar").interactable && Boton("Opcion1").interactable, "Reintento limpia selección sin bloquear");
                        Pulsar("Opcion2");
                        Pulsar("Opcion1");
                        Comprobar(!Boton("Opcion2").GetComponent<Outline>().enabled, "Cambiar selección elimina borde anterior");
                        Pulsar("Aceptar");
                        Comprobar(GameObject.Find("Siguiente") != null && GameObject.Find("Reintentar") == null, "Acierto felicita y habilita Siguiente");
                        Comprobar(_repositorio.Guardados == 0, "Contador se actualiza al pulsar Siguiente");
                        Capturar("04-acierto.png");
                        break;
                    case 4:
                        Pulsar("Siguiente");
                        Comprobar(_repositorio.Guardados == 1 && _repositorio.Progreso.retoGuacamayoCompletado, "Siguiente registra HU03");
                        Comprobar(_repositorio.Progreso.puntoProgresoActual == 8, "Conserva avance superior existente");
                        Pulsar("Siguiente");
                        Comprobar(_repositorio.Guardados == 1, "Siguiente repetido es idempotente");
                        Pulsar("Senas");
                        Comprobar(GameObject.Find("SinVideo") != null, "Video ausente muestra aviso sin inventar señas");
                        Pulsar("CerrarSenas");
                        _presenter.enabled = false;
                        _presenter.enabled = true;
                        Comprobar(GameObject.Find("Siguiente") != null, "Reactivar presenter conserva estado");
                        File.WriteAllText(Path.Combine(Carpeta, "resultado.txt"), "HU03_PLAYMODE_OK\nModelo, selección, confirmar, error, ayuda modal, reintento, acierto, audio/repetición, progreso y video ausente verificados.\nPersistencia inyectada en memoria: no se modificó el progreso del usuario.\n");
                        Debug.Log("HU03_PLAYMODE_OK");
                        Terminar(0);
                        break;
                }
            }
            catch (Exception excepcion)
            {
                File.WriteAllText(Path.Combine(Carpeta, "resultado.txt"), "HU03_PLAYMODE_FALLO\n" + excepcion);
                Debug.LogException(excepcion);
                Terminar(1);
            }
        }

        private static void Terminar(int codigo)
        {
            SessionState.SetBool("HU03_Pruebas", false);
            EditorApplication.update -= EjecutarPaso;
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += () => EditorApplication.Exit(codigo);
        }

        private static void Capturar(string nombre)
        {
            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            Camera camara = Camera.main;
            RenderTexture textura = new RenderTexture(1920, 1080, 24);
            RenderTexture anterior = RenderTexture.active;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camara;
            canvas.planeDistance = 1;
            camara.targetTexture = textura;
            Canvas.ForceUpdateCanvases();
            camara.Render();
            RenderTexture.active = textura;
            Texture2D imagen = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            imagen.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            imagen.Apply();
            File.WriteAllBytes(Path.Combine(Carpeta, nombre), imagen.EncodeToPNG());
            camara.targetTexture = null;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            RenderTexture.active = anterior;
            textura.Release();
            UnityEngine.Object.Destroy(textura);
            UnityEngine.Object.Destroy(imagen);
        }
        private static Button Boton(string nombre)
        {
            GameObject objeto = GameObject.Find(nombre);
            if (objeto == null) throw new InvalidOperationException("No se encontró botón activo: " + nombre);
            return objeto.GetComponent<Button>();
        }
        private static void Pulsar(string nombre)
        {
            Button boton = Boton(nombre);
            Comprobar(boton.interactable, "Botón interactuable: " + nombre);
            boton.onClick.Invoke();
        }
        private static void Comprobar(bool condicion, string descripcion)
        {
            if (!condicion) throw new InvalidOperationException(descripcion);
            Debug.Log("HU03_PASS: " + descripcion);
        }
        private sealed class RepositorioMemoria : IProgresoRepository
        {
            public int Guardados;
            public ProgresoEstudianteData Progreso = new ProgresoEstudianteData { puntoProgresoActual = 8 };
            public ProgresoEstudianteData Cargar() { return Progreso; }
            public void Guardar(ProgresoEstudianteData progreso) { Progreso = progreso; Guardados++; }
        }
    }
}
