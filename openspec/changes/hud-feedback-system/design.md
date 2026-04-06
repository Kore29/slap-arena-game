# HUD Feedback System Design

Architecture for the heads-up display and combat feedback integration.

## Problem

Players cannot currently distinguish between a range issue (being too far), an aim issue (not looking at the target), or a physics balancing issue (the slap force being too weak).

## Goals / Non-Goals

**Goals:**
- Provide clear visual feedback for slap cooldown.
- Alert the player when a hit is registered.
- Enhance the physical "punch" of the slap.

**Non-Goals:**
- Multiplayer scoreboards (out of scope for now).
- Sound effects (will use existing systems if any, but not adding new ones here).

## Decisions

- **UI System**: Use `UIDocument` (UIToolkit) for the HUD to match the Main Menu.
- **HUD Script**: Create a `GameHUD` MonoBehaviour that listens to `PlayerController`.
- **Slap Force**: Increase `slapForce` from `15f` to `25f` - `30f` to ensure the "punch" is visceral.
- **Cooldown Bar**: A horizontal progress bar in the bottom-center of the screen.

## Risks / Trade-offs

- **Performance**: High-frequency UI updates for the cooldown bar - will use `FixedUpdate` or `Update` only on the owner.
- **Visual Clutter**: Keeping logs brief to avoid obstructing the view.
