# Spec: First Person Controller (Capability)

## Requirements

### Requirement: Independent Look
El sistema debe permitir que el ratón controle la rotación horizontal del objeto raíz del jugador y la rotación vertical de la cámara de forma independiente.

#### Scenario: Looking around
- **WHEN** El jugador mueve el ratón horizontalmente
- **THEN** El objeto Player gira sobre su eje Y.
- **WHEN** El jugador mueve el ratón verticalmente
- **THEN** La cámara gira sobre su eje X (clamped entre -90 y 90 grados).

### Requirement: Relative Movement
El movimiento WASD debe ser relativo a la orientación actual del personaje (su Forward y Right).

#### Scenario: Moving Forward
- **WHEN** El jugador presiona 'W' y está mirando hacia un lado
- **THEN** El personaje se mueve en la dirección en la que está mirando.

---

# Spec: Refined Slap Combat (Capability)

## Requirements

### Requirement: Targeting with Camera
La bofetada debe originarse desde la cámara y detectar objetivos en el centro de la vista.

#### Scenario: Slapping an opponent
- **WHEN** El jugador dispara una bofetada
- **THEN** Se traza un SphereCast desde la cámara y detecta al oponente si está dentro del rango.

### Requirement: Slap Cooldown
Debe haber un tiempo de espera obligatorio entre bofetadas.

#### Scenario: Spamming clicks
- **WHEN** El jugador hace clic repetidamente
- **THEN** Solo se ejecuta la bofetada si han pasado 0.5 segundos desde la anterior.
