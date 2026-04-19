# Proposal: Premium UI Overhaul

## Problem
The current UI of "Slap Arena" is visually inconsistent and lacks a polished, high-end feel. It uses a mix of solid colors, default components, and fragmented styling across Lobby, Main Menu, and Game HUD. The "WAR ROOM" (Lobby) in particular looks basic and doesn't convey the high-tech, intense atmosphere of a combat arena.

## Proposed Solution
Implement a comprehensive visual overhaul using a unified "Neon-Futuristic" design system. This includes:
1.  **Global Stylesheet**: Centralizing all theme variables and classes in `GlobalStyles.uss`.
2.  **Glassmorphism**: Implementing semi-transparent dark panels with subtle glowing borders and depth.
3.  **Modern Typography**: Optimizing font weights, sizes, and letter spacing to match the theme.
4.  **Micro-Animations**: Adding smooth hover and active transitions to all interactive elements.
5.  **Component Refresh**: Redesigning the Lobby, Main Menu, Game HUD, and Match Results to use the new system.

## Impact
-   **Aesthetics**: Elevates the game's perceived quality to a professional, "premium" level.
-   **UX**: Improved visual hierarchy and feedback through better color use and animations.
-   **Maintainability**: Moving away from inline styles to a centralized CSS-like system makes future adjustments much easier.
-   **Consistency**: Ensures every screen feels part of the same digital world.
