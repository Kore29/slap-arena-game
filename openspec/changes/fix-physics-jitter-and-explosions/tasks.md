## 1. Refactorización de EnemyAgent.cs

- [x] 1.1 Eliminar la sobreescritura de `linearDamping` y `angularDamping` en `Initialize`.
- [x] 1.2 Cambiar `ForceMode.VelocityChange` por `ForceMode.Acceleration` en `OnActionReceived`.
- [x] 1.3 Implementar `Vector3.ClampMagnitude` para la velocidad al final de `OnActionReceived`.

## 2. Refactorización de PlayerController.cs

- [x] 2.1 Eliminar la sobreescritura de `linearDamping` y `angularDamping` en `Awake`.
- [x] 2.2 Cambiar el cálculo de fuerza en `HandleMovement` para usar aceleración limitada.
- [x] 2.3 Añadir un `Clamp` de velocidad global en `FixedUpdate`.
- [x] 2.4 Corregir el `LayerMask` en `PerformSlap` para no detectar la capa `Default`.

## 3. Ajustes Manuales en el Inspector (USER)

- [x] 3.1 Cambiar `Collision Detection` a `Continuous Dynamic` en el `PlayerPrefab`.
- [x] 3.2 Cambiar `Collision Detection` a `Continuous Dynamic` en el `AIAgentPrefab`.
- [x] 3.3 Ajustar `Linear Damping` a `1` y `Angular Damping` a `10` en ambos Rigidbody (Inspector).
- [x] 3.4 Verificar que el `OpponentLayer` no incluya la capa del propio objeto.

## 4. Verificación y Pruebas

- [x] 4.1 Probar movimiento básico sin "jitter".
- [x] 4.2 Probar colisiones directas a alta velocidad.
- [x] 4.3 Probar bofetadas y verificar impulsos controlados.
