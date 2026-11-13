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

## Excepción conocida: HU-03

`Escena03_HU03_Guacamayo` mantiene una vista propia (`GuacamayoView`) que construye la interfaz al ejecutar, con animación por fotogramas y su propio panel de configuración. Cumple fuente, animación y accesibilidad, pero no la estructura serializada. Su migración a la plantilla común requiere la aprobación del equipo porque reemplaza esa vista.

## Cómo aplicar y verificar

- **Guardianes > Sahid > Estandarizar escenas de actividad**: normaliza los datos de los seis retos, reconstruye HU-02 con la plantilla, reordena Build Profiles, enlaza `Siguiente` y aplica la tipografía.
- **Guardianes > Sahid > Verificar estándar de escenas**: audita datos y escenas; falla si algo se sale del estándar.
- En lote:

```powershell
Unity.exe -batchmode -nographics -projectPath <ruta> `
  -executeMethod GuardianesAmazonicos.Editor.EstandarizarEscenas.EstandarizarPorLote `
  -logFile <ruta>.log -quit
```

El estándar no modifica el arte, las coordenadas de la guía ni la arquitectura MVP + Observer.

## Pendientes

- Locución de HU-02 (`lectura_01_02.wav`): la escena ya usa la plantilla y el botón Lectura existe, pero el clip falta porque debe grabarse con la misma voz del resto de actividades.
- Migración de HU-03 a la plantilla común, si el equipo lo aprueba.
- Videos LSEC reales: los seis retos ya declaran el nombre del archivo, pero `Assets/StreamingAssets/LSEC/` sigue vacío.
