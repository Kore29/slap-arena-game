## 1. Local Mode Architecture
- [ ] 1.1 Modify `PlayerController.cs` for Netcode independence
- [ ] 1.2 Implement `LocalPlayerPrefab` (copy player without NetworkObject if needed)
- [ ] 1.3 Create the `GameManager.cs` singleton for spawning control

## 2. Main Menu Overhaul
- [ ] 2.1 Update `MainMenu.uxml` to include new "Local Practice" and "Coop" buttons
- [ ] 2.2 Re-style menu buttons in `MainMenu.uss` if necessary
- [ ] 2.3 Connect "Local Practice" button to `GameManager.StartLocalGame()`

## 3. Game Mode Integration
- [ ] 3.1 Implement "1v1 VS AI" (Single Player Mode)
- [ ] 3.2 Implement "2v2 COOP" (2 Players + 2 AIs in Relay Session)
- [ ] 3.3 Ensure elimination logic (`EjectionZone`) resets local practice session correctly
