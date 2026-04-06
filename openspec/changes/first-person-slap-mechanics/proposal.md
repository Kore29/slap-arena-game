# Proposal: First Person Slap Mechanics

## Why

El sistema de movimiento actual funciona como un juego de "vista aérea" (Top-Down), donde el personaje gira para mirar hacia donde camina. Para cumplir con la visión de un "First Person Slapper", el jugador debe tener el control total de la mirada con el ratón, y el combate debe ser preciso desde esa perspectiva.

Implementar esto ahora permitirá que el resto del desarrollo (IA, animaciones, efectos) se construya sobre una base sólida y divertida.

## What Changes

- **Controlador en Primera Persona**: Implementación de un sistema de "Look" (ratón) y movimiento relativo a la vista (WASD).
- **Refinamiento de Bofetada**: Cambio de `OverlapSphere` a `SphereCast/Raycast` desde la cámara para mayor precisión.
- **Sistema de Cooldown**: Sincronización de tiempos de ataque entre el servidor y el cliente.
- **Feedback Visual**: Preparación para efectos de sacudida de cámara (Screen Shake) al impactar.

## Capabilities

### New Capabilities
- `first-person-controller`: Manejo de cámara y rotación del cuerpo basado en la entrada del ratón y movimiento relativo.
- `refined-slap-combat`: Detección de impactos precisa desde la cámara con validación de servidor y cooldowns.

### Modified Capabilities
- Ninguna.

## Impact

- **PlayerController.cs**: Se reescribirá gran parte de la lógica de movimiento e inputs.
- **Prefab de Jugador**: Requerirá una estructura de cámara específica (Camera como hijo del Player).
- **Físicas**: Ajuste de inercias para que el movimiento en primera persona se sienta fluido.
