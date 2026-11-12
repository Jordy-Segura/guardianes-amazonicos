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
    /// <summary>Genera únicamente HU-07, usando la plantilla y los recursos comunes del proyecto.</summary>
    public static class BuildHU07IdentificarRio
    {
        public const string RutaEscena = "Assets/Scenes/Niveles/Nivel01/Escena07_HU07_IdentificarRio.unity";
        private const string RutaConfig = "Assets/Scripts/Models/NivelesData/RetoIdentificarRio.asset";
        private const string RutaHoja = "Assets/Art/Escenas/HU07_Agua_Triptych.png";

        [MenuItem("Guardianes/HU-07/Generar escena Identificar el río")]
        public static void Generar()
        {
            PlantillaUIBuilder.Preparar();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            PrepararHojaDeSprites();

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

            GameObject controlador = new GameObject("HU07_IdentificarRio");
            r.fuenteAudio = controlador.AddComponent<AudioSource>();
            r.fuenteAudio.playOnAwake = false;
            r.fuenteAudio.loop = false;
            RetoSeleccionView vista = controlador.AddComponent<RetoSeleccionView>();
            vista.Enlazar(r);
            IdentificarRioPresenter presenter = controlador.AddComponent<IdentificarRioPresenter>();
            presenter.Configurar(c, vista,
                Cargar<AudioClip>("Assets/Audio/Voice/HU07/acierto_03_07.wav"),
                Cargar<AudioClip>("Assets/Audio/Voice/HU07/error_03_07.wav"));

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena, RutaEscena);
            AssetDatabase.SaveAssets();
            BuildAssignedScenes.AgregarEscenasBuild(RutaEscena);
            VerificarEscena();
            Debug.Log("HU07_GENERACION_OK: escena, audio, tarjetas, estados y navegación preparados.");
        }

        [MenuItem("Guardianes/HU-07/Ajustar tarjetas y ocultar Regresar en la escena existente")]
        public static void AjustarEscenaExistente()
        {
            Scene escenaActiva = SceneManager.GetActiveScene();
            Scene escenaHu07 = SceneManager.GetSceneByPath(RutaEscena);
            bool cargarEnAdicional = !escenaHu07.IsValid() || !escenaHu07.isLoaded;
            if (cargarEnAdicional) escenaHu07 = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Additive);

            try
            {
                GameObject canvas = escenaHu07.GetRootGameObjects().FirstOrDefault(objeto => objeto.name == "CanvasActividad");
                Transform actividad = canvas != null ? canvas.transform.Find("CapaBase/Actividad") : null;
                if (actividad == null) throw new InvalidOperationException("No se encontró la actividad de HU-07.");

                const float posicionY = -635f;
                for (int indice = 1; indice <= RetoSeleccionConfiguracion.CANTIDAD_OPCIONES; indice++)
                {
                    Transform tarjeta = actividad.Find("Respuesta" + indice);
                    RectTransform rectTarjeta = tarjeta as RectTransform;
                    if (rectTarjeta == null) throw new InvalidOperationException("Falta la tarjeta de respuesta " + indice + " en HU-07.");
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

                EditorSceneManager.MarkSceneDirty(escenaHu07);
                EditorSceneManager.SaveScene(escenaHu07, RutaEscena);
                Debug.Log("HU07_AJUSTE_OK: tarjetas elevadas y botón Regresar oculto solo en HU-07.");
            }
            finally
            {
                if (cargarEnAdicional) EditorSceneManager.CloseScene(escenaHu07, true);
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
            c.NumeroEscena = 7;
            c.TotalEscenas = 12;
            c.TituloNivel = "Nivel 1 · Conozco la selva";
            c.Pregunta = "Selecciona el río.";
            c.Opciones = new[] { "Río", "Mar", "Laguna" };
            c.ImagenesOpciones = new[] { SpriteHoja("HU07_Rio"), SpriteHoja("HU07_Mar"), SpriteHoja("HU07_Laguna") };
            c.IndiceCorrecto = 0;
            c.ExplicacionesIncorrectas = new[]
            {
                string.Empty,
                "El mar es muy amplio y llega hasta la costa. Busca el río de agua dulce.",
                "La laguna es un cuerpo de agua quieta. Busca el río que fluye entre sus orillas."
            };
            c.Pista = "Busca el agua dulce que fluye entre dos orillas y atraviesa la selva.";
            c.Felicitacion = "¡Muy bien! Encontraste el río.";
            c.ImagenEscena = Cargar<Sprite>("Assets/Art/Backgrounds/Niveles/N3_Protejo_el_rio.png");
            c.Fondo = Cargar<Sprite>("Assets/Art/Backgrounds/Niveles/N1_Conozco_la_selva.png");
            c.Personaje = Cargar<Sprite>("Assets/Art/Characters/Explorador.png");
            c.PersonajeIncorrecto = Cargar<Sprite>("Assets/Art/Characters/ExploradorPreocupado.png");
            c.FotogramasCelebracion = BuildAssignedScenes.CargarFotogramasExplorador();
            c.AudioLectura = Cargar<AudioClip>("Assets/Audio/Voice/HU07/lectura_03_07.wav");
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
                throw new InvalidOperationException("HU-07 debe conservar exactamente tres tarjetas de respuesta.");

            const float posicionY = -635f;
            foreach (RetoSeleccionView.TarjetaRespuestaVista tarjeta in referencias.tarjetas)
            {
                if (tarjeta == null || tarjeta.boton == null || tarjeta.boton.transform.parent is not RectTransform contenedor)
                    throw new InvalidOperationException("No se pudo ubicar una de las tarjetas de HU-07.");

                Vector2 posicion = contenedor.anchoredPosition;
                contenedor.anchoredPosition = new Vector2(posicion.x, posicionY);
                AjustarImagenTarjeta(contenedor);
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
                throw new InvalidOperationException("Falta la ilustración de una tarjeta HU-07.");

            Image imagen = rectIlustracion.GetComponent<Image>();
            if (imagen == null) throw new InvalidOperationException("La ilustración de HU-07 no tiene componente Image.");

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
                if (mascaraRect == null) throw new InvalidOperationException("El recorte redondeado de HU-07 no es RectTransform.");
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

        private static void PrepararHojaDeSprites()
        {
            TextureImporter importer = AssetImporter.GetAtPath(RutaHoja) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("No se encontró la hoja ilustrada de HU-07: " + RutaHoja);
            importer.GetSourceTextureWidthAndHeight(out int ancho, out int alto);
            if (ancho % 3 != 0) throw new InvalidOperationException("La hoja debe tener tres paneles del mismo ancho.");
            int panel = ancho / 3;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritesheet = new[]
            {
                Meta("HU07_Rio", 0, panel, alto), Meta("HU07_Mar", panel, panel, alto),
                Meta("HU07_Laguna", panel * 2, panel, alto)
            };
            importer.SaveAndReimport();
        }

        private static SpriteMetaData Meta(string nombre, int x, int ancho, int alto) => new SpriteMetaData
        {
            name = nombre,
            rect = new Rect(x, 0, ancho, alto),
            alignment = (int)SpriteAlignment.Center,
            pivot = new Vector2(0.5f, 0.5f)
        };

        private static Sprite SpriteHoja(string nombre) => AssetDatabase.LoadAllAssetsAtPath(RutaHoja)
            .OfType<Sprite>().FirstOrDefault(sprite => sprite.name == nombre)
            ?? throw new InvalidOperationException("No se pudo importar la tarjeta ilustrada: " + nombre);

        private static T Cargar<T>(string ruta) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(ruta);
            if (asset == null) throw new InvalidOperationException("Falta recurso para HU-07: " + ruta);
            return asset;
        }

        private static void VerificarEscena()
        {
            Scene escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);
            if (!escena.GetRootGameObjects().SelectMany(obj => obj.GetComponentsInChildren<IdentificarRioPresenter>(true)).Any() ||
                !escena.GetRootGameObjects().SelectMany(obj => obj.GetComponentsInChildren<RetoSeleccionView>(true)).Any())
                throw new InvalidOperationException("Falta conectar el presenter y la vista en HU-07.");
        }
    }
}
