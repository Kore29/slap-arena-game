# Design: Fix Map Generator & Scene Sync

This document outlines the technical changes to fix environment generation and scene synchronization in multiplayer.

## Goals

- Ensure `MapGenerator` triggers for the Host immediately upon setting the seed.
- Synchronize scene loads across all clients using `NetworkSceneManager`.
- Remove unreliable `Invoke` calls in UI components.

## Decisions

### 1. Reliable Map Generation (Host-side)
In `MapGenerator.cs`, the `ServerSyncRoutine` currently just sets the `NetworkVariable`. We will change it to:
```csharp
syncedScale.Value = scale;
syncedSeed.Value = seed;
CleanAndGenerate(seed, scale); // Call directly on Host
```
This ensures the Host doesn't wait for its own `OnValueChanged` event, which can be inconsistent depending on the `NetworkVariable` tick rate and spawn state.

### 2. Networked Scene Loading
In `GameManager.cs`, replacing `SceneManager.LoadScene` with `NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single)`.
- **Reasoning**: Standard `SceneManager` only loads the scene locally. `NetworkSceneManager` sends a message to all clients to load the same scene, ensuring everyone is in the same world.

### 3. Event-Driven UI Initialization
In `WorldNameTag.cs`, we will subscribe to `TeamMember` variables in `OnNetworkSpawn`.
- We will also add a manual `RefreshVisuals()` call that can be triggered if data is already present during spawn (late joiners).

## Risks

- **Scene Load Failures**: If a client fails to load the scene, `NetworkSceneManager` might have different behaviors (timeouts). We should stick to `Single` mode to keep it simple.
- **Race conditions**: `GameManager` might try to spawn players before the scene has finished loading on all clients. However, NGO handles player object spawning relatively well during scene transitions.
