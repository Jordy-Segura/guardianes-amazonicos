# Guardianes de la Amazonía Lectora — Nivel 1

Proyecto 2D desarrollado con Unity 6.0 (6000.6.5f1), C#, uGUI y TextMeshPro.

## Historia HU-01

Como estudiante quiero comenzar la aventura de la selva, escuchar la bienvenida del Guardián y comprender el nivel 1.

La escena inicial presenta al Guardián de la Selva, la locución de bienvenida y botones grandes `AUDIO` y `COMENZAR`. Al iniciar, registra localmente el comienzo del nivel y muestra el primer punto de progreso (`1 de 12`). El estado se conserva mediante `PlayerPrefs`.

## Abrir y ejecutar

1. Abrir esta carpeta desde Unity Hub con Unity 6000.6.5f1.
2. Abrir `Assets/Scenes/BienvenidaNivel1.unity`.
3. Ejecutar Play.

El proyecto usa una ilustración original de la Amazonía en `Assets/Art` y locuciones distintas en español para bienvenida, inicio, continuación y pista. La interfaz se construye con uGUI/TextMeshPro: los textos se ajustan al espacio disponible y las tarjetas usan marcos 9-slice para conservar sus esquinas al cambiar de tamaño. La ilustración permanece como imagen para conservar su detalle.

### Prueba de servicio de señas

En Play, abre **Señas**, pulsa **Consultar API** y después **Ver avatar**. La escena consulta `https://three.ws/api/sign` con una frase genérica de prueba y muestra las palabras con seña, las deletreadas y la duración. El avatar se abre en el navegador.

Esta integración solo demuestra el consumo de un servicio externo: la API usa ASL en inglés, tiene vocabulario limitado y no ha sido validada como LSEC. No debe presentarse como traducción ecuatoriana ni sustituye una interpretación revisada por personas sordas. La consulta no envía texto del estudiante.

## Estructura

- `Assets/Scenes`: escena del Nivel 1.
- `Assets/Scripts`: lógica de bienvenida, audio y persistencia local.
- `Assets/Editor`: herramienta para regenerar la escena desde el menú Guardianes.
- `Assets/Art` y `Assets/Audio`: recursos locales para uso sin conexión.
## Unity Hub

Si abres el proyecto en una instalación distinta de Unity 6, verifica que `ProjectSettings/ProjectSettings.asset` habilite la entrada antigua o ambas entradas. El EventSystem de la escena usa `StandaloneInputModule` y funciona con `Input Manager (Old)` o `Both`.

Los recursos esenciales de TextMeshPro se importan automáticamente desde la instalación local de Unity al salir del modo Play. Si la instalación no expone el paquete, usa `Window > TextMesh Pro > Import TMP Essential Resources` una vez.

Para volver a mostrar la pantalla inicial en el Editor, usa `Guardianes > Reiniciar progreso local del Nivel 1`. La escena conserva el avance del estudiante entre sesiones.
