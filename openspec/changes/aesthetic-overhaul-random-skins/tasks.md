## 1. Importación y Preparación (USUARIO)

- [ ] 1.1 Descargar e Importar los modelos Rabbit (Duck, Ghost, Pinguin, Slime) desde Poly Pizza.
- [ ] 1.2 Descargar e Importar el Skybox Extended Shader desde el Asset Store.
- [ ] 1.3 Descargar e Importar el mapa Modular Floating Islands.

## 2. Desarrollo del Sistema Visual (C#)

- [x] 2.1 Crear el nuevo script `Assets/Scripts/Multiplayer/CharacterVisualRandomizer.cs`.
- [x] 2.2 Implementar el sistema de sincronización con `NetworkVariable<int>`.
- [x] 2.3 Añadir lógica de activación/desactivación de modelos basada en el índice.

## 3. Configuración de Prefabs (USUARIO & AI)

- [x] 3.1 Añadir los 4 modelos Rabbit como hijos del `PlayerPrefab`.
- [x] 3.2 Añadir el script `CharacterVisualRandomizer` al `PlayerPrefab`.
- [x] 3.3 Asignar los modelos hijos a la lista del script `CharacterVisualRandomizer`.
- [x] 3.4 Hacer lo mismo para el `AIAgentPrefab` (Enemy).

## 4. Estética de Escena (USUARIO)

- [ ] 4.1 Reemplazar el suelo actual en la escena `PlatformArena` por las Islas Flotantes.
- [x] 4.2 Crear un Material utilizando el "Extended Skybox Shader" y asignarlo en `Window > Rendering > Lighting Settings`.
- [x] 4.3 Ajustar la posición de muerte (`Y < -10f`) en `PlayerController.cs` y `EnemyAgent.cs`.

## 5. Verificación Final

- [ ] 5.1 Probar en modo servidor: ¿Se asigna un personaje al azar?
- [ ] 5.2 Probar con 2 clientes: ¿Ambos ven al otro con el personaje correcto?
- [ ] 5.3 Verificar que las animaciones de "Slap" siguen funcionando en los nuevos modelos.
