## 1. Project Setup
- [x] 1.1 Install Unity Netcode for GameObjects and Unity Relay packages
- [x] 1.2 Install ML-Agents package and set up Python/PyTorch environment
- [x] 1.3 Configure UI Toolkit in the project
- [x] 1.4 Setup base physical arena scene with bounds

## 2. Multiplayer Core
- [x] 2.1 Implement Relay allocation and join code generation
- [x] 2.2 Create Host session logic
- [x] 2.3 Create Client join session logic with Relay code
- [x] 2.4 Synchronize player spawning and despawning

## 3. Physics & Combat
- [x] 3.1 Implement NetworkRigidbody for player character synchronization
- [x] 3.2 Add mouse click inputs (L/R) for slapping action
- [x] 3.3 Apply and synchronize directional slap forces via Netcode Server RPCs
- [x] 3.4 Implement ejection logic (detect when a player crosses arena bounds)

## 4. UI Toolkit Implementation
- [ ] 4.1 Create Main Menu UXML and USS
- [ ] 4.2 Bind "Create Session" button to multiplayer Host logic
- [ ] 4.3 Bind "Join Game" and input field for Relay code to Client logic
- [ ] 4.4 Handle smooth UI transitions between menu and gameplay scene

## 5. ML-Agents Integration
- [ ] 5.1 Define agent observations (position, opponent distance, arena edges)
- [ ] 5.2 Define agent actions (movement forces, simulated slaps)
- [ ] 5.3 Implement reward system (positive for touching/hitting opponent, extreme negative for falling off)
- [ ] 5.4 Train the agent model (PPO) using fast timescale
- [ ] 5.5 Integrate the final ONNX model into the enemy prefab for gameplay
