## 1. Sistema de Datos de Skins

- [x] 1.1 Crear el ScriptableObject `CharacterSkin` en `Assets/Scripts/Multiplayer/Skins/CharacterSkin.cs`.
- [x] 1.2 Crear una instancia de `CharacterSkin` para cada conejo (`Bald`, `Blond`, `Cyan`, `Green`, `Grey`).
- [x] 1.3 Configurar cada `CharacterSkin` con su Prefab visual y su Avatar correspondiente.

## 2. Refactorización del Manager

- [x] 2.1 Crear el nuevo script `CharacterSkinManager.cs` que reemplaza al viejo `CharacterVisualRandomizer`.
- [x] 2.2 Implementar la lógica de `ApplySkin` con instanciación de prefabs.
- [x] 2.3 Implementar la función de **Unificación de Jerarquía** (renombrado de huesos al vuelo).
- [x] 2.4 Integrar la sincronización de red con `NetworkVariable<int>`.

## 3. Preparación de Prefabs Visuales

- [x] 3.1 Crear un Prefab individual para cada conejo que sea puramente visual (sin el script anterior).
- [x] 3.2 Asegurarse de que todos los conejos estén en modo **Generic** rig y con sus animaciones asociadas.

## 4. Limpieza del Player y Enemy

- [x] 4.1 Eliminar el script `CharacterVisualRandomizer` de `PlayerPrefab` y `EnemyPrefab`.
- [x] 4.2 Eliminar los modelos hijos del prefab raíz.
- [x] 4.3 Añadir el nuevo `CharacterSkinManager` y configurar la lista de skins.

## 5. Verificación

- [ ] 5.1 Probar en modo Entrenamiento el cambio aleatorio.
- [ ] 5.2 Probar en multijugador la sincronización de las skins.
