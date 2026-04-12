# Proposal: Complete Lobby and Match UI

## Problem
The game currently lacks a visual interface for room management. After establishing a connection via Relay, the Host immediately loads the match without allowing players to organize into teams or wait for others. Additionally, there is no visual feedback at the end of the match to declare a winner.

## Proposed Solution
Implement a robust Lobby UI system that synchronizes with the existing `SessionManager.lobbySlots`. This includes:
1. A **Lobby Screen** where players are listed and assigned to teams.
2. **Global Team Switching** using ServerRPCs.
3. **Transition Control**, allowing the Host to decide when to start the match.
4. **End-Match Screens** (Victory/Defeat) to close the game loop.

## Impact
- **UX**: Players have a clear transition from the menu to the combat.
- **Gameplay**: Support for organized Team Duel (4vs4) instead of forced alternate assignment.
- **Retention**: A complete game loop ("Start -> Play -> Result -> Back to Lobby") provides a much better experience.
