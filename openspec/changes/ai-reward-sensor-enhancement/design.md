# AI Perception and Rewards Design

Technical implementation details for adding sensors and densifying the reward model for the ML-Agent.

## Problem

The agent is currently failing to learn because it lacks the sensory input to detect the platform edge, and its reward signal is too sparse (only rewarded on a successful hit). Combined with a heavy time penalty, the agent quickly learns to "suicide" by falling off the edge to end the episode and stop the negative reward accumulation.

## Goals / Non-Goals

**Goals:**
- Provide edge detection capabilities without manual raycast coding (using ML-Agents components).
- Balance the reward structure to incentivize engagement and survival over immediate suicide.
- Keep the state space size manageable.

**Non-Goals:**
- Completely overhauling the neural network architecture.
- Implementing complex pathfinding.

## Decisions

- **Perception Component**: We will add a `RayPerceptionSensorComponent3D` to the `EnemyPrefab`.
  - Tags to detect: `Player`
  - Rays: 5 rays spread across 90 degrees.
  - This allows the agent to "see" the player and obstacles without needing to calculate perfect vectors. We can't easily detect the "edge" with tags if there's no wall. Instead of relying purely on rays for edge detection (since there are no walls), we will penalize distance from the center.

- **Reward Tuning in `EnemyAgent.cs`**:
  1.  **Reduce Time Penalty**: Reduce the step penalty from `-0.001f` to `-0.0002f`. This gives the agent 5000 steps (instead of 1000) before reaching -1.0, making survival more viable.
  2.  **Center-Bias Reward**: Calculate distance from `Vector3.zero` (the center of the platform). If distance is `< 2.5f`, give a tiny positive reward (e.g., `+0.0001f`), effectively neutralizing the time penalty when safe.
  3.  **Proximity Reward**: In `OnActionReceived`, if `Vector3.Distance(transform.position, targetOpponent.position) < 3.0f`, `AddReward(0.0005f)`. This acts as "bread crumbs" leading the agent to the player.
  4.  **Death Penalty**: Increase the falling penalty conceptually. Since `-1.0f` is standard, we must ensure `SetReward(-2.0f)` is used upon falling to clearly demarcate it as the worst possible outcome.

## Risks / Trade-offs

- **Reward Hacking**: By giving rewards for proximity, the agent might learn to simply stand next to the player without ever slapping, just to harvest proximity points. We must keep the slap reward (`+1.0f`) significantly higher than the accumulated proximity rewards.
- **Sensor Observation Space**: Adding a RayPerception sensor changes the observation size. The PPO model (`.onnx`) will be invalidated, and a completely fresh training run will be required. This is acceptable since current training is ineffective.
