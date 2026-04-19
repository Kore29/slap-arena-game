# Design: Premium UI Overhaul

This document describes the design system and technical approach for unifying the Slap Arena UI.

## Visual Design System (Shared USS)

We will implement a centralized design system in `Assets/UI/GlobalStyles.uss` using custom variables and reusable classes.

### 1. Color Palette (USS Variables)
- `--color-bg-dark`: `rgba(10, 12, 16, 0.98)` (Deep Slate)
- `--color-primary`: `#00D1FF` (Neon Cyan)
- `--color-secondary`: `#FF4747` (Vivid Red)
- `--color-success`: `#00C853` (Neon Green)
- `--color-accent`: `#FFC400` (Gold)
- `--color-glass-border`: `rgba(255, 255, 255, 0.1)`

### 2. Core Classes
- `.panel-glass`:
  - Background: `rgba(30, 34, 42, 0.7)`
  - Border: `1px solid var(--color-glass-border)`
  - Border Radius: `24px`
  - High-end feel achieved through subtle padding and layered transparency.
- `.btn-premium`:
  - Background: Linear gradient from primary to a darker shade.
  - Transitions: `transition-duration: 0.2s; transition-property: all;`
  - Hover: `scale: 1.05; background-color: var(--color-primary);`
  - Active: `scale: 0.95; opacity: 0.8;`
- `.title-hero`:
  - Font Size: `36px`
  - Letter Spacing: `8px`
  - Text Shadow: `0 4px 10px rgba(0, 0, 0, 0.5)`

## Implementation Strategy

### 1. Style Migration
- Replace all inline `style` attributes in `.uxml` files with class names.
- Ensure all screens link to `GlobalStyles.uss`.

### 2. Component Refinement
- **Lobby**:
  - Update the "War Room" layout to use the full `panel-glass` aesthetic.
  - Standardize the "Join Code" display as a floating pill with a neon pulse.
  - Refine `LobbyItem` with better padding and a subtle background for the local player.
- **Main Menu**:
  - Transition from the current gold/purple theme to the new Cyan/Red/Dark theme.
  - Align button sizes and spacing with the new HUD standards.
- **Game HUD**:
  - Modernize progress bars with neon gradients and "electronic" inner glows.
  - Refine hit-log typography for better readability during combat.
- **Match Results**:
  - Implement a full-screen blurred background (via a dark overlay) and a centered premium results card.

## Technical Considerations
- **Unity UI Toolkit Limitations**: We will use `linear-gradient` for background colors where supported and simulate glows using `border-color` with high-alpha colors.
- **Responsiveness**: Use `flex-grow` and percentage-based sizing to ensure the UI scales correctly from 4:3 to 21:9 aspect ratios.
