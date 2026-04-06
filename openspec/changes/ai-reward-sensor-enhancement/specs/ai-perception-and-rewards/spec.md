# AI Perception and Rewards Spec

Requirements for improving the ML-Agent's ability to learn through enhanced environmental sensing and a densified reward structure.

### Requirement: Edge Detection
The AI must be able to visually perceive the edge of the arena platform to avoid falling off accidentally.

#### Scenario: Approaching Edge
- **WHEN** The agent moves towards the edge of the platform and there is no floor directly in front of it.
- **THEN** The agent receives visual raycast data indicating a lack of ground, allowing it to learn to brake or turn away.

### Requirement: Densified Reward Signals
The AI must receive intermediate rewards to guide it towards the ultimate goal of hitting the opponent, minimizing the impact of the time penalty logic loop.

#### Scenario: Moving Closer
- **WHEN** The agent reduces its distance to the opponent.
- **THEN** The agent receives a small, continuous positive reward based on proximity to encourage engagement.

#### Scenario: Safe Positioning
- **WHEN** The agent remains on the platform (especially centrally) and avoids falling.
- **THEN** The agent receives a small positive reward (or less negative penalty) to counter the intrinsic "suicide to end penalty" behavior.

### Requirement: Deterrent Fall Penalty
The penalty for falling off the arena must outweigh any accumulated time penalties to prevent the agent from terminating the episode intentionally just to stop the clock.

#### Scenario: Falling Off
- **WHEN** The agent's Y position falls below the acceptable threshold (arena floor).
- **THEN** The agent receives a severe negative reward (-1.0 or more) that heavily dissuades this outcome, regardless of the time elapsed.
