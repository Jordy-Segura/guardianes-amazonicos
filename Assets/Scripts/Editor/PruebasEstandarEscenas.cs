using System;
using System.Collections.Generic;
using System.Linq;
using GuardianesAmazonicos.Models;
using GuardianesAmazonicos.Views;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GuardianesAmazonicos.Editor
{
    /// <summary>
    /// Comprueba que todas las escenas cumplan el estándar del proyecto: misma
    /// plantilla serializada, tipografía OpenDyslexic, animaciones de personaje,
    /// celebración y pista, audio ambiente, paneles de Ayuda y Señas, navegación
    /// completa y datos suficientes en cada configuración de reto.
    /// Se ejecuta desde el menú o con -executeMethod en modo batch.
    /// </summary>
    public static class PruebasEstandarEscenas
    {
        private sealed class EscenaEstandar
        {
            public string Ruta;
            public string Nombre;
            public string Canvas;
            public int AnimacionesMinimas;
            public bool RequierePista;
            public bool RequiereSenas;
            public bool RequiereEstadoCorrecto = true;
            public bool RequiereTarjetas = true;
            public string[] Botones;
            public bool Serializada = true;
        }

        private const string EscenaBienvenida =
            "Assets/Scenes/Niveles/Nivel01/BienvenidaDelGuardian.unity";
        private const string EscenaGuacamayo =
            "Assets/Scenes/Niveles/Nivel01/Escena03_HU03_Guacamayo.unity";

        private static readonly string[] BotonesActividad =
        {
            "BotonLectura", "BotonSenas", "BotonAyuda", "BotonRegresar",
            "BotonAvanzar", "BotonInicio", "BotonConfiguracion"
        };

        [MenuItem("Guardianes/Sahid/Verificar estándar de escenas")]
        public static void EjecutarPruebas()
        {
            Debug.Log("================ INICIANDO VERIFICACIÓN DEL ESTÁNDAR DE ESCENAS ================");

            ProbarDatosDeLosRetos();
            ProbarEscenas();

            Debug.Log("================ ESTÁNDAR DE ESCENAS: TODAS LAS COMPROBACIONES PASARON ================");
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
                Debug.LogError("PruebasEstandarEscenas: falló la verificación. " + excepcion);
                EditorApplication.Exit(1);
            }
        }

        private static EscenaEstandar[] EscenasEstandar()
        {
            List<EscenaEstandar> escenas = new List<EscenaEstandar>
            {
                new EscenaEstandar
                {
                    Ruta = EscenaBienvenida,
                    Nombre = "Bienvenida",
                    Canvas = "CanvasBienvenida",
                    AnimacionesMinimas = 1,
                    RequiereSenas = true,
                    RequiereEstadoCorrecto = false,
                    RequiereTarjetas = false,
                    Botones = new[]
                    {
                        "BotonComenzar", "BotonLectura", "BotonSenas",
                        "BotonRegresar", "BotonInicio", "BotonConfiguracion"
                    }
                }
            };

            foreach (EstandarizarEscenas.Reto reto in EstandarizarEscenas.Retos)
            {
                if (reto.RutaEscena == EscenaGuacamayo) continue;
                escenas.Add(new EscenaEstandar
                {
                    Ruta = reto.RutaEscena,
                    Nombre = reto.Nombre,
                    Canvas = "CanvasActividad",
                    AnimacionesMinimas = 3,
                    RequierePista = true,
                    RequiereSenas = true,
                    Botones = BotonesActividad
                });
            }

            // HU-03 mantiene su vista propia (GuacamayoView) y construye la interfaz
            // en tiempo de ejecución; se audita aparte hasta que el equipo decida
            // migrarla a la plantilla común.
            escenas.Add(new EscenaEstandar
            {
                Ruta = EscenaGuacamayo,
                Nombre = "HU-03 Guacamayo (vista propia)",
                Serializada = false
            });

            return escenas.ToArray();
        }

        private static void ProbarDatosDeLosRetos()
        {
            TMP_FontAsset fuente = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(IntegrarNivel01.FuenteAccesible);
            Assert(fuente != null, "Existe la fuente accesible OpenDyslexic SDF");

            foreach (EstandarizarEscenas.Reto reto in EstandarizarEscenas.Retos)
            {
                RetoSeleccionConfiguracion configuracion =
                    AssetDatabase.LoadAssetAtPath<RetoSeleccionConfiguracion>(reto.RutaConfiguracion);
                Assert(configuracion != null, "Existe la configuración: " + reto.RutaConfiguracion);

                string titulo = reto.Nivel == 1 ? "Nivel 1 • Conozco la selva" : "Nivel 2 • Cuido los animales";
                string video = string.Format("senas_nivel_{0:00}_pregunta_{1:00}.mp4", reto.Nivel, reto.NumeroEscena);

                Assert(configuracion.Nivel == reto.Nivel, reto.Nombre + ": nivel correcto");
                Assert(configuracion.NumeroEscena == reto.NumeroEscena, reto.Nombre + ": número de escena correcto");
                Assert(configuracion.TotalEscenas == 12, reto.Nombre + ": total de escenas es 12");
                Assert(configuracion.TituloNivel == titulo, reto.Nombre + ": título de nivel estandarizado");
                Assert(configuracion.Fuente == fuente, reto.Nombre + ": tipografía OpenDyslexic");
                Assert(configuracion.Personaje != null, reto.Nombre + ": personaje guía asignado");
                Assert(configuracion.PersonajeIncorrecto != null, reto.Nombre + ": personaje de error asignado");
                Assert(configuracion.FotogramasCelebracion != null && configuracion.FotogramasCelebracion.Length == 40,
                    reto.Nombre + ": fotogramas de celebración del Explorador");
                Assert(configuracion.VideoSenas == video, reto.Nombre + ": nombre del video de señas estandarizado");
                Assert(!string.IsNullOrWhiteSpace(configuracion.Pista), reto.Nombre + ": pista definida");
                Assert(!string.IsNullOrWhiteSpace(configuracion.Felicitacion) && configuracion.Felicitacion != "¡Lo lograste!",
                    reto.Nombre + ": felicitación propia de la actividad");
                Assert(configuracion.Opciones != null && configuracion.Opciones.Length == 3,
                    reto.Nombre + ": tres opciones");
                Assert(configuracion.ImagenesOpciones != null && configuracion.ImagenesOpciones.Length == 3
                    && configuracion.ImagenesOpciones.All(imagen => imagen != null),
                    reto.Nombre + ": tres imágenes de opción asignadas");
                Assert(configuracion.IndiceCorrecto >= 0 && configuracion.IndiceCorrecto < 3,
                    reto.Nombre + ": índice correcto válido");
                Assert(configuracion.ExplicacionesIncorrectas != null
                    && configuracion.ExplicacionesIncorrectas.Length == 3,
                    reto.Nombre + ": tres explicaciones");

                for (int indice = 0; indice < 3; indice++)
                {
                    if (indice == configuracion.IndiceCorrecto) continue;
                    Assert(!string.IsNullOrWhiteSpace(configuracion.ExplicacionesIncorrectas[indice]),
                        reto.Nombre + ": explicación para la opción " + (indice + 1));
                }
            }

            Debug.Log("Estándar: datos de los seis retos verificados.");
        }

        private static void ProbarEscenas()
        {
            TMP_FontAsset fuente = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(IntegrarNivel01.FuenteAccesible);

            foreach (EscenaEstandar esperada in EscenasEstandar())
            {
                Assert(System.IO.File.Exists(esperada.Ruta), "Existe la escena: " + esperada.Ruta);

                Scene escena = EditorSceneManager.OpenScene(esperada.Ruta, OpenSceneMode.Single);
                Assert(escena.IsValid(), "La escena es válida: " + esperada.Nombre);

                GameObject[] raices = escena.GetRootGameObjects();
                string[] nombres = raices
                    .SelectMany(objeto => objeto.GetComponentsInChildren<Transform>(true))
                    .Select(transform => transform.name)
                    .Distinct()
                    .ToArray();

                foreach (TMP_Text texto in raices.SelectMany(objeto => objeto.GetComponentsInChildren<TMP_Text>(true)))
                {
                    Assert(texto.font == fuente,
                        esperada.Nombre + ": texto con OpenDyslexic (" + texto.name + ")");
                }

                if (!esperada.Serializada)
                {
                    Assert(raices.SelectMany(objeto => objeto.GetComponentsInChildren<GuacamayoView>(true)).Any(),
                        "HU-03 conserva su vista propia GuacamayoView");
                    Debug.LogWarning("Estándar: " + esperada.Nombre +
                        " construye su interfaz en tiempo de ejecución y no puede auditarse como escena serializada. " +
                        "Pendiente: migrarla a la plantilla común si el equipo lo aprueba.");
                    continue;
                }

                Assert(nombres.Contains(esperada.Canvas), esperada.Nombre + ": usa " + esperada.Canvas);

                Canvas canvas = raices
                    .SelectMany(objeto => objeto.GetComponentsInChildren<Canvas>(true))
                    .FirstOrDefault();
                Assert(canvas != null, esperada.Nombre + ": tiene Canvas");

                CanvasScaler escalador = canvas.GetComponent<CanvasScaler>();
                Assert(escalador != null, esperada.Nombre + ": tiene CanvasScaler");
                Assert(escalador.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize,
                    esperada.Nombre + ": escala con el tamaño de la pantalla");
                Assert(escalador.referenceResolution == new Vector2(1920f, 1080f),
                    esperada.Nombre + ": resolución de referencia 1920x1080");
                Assert(Mathf.Approximately(escalador.matchWidthOrHeight, 0.5f),
                    esperada.Nombre + ": proporción 16:9 equilibrada");

                Assert(raices.SelectMany(objeto => objeto.GetComponentsInChildren<EventSystem>(true)).Any(),
                    esperada.Nombre + ": tiene EventSystem");

                int animaciones = raices
                    .SelectMany(objeto => objeto.GetComponentsInChildren<AnimacionPersonajePNG>(true))
                    .Count();
                Assert(animaciones >= esperada.AnimacionesMinimas,
                    esperada.Nombre + ": animaciones de personaje (" + animaciones + ")");

                Assert(nombres.Contains("AmbienteSelva"), esperada.Nombre + ": incluye audio ambiente de la selva");
                Assert(raices.SelectMany(objeto => objeto.GetComponentsInChildren<AudioSource>(true))
                    .Any(fuenteAudio => fuenteAudio.clip != null),
                    esperada.Nombre + ": tiene audio configurado");

                foreach (string boton in esperada.Botones)
                {
                    Assert(nombres.Contains(boton), esperada.Nombre + ": botón " + boton);
                }

                if (esperada.RequierePista) Assert(nombres.Contains("PanelPista"), esperada.Nombre + ": panel de pista");
                if (esperada.RequiereSenas) Assert(nombres.Contains("PanelSenas"), esperada.Nombre + ": panel de señas");
                if (esperada.RequiereEstadoCorrecto)
                    Assert(nombres.Contains("EstadoCorrecto"), esperada.Nombre + ": estado de respuesta correcta");

                if (esperada.RequiereTarjetas)
                {
                    int tarjetas = raices
                        .SelectMany(objeto => objeto.GetComponentsInChildren<Button>(true))
                        .Count(boton => boton.name.StartsWith("Respuesta"));
                    Assert(tarjetas == 3, esperada.Nombre + ": tres tarjetas de respuesta");
                }
            }

            Debug.Log("Estándar: escenas serializadas y vista propia revisadas.");
        }

        private static void Assert(bool condicion, string mensaje)
        {
            if (!condicion)
            {
                throw new Exception("FALLÓ EL ESTÁNDAR DE ESCENAS: " + mensaje);
            }
        }
    }
}
