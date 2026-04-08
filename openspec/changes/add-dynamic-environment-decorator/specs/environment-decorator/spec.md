# Capability Spec: environment-decorator

The `environment-decorator` capability allows the game to procedurally place trees, rocks, and grass on the arena mesh based on a provided integer seed.

## Requirements

### R1: Deterministic Randomization
The placement must be identical across different instances (Offline and Online) if provided with the same seed.

#### Scenario: Seed-based consistency
- **WHEN** Two different game instances receive the seed `42`.
- **THEN** Both instances must spawn trees and rocks in the exact same positions and rotations.

### R2: Asset Distribution
The system must randomly select and place a configured number of trees and rocks from a pool of prefabs.

#### Scenario: Random object counts
- **WHEN** The generator is called with `minTrees: 1` and `maxTrees: 3`.
- **THEN** The number of trees spawned must be between 1 and 3 (inclusive).

### R3: Surface Placement
Objects must be placed on the surface of the arena mesh, accounting for vertical height variations.

#### Scenario: Raycast check
- **WHEN** A spawn position is picked above the island.
- **THEN** The object must be instantiated at the exact Y-coordinate where the ground mesh is hit by a vertical raycast.

### R4: Grass Coverage
The system must always populate the island with grass, even if trees and rocks are minimal.

#### Scenario: Always-on grass
- **WHEN** Map generation is triggered.
- **THEN** A dense coverage of grass prefabs must be instantiated across the playable area.
