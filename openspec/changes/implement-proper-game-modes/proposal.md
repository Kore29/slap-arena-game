# Implement Proper Game Modes

Implement a comprehensive game mode system including 1vs1, Team Duel (4vs4), and Free-For-All (FFA). The system will feature a dynamic lobby with bot-filling, player replacement, and team management.

## Why

The current game mode system is minimal and lacks the polish required for a full multiplayer competitive experience. Players need a way to create matches, browse available games, and play against AI bots that behave like human competitors (including nicknames and team awareness).

## What Changes

- **Game Mode Logic**: Refactor `GameManager` and `SessionManager` to support multiple modes (1vs1, 4vs4 Teams, FFA).
- **Lobby System**: Create a new UI using UI Toolkit for match creation and lobby management.
- **Bot Management**: Implement dynamic bot spawning/replacement logic on the host. 
- **AI Enhancements**: Update `EnemyAgent` to support team identification and target selection (closest enemy).
- **HUD/UI**: Color-coded name tags and updated HUD for team status.
- **Environment**: Scalable map sizes for 1vs1 vs Team Duel.

## Capabilities

### New Capabilities
- `game-modes`: Comprehensive system for 1vs1, 4vs4 Teams, and FFA.
- `lobby-system`: Dynamic lobby for bot-filling, team switching, and match starting.
- `bot-management`: Logic to fill empty slots with named bots and replace them when players join.
- `nickname-system`: Persistent player nicknames and random Spanish bot names.

### Modified Capabilities
- `multiplayer-session`: Extend Unity Relay session creation to support Lobby metadata and custom game settings.

## Impact

- `GameManager`: Core logic for mode state and spawning.
- `SessionManager`: Integration with Unity Lobby and Relay.
- `EnemyAgent`: Targeting and team-awareness logic.
- `MainMenuController`: UI navigation for the new game mode selection screens.
- `UIDocument` (MainMenu & HUD): New visual elements.
