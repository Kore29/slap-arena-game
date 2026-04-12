# Tasks: Fix Map Generator & Scene Sync

## 1. Scene Management (GameManager)
- [ ] 1.1 Replace `SceneManager.LoadScene("PlatformArena")` with `NetworkManager.Singleton.SceneManager.LoadScene(...)` in `StartNetworkedGame`.
- [ ] 1.2 Add validation check for `currentModeData` before initiating networked scene load.

## 2. Map Generation (MapGenerator)
- [ ] 2.1 Modify `ServerSyncRoutine` to call `CleanAndGenerate` immediately on Host.
- [ ] 2.2 Add extra Debug logs in `CleanAndGenerate` to trace scale and seed values in real-time.
- [ ] 2.3 Ensure `CleanAndGenerate` is protected against null `currentModeData`.

## 3. UI Improvements (WorldNameTag)
- [ ] 3.1 Remove `Invoke` call in `OnNetworkSpawn`.
- [ ] 3.2 Implement reliable initialization using `TeamMember.teamId.OnValueChanged`.
- [ ] 3.3 Add immediate visual update in `OnNetworkSpawn` for late-joiners.

## 4. Verification
- [ ] 4.1 Test Host Room creation.
- [ ] 4.2 Test Client Room join.
- [ ] 4.3 Verify terrain identical on both.
