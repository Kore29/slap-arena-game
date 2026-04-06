## Why

La física del juego es inestable debido a un conflicto entre el modo de fuerza `VelocityChange`, la fricción cero y la falta de sincronización entre el código y el Inspector. Esto provoca que el jugador y la IA salgan disparados ("vuelen") al chocar o moverse, además de generar un efecto de teletransporte por desincronización de red.

## What Changes

- **Modo de Movimiento**: Cambiar de `ForceMode.VelocityChange` a `ForceMode.Acceleration` (IA) y un sistema de fuerza limitado (Jugador) para evitar impulsos infinitos.
- **Detección de Colisiones**: Cambiar a `Continuous Dynamic` en los prefabs para evitar el solapamiento y las explosiones físicas.
- **Normalización de Damping**: Sincronizar los valores de damping entre el código y el editor para evitar conflictos de red.
- **Filtro de Capas**: Corregir el `LayerMask` en las bofetadas para que no detecte el suelo o al propio atacante por error.

## Capabilities

### New Capabilities
- `stabilized-physics`: Define los límites de fuerza y aceleración permitidos en el mundo del juego.

### Modified Capabilities
- `unified-combat-physics`: Ajustar los requisitos de "Slap Force" para que no superen los límites de estabilidad.

## Impact

- `PlayerController.cs`: Refactorización del método `HandleMovement` y `PerformSlap`.
- `EnemyAgent.cs`: Refactorización de `OnActionReceived` y `ExecuteSlap`.
- `PlayerPrefab` & `AIAgentPrefab`: Cambios manuales en el Rigidbody y Colliders.
