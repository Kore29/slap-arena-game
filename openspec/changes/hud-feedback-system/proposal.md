# HUD Feedback System and Slap Physics Tuning

Provide visual confirmation of hits and cooldown state, and tune slap physics for better gameplay feel.

## Problem

Currently, players lack visual feedback when a slap connects or during the cooldown period. This makes it difficult to determine if a missed hit is due to poor aim, range issues, or physics balancing. The user reported that "the enemy doesn't move back much," suggesting a need for both better visual cues and potential physics adjustment.

## What Changes

- Add a Gameplay HUD with a dynamic cooldown bar.
- Implement a "Hit Log" or on-screen notification system for confirmed hits.
- Increase the impulse force applied to enemies upon a successful slap.

## Capabilities

### New Capabilities
- `gameplay-hud`: A screen-space UI that tracks and displays real-time player states (cooldown, hit confirmations).

### Modified Capabilities
- None

## Impact

- `PlayerController.cs`: Updated to trigger UI events and increased `slapForce`.
- `GameHUD.uxml/uss`: New UI assets.
- `GameHUD.cs`: New controller for UI logic.
