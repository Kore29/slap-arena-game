## Context

El juego actual utiliza un cilindro y un plano básico como prototipo. Queremos transicionar a una estética Low-Poly coherente con personajes coleccionables que se eligen al azar en cada partida multijugador para mejorar el "feel" del juego.

## Goals / Non-Goals

**Goals:**
- Implementar un sistema de cambio de modelo (skin) aleatorio y sincronizado en Red.
- Integrar el nuevo mapa de "Floating Islands" manteniendo el gameplay de empuje.
- Actualizar el Skybox para una atmósfera estilizada (Lush/Vibrant).
- Proveer instrucciones paso a paso para que el usuario importe los assets manualmente.

**Non-Goals:**
- Implementar un sistema de progresión o desbloqueo de skins (por ahora es solo azar).
- Crear animaciones nuevas (usamos las existentes de los modelos "Rabbit").

## Decisions

### 1. Sistema de "Visual Switcher" en el Prefab
**Decisión**: En lugar de instanciar diferentes prefabs, el `PlayerPrefab` contendrá todos los modelos como hijos. Un script `CharacterVisualRandomizer.cs` activará solo uno basado en una `NetworkVariable<int>`.
**Razón**: Facilita enormemente el mantenimiento de scripts (PlayerController, EnemyAgent) y evita tener que registrar 4 prefabs distintos en el `NetworkManager`.

### 2. Sincronización vía NetworkVariable
**Decisión**: Usar una `NetworkVariable<int>` llamada `characterIndex`.
**Razón**: Netcode for GameObjects sincroniza automáticamente las variables entre Server y Clientes, asegurando que si yo veo un Pato como mi personaje, el resto de jugadores también me vea como un Pato.

### 3. Mapa Estático (Modular Islands)
**Decisión**: Montar las islas modulares como un bloque único estático en la escena `PlatformArena`.
**Razón**: Simplicidad técnica inicial centrada en la estética antes de añadir plataformas móviles.

## Risks / Trade-offs

- **[Riesgo]** → Los modelos podrían tener escalas diferentes.
- **Mitigación** → Ajustaremos la escala local de cada modelo hijo para que el `CapsuleCollider` base les sirva a todos por igual.
- **[Riesgo]** → Conflictos con el Rig/Animations.
- **Mitigación** → Usar el `Animator` en el nodo raíz y asignar el `Avatar` correspondiente si es necesario, aunque al ser el mismo rig debería ser plug-and-play.
