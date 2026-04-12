# Design: Complete Lobby and Match UI

This document describes the technical architecture for the Lobby UI and Match End feedback system.

## Components

### 1. UI Toolkit (UXML/USS)
- **`Lobby.uxml`**:
  - `visual-element#lobby-container`: Root.
  - `label#room-code`: Displays the Relay join code.
  - `scroll-view#team-a-list`: Column for Team A.
  - `scroll-view#team-b-list`: Column for Team B (hidden in FFA).
  - `button#start-match`: Enabled only for Host.
- **`LobbyItem.uxml`**:
  - `template` for a player slot. Shows `Nickname` and a small `bot-indicator`.

### 2. UI Logic (Controller)
- **`LobbyController.cs`**:
  - Subscribes to `SessionManager.Instance.lobbySlots.OnListChanged`.
  - Clears and rebuilds the visual columns whenever the list changes.
  - Maps `LobbySlot` data to `LobbyItem` visuals.

### 3. Networking Logic (SessionManager)
- **Team Switching**:
  - `[ServerRpc] void ChangeTeamServerRpc(ulong clientId, int newTeam)`
  - Re-assigns the `LobbySlot` in the `NetworkList`.
- **Match Start**:
  - Host calls a `LoadGameSceneServerRpc` which triggers `NetworkSceneManager.LoadScene("PlatformArena")`.

### 4. End Match State
- **`GameManager`**:
  - Monitors the state of `TeamMember` components in the scene.
  - When only one team (or one player in FFA) remains, set `GameState.Win`.
  - Triggers a **ClientRpc** to show the `MatchResults.uxml` for all clients.

## UI Flow
1. **MainMenu**: Host creates room -> Shows Lobby UI (MainMenu scene).
2. **Lobby**: Players join -> Lists update -> Host clicks "Start Match".
3. **Arena**: Match starts -> Map Generator scales arena -> Combat.
4. **End**: Victory declared -> Match Results Overlay appears -> Return to Lobby.
