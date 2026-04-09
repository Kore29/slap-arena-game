# Tasks: Implement Proper Game Modes

## 1. Core Logic & Data Structures
- [x] 1.1 Create `GameModeData` ScriptableObject to define mode parameters.
- [x] 1.2 Implement `LobbySlot` struct and `LobbyManager` logic on top of `SessionManager`.
- [x] 1.3 Refactor `GameManager` to handle networked match initialization based on mode.

## 2. Dynamic Lobby UI
- [x] 2.1 Update `MainMenu.uxml` with the 3 mode selection cards.
- [ ] 2.2 Create `Lobby.uxml` with slot list, team buttons, and bot toggle.
- [x] 2.3 Implement `LobbyController` to handle UI-Network synchronization.

## 3. Bot Management & AI
- [x] 3.1 Create `BotNameUtility` with a JSON list of 50+ Spanish names.
- [x] 3.2 Implement Host-side logic for bot filling and player replacement.
- [x] 3.3 Update `EnemyAgent` with `TeamID` and target selection logic (closest enemy).
- [x] 3.4 Implement friendly fire protection in `PlayerController` and `EnemyAgent`.

## 4. Visuals & Environment
- [ ] 4.1 Implement color-coded name tags (Blue/Red) in the HUD.
- [ ] 4.2 Update `MapGenerator` to support scaling based on the number of players.
- [ ] 4.3 Add winning/losing UI screens for Team/FFA matches.

## 5. Verification
- [ ] 5.1 Test 1vs1 with 1 Bot.
- [ ] 5.2 Test Team Duel (4vs4) with bots only.
- [ ] 5.3 Test player joining and replacing a bot.
