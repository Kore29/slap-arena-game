# Tasks: Physics and Slap Force Alignment

## 1. Physics Damping Refactor

- [x] 1.1 Update `PlayerController.cs`: Set linear damping to 0.1f and angular damping to 10f in `Awake`.
- [x] 1.2 Update `EnemyAgent.cs`: Align linear damping (0.1f) and angular damping (10f) in `Initialize`.
- [x] 1.3 Audit manual friction logic in both scripts to ensure they don't fight the new damping values.

## 2. Slap Mechanism Synchronization

- [x] 2.1 Update `PlayerController.cs`: Set `slapRadius = 2.0f` and `maxSlapDistance = 4.0f`.
- [x] 2.2 Update `EnemyAgent.cs`: Set `slapRadius = 2.0f`.
- [x] 2.3 Update `EnemyAgent.cs`: Modify `ExecuteSlap` to use LayerMask detection instead of `CompareTag("Player")`.
- [x] 2.4 Verify `slapForce` is consistent (50f) in both prefabs/scripts.

## 3. Out-of-Bounds Normalization

- [x] 3.1 Update `PlayerController.cs`: Change Y threshold for `Respawn()` from -7.0f to -0.5f.
- [x] 3.2 Update `EnemyAgent.cs`: Change Y threshold for `EndEpisode()` from -3.0f to -0.5f.
- [x] 3.3 Audit `EjectionZone.cs`: Ensure it doesn't conflict with these code-based checks.

## 4. Verification & Testing

- [ ] 4.1 Test Local Practice mode: Verify player movement and slap feel.
- [ ] 4.2 Test AI behavior: Ensure it doesn't spin out of control with lower damping.
- [ ] 4.3 Verify Network Spawn: Ensure changes don't break Netcode synchronization.
