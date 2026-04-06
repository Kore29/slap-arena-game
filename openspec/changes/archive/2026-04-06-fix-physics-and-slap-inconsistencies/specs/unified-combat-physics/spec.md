# Spec: Unified Combat Physics

## Unified Entity Physics

Combatants must behave consistently to ensure fair gameplay and effective AI training.

### Requirement: Global Damping Alignment
All entities (Players and AI) must use identical Rigidbody damping settings.

#### Scenario: Movement Feel
- **WHEN** any entity moves on the platform
- **THEN** its linear damping MUST be 0.1f
- **AND** its angular damping MUST be 10.0f

### Requirement: Symmetric Slap Mechanics
The "Slap" ability must be identical for all combatants.

#### Scenario: Slap Detection
- **WHEN** an entity performs a slap
- **THEN** it should use a detection radius of 2.0f
- **AND** a maximum distance of 4.0f
- **AND** apply an impulse force of 50.0f

### Requirement: Standardized Out-of-Bounds
Detection of falling must occur at a unified vertical threshold.

#### Scenario: Boundary Crossing
- **WHEN** an entity's Y-coordinate falls below -0.5f
- **THEN** the entity must be considered out of bounds
- **AND** trigger the appropriate reset logic (Respawn or End Episode)
