# Changelog

## [0.3.1] — Infrastructure degradation and operational recommendations
**Data:** 2026-09-14

### Infrastructure

- Added progressive wear acceleration at higher degradation levels.
- Added `Severe` maintenance threshold at 95% wear.
- Added inspection/critical/severe state helpers to `TrackCell`.
- Added recommended operational speed and axle-load reductions derived from infrastructure condition.
- Added maintenance priority scoring using wear, line class, track role and junction status.
- Added filtered state reports and most-urgent-track inspection API.
- Added dedicated severe-track repair operation.

### Persistence

- Map saves are stamped `0.3.1`.
- `map.json` remains schema 3.
- Existing 0.3.0 saves remain structurally compatible.

### Domain boundary

- `GameMap.Maintenance` remains the maintenance owner.
- `TrackCell` remains the owner of actual wear.
- Recommended restrictions are domain data; train movement and dispatcher enforcement are intentionally deferred to the next infrastructure step.

### Documentation

- Added `docs/36-current-state-0.3.1.md`.
- Added `docs/changelog/0.3.1.md`.
- Updated `docs/00-index.md` and `docs/roadmap-0.3.0-to-1.0.0.md`.

### Verification

No Windows build or live gameplay verification was performed in this environment.

## [0.3.0] — Infrastructure maintenance
**Data:** 2026-09-14

### Infrastructure

- Activated simulation-driven track wear through `InfrastructureMaintenanceManager`.
- Tracks occupied by trains wear faster than unused infrastructure.
- Wear rate accounts for `LineClass`, `TrackType` and `TractionSystem`.
- Added `Good`, `Warning` and `Critical` maintenance states.
- Added maintenance summary and worst-track inspection APIs.
- Added repair-all and repair-critical operations.

### F10

- Added average infrastructure condition, warning/critical counts and simulated maintenance time.
- Added list of the most worn track cells.
- Added `NAPRAW KRYTYCZNE` and `NAPRAW WSZYSTKIE` actions.

### Persistence

- Continued using `map.json` schema 3.
- Existing `TrackCell.WearPercent` persistence is reused; no migration was introduced.

### Domain boundary

- `GameMap.Maintenance` owns maintenance simulation.
- `TrackCell` remains the owner of the current wear value.
- Dispatcher, blocks, signals, switches, routes and timetable logic are unchanged.
- Maintenance does not automatically close tracks or alter dispatcher decisions.

### Documentation

- Added `docs/35-current-state-0.3.0.md`.
- Added `docs/changelog/0.3.0.md`.
- Updated `docs/00-index.md`, `docs/16-screens-and-ui.md` and `docs/roadmap-0.3.0-to-1.0.0.md`.

### Verification

No Windows build or live gameplay verification was performed in this environment.

## [0.2.6a] — Complete F11 railway line editor
**Data:** 2026-09-06

### Fixed

- Fixed the F11 editor layout so documented controls are accessible instead of being pushed outside the visible area by oversized horizontal rows.
- Added a scrollable main F11 content area and a separate scrollable saved-line list.
- Separated selection, line management, membership and bulk-infrastructure operations into explicit sections.

### F11

- Create a named line from the current selection.
- Select an existing line.
- Rename the selected line.
- Select a line's tracks on the map.
- Delete a named line without deleting physical track.
- Add/remove the current selection from the selected line.
- Cycle and apply `TrackType` to the selection or selected line.
- Cycle and apply `LineClass` to the selection or selected line.
- Cycle and apply `TractionSystem` to the selection or selected line.
- Select a connected track area using existing track topology.
- Clear the current selection.
- Use the same line/member operations directly from each saved-line row.

### Domain boundary

- `RailwayLineManager` remains the owner of named-line grouping and bulk infrastructure operations.
- Named lines remain organizational metadata only.
- Blocks, signals, switches, routes, timetable logic, movement and physics are unchanged.
- No automatic dispatcher decisions or route repair were introduced.

### Documentation

- Added `docs/34-current-state-0.2.6a.md`.
- Added `docs/changelog/0.2.6a.md`.
- Updated `docs/00-index.md` and `docs/16-screens-and-ui.md`.

### Verification

No Windows build or live gameplay verification was performed in this environment.
