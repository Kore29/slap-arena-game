# Design: Proper Game Mode System

This document outlines the technical design for implementing 1vs1, Team Duel (4vs4), and FFA modes with a dynamic lobby and bot-filling system.

## Goals & Non-Goals

**Goals:**
- Flexible game mode system managed by a `GameModeManager`.
- Dynamic Lobby UI for slot management and team switching.
- Host-side bot spawning and replacement logic.
- Team-aware AI and color-coded name tags.
- Scalable maps for different player counts.

**Non-Goals:**
- Dedicated server support (staying with Host/Client for now).
- Complex skill-based matchmaking (just basic lobby listing).
- Advanced spectator mode.

## Decisions

### 1. Slot-Based Lobby System
We will use a slot-based system where each match has a fixed number of slots (e.g., 8 for 4vs4). 
- `LobbySlot` struct will track `ClientId`, `Nickname`, `TeamID`, and `IsBot`.
- A `NetworkList<LobbySlot>` will be synchronized across all clients to display the lobby state.

### 2. Bot Replacement Logic
The Host will monitor client connections. 
- When a new player joins, the Host identifies a bot-occupied slot and replaces it with the new client.
- If a player leaves and "Bots ON" is active, a new bot will be spawned in that slot.

### 3. Team-Aware AI (ML-Agents)
The `EnemyAgent` will be updated with a `NetworkVariable<int> TeamID`.
- **Targeting**: The AI will search for targets with a different `TeamID`.
- **Friendly Fire**: Collisions/Slaps will check `TeamID` before applying force.

### 4. Spanish Nicknames
A `BotNameUtility` will load a list of Spanish names from a JSON asset and assign unique names to each bot at spawn time.

### 5. UI Architecture
- **Create Match**: Three cards in the Main Menu that set the initial `GameMode`.
- **Lobby Screen**: A new UXML screen showing two columns for Team Duel or a single list for FFA.
- **Server Browser**: Unity Lobby integration to list active sessions with metadata (Mode, Players).

## Risks / Trade-offs

- **Unity Lobby Complexity**: Integrating Lobby + Relay is more complex than Relay alone but provides the required server list functionality.
- **ML-Agents Desync**: Running ML-Agents behavior on the Host and syncing via Netcode Transform might introduce minor lag for clients, but is standard for Host/Client physics.
- **Map Scaling**: Simple scale multiplication might distort some environmental assets; we may need to define "Small" and "Large" arena variations.
