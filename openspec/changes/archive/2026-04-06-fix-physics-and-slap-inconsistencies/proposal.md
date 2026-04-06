# Proposal: Fix Physics and Slap Inconsistencies

## Problem Statement

The Player and AI (EnemyAgent) are currently operating under different physical rules and slap detection logic. This creates a disconnect where training is suboptimal (the AI doesn't experience the same physics as the human) and multiplayer PVP feel is inconsistent. Additionally, the out-of-bounds (OOB) thresholds are hardcoded at different heights, leading to potential race conditions between scripts and the EjectionZone trigger.

## What Changes

- **Unify Physics Damping**: Align Rigidbody linear (0.1f) and angular (10f) damping between `PlayerController` and `EnemyAgent`.
- **Synchronize Slap Logic**: Ensure both Player and AI use consistent detection radius (e.g., 2.0f), layer masks, and apply force in a consistent manner.
- **Harmonize OOB Thresholds**: Set a consistent `Y` threshold (Y = -0.5f) across all scripts.
- **Multiplayer Priority**: Implement changes with a "Network-First" focus, preparing for stable PVP/Coop.

## Capabilities

### New Capabilities
- `unified-combat-physics`: A single source of truth for horizontal friction, slap force, and detection radius that applies to all entities.

### Modified Capabilities
- `slap-arena-ml`: Adjusting agent rewards and observations to reflect the updated physics and OOB thresholds.

## Impact

- **`PlayerController.cs`**: Damping and Slap logic adjustments.
- **`EnemyAgent.cs`**: Damping, Slap logic, and episode reset thresholds.
- **`EjectionZone.cs`**: Update to reflect the new Y threshold.
- **`GameManager.cs`**: Ensure correct setup for these components.
