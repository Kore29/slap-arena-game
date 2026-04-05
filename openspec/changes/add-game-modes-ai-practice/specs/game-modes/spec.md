## ADDED Requirements

### Requirement: Standalone Offline Play
The game SHALL allow the player to enter the arena and battle local agents without initializing Unity Relay or Authentication services.

#### Scenario: User starts Practice Mode
- **WHEN** the user selects "Practice (Local)" from the menu
- **THEN** the system bypasses Netcode initialization and spawns a local Player instance and a local AI agent in the arena.

### Requirement: Mode-Based Spawning
The `GameManager` SHALL dynamically decide which entity to spawn at each starting position based on the active mode (PVP, Practice, or Coop).

#### Scenario: Spawn in Coop Mode
- **WHEN** the game starts with 2 human players (Host & Client)
- **THEN** the system spawns 2 Player instances for humans and 2 AI agents for the opposing team, mirroring the network setup.
