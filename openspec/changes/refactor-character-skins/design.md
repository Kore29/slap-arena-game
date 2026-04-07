# Design: Professional Skin System

## Problem Statement
El sistema actual falla porque las animaciones "Generic" de Unity usan rutas de texto fijas. Si un modelo tiene un hueso raíz llamado `Armature` y otro `CharacterArmature`, el Animator no encuentra los huesos. El sistema actual intenta renombrar el padre, pero no los huesos internos.

## Proposed Solution
Migraremos a un sistema basado en datos (ScriptableObjects) que incluya un paso de **Unificación de Jerarquía** en tiempo de ejecución.

### Components

#### 1. CharacterSkin (ScriptableObject)
- `string skinName`
- `GameObject prefab`: El modelo FBX (sin Animator propio).
- `Avatar avatar`: El Avatar Generic correspondiente.
- `Material[] materialOverrides`: (Opcional) Para variaciones de color sin nuevos modelos.

#### 2. SkinManager (MonoBehaviour / NetworkBehaviour)
- `List<CharacterSkin> availableSkins`
- `NetworkVariable<int> currentSkinIndex`
- `void ApplySkin(int index)`:
    - Desactiva/Destruye el visual anterior.
    - Instancia el nuevo `prefab`.
    - Llama a `UnifyHierarchy(instantiatedObject)`.
    - Re-vincula el `Animator.avatar` y llama a `Rebind()`.

#### 3. HierarchyUnification (Utility)
- Función que identifica el primer nodo de huesos y lo renombra a un estándar (ej: `CharacterArmature`) para que coincida con las rutas del `AnimatorController`.

## Data Flow
1. Server cambia `currentSkinIndex`.
2. Todos los clientes reciben el evento `OnValueChanged`.
3. Cada cliente ejecuta `ApplySkin`.
4. El visual se actualiza y las animaciones "Generic" encuentran sus rutas gracias al renombrado.

## Infrastructure
- Carpeta: `Assets/Scripts/Multiplayer/Skins/`
- Carpeta: `Assets/Data/Skins/`
