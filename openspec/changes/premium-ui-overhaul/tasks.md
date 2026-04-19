# Tasks: Premium UI Overhaul

## Phase 1: Foundation
- [ ] Create `Assets/UI/GlobalStyles.uss` with all design tokens (colors, variables).
- [ ] Implement core shared classes: `.panel-glass`, `.btn-premium`, `.title-hero`.

## Phase 2: Lobby Redesign
- [ ] Import `GlobalStyles.uss` into `Lobby.uxml`.
- [ ] Remove all inline styles from `Lobby.uxml` and replace with classes.
- [ ] Refactor `LobbyItem.uxml` to match the new aesthetic.
- [ ] Add transition styles for lobby buttons.

## Phase 3: Main Menu Alignment
- [ ] Update `MainMenu.uss` to import and inherit from `GlobalStyles.uss`.
- [ ] Refactor `MainMenu.uxml` component classes to use the new color tokens.
- [ ] Enhance the main menu title with new hero styles.

## Phase 4: HUD and Polish
- [ ] Update `GameHUD.uss` with neon gradients for progress bars.
- [ ] Refine `GameHUD.uxml` layout and typography.
- [ ] Modernize `MatchResults.uxml` with the global design system.

## Phase 5: Verification
- [ ] Audit all screens for visual consistency.
- [ ] Verify hover/active states for all buttons.
- [ ] Test layout responsiveness on various aspect ratios.
