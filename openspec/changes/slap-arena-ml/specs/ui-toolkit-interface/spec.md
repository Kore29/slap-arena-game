## ADDED Requirements

### Requirement: Scalable User Interface Navigation
The game UI SHALL process transitions between the main menu, loading, and match exclusively via UI Toolkit UXML/USS documents.

#### Scenario: Navigate to Join Screen
- **WHEN** the user clicks "Join Game" from the main screen
- **THEN** the layout smoothly transitions to display a text input field for the Relay Code without triggering Console errors.
