# Design: Physics and Slap Consistency

## Goals / Non-Goals

**Goals:**
- **Equality**: Human and AI players must have the same movement properties (damping, friction).
- **Combat Symmetry**: Ensure the "Slap" ability works the same for everyone (range, radius, force).
- **Consistency**: Centralize the OOB (Out-of-Bounds) threshold at Y = -0.5f.
- **Netcode Ready**: Prepare detection logic to work with NetworkObjects correctly.

**Non-Goals:**
- Redesigning the entire movement system (keeping current Rigidbody-based movement).
- Implementing new game modes (this change is internal refactoring).

## Decisions

1. **Rigidbody Damping**: Both Player and AI will use `linearDamping = 0.1f` and `angularDamping = 10f`.
2. **Unified Slap detection**:
   - `slapDistance = 4.0f`
   - `slapRadius = 2.0f`
   - `slapForce = 50f` (as currently in player, but confirmed for AI).
   - Both will use `Physics.OverlapSphere` at a point in front of the character.
3. **Threshold Synchronization**: All code checking for "falling" will use a shared constant or match the value `-0.5f`.
4. **Targeting Mechanism**: Replace tag-based checks in AI with LayerMask checks (detecting both Layer 3 "Player" and Layer 6/7/etc "Enemy").

## Risks / Trade-offs

- **AI Retraining**: The AI will need to adapt to the lower damping (0.1f vs 7f). It might "over-drift" initially until the training loop compensates.
- **OOB Sensitivity**: Y = -0.5f is very close to the 1.1f spawn point. If the arena platform is thin, the agent might trigger OOB while just moving near the edge.
