## Context

El juego carece de animaciones sincronizadas. Los personajes se deslizan en lugar de caminar y bofetadas no tienen feedback visual. Dado que todos los modelos de conejo (Rabbit) comparten el mismo rig, podemos unificar toda la lógica de animación en un único controlador.

## Goals / Non-Goals

**Goals:**
- Implementar un `Animator Controller` universal.
- Sincronizar el "Slap" y el "Speed" por red usando `NetworkAnimator`.
- Implementar un Blend Tree para transiciones suaves de caminar/correr.
- Soportar tanto al Jugador como a la IA (ML-Agents).

**Non-Goals:**
- Crear nuevas animaciones (usar los clips FBX actuales).
- Implementar animaciones secundarias de expresiones faciales o ropa.

## Decisions

### 1. Parámetros del Animator
- **`Speed`** (Float): Velocidad relativa para controlar el Blend Tree entre Idle y Run.
- **`SlapTrigger`** (Trigger): Lanzará la animación de ataque.
- **`IsFalling`** (Bool): Identificará si el personaje está flotando o en una caída.

### 2. Sincronización de Animación en Red
**Decisión**: Usar el componente `NetworkAnimator` de Netcode for GameObjects.
**Razón**: Facilita la sincronización de triggers entre servidor y clientes de manera eficiente.

### 3. Blend Tree 1D para Movimiento
**Decisión**: Usar un Blend Tree de 1 solo eje basado en la magnitud de la velocidad.
**Razón**: Evita el "foot sliding" al mezclar caminar y correr basado en la velocidad física real del Rigidbody.

## Risks / Trade-offs

- **[Riesgo]** → Latencia de red en las bofetadas.
- **Mitigación** → El script del cliente propietario lanzará la animación inmediatamente (Predicción local) mientras el Servidor sincroniza el trigger.
- **[Riesgo]** → Conflicto con el FBX (Rigging).
- **Mitigación** → Asegurarse de que los modelos FBX estén configurados como "Humanoid" o que compartan el mismo "Avatar" de animación.
