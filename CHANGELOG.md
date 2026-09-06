# Changelog

## [0.2.3] — Operational Dispatcher UI
**Data:** 2026-09-06

### HUD

- Train list now shows `AUTO` / `RĘCZ`, timetable point `n/m`, delay and speed.
- Selected-train panel shows current/next point, ETA, required departure, delay and dispatcher state.
- Added selected-train actions for F6 dispatcher override, route release and AUTO/RĘCZ switching.
- Added compact dispatcher/timetable notification text to the gameplay HUD.

### Dispatcher

- FCFS requests retain target block and request timestamp.
- FCFS ordering is local to the requested block; unrelated blocks do not share one global queue head.
- Added `RailwayDispatcher.ForceProceed` as the F6 manual override.
- Added dispatcher request wait-time reporting.
- F6 bypasses dispatcher arbitration only; physical block occupancy and signal/switch state remain authoritative.

### F10 diagnostics

- Pending requests show train, target block and wait time.
- Blocks show state and reservation/occupancy owner.
- Signals show aspect and associated block state.
- Added safe dispatcher controls for forcing a selected train and releasing a dispatcher reservation/request.
- Route release operates through `RailwayDispatcher`, not direct block-state mutation.

### Player responsibility

- Dispatcher still does not operate switches or signal aspects.
- F6 is an explicit player intervention, not an automatic recovery mechanism.
- Coupling/decoupling and RadioStop remain unchanged.

### Documentation

- Documentation baseline moved to `0.2.3`.
- Added `docs/30-current-state-0.2.3.md`.
- Added `docs/changelog/0.2.3.md`.
- Updated `docs/00-index.md`.

### Verification

A Windows build and live gameplay verification remain required. Critical checks are F6 override, F10 diagnostics, route release, AUTO/RĘCZ switching, timetable delay display and multiple trains competing for the same block.

## [0.2.2] — Timetable authoring and operational diagnostics
**Data:** 2026-09-06

### Locomotive timetable editor

- Added a dedicated Myra locomotive timetable editor.
- F9 opens the editor for the selected train.
- Points can be added, removed and reordered.
- Stations can be changed per point.
- Arrival and departure can be adjusted independently.
- Timetable can be enabled/disabled before saving.
- Draft changes are committed only after timetable validation succeeds.

### Dispatcher and blocks

- `RailwayDispatcher` now maintains an explicit first-come-first-served request queue.
- A train waits when the next block is occupied, reserved or inside release cooldown.
- A later request cannot overtake an earlier request for the dispatcher queue.
- Queue state is available to diagnostics.
- Existing `BlockController`/`Block` state remains authoritative; no parallel route-reservation model was introduced.

### Timetable runtime

- Runtime records actual departure time and observed travel duration.
- Expected arrival/departure for future timetable points include propagated positive delay.
- Early arrival does not propagate a negative delay and still waits until the required departure time.
- Station departure control uses runtime expected departure.

### Diagnostics

- F10 opens a full railway diagnostics view.
- Every block is shown as `FREE`, `RESERVED` or `OCCUPIED`, with cooldown/owner information.
- Dispatcher FCFS queue is visible.
- Every signal aspect is shown with its associated block state.
- Train timetable runtime and dispatcher state are visible together.

### Player responsibility

- Dispatcher does not operate switches or change signal aspects.
- Coupling/decoupling, RadioStop, manual driving and infrastructure decisions remain player-controlled.
- Existing station braking/dwell logic remains the timetable's execution boundary.

### Documentation

- Documentation baseline moved to `0.2.2`.
- Added `docs/29-current-state-0.2.2.md`.
- Added `docs/changelog/0.2.2.md`.
- Updated `docs/00-index.md`.

### Verification

A Windows build and live gameplay verification remain required. F9 timetable editing, cyclic execution, early/late arrivals, block contention, F10 diagnostics and resized Myra layouts should be checked manually.
