# Slap Arena ML

## Project Overview
Slap Arena ML is an experimental 1v1 multiplayer brawler game developed within Unity. The core objective of the project is to replace traditional, rigid Finite State Machines (FSMs) for artificial intelligence with emergent behaviors driven by Reinforcement Learning. Players engage in physics-heavy combat, using synchronized directional forces to push opponents off a bounded platform. 

## Technical Architecture
The system is built upon three primary pillars:
- **Core Engine:** Unity 2022.3 LTS utilizing the Universal Render Pipeline (URP).
- **Network Synchronization:** Unity Netcode for GameObjects combined with Unity Relay allowing seamless host-client connectivity. This structure bypasses complex port-forwarding constraints while ensuring physics interactions are properly authenticated via Server RPC operations.
- **Machine Learning Integration:** Unity ML-Agents toolkit executing the Proximal Policy Optimization (PPO) algorithm. The AI models spatial awareness by evaluating proximity to edges to prevent boundary ejection while optimizing contact alignment with the opposing entity.

## Core Mechanics
1. **Dynamic Physics Combat:** Player inputs trigger spatial overlap checks resulting in localized physical impulses. Forces are propagated symmetrically across all connected clients utilizing NetworkRigidbody structures to maintain systemic consistency.
2. **Bounds Elimination:** An architectural out-of-bounds trigger zone constantly evaluates coordinate bounds. It actively detects player displacement out of the arena domain to instantly govern match status and spawn logic.
3. **Scalable Interface:** Transition and connection flows are orchestrated via the modern UI Toolkit framework, separating structural layout logic into robust UXML and USS definitions.

## Installation and Setup
Ensure the development environment is properly configured prior to execution:
1. Open the project utilizing Unity 2022.3 LTS.
2. Confirm the local Python virtual environment is successfully instantiated under the `/venv` directory and the required dependencies have been resolved.
3. Ensure your local Unity setup is linked to your Unity Gaming Services Dashboard and the Relay service is activated.
4. Launch `Assets/Scenes/PlatformArena.unity` to inspect the underlying geometric foundation.

## Roadmap and Current Status
The environment currently features a stabilized networking substrate with foundational Netcode and Relay protocols active. Server-authorized interaction physics and autonomous EjectionZone checks have been securely integrated into the player instances. The next phase introduces the visual menu interface for room connections and the final programmatic training scenarios for the opponent network model.
