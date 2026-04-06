## Why

Se busca mejorar la identidad visual del juego "Slap Arena" mediante una renovación estética completa que incluye un nuevo mapa temático, un shader de cielo mejorado y un sistema de personajes aleatorios para aumentar la variedad y la gracia en las partidas multijugador.

## What Changes

- **Nuevo Mapa**: Reemplazo del prototipo actual por una arena basada en "Modular Floating Islands".
- **Cielo Dinámico**: Implementación del "Free Skybox Extended Shader" para una atmósfera más estilizada.
- **Sistema de Personajes Aleatorios**: Nuevo componente `CharacterVisualRandomizer` que elige una skin (Duck, Ghost, Pinguin, Slime) al azar para cada jugador al inicio de la partida.
- **Sincronización en Red**: Uso de `NetworkVariable` para asegurar que todos los jugadores vean el mismo modelo asignado a cada contrincante.

## Capabilities

### New Capabilities
- `character-skin-randomization`: Gestión y sincronización de modelos visuales aleatorios.
- `floating-island-arena`: Nueva configuración de escena y límites de colisión perimetral.

### Modified Capabilities
- `unified-combat-physics`: Ajustar los límites de caída (`EjectionZone`) para adaptarse al nuevo mapa de islas flotantes.

## Impact

- `GameManager.cs`: Podría requerir ajustes menores en el punto de spawn si el mapa es modular.
- `PlayerPrefab` & `AIAgentPrefab`: Se convertirán en contenedores de múltiples modelos visuales.
- `Assets/Scripts/Multiplayer/CharacterVisualRandomizer.cs`: Nuevo script para la lógica de aleatoriedad.
