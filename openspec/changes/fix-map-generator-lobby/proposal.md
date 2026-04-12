# Fix Map Generator in Lobby & Online Mode

Fix the issue where procedural vegetation (trees, rocks, grass) fails to spawn when starting a networked match from a Lobby. Additionally, improve network scene synchronization and UI initialization stability.

## Why

Currently, the `MapGenerator` relies on a `NetworkVariable` change to trigger generation. On the Host, this event sometimes doesn't fire correctly during the initial spawn/scene-load sequence, or it fires before other dependencies (like GameModeData) are ready. Furthermore, the use of standard `SceneManager` instead of `NetworkSceneManager` causes desynchronization where clients don't follow the Host into the arena.

## What Changes

- **Environment Generation**: Refactor `MapGenerator` to ensure the Host triggers generation immediately after setting the seed, ensuring deterministic generation for all.
- **Scene Management**: Upgrade `GameManager` to use `NetworkSceneManager`, ensuring all clients synchronize scene loads correctly.
- **UI Robustness**: Refactor `WorldNameTag` to use event-driven initialization instead of timed delays.

## Capabilities

### Modified Capabilities
- `multiplayer-session`: Improved scene synchronization and reliable environment generation.
- `ui-world-tags`: More reliable name tag display in multiplayer.

## Impact

- `GameManager.cs`: Changed scene loading logic.
- `MapGenerator.cs`: Changed generation trigger logic for Host.
- `WorldNameTag.cs`: Changed initialization sequence.
