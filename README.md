# Guardianes de la Amazonía Lectora — Nivel 1

Proyecto 2D desarrollado con Unity 6.0 (6000.6.5f1), C#, uGUI y TextMeshPro.

## Historia HU-01

Como estudiante quiero comenzar la aventura de la selva, escuchar la bienvenida del Guardián y comprender el nivel 1.

La escena inicial presenta al Guardián de la Selva, la locución de bienvenida y botones grandes `AUDIO` y `COMENZAR`. Al iniciar, registra localmente el comienzo del nivel y muestra el primer punto de progreso (`1 de 12`). El estado se conserva mediante `PlayerPrefs`.

## Abrir y ejecutar

1. Abrir esta carpeta desde Unity Hub con Unity 6000.6.5f1.
2. Abrir `Assets/Scenes/BienvenidaNivel1.unity`.
3. Ejecutar Play.

El proyecto usa una ilustración original de la Amazonía en `Assets/Art` y la locución en español en `Assets/Audio`. La interfaz se construye con componentes uGUI/TextMeshPro para adaptarse a distintas resoluciones.

## Estructura

- `Assets/Scenes`: escena del Nivel 1.
- `Assets/Scripts`: lógica de bienvenida, audio y persistencia local.
- `Assets/Editor`: herramienta para regenerar la escena desde el menú Guardianes.
- `Assets/Art` y `Assets/Audio`: recursos locales para uso sin conexión.
