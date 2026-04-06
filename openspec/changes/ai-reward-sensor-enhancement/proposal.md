# AI Reward and Sensor Enhancement

Enhance the AI's training efficiency by providing better environmental perception and a more granular reward system.

## Problem

The agent is currently failing to learn effectively (mean reward around -1.4). The primary issues are:
1.  **Lack of Perception**: The agent doesn't see the edges of the platform, leading to frequent accidental falls.
2.  **Reward Sparsity**: The agent only receives rewards for hitting the opponent, which is a rare event during early random exploration. Without intermediate goals, it lacks direction.
3.  **Suicidal Behavior**: Due to the time penalty (-0.001 per step), the agent learns that falling off (-1.0) is better than staying alive and doing nothing, which ends the episode and resets the penalty.

## What Changes

- Add **RayPerceptionSensorComponent3D** to the Enemy prefab to detect the edge and the opponent visually.
- Update `EnemyAgent.cs` to include:
    - **Proximity Reward**: Small continuous reward for being near the target.
    - **Safety Reward**: Reward for staying near the center of the platform.
    - **Death Penalty Tuning**: Adjust the fall penalty to be more significant compared to the time penalty.

## Capabilities

### New Capabilities
- `ai-perception-and-rewards`: Standardized sensing and incentive architecture for ML-Agents.

## Impact

- `EnemyAgent.cs`: Implement reward logic in `Update` or `OnActionReceived`.
- Prefabs: Add Raycast sensors.
- `SlapArenaTrainer.yaml`: Potentially adjust hyperparameters if needed.
