## 1. Implementación de UI (HUD)

- [ ] 1.1 Crear el archivo `Assets/UI/GameHUD/GameHUD.uxml` con una barra de progreso.
- [ ] 1.2 Crear el archivo `Assets/UI/GameHUD/GameHUD.uss` con estilos de HUD modernos.
- [ ] 1.3 Implementar `GameHUD.cs` para enlazar los elementos de UI con el jugador.

## 2. Refactor y Exposición del Jugador

- [ ] 2.1 Añadir propiedad `CooldownProgress` en `PlayerController.cs`.
- [ ] 2.2 Implementar sistema de eventos para confirmar impactos de bofetada.
- [ ] 2.3 Incrementar `slapForce` base a **25f** para mejorar el "feeling" del impacto.

## 3. Integración en Escena

- [ ] 3.1 Añadir un nuevo `UIDocument` a la escena encargado del HUD.
- [ ] 3.2 Asegurar que el HUD solo se muestre una vez empezada la partida.

## 4. Pruebas y Pulido

- [ ] 4.1 Verificar que la barra de cooldown se sincroniza con el ataque.
- [ ] 4.2 Probar el "Hit Log" visual al impactar a la IA.
- [ ] 4.3 Limpieza final de Debug Logs antiguos.
