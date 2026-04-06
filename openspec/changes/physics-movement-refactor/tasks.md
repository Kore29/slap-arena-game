## 1. Configuración de Prefabs y Física

- [ ] 1.1 Crear el material físico `Assets/Physics/ZeroFriction.physicMaterial` (Fricción 0).
- [ ] 1.2 Aplicar el material a los `CapsuleCollider` (Jugador y IA).
- [ ] 1.3 Aumentar `Linear Damping` a **7** en el Rigidbody para detener el deslizamiento.

## 2. Refactor de Código de Movimiento

- [ ] 2.1 Cambiar `PlayerController.cs` a movimiento basado en `AddForce(VelocityChange)`.
- [ ] 2.2 Sincronizar `EnemyAgent.cs` para que use el mismo sistema de fuerzas que el jugador.
- [ ] 2.3 Implementar lógica de parada por proximidad en `EnemyAgent.cs`.

## 3. Pruebas de Sensación (Tactility)

- [ ] 3.1 Probar el choque hombro con hombro (debe sentirse reactivo).
- [ ] 3.2 Verificar que ya no hay "fricción pegajosa" al rozar objetos.
- [ ] 3.3 Ajustar `slapForce` si el nuevo damping lo hace sentir más débil.
