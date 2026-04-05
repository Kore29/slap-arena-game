## Why

En el desarrollo de videojuegos multijugador, crear una experiencia equilibrada y desafiante para un solo jugador suele requerir programar comportamientos de IA complejos mediante máquinas de estados finitos (FSM), lo cual es rígido y predecible. 

Slap Arena ML resuelve esto utilizando Machine Learning para que el enemigo genere comportamientos emergentes, aprendiendo de sus propios errores en un entorno físico dinámico, eliminando la necesidad de programar cada movimiento de la IA manualmente y proporcionando una experiencia reactiva.

## What Changes

- Implementación del sistema de multijugador básico con Unity Netcode for GameObjects y Unity Relay.
- Integración de ML-Agents con PyTorch en Unity 2022.3 LTS (URP) para el entrenamiento de IA mediante PPO.
- Creación del sistema de combate "brawler" basado en físicas (inercia, peso de bofetones, colisiones de expulsión).
- Construcción de la interfaz de usuario con UI Toolkit para navegación de menú y conexión.

## Capabilities

### New Capabilities
- `multiplayer-session`: Sistema de conexión Host/Client usando Unity Netcode y Relay para emparejar jugadores mediante código de sala.
- `ai-combat-agent`: Agente enemigo entrenado con Reinforcement Learning (PPO) que evalúa peligros (bordes de arena) y oportunidades de contacto.
- `physics-combat-system`: Sistema de combate gestionado por física reactiva y sincronización en red bidireccional (ratón L/R bofetones, WASD movimiento).
- `ui-toolkit-interface`: Interfaz de usuario limpia e inicio y flujo de partida controlados puramente mediante UI Toolkit.

### Modified Capabilities
<!-- No modified capabilities as this is a new implementation -->

## Impact

Este proyecto establece el marco principal (backend multijugador y de físicas). Introducirá dependencias críticas como Unity.Netcode.GameObjects, ML-Agents de unity y UI Toolkit para los menús. Las físicas de red deberán configurarse para autorizar correctamente y sincronizar las inercias en la arena para asegurar el objetivo principal técnico del combate 1v1 fluído.
