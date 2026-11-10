# Guardianes de la Amazonía Lectora

Proyecto educativo desarrollado en Unity para que estudiantes exploren la selva amazónica mediante actividades interactivas y recursos de accesibilidad.

## Requisitos y ejecución

- Unity `6000.6.5f1` (ver `ProjectSettings/ProjectVersion.txt`).
- C# y Unity uGUI con TextMesh Pro.
- Abrir la carpeta raíz del repositorio desde Unity Hub.
- Abrir la escena de inicio indicada en **File > Build Profiles > Scene List** y pulsar **Play**. Los recursos de Unity y sus paquetes se importan al abrir el proyecto.

Las dependencias se registran en `Packages/manifest.json` y `Packages/packages-lock.json`. No se debe borrar ni ignorar el archivo de bloqueo de paquetes.

## Organización del proyecto

```text
Assets/
├── Scenes/Niveles/       Cinco niveles con doce escenas cada uno
├── Art/                  Fondos, personajes e ilustraciones 2D
├── Audio/                Locuciones, efectos y música
├── Fonts/                Fuentes y recursos de TextMesh Pro
├── Prefabs/              Controles y componentes visuales reutilizables
├── StreamingAssets/LSEC/ Videos locales de lengua de señas
└── Scripts/
    ├── Models/NivelesData/ Datos de actividades y progreso
    ├── Views/               Vistas uGUI
    ├── Presenters/          Coordinación entre vistas y modelos
    ├── Events/              Eventos tipados del juego
    └── Persistence/         Persistencia local
```

La aventura se organiza en cinco niveles, cada uno con doce escenas (60 escenas en total). Dentro de cada nivel, las actividades se implementan en orden. El segundo nivel comienza con la actividad de protección del hábitat del Mono Curioso. Las demás escenas se incorporarán conforme se implementen sus actividades. Las pantallas usan Canvas con `Scale With Screen Size`, resolución de referencia 1920×1080 y proporción 16:9. Los textos se renderizan sobre la plantilla para que puedan ajustarse y traducirse sin volver a editar las ilustraciones.

## Plantilla de actividades (Guía Unity)

Las escenas de Jordy (Nivel 1: Bienvenida, Escena 5 y Escena 9; Nivel 2: Escena 1) se generan con la plantilla aprobada en *Guardianes de la Amazonía – Guía Unity*:

- Menú **Guardianes > Generar mis escenas con la plantilla** reconstruye esas cuatro escenas; no abre ni modifica escenas de otros integrantes.
- `Assets/Scripts/Editor/PlantillaUIBuilder.cs` coloca cada componente con las coordenadas de la guía (Anchor y Pivot en `(0, 1)`, Canvas 1920 × 1080): Lectura, Señas, Ayuda, título, progreso, navegación, globo, marco, imagen principal y tres tarjetas de 256 × 307.
- Estados incluidos: Bienvenida → Sin selección → Seleccionada (destello + Aceptar) → Incorrecta (globo y borde rojo + Intentarlo de nuevo) o Correcta (MUY BIEN + Siguiente), y ventanas independientes de Pista y Señas.
- El contenido variable de cada pregunta está en `Assets/Scripts/Models/NivelesData/*.asset` (enunciado, opciones, respuesta correcta, explicaciones, pista, audio y video).
- Recursos con la convención de la guía: `Assets/Art/Escenas/pregunta_NN_EE_escena.png`, `Assets/Art/Items/Respuestas/respuesta_NN_EE_x.png` y `Assets/Audio/Voice/Lectura/lectura_NN_EE.wav`. Los videos de señas se buscan en `Assets/StreamingAssets/LSEC/senas_nivel_NN_pregunta_EE.mp4`.
- **Guardianes > Capturar estados de mis escenas** genera capturas 1920 × 1080 de cada estado para revisar la composición.

## Arquitectura y accesibilidad

- Se utiliza MVP: las vistas presentan información y emiten acciones; los presenters coordinan la lógica; los modelos contienen datos y reglas sin depender de Unity.
- Los eventos son tipados y locales a cada funcionalidad. Las suscripciones se registran y liberan en los ciclos de vida del componente.
- El avance se guarda como JSON local en `Application.persistentDataPath`; las preferencias de accesibilidad se reservan para `PlayerPrefs`.
- La interfaz contempla lectura en pantalla, reproducción de locuciones y un visor para videos LSEC almacenados localmente. Solo deben incorporarse videos de señas revisados y grabados siguiendo los estándares del proyecto; no se simulan señas con gestos o imágenes genéricas.
- El visor busca el archivo `Assets/StreamingAssets/LSEC/BienvenidaLSEC.mp4`; mientras no esté disponible, la opción de reproducir permanece deshabilitada y se muestra el aviso correspondiente.
- Los controles deben tener objetivos táctiles amplios, contraste legible, texto ajustable y alternativas visuales o auditivas cuando correspondan.

## Políticas de trabajo con Git y GitHub

### Ramas

| Rama | Propósito |
| --- | --- |
| `main` | Versiones estables, aprobadas y etiquetadas. |
| `develop` | Integración de cambios revisados. |
| `test` | Validación de una versión candidata antes de aprobar su publicación. |
| Rama personal (por ejemplo, `jordy`) | Rama de trabajo de cada integrante; desde ella se envían cambios a `develop`. |

No se trabaja directamente en `main`, `develop` ni `test`. Cada integrante trabaja en su propia rama; no se crean ramas `feature/*` ni `fix/*`. Cuando la configuración de GitHub lo permita, se protegen las ramas compartidas contra pushes directos y force-pushes.

### Flujo de integración

1. Confirmar que Unity Hub usa `6000.6.5f1`.
2. Actualizar la rama personal con los cambios recientes de `develop` y resolver ahí cualquier conflicto.
3. Abrir un Pull Request desde la rama personal hacia `develop`. Incluir objetivo, resumen, pasos y resultado de verificación; adjuntar capturas cuando cambie la interfaz.
4. Revisar el código y comprobar el proyecto en Unity antes de aprobar y fusionar. Si se necesita validación de equipo, promover `develop` a `test` mediante Pull Request; las correcciones se envían desde la rama personal hacia `develop` mediante otro Pull Request.
5. Cuando la versión candidata esté aprobada, integrar `develop` a `main` mediante Pull Request y crear una etiqueta, por ejemplo `v1.0.0`.

La rama `test` es un entorno de validación, no un destino alternativo para omitir la revisión de `develop`. No se integra una versión en `main` sin aprobación y verificación satisfactorias.

### Commits y revisión

Usar mensajes breves en minúsculas que describan el cambio, siguiendo Conventional Commits:

- `feat: agregar una mecánica`
- `fix(audio): corregir la reproducción`
- `docs: actualizar instrucciones`

Antes de solicitar revisión, comprobar que el proyecto abre en Unity, que los flujos modificados funcionan y que no hay errores nuevos en Console. Para cambios visuales, revisar Game View en la resolución objetivo y adjuntar una captura al Pull Request. Registrar cambios importantes de dependencias y configuración.

### Unity, dependencias y archivos versionados

- Todo el equipo utiliza Unity `6000.6.5f1`.
- Versionar scripts, escenas, recursos, archivos `.meta`, `ProjectSettings` y la configuración necesaria de `Packages`.
- Mantener `Packages/manifest.json` y `Packages/packages-lock.json` en Git.
- No versionar cachés, compilaciones, preferencias personales, temporales ni credenciales. Nunca incluir tokens, claves privadas ni contraseñas.
- Después de clonar, abrir el proyecto desde Unity Hub y dejar que Unity regenere `Library`.
- Gestionar los archivos `.meta` junto con cada recurso nuevo, eliminado o renombrado.

### Comprobación antes de integrar

- Unity abre el proyecto con la versión indicada y sin errores nuevos de compilación.
- Los flujos modificados se prueban en Play según sus criterios de aceptación.
- Los cambios visuales se verifican en la resolución objetivo; los cambios de audio se escuchan en el Editor.
- El diff no contiene archivos generados, configuraciones locales ni secretos.
- El Pull Request tiene revisión aprobada y conflictos resueltos.
