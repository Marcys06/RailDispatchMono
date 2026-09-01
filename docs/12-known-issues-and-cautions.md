# Known issues and cautions

## Current baseline: `0.1.2a`

`0.1.2a` is the first infrastructure stage of the Myra UI integration series. It adds the standard Myra dependency and a shared initialization boundary but does not migrate existing screens yet.

### Myra integration

- Myra is currently referenced from `RailDispatchMono.Core.csproj` as version `1.6.5`.
- `MyraUIManager` initializes `MyraEnvironment.Game` and creates the shared `Desktop` during `RailDispatchMonoGame.LoadContent()`.
- The shared Myra desktop is not rendered globally at this stage.
- Existing menu, settings and pause screens continue using the established screen/UI implementation.
- A later stage must integrate Myra with the existing input and presentation-scaling contracts rather than introducing an independent input stack.

### Persistence

- Save data uses separate JSON files inside each save directory.
- `metadata.json` identifies the save and stores its metadata.
- Save schema is versioned through `schemaVersion`.
- Auto-save is intentionally disabled.
- Invalid/incomplete save data must result in a user-facing notification rather than silent partial loading.

### Startup and screen lifecycle

- Main Menu is the application entry point.
- New Game creates a new empty game state immediately; no confirmation is required by design.
- Pause is a `ScreenManager`-managed overlay and `ESC` toggles it.
- Do not bypass `ScreenManager` when migrating UI to Myra.

### Diagnostics

- Debug output is globally rate-limited; excess diagnostics may be intentionally dropped.
- Duplicate log lines should be investigated as a logging/subscription issue rather than treated as proof that the underlying game update runs twice.

## Rule for future agents

When a reported bug contradicts this document, inspect the current source and call sites first. If `0.1.2a` has a defect, do not modify this stage after commit; implement the correction as `0.1.2b`.
