## ADDED Requirements

### Requirement: Session Creation and Join
The system SHALL allow players to create and join game sessions.

#### Scenario: Host creates a room
- **WHEN** the user selects "Create Session" in the menu
- **THEN** the system requests an allocation from Unity Relay, creates a Host instance via Unity Netcode, and displays the Relay join code.

#### Scenario: Client joins a room
- **WHEN** the user enters a valid Relay join code and clicks "Join"
- **THEN** the system connects them as a Client to the Host session.
