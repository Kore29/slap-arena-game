# Physics Refactor Design

Technical implementation details for reactive movement and smooth collisions.

## Problem

Direct assignment of `linearVelocity` creates a "kinematic-like" behavior for the XZ axes, effectively giving the player infinite mass during collisions. Friction between capsules causes characters to "stick" and makes them feel clunky.

## Goals / Non-Goals

**Goals:**
- SNAPPY movement that respects collisions.
- Smooth sliding between characters.
- Consistent physics for both Player and AI.

**Non-Goals:**
- Root Motion animation (outside scope).
- Flying mechanics.

## Decisions

- **Movement Code**: Switch from `linearVelocity = V` to:
  ```csharp
  Vector3 velocityChange = targetVelocity - currentVelocity;
  _rb.AddForce(velocityChange, ForceMode.VelocityChange);
  ```
- **Physics Material**: Create a new asset `Assets/Physics/ZeroFriction.physicMaterial` with all friction values at 0.
- **Character Setup**: 
  - Player: Mass 1, Damping 7-10.
  - AI: Mass 1, Damping 7-10.
- **AI AI proximity**: Threshold in `EnemyAgent` to stop pushing into the player.

## Risks / Trade-offs

- **Slap Power**: Higher damping might require even more `slapForce` to achieve the same displacement.
- **Stopping Distance**: A high damping (7-10) is needed to avoid "drifting" (ice-skating feel) when using `VelocityChange`.
