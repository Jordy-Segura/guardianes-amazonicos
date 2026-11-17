# Estándar de escenas

Todas las actividades del juego comparten la misma plantilla aprobada de la Guía Unity. Este documento fija qué debe cumplir cada escena y cómo comprobarlo.

## Datos de cada reto (`RetoSeleccionConfiguracion`)

| Campo | Estándar |
| --- | --- |
| `Nivel`, `NumeroEscena`, `TotalEscenas` | Nivel real, escena real y 12 escenas por nivel. |
| `TituloNivel` | `Nivel 1 • Conozco la selva` / `Nivel 2 • Cuido los animales` (viñeta U+2022). |
| `Fuente` | `Assets/Fonts/OpenDyslexic3-Regular SDF.asset` (RNF-01). |
| `Personaje` | Explorador u otro guía de la actividad, nunca vacío. |
| `PersonajeIncorrecto` | `ExploradorPreocupado`, para la retroalimentación del error. |
| `FotogramasCelebracion` | Los 40 fotogramas de `Assets/Art/Animaciones/Explorador/Fotogramas`. |
| `VideoSenas` | `senas_nivel_NN_pregunta_EE.mp4`, la convención de la guía. |
| `Pista`, `Felicitacion` | Texto propio de la actividad; la felicitación no puede ser genérica. |
| `ExplicacionesIncorrectas` | Tres entradas; las dos opciones incorrectas con texto propio. |
| `Opciones`, `ImagenesOpciones` | Tres opciones y tres imágenes asignadas. |
| `SiguienteEscena` | La actividad siguiente del nivel; vacío solo al final del bloque implementado. |

## Escena serializada

Las escenas de actividad y la bienvenida se guardan con la plantilla ya construida en la escena (no se genera al ejecutar):

- Canvas `CanvasActividad` o `CanvasBienvenida`, `Scale With Screen Size`, referencia 1920 × 1080 y `matchWidthOrHeight = 0.5`.
- `EventSystem` con el módulo de acciones del Input System.
- Cámara ortográfica y audio ambiente `AmbienteSelva`.
- Tres animaciones de personaje (`AnimacionPersonajePNG`): guía, celebración y pista. En la bienvenida, al menos la animación del saludo.
- Paneles independientes `PanelPista` y `PanelSenas`, y `EstadoCorrecto` con el letrero y el personaje animado.
- Cabecera completa: Lectura, Señas, Ayuda, Regresar, Avanzar, Inicio y Configuración, más el progreso y el título.
- Tres tarjetas de respuesta de 256 × 307 con borde de selección, error y acierto.
- Todos los textos en OpenDyslexic, incluidos los creados por la plantilla.

## Escenas migradas a la plantilla

HU-02, HU-03 y HU-06 generaban su interfaz al ejecutar. Se reconstruyeron con la plantilla serializada: ahora comparten animaciones, paneles, cabecera, audio ambiente y referencias con el resto de actividades. La vista propia `GuacamayoView` se conserva como legado y ya no se usa en la escena.

## Escenas con vista propia

Las escenas HU-04, HU-07, HU-08, HU-11 y HU-12 llegaron de `develop` con vistas propias (`ActividadPlantas`, `IdentificarRio`, `ReconocerArbol`, mariposa y recompensa). Comparten la tipografía OpenDyslexic y el audio por estado, pero su composición no es la plantilla serializada; migrarlas es una decisión pendiente del equipo.

## Cómo aplicar y verificar

- **Guardianes > Sahid > Estandarizar escenas de actividad**: normaliza los datos de los seis retos de plantilla, reconstruye las escenas que lo necesiten, reordena Build Profiles, enlaza `Siguiente` y aplica la tipografía.
- **Guardianes > Sahid > Verificar estándar de escenas**: audita datos y escenas; falla si algo se sale del estándar.
- En lote:

```powershell
Unity.exe -batchmode -nographics -projectPath <ruta> `
  -executeMethod GuardianesAmazonicos.Editor.EstandarizarEscenas.EstandarizarPorLote `
  -logFile <ruta>.log -quit
```

El estándar no modifica el arte, las coordenadas de la guía ni la arquitectura MVP + Observer.

## Pendientes

- Locución de HU-02: resuelta; `develop` incorporó `lectura_01_02.wav` y los audios de pista y felicitación.
- Videos LSEC reales: los retos declaran el nombre del archivo, pero `Assets/StreamingAssets/LSEC/` sigue vacío.
