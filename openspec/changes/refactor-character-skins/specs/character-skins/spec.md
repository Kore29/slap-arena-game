# Capability: character-skins

## Context
El sistema debe permitir cambiar el avatar visual de un jugador o enemigo en tiempo de ejecución, sincronizándose en red y asegurando que las animaciones "Generic" siempre encuentren sus huesos, independientemente de si el modelo original los llama `Armature`, `CharacterArmature` o de otra forma.

## Requirements

### Requirement: Visual Synchronization
El índice de la skin debe sincronizarse entre todos los clientes para que cada jugador vea el aspecto correcto de los demás.

#### Scenario: Skin Initialized on Join
- **WHEN** un nuevo jugador se une a una partida
- **THEN** debe cargar el visual correspondiente al `skinIndex` actual del resto de jugadores.

### Requirement: Animation Reliability
Las animaciones deben reproducirse correctamente sin importar el nombre del hueso raíz del modelo FBX.

#### Scenario: Dynamic Hierarchy Unification
- **WHEN** se instancia un modelo con un nombre de hueso raíz distinto (ej: `Armature`)
- **THEN** el sistema debe renombrar ese hueso al estándar esperado por el Animator (ej: `CharacterArmature`) antes de activar las animaciones.
