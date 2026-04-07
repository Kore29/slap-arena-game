## Why

Varios modelos de personajes (como Rabbit_Grey) no se animan correctamente debido a inconsistencias en la jerarquía de huesos, conflictos en la reinicialización del Animator y ejecuciones duplicadas de corrutinas en red. Este cambio es necesario para asegurar que todos los skins visuales funcionen de forma fluida y consistente tanto en modo local como multijugador.

## What Changes

- **Modificación de CharacterVisualRandomizer.cs**: Corregir la lógica de sincronización para evitar disparos dobles de corrutinas en el cliente y asegurar un reset limpio del Animator.
- **Normalización de Jerarquías**: Renombrar los objetos visuales hijos en los prefabs PlayerPrefab y EnemyPrefab para asegurar rutas de animación universales.
- **Corrección de Avatars**: Ajustar la configuración de los Avatars (especialmente Rabbit_Grey) para eliminar la rigidez de nombres de nodos raíz.
- **Sincronización de Animator**: Asegurar que los parámetros del Animator persistan tras el Rebind() dinámico.

## Capabilities

### New Capabilities
- `character-animation-retargeting`: Sistema para asegurar que cualquier modelo visual de conejo pueda usar el AnimatorController universal sin fallos de ruta.

### Modified Capabilities
<!-- Dejar vacío si no hay cambios en requerimientos de specs existentes -->

## Impact

- **Assets/Scripts/Multiplayer/CharacterVisualRandomizer.cs**: Cambio principal en la lógica de swap visual.
- **Assets/Prefabs/PlayerPrefab.prefab**: Cambios en la jerarquía y nombres de objetos hijos.
- **Assets/Prefabs/EnemyPrefab.prefab**: Cambios en la jerarquía y nombres de objetos hijos.
- **Assets/Models/Rabbits/*.fbx.meta**: Posibles ajustes en la configuración de importación de Avatars.
