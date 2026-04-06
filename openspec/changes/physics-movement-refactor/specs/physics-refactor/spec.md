# Physics Refactor Spec

Requirements for reactive and physical movement and collisions.

### Requirement: Reactive Collision Movement
The player's movement on the X and Z axes should respect external collisions (e.g., getting pushed by a punch or colliding with an enemy).

#### Scenario: Running into Enemy
- **WHEN** The player moves towards an enemy capsule.
- **THEN** The player stops or "slides" around the enemy rather than pushing them with infinite force.

#### Scenario: Getting Hit
- **WHEN** The enemy slaps the player.
- **THEN** The player's current movement velocity is pushed back on the XZ axes based on the slap force.

### Requirement: Frictionless Sliding
Combatants should not "stick" to each other or walls when rubbing against them.

#### Scenario: Gliding contact
- **WHEN** Two combatants rub against each other while moving.
- **THEN** They slide smoothly without friction-based stalling.

### Requirement: AI Proximity
AI should avoid overlapping perfectly with the player to reduce physical "fighting" for space.

#### Scenario: AI Chase Stop
- **WHEN** The AI gets within 1 meter of the player target.
- **THEN** The AI reduces its `moveSpeed` or stops its forward movement force.
