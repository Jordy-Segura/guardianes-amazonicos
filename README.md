# Guardianes de la Amazonía Lectora

Proyecto Unity nuevo y vacío, preparado para comenzar el desarrollo desde cero. Usa Unity `6000.6.5f1`, C# y Universal Render Pipeline (URP) 3D. La escena inicial es `Assets/_Project/Scenes/Nivel01/SampleScene.unity`.

## Abrir el proyecto

1. En Unity Hub, selecciona **Add project from disk** y elige la carpeta raíz del repositorio.
2. Usa Unity `6000.6.5f1` para conservar la compatibilidad.
3. Abre `Assets/_Project/Scenes/Nivel01/SampleScene.unity` y pulsa **Play**.

Unity volverá a crear `Library` y otros cachés locales al abrir el proyecto. Las dependencias se encuentran en `Packages/manifest.json` y `Packages/packages-lock.json`.

## Políticas de trabajo con Git y GitHub

### Ramas

| Rama | Propósito |
| --- | --- |
| `main` | Versiones estables, aprobadas y etiquetadas. |
| `develop` | Integración de cambios revisados. |
| `test` | Validación de una versión candidata antes de aprobar su publicación. |
| Rama personal (por ejemplo, `jordy`) | Rama de trabajo exclusiva de cada integrante; desde ella se envían cambios a `develop`. |

No se trabaja directamente en `main`, `develop` ni `test`. Cada integrante trabaja en su propia rama; no se requiere crear ramas `feature/*` ni `fix/*`. Cuando la configuración de GitHub lo permita, se protegen las ramas compartidas contra pushes directos y force-pushes.

### Flujo de integración

1. Confirmar que Unity Hub usa `6000.6.5f1`.
2. Actualizar la rama personal con los cambios recientes de `develop` y resolver ahí cualquier conflicto.
3. Abrir un Pull Request desde la rama personal hacia `develop`. Incluir el objetivo, resumen, pasos y resultado de verificación; adjuntar capturas cuando cambie la interfaz.
4. Revisar el código y comprobar el proyecto en Unity antes de aprobar y fusionar. Si se necesita validación de equipo, promover `develop` a `test` mediante Pull Request; las correcciones se envían desde la rama personal a `develop` mediante otro Pull Request.
5. Cuando la versión candidata esté aprobada, integrar `develop` a `main` mediante Pull Request y crear una etiqueta, por ejemplo `v1.0.0`.

La rama `test` es un entorno de validación, no un destino alternativo para omitir la revisión de `develop`. No se integra una versión en `main` sin aprobación y verificación satisfactorias.

### Commits y revisión

Usar mensajes breves en minúsculas que describan el cambio, siguiendo Conventional Commits:

- `feat: agregar una mecánica`
- `fix(audio): corregir la reproducción`
- `docs: actualizar instrucciones`

Antes de solicitar revisión, comprobar que el proyecto abre en Unity, que los flujos modificados funcionan y que no hay errores nuevos en Console. Para cambios visuales, revisar Game View en la resolución objetivo y adjuntar una captura al Pull Request. Registrar los cambios importantes de dependencias y configuración.

### Unity, dependencias y archivos versionados

- Todo el equipo utiliza Unity `6000.6.5f1`.
- Versionar scripts, escenas, recursos, archivos `.meta`, `ProjectSettings` y la configuración necesaria de `Packages`.
- Mantener `Packages/manifest.json` y `Packages/packages-lock.json` en Git.
- No versionar cachés, compilaciones, preferencias personales, temporales ni credenciales. Nunca incluir tokens, claves privadas ni contraseñas.
- Después de clonar, abrir el proyecto desde Unity Hub y dejar que Unity regenere `Library`.
- Gestionar los archivos `.meta` junto con cada recurso nuevo, eliminado o renombrado.

### Comprobación mínima antes de integrar

- Unity abre el proyecto con la versión indicada y sin errores nuevos de compilación.
- Los flujos modificados se prueban en Play según sus criterios de aceptación.
- Los cambios visuales se verifican en la resolución objetivo; los cambios de audio se escuchan en el Editor.
- El diff no contiene archivos generados, configuraciones locales ni secretos.
- El Pull Request tiene revisión aprobada y conflictos resueltos.

