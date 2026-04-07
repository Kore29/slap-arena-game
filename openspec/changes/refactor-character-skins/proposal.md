## Why

El sistema actual de `CharacterVisualRandomizer` es frágil y poco escalable. Depende de renombrar objetos manualmente a `"CharacterModel"` y falla cuando los modelos tienen jerarquías de huesos distintas (ej: `CharacterArmature` vs `Armature`). Esto causa que las animaciones dejen de funcionar al cambiar de skin. Necesitamos un sistema profesional, basado en datos, que unifique cualquier modelo de bicho automáticamente.

## What Changes

- **Eliminación** del sistema de arrays directos en el player prefab.
- **Creación** de un sistema de `CharacterSkin` (ScriptableObjects) para definir visuales y sus parámetros.
- **Implementación** de un `SkinManager` robusto que maneja la instanciación y la unificación de jerarquía por código (Runtime Bone Renaming).
- **Sincronización** mejorada con Netcode for GameObjects.

## Capabilities

### New Capabilities
- `character-skins`: Sistema modular para gestionar, cargar y sincronizar skins de personajes con diferentes esqueletos.

### Modified Capabilities
- `multiplayer-core`: Ajustes en la sincronización de visuales para usar el nuevo gestor.

## Impact

- `Assets/Scripts/Multiplayer/CharacterVisualRandomizer.cs`: Será reemplazado.
- `Assets/Prefabs/PlayerPrefab.prefab` y `EnemyPrefab.prefab`: Requieren limpieza de los modelos "hardcoded".
- `Assets/Scripts/Data/`: Nuevo directorio para los ScriptableObjects de skins.
