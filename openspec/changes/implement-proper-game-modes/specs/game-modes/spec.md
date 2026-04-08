# Specification: Proper Game Modes

This spec defines the requirements for the 1vs1, Team Duel (4vs4), and FFA game modes.

## Requirements

### R1: Game Mode Selection
- **Requirement**: The system must allow the user to select between 1vs1, Team Duel, and FFA modes from the Main Menu.
- **Scenario**: When a user clicks a mode card, a new networked session (Lobby) must be created with the corresponding parameters.

### R2: Dynamic Lobby System
- **Requirement**: The lobby must display 8 slots (for 4vs4) or 2 slots (for 1vs1).
- **Requirement**: Slots must be filled with named AI bots by default.
- **Requirement**: When a player joins, an AI bot must be removed to free a slot.

### R3: Team Management
- **Requirement**: In Team Duel, players must be able to switch between Team A and Team B.
- **Requirement**: The Host must be able to toggle "Bots ON/OFF" for the lobby.

### R4: AI Behavior (Team Awareness)
- **Requirement**: AI agents must only target players or bots from opposing teams.
- **Requirement**: Friendly fire must be disabled for all players/bots on the same team.

### R5: Visual Identification
- **Requirement**: Names of allies must be displayed in Blue.
- **Requirement**: Names of enemies must be displayed in Red.

### R6: Victory Conditions
- **Requirement**: 1vs1 matches end after one player is knocked out of the arena.
- **Requirement**: Team/FFA matches end when only one team/player remains in the arena.
