# Gameplay HUD Spec

Functional requirements for the In-Game HUD and Feedback system.

### Requirement: Cooldown Visualization
Display the remaining cooldown time for the slap ability to the player.

#### Scenario: Slap Cooldown Ready
- **WHEN** The slap cooldown has fully elapsed.
- **THEN** The HUD cooldown bar is full (100%).

#### Scenario: Slap Cooldown Active
- **WHEN** The player performs a slap.
- **THEN** The HUD cooldown bar resets to 0% and fills over 0.5s.

### Requirement: Hit Confirmation
Provide visual/textual feedback when a slap successfully hits an opponent Rigidbody.

#### Scenario: Hit Confirmed
- **WHEN** The `SphereCast` in `PlayerController` detects a valid target.
- **THEN** A "HIT!" message or log appears in the HUD for 1.5s.

### Requirement: Physics Impact
Ensure the physical feedback of a slap is satisfying and noticeable.

#### Scenario: Impact Applied
- **WHEN** A slap hits an opponent.
- **THEN** The opponent receives a `slapForce` impulse that moves them at least 2 meters.
