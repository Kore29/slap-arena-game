## 1. Modificación de Lógica de Sincronización

- [x] 1.1 Modificar `CharacterVisualRandomizer.cs` para evitar ejecuciones dobles en clientes eliminando la llamada redundante en `OnNetworkSpawn`.
- [x] 1.2 Implementar control de corrutinas (`StopCoroutine`) en `CharacterVisualRandomizer.cs` para que solo una actualización visual esté activa a la vez.
- [x] 1.3 Implementar cache y restauración de parámetros del `Animator` (`Speed`, `IsFalling`) durante el proceso de `Rebind()`.

## 2. Normalización de Prefabs

- [x] 2.1 Renombrar todos los objetos hijos visuales en `PlayerPrefab.prefab` a un nombre genérico (ej. "Visuals").
- [x] 2.2 Renombrar todos los objetos hijos visuales en `EnemyPrefab.prefab` a un nombre genérico (ej. "Visuals").
- [x] 2.3 Verificar que el `AnimatorController` en ambos prefabs apunta correctamente a la nueva jerarquía.

## 3. Ajuste de Assets y Metadatos

- [x] 3.1 Limpiar la sección `skeleton` en el archivo `Rabbit_Grey.fbx.meta` para eliminar la rigidez de nombres de raíz.
- [x] 3.2 Verificar que el resto de los archivos `.fbx.meta` en `Assets/Models/Rabbits` tengan el campo `skeleton` vacío.

## 4. Validación y Pruebas

- [ ] 4.1 Probar el modo Práctica Local y confirmar que todos los skins se animan correctamente al rotar al azar.
- [ ] 4.2 Probar el modo Multijugador (Host/Client) y verificar la sincronización visual sin fallos de animación.
- [ ] 4.3 Confirmar que el trigger `Slap` se dispara correctamente tras el swap visual en todos los modelos.
