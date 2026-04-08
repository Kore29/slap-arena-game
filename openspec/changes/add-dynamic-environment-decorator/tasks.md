# Tasks: Implement Dynamic Environment Decoration

## 1. Setup Assets

- [x] 1.1 Create `Assets/Prefabs/Environment/` folder.
- [x] 1.2 Prepare Tree Prefabs (3 models from MegaKit).
- [x] 1.3 Prepare Rock Prefabs (4 models from MegaKit).
- [x] 1.4 Prepare Grass Prefab with "Enable GPU Instancing".

## 2. Core Implementation

- [x] 2.1 Create `Assets/Scripts/Environment/MapGenerator.cs`.
- [x] 2.2 Define serialized fields for prefab arrays and spawn ranges.
- [x] 2.3 Implement `CleanAndGenerate(int seed)`.
- [x] 2.4 Implement vertical raycast placement logic.
- [x] 2.5 Implement random rotation (0-360 on Y) and random scale (0.8x to 1.2x).

## 3. GameManager Integration

- [x] 3.1 Update `GameManager.cs` to include a reference to `MapGenerator`.
- [x] 3.2 In `ExecuteSpawning()` (Offline), retrieve a random seed and call the generator.
- [x] 3.3 Add logic to skip generation if a `MapGenerator` is not present in the scene.

## 4. Multiplayer Support

- [x] 4.1 Implement seed synchronization (Netcode NetworkVariable).
- [x] 4.2 Validate that all clients see the same environment.

## 5. Polish & Performance

- [x] 5.1 Implement "Exclusion Zones" around player spawn points.
- [x] 5.2 Tune grass density for optimal visual/performance balance.
