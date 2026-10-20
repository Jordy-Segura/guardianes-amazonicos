# Guardianes de la Amazonía Lectora — Nivel 1

Proyecto 2D desarrollado con Unity 6.0 (`6000.6.5f1`), C#, uGUI y TextMeshPro.

## Historia HU-01

Como estudiante quiero comenzar la aventura de la selva, escuchar la bienvenida del Guardián y comprender el nivel 1.

La escena inicial presenta al Guardián de la Selva, la locución de bienvenida y botones grandes `AUDIO` y `COMENZAR`. Al iniciar, registra localmente el comienzo del nivel y muestra el primer punto de progreso (`1 de 12`). El estado se conserva mediante `PlayerPrefs`.

## Abrir y ejecutar

1. Abrir esta carpeta desde Unity Hub con Unity `6000.6.5f1`.
2. Abrir `Assets/Scenes/BienvenidaNivel1.unity`.
3. Ejecutar Play.

El proyecto usa una ilustración original de la Amazonía en `Assets/Art` y locuciones distintas en español para bienvenida, inicio, continuación y pista. La interfaz se construye con uGUI/TextMeshPro: los textos se ajustan al espacio disponible y las tarjetas usan marcos 9-slice para conservar sus esquinas al cambiar de tamaño. La ilustración permanece como imagen para conservar su detalle.

### Prueba de servicio de señas

En Play, abre **Señas**, pulsa **Consultar API** y después **Ver avatar**. La escena consulta `https://three.ws/api/sign` con una frase genérica de prueba y muestra las palabras con seña, las deletreadas y la duración. El avatar se abre en el navegador.

Esta integración demuestra el consumo de un servicio externo: la API usa ASL en inglés, tiene vocabulario limitado y no ha sido validada como LSEC. No debe presentarse como traducción ecuatoriana ni sustituye una interpretación revisada por personas sordas. La consulta no envía texto del estudiante.

## Estructura

- `Assets/Scenes`: escena del Nivel 1.
- `Assets/Scripts`: lógica de bienvenida, audio y persistencia local.
- `Assets/Editor`: herramienta para regenerar la escena desde el menú Guardianes.
- `Assets/Art` y `Assets/Audio`: recursos locales para uso sin conexión.
- `Packages/manifest.json` y `Packages/packages-lock.json`: dependencias fijadas del proyecto.

## Políticas de trabajo con Git y GitHub

### Ramas

| Rama | Propósito |
| --- | --- |
| `main` | Versiones estables, aprobadas y etiquetadas. |
| `develop` | Integración de funcionalidades ya revisadas. |
| `test` | Validación de una versión candidata antes de aprobar su publicación. |
| Rama personal (por ejemplo, `jordy`) | Rama de trabajo exclusiva de cada integrante. Desde ella se envían cambios revisados a `develop`. |

No se trabaja directamente en `main`, `develop` ni `test`. Las ramas `main`, `develop` y `test` deben protegerse en GitHub contra pushes directos y force-pushes cuando la configuración del repositorio lo permita. Cada integrante trabaja en su propia rama personal; no se requiere crear ramas `feature/*` ni `fix/*`.

### Flujo de integración

1. Confirmar que Unity Hub usa la versión `6000.6.5f1`.
2. Actualizar la rama personal con los cambios recientes de `develop` y resolver ahí cualquier conflicto.
3. Abrir un Pull Request desde la rama personal hacia `develop`. Incluir objetivo, resumen, pasos y resultado de verificación; adjuntar capturas cuando cambie la interfaz.
4. Revisar el código y comprobar la escena en Unity antes de aprobar y fusionar. Si se necesita validación de equipo, promover `develop` a `test` mediante Pull Request; las correcciones se realizan en la rama personal de quien las atienda y se envían a `develop` con otro Pull Request.
5. Cuando la versión candidata esté aprobada, integrar `develop` a `main` mediante Pull Request y crear una etiqueta de versión, por ejemplo `v1.0.0`.

La rama `test` es un entorno de validación, no un destino alternativo para saltarse la revisión de `develop`. No se debe fusionar una versión a `main` sin aprobación y verificación satisfactorias.

### Commits y revisión

Los mensajes de commit deben ser breves y describir el cambio. Se usa el estilo Conventional Commits en minúsculas; se puede añadir un ámbito para precisar el área:

- `feat: agregar bienvenida del guardian`
- `fix(audio): corregir locución del nivel 1`
- `docs: actualizar instrucciones del proyecto`

Antes de solicitar revisión, comprobar que el proyecto abre en Unity, que la escena afectada funciona y que no hay errores nuevos en Console. Para cambios de UI, revisar la vista Game en la resolución objetivo y adjuntar una captura al Pull Request. Registrar los cambios importantes de dependencias y configuración en el Pull Request o en esta documentación.

### Unity, dependencias y archivos versionados

- El equipo utiliza Unity `6000.6.5f1` para evitar diferencias de serialización y compatibilidad.
- Se versionan scripts, escenas, recursos, archivos `.meta`, `ProjectSettings` y la configuración necesaria de `Packages`.
- Se mantienen `Packages/manifest.json` y `Packages/packages-lock.json` en Git para documentar y fijar las dependencias.
- No se versionan cachés, compilaciones, preferencias personales, temporales ni credenciales. Nunca se incluyen tokens, claves privadas o contraseñas en commits.
- Después de clonar, abrir el proyecto desde Unity Hub y dejar que Unity regenere `Library`; si aparece una diferencia de paquetes, comunicarla antes de actualizar versiones.
- No eliminar ni renombrar archivos de `Assets` desde el Explorador sin gestionar sus `.meta` correspondientes en el mismo cambio.

### Comprobación mínima antes de integrar

- Unity abre el proyecto con la versión indicada y sin errores nuevos de compilación.
- La escena y el flujo modificados se prueban en Play siguiendo los criterios de aceptación de la historia.
- Los cambios visuales se verifican en la resolución objetivo; los cambios de audio se escuchan en el Editor.
- El diff no contiene archivos generados, configuraciones locales ni secretos.
- El Pull Request tiene revisión aprobada y conflictos resueltos.

## Unity Hub

Si abres el proyecto en una instalación distinta de Unity 6, verifica que `ProjectSettings/ProjectSettings.asset` habilite la entrada antigua o ambas entradas. El EventSystem de la escena usa `StandaloneInputModule` y funciona con `Input Manager (Old)` o `Both`.

Los recursos esenciales de TextMeshPro se importan automáticamente desde la instalación local de Unity al salir del modo Play. Si la instalación no expone el paquete, usa `Window > Text Mesh Pro > Import TMP Essential Resources` una vez.

Para volver a mostrar la pantalla inicial en el Editor, usa `Guardianes > Reiniciar progreso local del Nivel 1`. La escena conserva el avance del estudiante entre sesiones.
