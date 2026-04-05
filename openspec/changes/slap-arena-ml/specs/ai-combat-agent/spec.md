## ADDED Requirements

### Requirement: Autonomous Combat AI
The enemy agent SHALL physically navigate the arena and attempt collisions using Reinforcement Learning rules.

#### Scenario: Agent avoids falling
- **WHEN** the agent approaches the arena edge
- **THEN** its trained policy applies forces to steer away to avoid a negative reward.

#### Scenario: Agent strikes player
- **WHEN** the agent aligns its physical "hands" with the human player
- **THEN** it executes a physical movement toward the player to maximize positive reward.
