## 1. Configuración del Animator (USUARIO)

- [ ] 1.1 Crear un nuevo `Animator Controller` llamado `RabbitUniversalController` en `Assets/Animations/`.
- [ ] 1.2 Definir los parámetros: `Speed` (Float), `Slap` (Trigger) e `IsFalling` (Bool).
- [ ] 1.3 Crear un Blend Tree 1D para el estado `Locomotion` (Idle → Run) usando `Speed`.
- [ ] 1.4 Añadir el estado `Slap` y conectarlo mediante el disparo del Trigger.

## 2. Actualización de Prefabs (USUARIO)

- [ ] 2.1 Añadir el componente `Animator` a la raíz del `PlayerPrefab` y el `AIAgentPrefab`.
- [ ] 2.2 Asignar el `RabbitUniversalController` a ambos componentes Animator.
- [ ] 2.3 Añadir el componente `NetworkAnimator` a ambos prefabs y enlazarlo con el Animator.

## 3. Implementación de Lógica en C# (AI)

- [x] 3.1 Modificar `PlayerController.cs` para enviar `Speed` y `Slap` al Animator.
- [x] 3.2 Modificar `EnemyAgent.cs` para enviar `Speed` y `Slap` al Animator.
- [x] 3.3 Asegurar que el sistema de skin aleatorio respete el Animator del modelo activo.

## 4. Verificación y Pulido

- [ ] 4.1 Probar movimiento: ¿El conejo camina y se detiene correctamente?
- [ ] 4.2 Probar bofetada: ¿La animación se ve sincronizada en red?
- [ ] 4.3 Verificar estado de caída: ¿Se activa la animación cuando se cae de la isla?
