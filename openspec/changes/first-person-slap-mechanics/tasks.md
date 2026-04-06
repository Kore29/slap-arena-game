## 1. Cámara e Input en FPS

- [x] 1.1 Configurar jerarquía en el Prefab de Jugador (Cámara como hijo).
- [x] 1.2 Implementar lógica de rotación de ratón (Look) en `PlayerController.cs`.
- [x] 1.3 Añadir bloqueo de cursor (`Cursor.lockState`) al iniciar la partida.

## 2. Movimiento Relativo

- [x] 2.1 Refactorizar `HandleInput` para que el movimiento WASD sea relativo a la vista del jugador.
- [x] 2.2 Asegurar que el cuerpo solo rota sobre el eje Y (horizontal).

## 3. Combate Refinado (Slapping)

- [x] 3.1 Sustituir `OverlapSphere` por `SphereCast` con offset para evitar colisión propia.
- [x] 3.2 Implementar sistema de Cooldown (0.5s) para el jugador.
- [x] 3.3 Sincronizar la dirección del ataque en el `ServerRpc` para validación.

## 4. Feedback y Pulido

- [x] 4.1 Añadir un pequeño "retroceso" de cámara al lanzar bofetada.
- [x] 4.2 Corregir física de caída (cambio a linearVelocity).
- [x] 4.3 Verificación final y limpieza de código.
