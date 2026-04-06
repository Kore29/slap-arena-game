# Capability: Stabilized Physics

## Description
Define los límites de estabilidad física para asegurar que los agentes y jugadores no superen velocidades que causen inestabilidades en el motor de Unity o en la sincronización de red.

## Requirements

### R1: Control de Fuerza de Movimiento
- El sistema de movimiento debe usar `Acceleration` en lugar de `VelocityChange`.
- La fuerza aplicada debe estar limitada por una variable `maxAcceleration`.

### R2: Límite de Velocidad (Speed Clamp)
- Ningún objeto con Rigidbody debe superar una velocidad de `25f` en magnitud total.
- Si la velocidad supera este límite, se debe aplicar un recorte inmediato tras procesar las fuerzas.

### R3: Detección de Colisiones Segura
- Todos los personajes dinámicos deben usar `CollisionDetectionMode.ContinuousDynamic`.

### R4: Sincronización de Damping
- Los valores de `linearDamping` y `angularDamping` no deben ser sobreescritos por código al iniciar, permitiendo el ajuste manual desde el Inspector.
