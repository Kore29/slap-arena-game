# Design: Dynamic Environment Decoration

## Overview

The `MapGenerator` system is designed to procedurally decorate the arena mesh at runtime (or in-editor) using a provided seed. This ensures that every match has a unique but deterministic layout of trees, rocks, and grass.

## Goals / Non-Goals

**Goals:**
- Provide deterministic randomization using a `seed`.
- Place 3 distinct tree types and 4 distinct rock types.
- Ensure objects do not spawn in mid-air or overlapping excessively.
- Support both offline and networked game modes.
- Achieve a dense, dynamic look for grass while maintaining performance.

**Non-Goals:**
- Advanced terrain sculpting.
- Destroyable environment objects (out of scope for now).
- Intelligent pathfinding adjustment (AI will use NavMesh, which needs to be bake-friendly).

## Decisions

- **Raycast-Based Placement**: Instead of complex mesh analysis, we will use vertical raycasts from a "cloud" above the island. This is reliable, simple, and accounts for any height variation in the island mesh.
- **Prefab Lists**: The generator will hold arrays of prefabs to allow easy artist-side extension.
- **Exclusion Mask**: We will use a `LayerMask` to ensure objects only land on the "Ground" layer and avoid spawning on players or UI elements.
- **Randomization Logic**: We will wrap the selection logic in a `Random.InitState` call to ensure cross-platform consistency of the generated map.

## Risks / Trade-offs

- **Collision Overlap**: Random placement might occasionally spawn two rocks very close to each other. We will implement a simple "Min Distance" check or accept it as part of the stylized look.
- **Performance**: High-density grass can impact mobile/low-end performance. We will use GPU Instancing and potentially a `density` slider.
