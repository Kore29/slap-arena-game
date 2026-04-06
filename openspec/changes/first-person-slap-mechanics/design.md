# Design: First Person Slap Mechanics

## Context

El juego actualmente utiliza un esquema de control de tercera persona/top-down donde el personaje rota hacia la entrada de movimiento. La bofetada se detecta mediante una esfera estática frente al personaje. No hay diferenciación entre la rotación del cuerpo y la de la cámara.

## Goals / Non-Goals

**Goals:**
- Implementar rotación de cámara independiente (Look arriba/abajo) y rotación de cuerpo (izquierda/derecha).
- Hacer que el movimiento WASD sea relativo a hacia donde mira el jugador.
- Cambiar la detección de bofetada a un sistema basado en la mira (SphereCast desde la cámara).
- Introducir un cooldown de bofetada para el jugador (alineado con la IA).

**Non-Goals:**
- No se implementarán animaciones de manos en este paso (se usarán logs/placeholders).
- No se cambiará el sistema de `NetworkManager` ni la lógica de `EjectionZone`.

## Decisions

### 1. Estructura de Cámara
Se utilizará una jerarquía donde la Cámara es hija del objeto Player.
*   **Player Body (Y-Axis)**: Rotado por Mouse X.
*   **Camera (X-Axis)**: Rotado por Mouse Y (con límites de -90 a 90 grados).
*   *Razón*: Es el estándar de la industria para controladores FPS manuales en Unity.

### 2. Detección de Impacto (SphereCast)
Se usará `Physics.SphereCast` desde el centro de la cámara.
*   *Razón*: `OverlapSphere` es impreciso en primera persona porque no garantiza que el jugador esté "apuntando" al objetivo. `SphereCast` permite un margen de error (el radio de la esfera) pero sigue una línea de visión.

### 3. Movimiento con Inercia vs Posición
Seguiremos usando `Rigidbody.MovePosition` pero transformando el vector de entrada.
*   *Razón*: Mantiene la consistencia con el sistema actual de `linearDamping` que ya está ajustado para evitar que los jugadores salgan volando por su propio movimiento.

## Risks / Trade-offs

- **Mareo (Motion Sickness)**: Un movimiento muy rápido o sin suavizado en 1ª persona puede marear. → *Mitigación*: Mantener el `linearDamping` alto y una sensibilidad de ratón configurable.
- **Latencia en Red**: El `SphereCast` se hará en el cliente para feedback inmediato, pero el servidor debe validar la posición. → *Mitigación*: Pasar la posición y dirección en el `ServerRpc`.

## Open Questions

- ¿Deberíamos bloquear el cursor del ratón automáticamente al iniciar? (Probablemente sí: `Cursor.lockState = CursorLockMode.Locked`).
