## Context

Slap Arena ML es un prototipo 1vs1 multijugador ("brawler" minimalista en 3D) en el que los jugadores usan manos físicas para expulsar al rival de la arena. El problema actual en este tipo de juegos es la IA: programar FSMs (Máquinas de Estados Finitos) para reflejar las físicas en tiempo real de los golpes es muy rígido y tedioso.

Para resolver esto, el proyecto integrará Unity Netcode for GameObjects con Unity Relay para la gestión de las sesiones (Host/Client), el nuevo UI Toolkit en Unity 2022.3 LTS para menús, y ML-Agents con PyTorch para que la IA actúe de manera emergente, evaluando en todo momento los bordes de la plataforma y el contacto con el oponente.

## Goals / Non-Goals

**Goals:**
- Implementar conectividad Host/Client sin necesidad de un servidor dedicado caro (usando Relay).
- Sincronizar las manos y el peso/empuje de los bofetones de forma fluida a través de la red.
- Entrenar un agente con ML-Agents que entienda de manera autónoma que debe mantenerse dentro de la arena y tocar al rival.
- Validar el flujo del usuario desde el Main Menu (UI Toolkit) a la sesión jugable mediante códigos de unión.

**Non-Goals:**
- No se creará Matchmaking automático (solo con código Host/Join).
- No habrá una campaña PVE compleja ni modos adicionales más allá del entrenamiento de este agente y el combate emergente.
- No se modelarán personajes con animaciones y rigging complejos.

## Decisions

- **Netcode for GameObjects + Relay vs Photon PUN2:** Se usará Netcode for GameObjects por ser la librería 1st party oficial en Unity. Para evitar port-forwarding se usará Unity Relay.
- **ML-Agents (PPO) vs FSM/Heuristics:** En lugar de codificar reglas fijas con FSM, dejaremos que el algoritmo de RL (PPO) recompense al agente por cercanía y contacto, castigando salir de la plataforma.
- **UI Toolkit vs Unity UI (Canvas):** Utilizaremos UI Toolkit por acercarnos al modelo CSS/Flexbox asegurando escalabilidad, evitando los cuellos de botella de RectTransforms.

## Risks / Trade-offs

- [Risk] Sincronización de físicas mediante red (Rigidbody Authority) → Mitigación: Usar NetworkRigidbody y aplicar Server Authority, o Client Authority compartida si la latencia es demasiado alta.
- [Risk] Tiempos largos de entrenamiento de ML-Agents → Mitigación: Configurar recompensas intermedias (Curriculum Learning) y entrenar en builds paralelos de entorno headless.
