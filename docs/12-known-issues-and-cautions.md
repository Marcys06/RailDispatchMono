# Known issues and cautions

## Current baseline: `0.1.0`

The `0.0.16` feature series is considered complete for the current scope. `0.1.0` is a stabilization/bugfix baseline. The game is considered operational within the current prototype scope; remaining minor defects are not release blockers unless they prevent the core gameplay loop.

### Persistence

- Save data uses separate JSON files inside each save directory.
- `metadata.json` identifies the save and stores its metadata.
- Save schema is versioned through `schemaVersion`.
- Auto-save is intentionally disabled.
- Invalid/incomplete save data must result in a user-facing notification rather than silent partial loading.
- Do not assume that a future unified single-JSON save format exists; the current contract is deliberately multi-file.

### Startup and screen lifecycle

- Main Menu is the application entry point.
- New Game creates a new empty game state immediately; no confirmation is required by design.
- Load Game operates on save directories.
- Pause is a `ScreenManager`-managed overlay and `ESC` toggles it.
- Do not bypass `ScreenManager` when changing menu/pause behavior.

### Simulation timing

- `GameDay` and `GameTime` are authoritative simulation-time values.
- x1/x2/x5 are simulation speed multipliers.
- Pause stops simulation progression.
- The presentation/game-time scale must not be accidentally applied as an additional multiplier to physical train velocity, acceleration, braking or travelled distance.

### Gameplay

- Depots are world buildings and the train-creation entry point.
- A depot may be used to create multiple trains through the existing depot workflow.
- Wagon routes describe passenger-service destinations and do not directly drive locomotive movement.
- Passenger exchange is wagon-specific. Floating `+X` and `-X` notifications are generated when the passenger count changes during an exchange.
- Signal protection remains part of train movement and has priority over the simpler collision safety rule.

### Diagnostics

- Debug output is globally rate-limited; excess diagnostics may be intentionally dropped.
- Duplicate log lines should be investigated as a logging/subscription issue rather than treated as proof that the underlying game update runs twice.

## Rule for future agents

When a reported bug contradicts this document, inspect the current source and call sites first. Update the documentation only after the executable behavior has been verified. Do not restore obsolete behavior merely because it is described in an older changelog.
