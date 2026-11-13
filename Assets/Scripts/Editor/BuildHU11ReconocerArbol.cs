using System;
using System.Linq;
using GuardianesAmazonicos.Models;
using GuardianesAmazonicos.Presenters;
using GuardianesAmazonicos.Views;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GuardianesAmazonicos.Editor
{
    /// <summary>Genera únicamente HU-11, usando la plantilla y los recursos comunes del proyecto.</summary>
    public static class BuildHU11ReconocerArbol
    {
        public const string RutaEscena = "Assets/Scenes/Niveles/Nivel01/Escena11_HU11_ReconocerArbol.unity";
        private const string RutaConfig = "Assets/Scripts/Models/NivelesData/RetoReconocerArbol.asset";

        [MenuItem("Guardianes/HU-11/Generar escena Reconocer el árbol")]
        public static void Generar()
        {
            PlantillaUIBuilder.Preparar();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            PrepararIlustraciones();

            RetoSeleccionConfiguracion c = CrearConfiguracion();
            Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            PlantillaUIBuilder.CrearCamaraYEventos();
            Canvas canvas = PlantillaUIBuilder.CrearCanvas("CanvasActividad");
            RetoSeleccionView.ReferenciasActividad r = new RetoSeleccionView.ReferenciasActividad();
            RectTransform baseUi = PlantillaUIBuilder.Capa("CapaBase", canvas.transform);
            r.capaBase = baseUi.gameObject.AddComponent<CanvasGroup>();
            PlantillaUIBuilder.Imagen("FondoNivel", baseUi, c.Fondo, 0f, 0f,
                PlantillaUIBuilder.ANCHO, PlantillaUIBuilder.ALTO);
            PlantillaUIBuilder.ConstruirActividad(baseUi, c.Pregunta, c.Opciones, c.ImagenesOpciones,
                c.Personaje, c.PersonajeIncorrecto, c.FotogramasCelebracion, c.ImagenEscena,
                null, Rect.zero, r);
            AjustarTarjetasRespuesta(r);

            PlantillaUIBuilder.Cabecera cabecera = PlantillaUIBuilder.ConstruirCabecera(baseUi,
                c.TituloNivel, c.NumeroEscena, c.TotalEscenas, true);
            r.botonLectura = cabecera.lectura;
            r.botonSenas = cabecera.senas;
            r.botonAyuda = cabecera.ayuda;
            r.botonRegresar = cabecera.regresar;
            if (r.botonRegresar != null) r.botonRegresar.gameObject.SetActive(false);
            r.botonAvanzar = cabecera.avanzar;
            r.botonInicio = cabecera.inicio;
            r.botonConfiguracion = cabecera.configuracion;
            r.textoProgreso = cabecera.progreso;

            PlantillaUIBuilder.ConstruirEstadoCorrecto(canvas.transform, c.Personaje,
                c.FotogramasCelebracion, c.Felicitacion, r);
            PlantillaUIBuilder.ConstruirPanelPista(canvas.transform, c.Pista, c.Personaje,
                c.FotogramasCelebracion, r);
            PlantillaUIBuilder.PanelSenas senas = PlantillaUIBuilder.ConstruirPanelSenas(canvas.transform);
            r.panelSenas = senas.raiz;
            r.botonReproducirSenas = senas.reproducir;
            r.botonCerrarSenas = senas.cerrar;
            r.textoEstadoSenas = senas.estado;
            r.pantallaVideo = senas.pantalla;
            r.reproductorSenas = senas.reproductor;

            GameObject controlador = new GameObject("HU11_ReconocerArbol");
            r.fuenteAudio = controlador.AddComponent<AudioSource>();
            r.fuenteAudio.playOnAwake = false;
            r.fuenteAudio.loop = false;
            RetoSeleccionView vista = controlador.AddComponent<RetoSeleccionView>();
            vista.Enlazar(r);
            ReconocerArbolPresenter presenter = controlador.AddComponent<ReconocerArbolPresenter>();
            presenter.Configurar(c, vista,
                Cargar<AudioClip>("Assets/Audio/Voice/HU11/acierto_01_11.wav"),
                Cargar<AudioClip>("Assets/Audio/Voice/HU11/error_01_11.wav"));

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena, RutaEscena);
            AssetDatabase.SaveAssets();
            BuildAssignedScenes.AgregarEscenasBuild(RutaEscena);
            VerificarEscena();
            Debug.Log("HU11_GENERACION_OK: escena, audio, tarjetas, estados y navegación preparados.");
        }

        [MenuItem("Guardianes/HU-11/Ajustar tarjetas y ocultar Regresar en la escena existente")]
        public static void AjustarEscenaExistente()
        {
            Scene escenaActiva = SceneManager.GetActiveScene();
            Scene escenaHu11 = SceneManager.GetSceneByPath(RutaEscena);
            bool cargarEnAdicional = !escenaHu11.IsValid() || !escenaHu11.isLoaded;
            if (cargarEnAdicional) escenaHu11 = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Additive);

            try
            {
                GameObject canvas = escenaHu11.GetRootGameObjects().FirstOrDefault(objeto => objeto.name == "CanvasActividad");
                Transform actividad = canvas != null ? canvas.transform.Find("CapaBase/Actividad") : null;
                if (actividad == null) throw new InvalidOperationException("No se encontró la actividad de HU-11.");

                const float posicionY = -635f;
                for (int indice = 1; indice <= RetoSeleccionConfiguracion.CANTIDAD_OPCIONES; indice++)
                {
                    Transform tarjeta = actividad.Find("Respuesta" + indice);
                    RectTransform rectTarjeta = tarjeta as RectTransform;
                    if (rectTarjeta == null) throw new InvalidOperationException("Falta la tarjeta de respuesta " + indice + " en HU-11.");
                    Vector2 posicion = rectTarjeta.anchoredPosition;
                    rectTarjeta.anchoredPosition = new Vector2(posicion.x, posicionY);
                    EditorUtility.SetDirty(rectTarjeta);
                    AjustarImagenTarjeta(rectTarjeta);
                }

                Button botonRegresar = canvas.GetComponentsInChildren<Button>(true)
                    .FirstOrDefault(boton => boton.name == "BotonRegresar");
                if (botonRegresar != null)
                {
                    botonRegresar.gameObject.SetActive(false);
                    EditorUtility.SetDirty(botonRegresar.gameObject);
                }

                EditorSceneManager.MarkSceneDirty(escenaHu11);
                EditorSceneManager.SaveScene(escenaHu11, RutaEscena);
                Debug.Log("HU11_AJUSTE_OK: tarjetas elevadas y botón Regresar oculto solo en HU-11.");
            }
            finally
            {
                if (cargarEnAdicional) EditorSceneManager.CloseScene(escenaHu11, true);
                if (escenaActiva.IsValid() && escenaActiva.isLoaded) SceneManager.SetActiveScene(escenaActiva);
            }
        }

        private static RetoSeleccionConfiguracion CrearConfiguracion()
        {
            RetoSeleccionConfiguracion c = AssetDatabase.LoadAssetAtPath<RetoSeleccionConfiguracion>(RutaConfig);
            if (c == null)
            {
                c = ScriptableObject.CreateInstance<RetoSeleccionConfiguracion>();
                AssetDatabase.CreateAsset(c, RutaConfig);
            }

            c.Nivel = 1;
            c.NumeroEscena = 11;
            c.TotalEscenas = 12;
            c.TituloNivel = "Nivel 1 · Conozco la selva";
            c.Pregunta = "Escucha la descripción. ¿Cuál es el árbol?";
            c.Opciones = new[] { "Árbol", "Río", "Piedra" };
            c.ImagenesOpciones = new[] { Cargar<Sprite>("Assets/Art/Escenas/HU11_Tarjeta_Arbol.png"), Cargar<Sprite>("Assets/Art/Escenas/HU11_Tarjeta_Rio.png"), Cargar<Sprite>("Assets/Art/Escenas/HU11_Tarjeta_Piedra.png") };
            c.IndiceCorrecto = 0;
            c.ExplicacionesIncorrectas = new[]
            {
                string.Empty,
                "El río lleva agua entre sus orillas. Escucha las pistas y busca el árbol.",
                "La piedra es dura y no tiene tronco ni hojas. Busca el árbol de la selva."
            };
            c.Pista = "Busca el ser vivo alto con tronco, ramas y muchas hojas.";
            c.Felicitacion = "¡Muy bien! El árbol forma parte importante de la selva. Da sombra y hogar a muchos animales.";
            c.ImagenEscena = Cargar<Sprite>("Assets/Art/Escenas/HU11_Escena_Referencia.png");
            c.Fondo = Cargar<Sprite>("Assets/Art/Backgrounds/Niveles/N1_Conozco_la_selva.png");
            c.Personaje = Cargar<Sprite>("Assets/Art/Characters/Explorador.png");
            c.PersonajeIncorrecto = Cargar<Sprite>("Assets/Art/Characters/ExploradorPreocupado.png");
            c.FotogramasCelebracion = BuildAssignedScenes.CargarFotogramasExplorador();
            c.AudioLectura = Cargar<AudioClip>("Assets/Audio/Voice/HU11/lectura_01_11.wav");
            c.AudioPista = Cargar<AudioClip>("Assets/Audio/Voice/HU11/pista_01_11.wav");
            c.AudioCorrecciones = new[]
            {
                null,
                Cargar<AudioClip>("Assets/Audio/Voice/HU11/correccion_rio_01_11.wav"),
                Cargar<AudioClip>("Assets/Audio/Voice/HU11/correccion_piedra_01_11.wav")
            };
            c.VideoSenas = string.Empty;
            c.MostrarBienvenida = false;
            c.SiguienteEscena = string.Empty;
            c.Fuente = Cargar<TMP_FontAsset>(PlantillaUIBuilder.FUENTE);
            EditorUtility.SetDirty(c);
            AssetDatabase.SaveAssets();
            return c;
        }

        /// <summary>Levanta la fila para equilibrar el espacio entre la imagen y el borde inferior.</summary>
        private static void AjustarTarjetasRespuesta(RetoSeleccionView.ReferenciasActividad referencias)
        {
            if (referencias.tarjetas == null || referencias.tarjetas.Length != RetoSeleccionConfiguracion.CANTIDAD_OPCIONES)
                throw new InvalidOperationException("HU-11 debe conservar exactamente tres tarjetas de respuesta.");

            const float posicionY = -635f;
            foreach (RetoSeleccionView.TarjetaRespuestaVista tarjeta in referencias.tarjetas)
            {
                if (tarjeta == null || tarjeta.boton == null || tarjeta.boton.transform is not RectTransform rectTarjeta)
                    throw new InvalidOperationException("No se pudo ubicar una de las tarjetas de HU-11.");

                if (tarjeta.nombre != null)
                {
                    tarjeta.nombre.gameObject.SetActive(false);
                    EditorUtility.SetDirty(tarjeta.nombre.gameObject);
                }

                Vector2 posicion = rectTarjeta.anchoredPosition;
                rectTarjeta.anchoredPosition = new Vector2(posicion.x, posicionY);
                AjustarImagenTarjeta(rectTarjeta);
            }
        }

        /// <summary>Centra y recorta las ilustraciones dentro de sus tarjetas con esquinas redondeadas.</summary>
        private static void AjustarImagenTarjeta(RectTransform tarjeta)
        {
            Transform mascaraExistente = tarjeta.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(transformacion => transformacion.name == "RecorteIlustracionRedondeado"
                    && transformacion.parent == tarjeta);
            Transform ilustracion = tarjeta.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(transformacion => transformacion.name == "Ilustracion");
            if (ilustracion is not RectTransform rectIlustracion)
                throw new InvalidOperationException("Falta la ilustración de una tarjeta HU-11.");

            Image imagen = rectIlustracion.GetComponent<Image>();
            if (imagen == null) throw new InvalidOperationException("La ilustración de HU-11 no tiene componente Image.");

            RectTransform mascaraRect;
            if (mascaraExistente == null)
            {
                int indiceHermano = rectIlustracion.GetSiblingIndex();
                GameObject contenedor = new GameObject("RecorteIlustracionRedondeado", typeof(RectTransform),
                    typeof(CanvasRenderer), typeof(RoundedRectangleGraphic), typeof(Mask));
                mascaraRect = contenedor.GetComponent<RectTransform>();
                mascaraRect.SetParent(tarjeta, false);
                mascaraRect.SetSiblingIndex(indiceHermano);
                rectIlustracion.SetParent(mascaraRect, false);
            }
            else
            {
                mascaraRect = mascaraExistente as RectTransform;
                if (mascaraRect == null) throw new InvalidOperationException("El recorte redondeado de HU-11 no es RectTransform.");
                if (rectIlustracion.parent != mascaraRect) rectIlustracion.SetParent(mascaraRect, false);
            }

            const float ancho = 188f;
            const float alto = 188f;
            mascaraRect.anchorMin = new Vector2(0f, 1f);
            mascaraRect.anchorMax = new Vector2(0f, 1f);
            mascaraRect.pivot = new Vector2(0f, 1f);
            mascaraRect.anchoredPosition = new Vector2(34f, -22f);
            mascaraRect.sizeDelta = new Vector2(ancho, alto);

            RoundedRectangleGraphic graficoMascara = mascaraRect.GetComponent<RoundedRectangleGraphic>();
            graficoMascara.color = Color.white;
            graficoMascara.raycastTarget = false;

            Mask mascara = mascaraRect.GetComponent<Mask>();
            mascara.showMaskGraphic = false;

            rectIlustracion.anchorMin = Vector2.zero;
            rectIlustracion.anchorMax = Vector2.one;
            rectIlustracion.pivot = new Vector2(0.5f, 0.5f);
            rectIlustracion.anchoredPosition = Vector2.zero;
            rectIlustracion.sizeDelta = Vector2.zero;
            imagen.preserveAspect = true;

            EditorUtility.SetDirty(mascaraRect);
            EditorUtility.SetDirty(imagen);
        }

        private static void PrepararIlustraciones()
        {
            PlantillaUIBuilder.ImportarComoSprite("Assets/Art/Escenas/HU11_Tarjeta_Arbol.png");
            PlantillaUIBuilder.ImportarComoSprite("Assets/Art/Escenas/HU11_Tarjeta_Rio.png");
            PlantillaUIBuilder.ImportarComoSprite("Assets/Art/Escenas/HU11_Tarjeta_Piedra.png");
            PlantillaUIBuilder.ImportarComoSprite("Assets/Art/Escenas/HU11_Escena_Referencia.png");
        }

        private static T Cargar<T>(string ruta) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(ruta);
            if (asset == null) throw new InvalidOperationException("Falta recurso para HU-11: " + ruta);
            return asset;
        }

        private static void VerificarEscena()
        {
            Scene escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);
            if (!escena.GetRootGameObjects().SelectMany(obj => obj.GetComponentsInChildren<ReconocerArbolPresenter>(true)).Any() ||
                !escena.GetRootGameObjects().SelectMany(obj => obj.GetComponentsInChildren<RetoSeleccionView>(true)).Any())
                throw new InvalidOperationException("Falta conectar el presenter y la vista en HU-11.");
        }
    }
}
