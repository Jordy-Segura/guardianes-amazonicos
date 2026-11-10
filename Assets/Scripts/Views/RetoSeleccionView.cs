using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using GuardianesAmazonicos.Models;

namespace GuardianesAmazonicos.Views
{
    public sealed class RetoSeleccionView : MonoBehaviour
    {
        private static readonly Color Tinta = new Color(0.12f, 0.18f, 0.14f, 1f);
        private static readonly Color TintaBlanca = Color.white;
        private static readonly Color VerdeAcierto = new Color(0.14f, 0.62f, 0.23f, 1f);
        private static readonly Color RojoFallo = new Color(0.85f, 0.18f, 0.18f, 1f);
        private static readonly Color AmarilloSeleccion = new Color(1f, 0.85f, 0.2f, 1f);

        public event Action<int> OnOpcionSeleccionada;
        public event Action OnAudioSolicitado;
        public event Action OnContinuarSolicitado;

        public int IndiceSeleccionado { get; private set; } = -1;

        private RetoSeleccionConfiguracion _configuracion;
        private Button[] _botonesOpciones;
        private Image[] _imagenesTarjetas;
        private Image[] _brillosOpciones;
        private Outline[] _bordesOpciones;
        private TMP_Text _textoPregunta;
        private Button _botonAceptar;
        private GameObject _panelVictoria;
        private GameObject _modalPista;
        private GameObject _modalSenas;
        private AudioSource _audioSource;
        private Sprite[] _spritesAtlas;

        // Sprites cacheados de Resources / Editor
        private Sprite _spriteTarjeta;
        private Sprite _spriteMarco;
        private Sprite _spriteGlobo;
        private Sprite _spritePlacaNivel;
        private Sprite _spritePlacaProgreso;
        private Sprite _spriteLectura;
        private Sprite _spriteSenas;
        private Sprite _spriteAyuda;
        private Sprite _spriteRegresar;
        private Sprite _spriteAvanzar;
        private Sprite _spriteInicio;
        private Sprite _spriteConfiguracion;
        private Sprite _spriteBotonAceptar;
        private Sprite _spriteBotonSiguiente;
        private Sprite _spriteBrillo;
        private Sprite _spriteLetreroMuyBien;
        private Sprite _spritePanelPista;
        private Sprite _spriteBotonVoyIntentarlo;
        private Sprite _spritePanelSenas;
        private Sprite _spriteBotonReproducir;
        private Sprite _spriteBotonCerrar;

        private void CargarSpritesUI()
        {
            _spriteTarjeta = CargarSprite("TarjetaRespuesta");
            _spriteMarco = CargarSprite("MarcoEscena");
            _spriteGlobo = CargarSprite("GloboPregunta");
            _spritePlacaNivel = CargarSprite("PlacaTituloNivel");
            _spritePlacaProgreso = CargarSprite("PlacaProgreso");
            _spriteLectura = CargarSprite("BotonLectura");
            _spriteSenas = CargarSprite("BotonSenas");
            _spriteAyuda = CargarSprite("BotonAyuda");
            _spriteRegresar = CargarSprite("BotonRegresar");
            _spriteAvanzar = CargarSprite("BotonAvanzar");
            _spriteInicio = CargarSprite("BotonInicio");
            _spriteConfiguracion = CargarSprite("BotonConfiguracion");
            _spriteBotonAceptar = CargarSprite("BotonAceptar");
            _spriteBotonSiguiente = CargarSprite("BotonSiguiente");
            _spriteBrillo = CargarSprite("BrilloSeleccion");
            _spriteLetreroMuyBien = CargarSprite("LetreroMuyBien");
            _spritePanelPista = CargarSprite("PanelPista");
            _spriteBotonVoyIntentarlo = CargarSprite("BotonVoyIntentarlo");
            _spritePanelSenas = CargarSprite("PanelSenas");
            _spriteBotonReproducir = CargarSprite("BotonReproducir");
            _spriteBotonCerrar = CargarSprite("BotonCerrar");
        }

        private static Sprite CargarSprite(string nombre)
        {
            Sprite s = Resources.Load<Sprite>("UI/Sprites/" + nombre);
            if (s != null) return s;
#if UNITY_EDITOR
            s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Sprites/" + nombre + ".png");
            if (s != null) return s;
            s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/Sprites/" + nombre + ".png");
            if (s != null) return s;
#endif
            return null;
        }

        public void Construir(RetoSeleccionConfiguracion configuracion)
        {
            _configuracion = configuracion;
            CargarSpritesUI();
            CrearEventSystemSiHaceFalta();

            _audioSource = gameObject.GetComponent<AudioSource>();
            if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;

            Canvas canvas = CrearCanvas();
            RectTransform root = canvas.transform as RectTransform;

            // 1. Fondo de selva (0, 0, 1920, 1080)
            CrearImagenTopLeft("FondoSelva", root, _configuracion.fondo, Color.white, new Vector2(0f, 0f), new Vector2(1920f, 1080f), false);

            // 2. Cabecera fija (Valores de la guía oficial, pág 3)
            CrearBotonTopLeft("BotonLectura", root, _spriteLectura, new Vector2(30f, -16f), new Vector2(132f, 114f), () =>
            {
                if (_configuracion.pronunciaciones != null && _configuracion.pronunciaciones.Length > 0)
                    ReproducirPronunciacion(_configuracion.indiceCorrecto);
                else
                    OnAudioSolicitado?.Invoke();
            });

            CrearBotonTopLeft("BotonSenas", root, _spriteSenas, new Vector2(170f, -16f), new Vector2(132f, 114f), () =>
            {
                if (_modalSenas != null) _modalSenas.SetActive(true);
            });

            CrearBotonTopLeft("BotonAyuda", root, _spriteAyuda, new Vector2(310f, -16f), new Vector2(132f, 114f), () =>
            {
                if (_modalPista != null) _modalPista.SetActive(true);
            });

            // Placa Título de Nivel (592, -12, 484, 108)
            RectTransform placaTitulo = CrearImagenTopLeft("PlacaTitulo", root, _spritePlacaNivel, Color.white, new Vector2(592f, -12f), new Vector2(484f, 108f), false).rectTransform;
            CrearTextoRelativo("TextoTitulo", placaTitulo, _configuracion.titulo, new Vector2(25f, -25f), new Vector2(434f, 58f), 26f, Tinta, TextAlignmentOptions.Center, true);

            // Placa Progreso (1092, -16, 236, 94)
            RectTransform placaProgreso = CrearImagenTopLeft("PlacaProgreso", root, _spritePlacaProgreso, Color.white, new Vector2(1092f, -16f), new Vector2(236f, 94f), false).rectTransform;
            CrearTextoRelativo("TextoProgreso", placaProgreso, _configuracion.numeroEscena + " de 12", new Vector2(20f, -20f), new Vector2(196f, 54f), 28f, TintaBlanca, TextAlignmentOptions.Center, true);

            // Botones de Navegación derecha (84x84)
            CrearBotonTopLeft("BotonRegresar", root, _spriteRegresar, new Vector2(1506f, -12f), new Vector2(84f, 84f), null);
            CrearBotonTopLeft("BotonAvanzar", root, _spriteAvanzar, new Vector2(1606f, -12f), new Vector2(84f, 84f), null);
            CrearBotonTopLeft("BotonInicio", root, _spriteInicio, new Vector2(1706f, -12f), new Vector2(84f, 84f), null);
            CrearBotonTopLeft("BotonConfiguracion", root, _spriteConfiguracion, new Vector2(1806f, -12f), new Vector2(84f, 84f), null);

            // 3. Columna Izquierda: Personaje y Globo de Pregunta
            // Globo de pregunta (158, -174, 684, 240)
            RectTransform globo = CrearImagenTopLeft("GloboPregunta", root, _spriteGlobo, Color.white, new Vector2(158f, -174f), new Vector2(684f, 240f), false).rectTransform;
            _textoPregunta = CrearTextoRelativo("TextoPregunta", globo, _configuracion.pregunta, new Vector2(43f, -34f), new Vector2(598f, 150f), 32f, Tinta, TextAlignmentOptions.Center, true);

            // Personaje Guía (147, -428, 390, 562)
            Image personaje = CrearImagenTopLeft("PersonajeGuia", root, _configuracion.explorador, Color.white, new Vector2(147f, -428f), new Vector2(390f, 562f), true);
            personaje.preserveAspect = true;

            // 4. Columna Derecha Superior: Marco de Escena e Ilustración
            // Marco de escena (921, -188, 800, 425)
            CrearImagenTopLeft("MarcoEscena", root, _spriteMarco, Color.white, new Vector2(921f, -188f), new Vector2(800f, 425f), false);
            // Ilustración temática de escena
            Sprite ilustracionEscena = ObtenerSprite(0);
            if (ilustracionEscena != null)
            {
                Image imagenEscena = CrearImagenTopLeft("ImagenEscena", root, ilustracionEscena, Color.white, new Vector2(942f, -206f), new Vector2(758f, 390f), true);
                imagenEscena.preserveAspect = true;
            }

            // 5. Columna Derecha Inferior: 3 Tarjetas de Respuesta (Y = -663, 256 x 307)
            if (_configuracion.AtlasOpciones != null)
            {
                _spritesAtlas = CrearSpritesAtlas(_configuracion.AtlasOpciones, _configuracion.ColumnasAtlas, _configuracion.FilasAtlas);
            }
            else if (_configuracion.atlasDosPorDos != null)
            {
                _spritesAtlas = CrearSpritesAtlas(_configuracion.atlasDosPorDos, 2, 2);
            }

            CrearTarjetas(root);

            // 6. Botón Aceptar (centrado bajo las tarjetas: X = 1191, Y = -980, 260 x 74)
            _botonAceptar = CrearBotonTopLeft("BotonAceptar", root, _spriteBotonAceptar, new Vector2(1191f, -980f), new Vector2(260f, 74f), () =>
            {
                if (IndiceSeleccionado >= 0)
                {
                    OnOpcionSeleccionada?.Invoke(IndiceSeleccionado);
                }
            });
            _botonAceptar.gameObject.SetActive(false);

            // 7. Modales y Paneles
            CrearModalPista(root);
            CrearModalSenas(root);
            CrearPanelVictoria(root);
        }

        private void CrearTarjetas(RectTransform root)
        {
            int cantidad = Mathf.Min(3, _configuracion.opciones.Length);
            _botonesOpciones = new Button[cantidad];
            _imagenesTarjetas = new Image[cantidad];
            _brillosOpciones = new Image[cantidad];
            _bordesOpciones = new Outline[cantidad];

            float[] posicionesX = { 921f, 1193f, 1465f };

            for (int i = 0; i < cantidad; i++)
            {
                int indice = i;
                Vector2 posTarjeta = new Vector2(posicionesX[i], -663f);
                Vector2 tamTarjeta = new Vector2(256f, 307f);

                GameObject goTarjeta = new GameObject("TarjetaRespuesta" + (i + 1), typeof(RectTransform), typeof(CanvasRenderer));
                goTarjeta.transform.SetParent(root, false);
                RectTransform rectTarjeta = goTarjeta.transform as RectTransform;
                PrepararRectTopLeft(rectTarjeta, posTarjeta, tamTarjeta);

                Image imgFondo = goTarjeta.AddComponent<Image>();
                imgFondo.sprite = _spriteTarjeta;
                imgFondo.color = Color.white;
                imgFondo.type = Image.Type.Simple;
                imgFondo.raycastTarget = true;

                Button btn = goTarjeta.AddComponent<Button>();
                btn.targetGraphic = imgFondo;

                Outline borde = goTarjeta.AddComponent<Outline>();
                borde.effectColor = AmarilloSeleccion;
                borde.effectDistance = new Vector2(4f, -4f);
                borde.enabled = false;

                // Brillo de selección sobre la tarjeta
                if (_spriteBrillo != null)
                {
                    Image brillo = CrearImagenRelativa("Brillo", rectTarjeta, _spriteBrillo, Color.white, Vector2.zero, tamTarjeta, false);
                    brillo.gameObject.SetActive(false);
                    _brillosOpciones[i] = brillo;
                }

                // Ilustración de la opción (18, -22, 220, 188)
                Sprite sp = ObtenerSprite(i);
                Image imgOpcion = CrearImagenRelativa("Ilustracion", rectTarjeta, sp, Color.white, new Vector2(18f, -22f), new Vector2(220f, 188f), false);
                imgOpcion.preserveAspect = true;

                // Nombre TMP de la opción (15, -216, 226, 76)
                CrearTextoRelativo("Nombre", rectTarjeta, _configuracion.opciones[i], new Vector2(15f, -216f), new Vector2(226f, 76f), 24f, Tinta, TextAlignmentOptions.Center, true);

                btn.onClick.AddListener(() =>
                {
                    if (IndiceSeleccionado == indice)
                    {
                        // Pulsar sobre la ya seleccionada valida directamente
                        OnOpcionSeleccionada?.Invoke(indice);
                    }
                    else
                    {
                        MostrarSeleccion(indice);
                    }
                });

                _botonesOpciones[i] = btn;
                _imagenesTarjetas[i] = imgFondo;
                _bordesOpciones[i] = borde;
            }
        }

        public void MostrarSeleccion(int indice)
        {
            IndiceSeleccionado = indice;

            for (int i = 0; i < _botonesOpciones.Length; i++)
            {
                bool esSeleccionado = (i == indice);

                if (_brillosOpciones != null && i < _brillosOpciones.Length && _brillosOpciones[i] != null)
                {
                    _brillosOpciones[i].gameObject.SetActive(esSeleccionado);
                }

                if (_bordesOpciones != null && i < _bordesOpciones.Length && _bordesOpciones[i] != null)
                {
                    _bordesOpciones[i].enabled = esSeleccionado;
                    _bordesOpciones[i].effectColor = AmarilloSeleccion;
                    _bordesOpciones[i].effectDistance = new Vector2(5f, -5f);
                }
            }

            if (_botonAceptar != null)
            {
                _botonAceptar.gameObject.SetActive(true);
            }
        }

        public void MostrarIncorrecta(string pista)
        {
            if (IndiceSeleccionado >= 0 && _bordesOpciones != null && IndiceSeleccionado < _bordesOpciones.Length)
            {
                if (_bordesOpciones[IndiceSeleccionado] != null)
                {
                    _bordesOpciones[IndiceSeleccionado].enabled = true;
                    _bordesOpciones[IndiceSeleccionado].effectColor = RojoFallo;
                    _bordesOpciones[IndiceSeleccionado].effectDistance = new Vector2(6f, -6f);
                }
            }

            if (_textoPregunta != null)
            {
                _textoPregunta.text = "¡Buen intento! " + pista;
            }

            if (_botonAceptar != null)
            {
                _botonAceptar.gameObject.SetActive(false);
            }
            IndiceSeleccionado = -1;
        }

        public void MostrarCorrecta(string mensaje)
        {
            if (IndiceSeleccionado >= 0 && _bordesOpciones != null && IndiceSeleccionado < _bordesOpciones.Length)
            {
                if (_bordesOpciones[IndiceSeleccionado] != null)
                {
                    _bordesOpciones[IndiceSeleccionado].enabled = true;
                    _bordesOpciones[IndiceSeleccionado].effectColor = VerdeAcierto;
                    _bordesOpciones[IndiceSeleccionado].effectDistance = new Vector2(6f, -6f);
                }
            }

            for (int i = 0; i < _botonesOpciones.Length; i++)
            {
                _botonesOpciones[i].interactable = false;
            }

            if (_botonAceptar != null)
            {
                _botonAceptar.gameObject.SetActive(false);
            }

            if (_panelVictoria != null)
            {
                _panelVictoria.SetActive(true);
            }

            if (_textoPregunta != null)
            {
                _textoPregunta.text = mensaje;
            }
        }

        public void MostrarSiguienteEnPreparacion()
        {
            if (_textoPregunta != null)
            {
                _textoPregunta.text = "¡Progreso guardado! El siguiente reto se añadirá después.";
            }
        }

        public void ReproducirPronunciacion(int indice)
        {
            if (_configuracion.pronunciaciones == null || _configuracion.pronunciaciones.Length == 0) return;
            if (indice < 0 || indice >= _configuracion.pronunciaciones.Length) indice = _configuracion.indiceCorrecto;
            AudioClip clip = _configuracion.pronunciaciones[indice];
            if (clip == null) return;
            _audioSource.Stop();
            _audioSource.PlayOneShot(clip);
        }

        private void CrearPanelVictoria(RectTransform root)
        {
            _panelVictoria = new GameObject("PanelVictoria", typeof(RectTransform));
            _panelVictoria.transform.SetParent(root, false);
            RectTransform rect = _panelVictoria.transform as RectTransform;
            PrepararRectTopLeft(rect, Vector2.zero, new Vector2(1920f, 1080f));

            // Rótulo ¡MUY BIEN!
            CrearImagenTopLeft("LetreroMuyBien", rect, _spriteLetreroMuyBien, Color.white, new Vector2(1091f, -340f), new Vector2(460f, 180f), false);

            // Botón Siguiente
            CrearBotonTopLeft("BotonSiguiente", rect, _spriteBotonSiguiente, new Vector2(1191f, -540f), new Vector2(260f, 76f), () =>
            {
                OnContinuarSolicitado?.Invoke();
            });

            _panelVictoria.SetActive(false);
        }

        private void CrearModalPista(RectTransform root)
        {
            _modalPista = new GameObject("ModalPista", typeof(RectTransform));
            _modalPista.transform.SetParent(root, false);
            RectTransform rect = _modalPista.transform as RectTransform;
            PrepararRectTopLeft(rect, Vector2.zero, new Vector2(1920f, 1080f));

            Image oscurecedor = _modalPista.AddComponent<Image>();
            oscurecedor.color = new Color(0f, 0f, 0f, 0.45f);
            oscurecedor.raycastTarget = true;

            // Panel de Pista (centrado)
            RectTransform panel = CrearImagenTopLeft("CartaPista", rect, _spritePanelPista, Color.white, new Vector2(620f, -280f), new Vector2(680f, 520f), false).rectTransform;
            CrearTextoRelativo("TextoPista", panel, _configuracion.pistaIncorrecta, new Vector2(80f, -140f), new Vector2(520f, 220f), 30f, Tinta, TextAlignmentOptions.Center, true);

            // Botón ¡Voy a intentarlo!
            CrearBotonRelativo("BotonCerrarPista", panel, _spriteBotonVoyIntentarlo, new Vector2(180f, -390f), new Vector2(320f, 75f), () =>
            {
                _modalPista.SetActive(false);
            });

            _modalPista.SetActive(false);
        }

        private void CrearModalSenas(RectTransform root)
        {
            _modalSenas = new GameObject("ModalSenas", typeof(RectTransform));
            _modalSenas.transform.SetParent(root, false);
            RectTransform rect = _modalSenas.transform as RectTransform;
            PrepararRectTopLeft(rect, Vector2.zero, new Vector2(1920f, 1080f));

            Image oscurecedor = _modalSenas.AddComponent<Image>();
            oscurecedor.color = new Color(0f, 0f, 0f, 0.45f);
            oscurecedor.raycastTarget = true;

            // Panel de video señas
            RectTransform panel = CrearImagenTopLeft("PanelSenas", rect, _spritePanelSenas, Color.white, new Vector2(560f, -240f), new Vector2(800f, 600f), false).rectTransform;

            // Botón reproducir
            CrearBotonRelativo("BotonPlay", panel, _spriteBotonReproducir, new Vector2(350f, -260f), new Vector2(100f, 100f), null);

            // Botón cerrar X
            CrearBotonRelativo("BotonCerrar", panel, _spriteBotonCerrar, new Vector2(720f, -30f), new Vector2(60f, 60f), () =>
            {
                _modalSenas.SetActive(false);
            });

            _modalSenas.SetActive(false);
        }

        private Sprite ObtenerSprite(int indice)
        {
            if (_configuracion.imagenes != null && indice < _configuracion.imagenes.Length && _configuracion.imagenes[indice] != null)
                return _configuracion.imagenes[indice];
            if (_spritesAtlas != null && indice < _spritesAtlas.Length) return _spritesAtlas[indice];
            return null;
        }

        private static Sprite[] CrearSpritesAtlas(Texture2D atlas, int columnas, int filas)
        {
            columnas = Mathf.Max(1, columnas);
            filas = Mathf.Max(1, filas);
            int ancho = atlas.width / columnas;
            int alto = atlas.height / filas;
            Sprite[] sprites = new Sprite[columnas * filas];
            for (int indice = 0; indice < sprites.Length; indice++)
            {
                int columna = indice % columnas;
                int filaDesdeAbajo = filas - 1 - indice / columnas;
                sprites[indice] = Sprite.Create(atlas,
                    new Rect(columna * ancho, filaDesdeAbajo * alto, ancho, alto),
                    new Vector2(0.5f, 0.5f), 100f);
            }
            return sprites;
        }

        private Canvas CrearCanvas()
        {
            GameObject objeto = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            objeto.transform.SetParent(transform, false);
            Canvas canvas = objeto.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = objeto.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;
            return canvas;
        }

        private void CrearEventSystemSiHaceFalta()
        {
            if (EventSystem.current != null || UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null) return;
            GameObject objeto = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            InputSystemUIInputModule modulo = objeto.GetComponent<InputSystemUIInputModule>();
            modulo.AssignDefaultActions();
        }

        private static void PrepararRectTopLeft(RectTransform rect, Vector2 posicion, Vector2 tamano)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = posicion;
            rect.sizeDelta = tamano;
        }

        private static Image CrearImagenTopLeft(string nombre, RectTransform padre, Sprite sprite, Color color, Vector2 posicion, Vector2 tamano, bool preservarAspecto)
        {
            GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            objeto.transform.SetParent(padre, false);
            RectTransform rect = objeto.transform as RectTransform;
            PrepararRectTopLeft(rect, posicion, tamano);
            Image imagen = objeto.GetComponent<Image>();
            imagen.sprite = sprite;
            imagen.color = color;
            imagen.preserveAspect = preservarAspecto;
            imagen.raycastTarget = false;
            return imagen;
        }

        private static Image CrearImagenRelativa(string nombre, RectTransform padre, Sprite sprite, Color color, Vector2 posicion, Vector2 tamano, bool preservarAspecto)
        {
            GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            objeto.transform.SetParent(padre, false);
            RectTransform rect = objeto.transform as RectTransform;
            PrepararRectTopLeft(rect, posicion, tamano);
            Image imagen = objeto.GetComponent<Image>();
            imagen.sprite = sprite;
            imagen.color = color;
            imagen.preserveAspect = preservarAspecto;
            imagen.raycastTarget = false;
            return imagen;
        }

        private static Button CrearBotonTopLeft(string nombre, RectTransform padre, Sprite sprite, Vector2 posicion, Vector2 tamano, Action alPulsar)
        {
            GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(Image), typeof(Button));
            objeto.transform.SetParent(padre, false);
            RectTransform rect = objeto.transform as RectTransform;
            PrepararRectTopLeft(rect, posicion, tamano);
            Image imagen = objeto.GetComponent<Image>();
            imagen.sprite = sprite;
            imagen.color = Color.white;
            imagen.raycastTarget = true;
            Button boton = objeto.GetComponent<Button>();
            boton.targetGraphic = imagen;
            if (alPulsar != null) boton.onClick.AddListener(() => alPulsar());
            return boton;
        }

        private static Button CrearBotonRelativo(string nombre, RectTransform padre, Sprite sprite, Vector2 posicion, Vector2 tamano, Action alPulsar)
        {
            GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(Image), typeof(Button));
            objeto.transform.SetParent(padre, false);
            RectTransform rect = objeto.transform as RectTransform;
            PrepararRectTopLeft(rect, posicion, tamano);
            Image imagen = objeto.GetComponent<Image>();
            imagen.sprite = sprite;
            imagen.color = Color.white;
            imagen.raycastTarget = true;
            Button boton = objeto.GetComponent<Button>();
            boton.targetGraphic = imagen;
            if (alPulsar != null) boton.onClick.AddListener(() => alPulsar());
            return boton;
        }

        private TMP_Text CrearTextoRelativo(string nombre, RectTransform padre, string texto, Vector2 posicion, Vector2 tamano, float tamanoFuente, Color color, TextAlignmentOptions alineacion, bool negrita)
        {
            GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            objeto.transform.SetParent(padre, false);
            RectTransform rect = objeto.transform as RectTransform;
            PrepararRectTopLeft(rect, posicion, tamano);
            TMP_Text tmp = objeto.GetComponent<TextMeshProUGUI>();
            tmp.text = texto;
            tmp.font = _configuracion.fuente;
            tmp.fontSize = tamanoFuente;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = Mathf.Max(14f, tamanoFuente - 10f);
            tmp.fontSizeMax = tamanoFuente;
            tmp.color = color;
            tmp.alignment = alineacion;
            tmp.fontStyle = negrita ? FontStyles.Bold : FontStyles.Normal;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            return tmp;
        }
    }
}
