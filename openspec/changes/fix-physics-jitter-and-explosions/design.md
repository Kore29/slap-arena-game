## Context

El sistema actual intenta forzar una velocidad objetivo en cada frame usando `ForceMode.VelocityChange`. Esto ignora la masa y crea impulsos infinitos cuando dos objetos con este sistema chocan, resultando en una "explosión física" que lanza a los personajes fuera del mapa. Además, la desincronización de damping entre el código y el Inspector causa jitter en red.

## Goals / Non-Goals

**Goals:**
- Eliminar las explosiones físicas al chocar entre agentes o con el jugador.
- Estabilizar el movimiento en red eliminando el jitter originado por configuraciones contradictorias.
- Mantener la respuesta táctil de las bofetadas pero con límites de seguridad.

**Non-Goals:**
- Rediseñar el sistema de entrenamiento de ML-Agents (solo se ajustan las fuerzas).
- Cambiar la lógica de victoria/derrota.

## Decisions

### 1. Cambio a ForceMode.Acceleration para Movimiento
**Razón**: `VelocityChange` es demasiado agresivo para colisiones. `Acceleration` permite que el motor de física mantenga la masa en la ecuación, suavizando el impacto inicial de los choques.
**Alternativa**: Usar `Force`, pero requiere calcular la masa manualmente para obtener la misma respuesta en IA y Jugador.

### 2. Implementación de Velocidad Terminal (Clamping)
**Razón**: Para evitar que un golpe envíe a alguien al "infinito", limitaremos la magnitud de la velocidad en el Rigidbody después de aplicar fuerzas.

### 3. Sincronización de Inspector y Código
**Razón**: Actualmente el código sobreescribe el damping. Eliminaremos estas líneas para que el "Source of Truth" sea el Inspector, facilitando el tunning sin recompilar.

### 4. Modo de Colisión Continuous Dynamic
**Razón**: Cambiar de `Discrete` a `ContinuousDynamic` evita que los personajes se "atraviesen" a altas velocidades, lo cual es la causa raíz de las explosiones de presión física.

## Risks / Trade-offs

- **[Riesgo]**: El movimiento podría sentirse menos "instantáneo" al cambiar a aceleración.
- **[Mitigación]**: Ajustaremos los valores de `moveSpeed` y la fuerza de aceleración en el Inspector para compensar.
- **[Riesgo]**: Mayor consumo de CPU por usar colisión continua.
- **[Mitigación]**: Solo se aplicará a los prefabs de los personajes (IA y Jugador), no a todo el escenario.
