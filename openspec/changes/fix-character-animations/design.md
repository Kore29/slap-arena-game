## Context

Actualmente, el cambio de modelo visual en los personajes (`CharacterVisualRandomizer.cs`) es inestable. En red, se disparan múltiples actualizaciones simultáneas del Animator, y la falta de una jerarquía de nombres estandarizada en los prefabs impide que las animaciones encuentren los huesos correctos. Además, el uso de `Avatar` en modo `Generic` con nombres de raíz específicos (como en `Rabbit_Grey`) agrava el problema.

## Goals / Non-Goals

**Goals:**
- Asegurar que el cambio de skin visual sea atómico y no se dispare por duplicado.
- Estandarizar los nombres de los objetos visuales hijos en los prefabs (`PlayerPrefab` y `EnemyPrefab`).
- Garantizar que los parámetros del `Animator` se restauren correctamente tras un `Rebind()`.
- Eliminar la rigidez de nombres de raíz en los archivos `.fbx.meta`.

**Non-Goals:**
- No se cambiarán las animaciones en sí (los clips FBX).
- No se modificará el `AnimatorController` universal, solo su vinculación dinámica.

## Decisions

- **Control de Corrutina Única**: En `CharacterVisualRandomizer.cs`, usaremos una referencia a la corrutina actual para detener la anterior si se inicia una nueva (`StopCoroutine`). Además, eliminaremos la llamada manual en `OnNetworkSpawn` si ya se ha suscrito al evento.
- **Normalización a "Visuals"**: Renombraremos todos los modelos de conejos dentro de los prefabs a un nombre común (ej. "Visuals"). Esto permitirá que el Animator encuentre siempre la ruta `Visuals/CharacterArmature/...` independientemente del skin activo.
- **Cache de Parámetros**: Antes de llamar a `Rebind()`, capturaremos los valores actuales de `Speed` e `IsFalling` para re-aplicarlos inmediatamente después, evitando que el personaje se quede "congelado" en Idle.
- **Limpieza de Meta-Files**: Editaremos los `.fbx.meta` para dejar la sección `skeleton` vacía, forzando a Unity a usar un mapeo flexible basado en la jerarquía real del objeto.

## Risks / Trade-offs

- **[Riesgo]** → El renombrado de objetos en los prefabs podría romper referencias en otros scripts.
- **[Mitigación]** → `CharacterVisualRandomizer` ya usa un array de referencias (`visuals`), por lo que el nombre del objeto no debería afectar a la lógica de activación, solo a las rutas internas del `Animator`.
- **[Riesgo]** → `NetworkAnimator` puede perder la sincronización si el `Animator` se deshabilita momentáneamente.
- **[Mitigación]** → Mantendremos el tiempo de deshabilitación al mínimo absoluto (un frame) y usaremos `Update(0)` para forzar un refresco inmediato tras habilitarlo.
