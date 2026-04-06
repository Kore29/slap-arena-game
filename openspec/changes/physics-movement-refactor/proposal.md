# Physics and Movement Refactor

Refactor character movement and collision physics to provide a more responsive and "physical" gameplay experience, eliminating the "bulldozer" effect.

## Problem

The current movement implementation in `PlayerController.cs` sets `Rigidbody.linearVelocity` directly every frame. This overrides Unity's collision resolution on the X and Z axes, making the player an unstoppable force that pushes enemies effortlessly without feeling their mass. Additionally, default physics friction causes capsules to "stick" to each other when rubbing, making close-range movement feel clunky and "terrible" for the user.

## What Changes

- Change player movement from direct velocity setting to `Rigidbody.AddForce` (using `ForceMode.VelocityChange`).
- Implement a `PhysicMaterial` with zero friction for all characters (Player and AI).
- Tune Rigidbody mass and damping for better physical reactions.
- Refine AI chase behavior to maintain a small "buffer" distance to avoid constant clipping.

## Capabilities

### New Capabilities
- `physics-refactor`: Standardized movement and collision physics for all arena participants.

### Modified Capabilities
- None (This is an implementation detail improvement).

## Impact

- `PlayerController.cs`: Refactor `HandleMovement`.
- `EnemyAgent.cs`: Review `OnActionReceived` to ensure force parity with player.
- `Assets/Physics/ZeroFriction.physicMaterial`: New asset.
- Prefabs: Update `CapsuleCollider` components with the new material.
