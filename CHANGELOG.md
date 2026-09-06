# Changelog

## [0.2.1] — Myra gameplay HUD rebuild
**Data:** 2026-09-06

### HUD

- Rebuilt `MyraGameplayView` as one coherent operational dashboard.
- Replaced the previous unrelated fixed-width list groups with a consistent four-zone hierarchy: status bar, operations, operational centre and traffic.
- Added explicit automatic/manual train markers.
- Added selected-train operational summary with speed, consist Vmax, signal target, direction and dispatcher/block state.
- Added locomotive timetable status to the selected-train panel, including current point, delay and planned departure.
- Added an expandable diagnostic panel for selected trains.
- Kept station passenger breakdowns expandable while simplifying the station list presentation.
- Kept wagon occupancy, delay and route information in one coherent wagon section.
- Moved the operating-control explanation into the HUD so keyboard and UI actions describe the same workflow.
- Reworked sizing so the centre column is flexible while side rails remain stable.
- Long labels wrap and information lists scroll rather than overlap neighbouring controls.

### Player responsibility

- The HUD remains presentation-only.
- It does not automate switches, signal aspects, coupling/decoupling, RadioStop clearing or passenger transfers.
- Dispatcher state is shown as operational information; infrastructure decisions remain with the player.

### Compatibility

- The previous HUD layout is not a compatibility target.
- No save-schema compatibility requirement was added for the HUD.

### Documentation

- Documentation baseline moved to `0.2.1`.
- Updated `docs/16-screens-and-ui.md` with the new HUD architecture.
- Added `docs/28-current-state-0.2.1.md`.
- Updated the documentation index and maintained UI contract.

### Verification

A Windows solution build and live UI verification remain required. Resized-window behaviour, long station names, expanded passenger summaries, multiple trains/wagons and the diagnostics panel should be checked manually.

## [0.2.0] — Operational railway simulation
**Data:** 2026-09-06

### Locomotive timetable

- Locomotives can own an independent operational timetable.
- Wagon timetables remain independent and retain their existing wagon ownership model.
- Timetable arrival is the expected arrival time; departure is the required departure time.
- An automatically operated locomotive waits at a scheduled station until the required departure time, including when it arrives early.
- Actual arrival and timetable delay are retained in locomotive runtime state.
- Locomotive timetables remain cyclic.

### Dispatcher and blocks

- Added first-come-first-served `RailwayDispatcher` arbitration for the existing block chain.
- Automatic movement is held when the next connected block is occupied or reserved by another train.
- Block reservations are released after the train enters the reserved block.
- The dispatcher does not move switches and does not change signal aspects.
- Existing signal and player-controlled junction systems remain authoritative for infrastructure operation.

### Stations and passengers

- Locomotive timetable execution is integrated with the existing `StationController`.
- Existing passenger boarding/alighting and station dwell remain active.
- Passenger ownership remains at the concrete wagon level.
- Automatic passenger transfers are not introduced.

### Manual gameplay

- F6/manual shunting remains available.
- F7 reversal remains available.
- RadioStop semantics remain unchanged.
- Coupling and decoupling remain manual.
- The player remains responsible for switches, signal aspects and resolving infrastructure situations that automation cannot solve.

### Persistence

- `TrainSchedule` is now version `2` and can persist an optional locomotive timetable alongside wagon schedules.
- `ScheduleStorage` document schema is now `2`.
- Backward compatibility with pre-0.2.0 saves is not required.

### Documentation

- Documentation baseline moved to `0.2.0`.
- Added `docs/27-current-state-0.2.0.md` as the authoritative current-state snapshot.
- Added `docs/changelog/0.2.0.md` with the complete milestone contract.

## [0.1.7b] — Timetable editor UI
**Data:** 2026-09-04

### UI

- `S` remains the single entry point for wagon timetable editing.
- The timetable editor is split into clear route and timetable sections.
- Route operations use explicit buttons rather than implicit clickable text.
- Base-route stations can be added, removed and reordered with `GÓRA` / `DÓŁ` controls.
- The timetable table uses separate `STACJA`, `PRZYJAZD` and `ODJAZD` columns.
- The active time field is visually distinct.
- Long station names and status messages are constrained so they do not overlap neighbouring controls.
- Save, delete-timetable and cancel actions are separated from timetable rows.
- Longer routes use scrolling instead of growing the editor beyond the screen.

### Model compatibility

- The `0.1.7a` timetable model is retained unchanged.
- A wagon still owns its repeating timetable.
- Base route `A-B-C-D` still expands to `A-B-C-D-C-B-A`.
- Arrival and departure remain independently editable for every control point.

### Verification

A normal solution build and live UI verification are still required. The repository has no automated CI build available for this snapshot.

## [0.1.7a] — Wagon loop timetables
**Data:** 2026-09-04

### Timetable model
