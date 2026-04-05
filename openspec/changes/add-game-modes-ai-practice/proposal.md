## Why

Actualmente, Slap Arena ML es puramente multijugador y depende de una sesión de Unity Relay activa para funcionar. Esto impide que los jugadores puedan practicar solos contra la IA o jugar partidas cooperativas (Humano + IA) de forma local. Para mejorar la retención y accesibilidad, necesitamos un sistema de modos de juego que permita tanto el juego offline como configuraciones flexibles de equipos.

## What Changes

- Refactorización de `PlayerController.cs` para soportar un modo "Local" que no herede de `NetworkBehaviour` o que gestione su lógica sin depender de un `IsOwner` de red.
- Introducción de un `GameModeManager` que controle qué entidades (Jugadores o AIs) se spawnean en cada equipo según el modo seleccionado.
- Actualización de `MainMenu.uxml` para incluir botones de acceso directo a "Práctica contra IA" y "Modo Cooperativo".
- Implementación de la lógica de "Humano + IA vs 2 AIs" y el modo puro "1v1 vs Jugador" local (si se desea) o remoto.

## Capabilities

### New Capabilities
- `local-game-engine`: Motor de juego simplificado que inicializa la escena directamente sin pasar por el flujo de Netcode (Relay).
- `flexible-team-spawner`: Sistema que asigna prefabs de Jugador o Agente IA a posiciones de spawn basándose en la configuración del modo de juego.
- `game-mode-ui-navigation`: Interfaz de usuario ampliada en UI Toolkit para seleccionar entre Practice (Local), PVP (Online) y Coop (Online).

### Modified Capabilities
- `physics-combat-system`: Se modificará para aplicar fuerzas localmente cuando no haya red, manteniendo la compatibilidad con el sistema de ServerRPC actual.

## Impact

Este cambio afecta profundamente a cómo se instancian los objetos en la escena. Tendremos que asegurar que los scripts de combate funcionen tanto con un `NetworkObject` activo como de forma aislada. Esto simplificará enormemente las pruebas de la IA (Fase 5 anterior) ya que no requerirá levantar un servidor de Relay cada vez que queramos ver el progreso del entrenamiento.
