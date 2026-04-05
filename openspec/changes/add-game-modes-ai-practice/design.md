## Context

Actualmente el flujo de juego está acoplado a Unity Netcode. Para permitir el entrenamiento local y modos offline contra la IA, el sistema debe ser capaz de inicializarse sin necesidad de un `NetworkManager` activo o ignorando los componentes de red si no hay sesión.

Afecta a: `PlayerController.cs`, `SessionManager.cs` y `MainMenuController.cs`.

## Goals / Non-Goals

**Goals:**
- Permitir jugar 1vs1 contra la IA localmente sin usar Relay.
- Permitir un modo cooperativo (2 vs AI) online.
- Separar la entrada de input (teclado/ratón) de la replicación de red.

**Non-Goals:**
- No se creará una IA de equipo compleja, usaremos el mismo `EnemyAgent` entrenado.
- No se implementará matchmaking automático aún.

## Decisions

- **Herencia de PlayerController:** En lugar de duplicar el script, haremos que `PlayerController` soporte estados. Si `NetworkManager.Singleton.IsListening` es falso, actuará como un objeto local.
- **GameManager Singleton:** Centralizaremos el spawn en un nuevo `GameManager.cs`. Este objeto decidirá si spawnea el `PlayerPrefab` (para red) o un `LocalPlayerPrefab` (para local).
- **UIDocument Switching:** El Menú Principal tendrá una vista de "Modos de Juego" que aparecerá antes de pedir el código de Relay.

## Risks / Trade-offs

- [Risk] Duplicidad de prefabs (Local vs Network) → Mitigación: Usar el mismo Prefab pero con los componentes de red desactivados si no hay sesión iniciada.
- [Risk] Diferencia en físicas de red (Inercia de Server) vs Local → Mitigación: Sincronizar los parámetros de Rigidbody en ambos modos para que la IA entrene en el entorno correcto.
