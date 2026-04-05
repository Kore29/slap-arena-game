## ADDED Requirements

### Requirement: Physical Slap Replication
Player combat actions SHALL apply simulated forces synchronously across the network.

#### Scenario: Slap Registration
- **WHEN** the player clicks L/R mouse buttons near an opponent
- **THEN** a directional physics force is applied to the opponent's Rigidbody and networked to all clients.

#### Scenario: Ejection
- **WHEN** a player's Rigidbody is pushed beyond the arena bounds
- **THEN** they fall out and the combat logic triggers an elimination event.
