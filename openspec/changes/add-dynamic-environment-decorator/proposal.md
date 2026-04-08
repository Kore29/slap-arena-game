# Proposal: Dynamic Environment Decoration

## Motivation

To enhance replayability and visual variety in "Slap Arena", we want to introduce a procedural decoration system. Each match set on the floating island should feel unique by having trees, rocks, and grass placed in randomized positions and configurations. This system must work in both Offline Practice mode and Online Multiplayer (using seed synchronization).

## What Changes

- Implementation of a `MapGenerator` system that takes a seed and decorates the `PlatformArena` scene.
- Integration with the "Stylized Nature MegaKit" (3 tree types, 4 rock types, and grass).
- Modification of the match sequence to trigger generation before players spawn.
- Support for seed-based synchronization to ensure all clients see the same environment in multiplayer.

## Capabilities

### New Capabilities
- `environment-decorator`: Scriptable system to place trees, rocks, and grass prefabs on a target mesh with random rotation, scale, and placement logic.

## Impact

- **Gameplay**: Environmental objects (trees and rocks) will provide cover or obstacles, changing the navigation of the arena.
- **Multiplayer**: Requires sending a `seed` from the host to clients.
- **Performance**: Use of GPU instancing for grass to maintain high FPS.
