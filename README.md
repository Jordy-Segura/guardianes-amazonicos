# Guardianes de la Amazonía Lectora

Aplicación educativa interactiva ambientada en la Amazonía. Está desarrollada con Unity 6.0 (`6000.6.5f1`), C#, uGUI y TextMeshPro.

## Abrir y ejecutar

1. Abrir la carpeta del repositorio desde Unity Hub con Unity `6000.6.5f1`.
2. Abrir `Assets/_Project/Scenes/Nivel01/Nivel01_ConozcoLaSelva.unity`.
3. Ejecutar Play.

La interfaz utiliza uGUI y TextMeshPro. Los recursos visuales, sonoros y tipográficos se organizan en `Assets/Sprites`, `Assets/Audio` y `Assets/Fonts`. Las dependencias se declaran en `Packages/manifest.json` y se fijan en `Packages/packages-lock.json`.

## Estructura

- `Assets/_Project/Scenes/Nivel01` a `Nivel05`: escenas agrupadas por nivel. Las carpetas de los niveles aún sin escenas quedan preparadas para el equipo.
- `Assets/_Project/Scripts/Core`: control y navegación general.
- `Assets/_Project/Scripts/UI`: componentes de interfaz.
- `Assets/_Project/Scripts/Niveles`: lógica específica de cada actividad.
- `Assets/_Project/Scripts/Audio` y `Assets/_Project/Scripts/Data`: servicios de audio y persistencia.
- `Assets/_Project/Editor`: herramientas del Editor.
- `Assets/Settings`: perfiles de compilación de Unity.
- `Assets/Sprites`, `Assets/Audio`, `Assets/Fonts`, `Assets/Prefabs` y `Assets/Materials`: recursos del juego.
- `Assets/Documentation`: documentación complementaria.
- `ProjectSettings`: configuración compartida de Unity.
- `Packages`: dependencias del proyecto.

### Escenas y actividades

La escena de entrada está en la carpeta del primer nivel. La actividad de identificación del jaguar también se guarda allí como escena independiente. Para volver a generar las carpetas, importar el atlas de animales y actualizar las escenas de compilación, ejecutar `Guardianes > Preparar estructura y reto del jaguar` desde Unity. Las cuatro opciones y sus ilustraciones son recursos independientes dentro de una tarjeta reutilizable; la respuesta correcta habilita el botón del siguiente reto y el avance se conserva localmente.

## Políticas de trabajo con Git y GitHub

### Ramas

| Rama | Propósito |
| --- | --- |
| `main` | Versiones estables, aprobadas y etiquetadas. |
| `develop` | Integración de cambios revisados. |
| `test` | Validación de una versión candidata antes de aprobar su publicación. |
| Rama personal (por ejemplo, `jordy`) | Rama de trabajo exclusiva de cada integrante. Desde ella se envían cambios a `develop`. |

No se trabaja directamente en `main`, `develop` ni `test`. Cuando la configuración de GitHub lo permita, se protegen contra pushes directos y force-pushes. Cada integrante trabaja en su propia rama personal; no se requiere crear ramas `feature/*` ni `fix/*`.

### Flujo de integración

1. Confirmar que Unity Hub usa la versión `6000.6.5f1`.
2. Actualizar la rama personal con los cambios recientes de `develop` y resolver ahí cualquier conflicto.
3. Abrir un Pull Request desde la rama personal hacia `develop`. Incluir objetivo, resumen, pasos y resultado de verificación; adjuntar capturas cuando cambie la interfaz.
4. Revisar el código y comprobar el proyecto en Unity antes de aprobar y fusionar. Si se necesita validación de equipo, promover `develop` a `test` mediante Pull Request; las correcciones se realizan en la rama personal y se envían a `develop` con otro Pull Request.
5. Cuando la versión candidata esté aprobada, integrar `develop` a `main` mediante Pull Request y crear una etiqueta, por ejemplo `v1.0.0`.

La rama `test` es un entorno de validación, no un destino alternativo para saltarse la revisión de `develop`. No se integra una versión en `main` sin aprobación y verificación satisfactorias.

### Commits y revisión

Los mensajes de commit deben ser breves, describir el cambio y seguir el estilo Conventional Commits en minúsculas. Se puede añadir un ámbito para precisar el área:

- `feat: agregar bienvenida del guardian`
- `fix(audio): corregir locución de bienvenida`
- `docs: actualizar instrucciones del proyecto`

Antes de solicitar revisión, comprobar que el proyecto abre en Unity, que los flujos modificados funcionan y que no hay errores nuevos en Console. Para cambios de UI, revisar la vista Game en la resolución objetivo y adjuntar una captura al Pull Request. Registrar cambios importantes de dependencias y configuración en el Pull Request o en esta documentación.

### Unity, dependencias y archivos versionados

- El equipo utiliza Unity `6000.6.5f1` para evitar diferencias de serialización y compatibilidad.
- Se versionan scripts, escenas, recursos, archivos `.meta`, `ProjectSettings` y la configuración necesaria de `Packages`.
- Se mantienen `Packages/manifest.json` y `Packages/packages-lock.json` en Git para documentar y fijar las dependencias.
- No se versionan cachés, compilaciones, preferencias personales, temporales ni credenciales. Nunca se incluyen tokens, claves privadas o contraseñas en commits.
- Después de clonar, abrir el proyecto desde Unity Hub y dejar que Unity regenere `Library`; comunicar diferencias de paquetes antes de actualizar versiones.
- Gestionar los archivos `.meta` junto con cada recurso nuevo, eliminado o renombrado.

### Comprobación mínima antes de integrar

- Unity abre el proyecto con la versión indicada y sin errores nuevos de compilación.
- Los flujos modificados se prueban en Play según sus criterios de aceptación.
- Los cambios visuales se verifican en la resolución objetivo y los cambios de audio se escuchan en el Editor.
- El diff no contiene archivos generados, configuraciones locales ni secretos.
- El Pull Request tiene revisión aprobada y conflictos resueltos.

## Unity Hub

Si se abre el proyecto con otra instalación de Unity 6, verificar que `ProjectSettings/ProjectSettings.asset` habilite la entrada antigua o ambas entradas. El EventSystem usa `StandaloneInputModule` y funciona con `Input Manager (Old)` o `Both`.

Si Unity no dispone de los recursos esenciales de TextMeshPro, importarlos desde `Window > TextMesh Pro > Import TMP Essential Resources`.
