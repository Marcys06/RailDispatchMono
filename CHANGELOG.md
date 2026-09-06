# Changelog

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

- A wagon can permanently own a repeating timetable.
- The user defines the base route `A-B-C-D`; the system expands it to `A-B-C-D-C-B-A`.
- Arrival and departure are entered separately for every control point, including the return direction.
- Timetable times are control points used to calculate operational delay.
- The timetable repeats automatically during the simulation day.
- Terminal dwell/manoeuvre time is represented directly by the arrival-to-departure interval at terminal points.
- The locomotive does not own or execute the timetable; it remains responsible only for moving the current operational consist under signals and dispatcher actions.
- Coupling and decoupling are not separate timetable operations.

### UI

- The existing `S` wagon route menu is now the wagon timetable editor.
- The editor keeps the existing wagon/station route workflow and adds full-loop arrival/departure time entry.
- Time fields accept `HH:MM` and support keyboard editing after clicking a field.

### Persistence

- Runtime save schema is now `2` and game version is `0.1.7a`.
- Wagon schedule definition and schedule runtime state are stored inside `trains.json` with the wagon.
- Schedule state survives coupling/decoupling because it belongs to the wagon, not the train.
- Schema 1 runtime saves remain loadable; wagons without a schedule simply continue without timetable state.

### Runtime tracking

- Arrival at a served station records the wagon's current timetable point, actual arrival time, day and calculated delay.
- The runtime state also stores cycle number, current point, state and expected dwell release time.
- This milestone does not yet make the timetable an autonomous driving system; it is a dispatcher/control-point model.

No automated CI build is available for this repository snapshot. A normal solution build and live UI/save verification are still required.

## [0.1.6pre] — Consolidated 0.1.6 pre-release
**Data:** 2026-09-04

Historical consolidated milestone. Detailed notes: `docs/changelog/0.1.6pre.md`.

## [0.1.6g] — Runtime safety and geometry cleanup
**Data:** 2026-09-04

Historical development stage. See `docs/changelog/0.1.6g.md`.

## [0.1.6f] — Explicit consist ordering

Historical development stage. See `docs/changelog/0.1.6f.md`.

## [0.1.6e] — Coupling/decoupling stabilisation

Historical development stage. See `docs/changelog/0.1.6e.md`.

## [0.1.6d] — Passenger journey continuity
**Data:** 2026-09-04

Historical development stage. See `docs/changelog/0.1.6d.md`.

## [0.1.6c] — Wagon-aware passenger boarding

Historical development stage. See `docs/changelog/0.1.6c.md`.

## [0.1.5pre] — Final 0.1.5 pre-release
**Data:** 2026-09-03

Historical consolidated milestone. Detailed notes: `docs/changelog/0.1.5pre.md`.
