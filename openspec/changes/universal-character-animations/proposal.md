## Why

El juego "Slap Arena" cuenta actualmente con modelos visuales atractivos pero estáticos. Para mejorar el "game-feel" y la retroalimentación visual, es necesario implementar un sistema de animaciones que permita a los personajes (tanto jugadores como IAs) reaccionar al movimiento y al combate, sincronizando estas acciones en entorno multijugador.

## What Changes

- **Animator Controller Universal**: Creación de un único controlador de animaciones que servirá para todos los modelos de tipo "Rabbit".
- **Sincronización en Red**: Implementación de `NetworkAnimator` para asegurar que las animaciones se vean correctamente en todos los clientes.
- **Integración con Scripts**: Modificación de `PlayerController.cs` y `EnemyAgent.cs` para enviar parámetros al Animator (Velocidad, Bofetada, etc.).
- **Feedback de Combate**: Animación específica para la bofetada que proporcione una sensación de impacto clara.

## Capabilities

### New Capabilities
- `character-animation-sync`: Sincronización de estados de animación por red.
- `animation-state-management`: Control de transiciones (Idle, Run, Slap, Falling).

### Modified Capabilities
- `character-skin-randomization`: Asegurar que el sistema de cambio de skin asigne correctamente el Animator al modelo activo.

## Impact

- `PlayerController.cs`: Se añadirá una referencia al `Animator` y llamadas a `SetFloat` y `SetTrigger`.
- `EnemyAgent.cs`: Se añadirá lógica similar para que la IA también anime sus acciones.
- `PlayerPrefab` & `AIAgentPrefab`: Requerirán el componente `Animator` y `NetworkAnimator`.
