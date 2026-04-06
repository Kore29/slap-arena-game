## 1. Modificación de Recompensas (Cerebro de la IA)

- [x] 1.1 Reducir la penalización por tiempo en `EnemyAgent.cs` de `-0.001f` a `-0.0002f` para evitar el suicidio prematuro.
- [x] 1.2 Implementar recompensa de proximidad: Añadir `+0.0005f` si la distancia al jugador es menor de `3.0f` metros.
- [x] 1.3 Implementar recompensa de zona segura: Añadir `+0.0001f` si el agente está a menos de `2.5f` metros del centro de la plataforma (`Vector3.zero`).
- [x] 1.4 Aumentar el castigo por caída: Cambiar la recompensa al caer del mapa de `-1.0f` a `-2.0f`.

## 2. Implementación de Sensores (Unity Editor)

- [ ] 2.1 Añadir el componente `Ray Perception Sensor 3D` al `EnemyPrefab`.
- [ ] 2.2 Configurar `Detectable Tags` e incluir el tag `Player` (añadiendo el tag a la lista en Unity).
- [ ] 2.3 Configurar `Rays Per Direction` a `2` y `Max Ray Degrees` a `45` (para crear un cono de 5 rayos frontales).

## 3. Preparación para Entrenamiento

- [ ] 3.1 Revisar que el componente `Behavior Parameters` no tenga un tamaño de observación fijo restrictivo que choque con el nuevo sensor.
- [x] 3.2 Eliminar o comentar la función `Heuristic` en `EnemyAgent.cs` para evitar el problema del control espejo.
- [x] 3.3 Sugerir un nuevo comando de entrenamiento.
