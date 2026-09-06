# Changelog

## [0.2.4] — Track infrastructure and traction
**Data:** 2026-09-06

### Infrastructure

- Separated track geometry from operational track role.
- Added `TrackType`: `Mainline`, `Secondary`, `Siding`, `Platform`.
- Added `LineClass`: `Local`, `Regional`, `Mainline`, `Magistral`.
- Added `TractionSystem`: `None`, `DC`, `AC`.
- `TrackCell` now stores track type, line class, traction and prepared wear state.
- Added prepared infrastructure Vmax and axle-load profiles without replacing the current movement model.

### Rolling stock

- Kept `TractionType` as the propulsion category `Electric` / `Diesel`.
- Electric locomotives now declare supported electrical systems, allowing multi-system locomotives in the data model.
- EP07 is configured for DC and EU200 for AC.
- Diesel locomotives remain independent of track electrification.

### Infrastructure editing

- Added F8 track infrastructure editor for the track under the mouse cursor.
- Existing track can change role, line class and traction without changing geometry, connections, blocks, signals or switches.
- `TrackBuilder` applies selected infrastructure defaults to newly built track.

### Persistence

- `map.json` schema is now `2`.
- Track infrastructure metadata and wear are persisted.
- Old map saves are not a compatibility target.

### Scope deliberately deferred

- no economy or infrastructure costs;
- no active wear simulation;
- no automatic route repair for incompatible traction;
- no automatic infrastructure conflict resolution;
- no movement/physics rewrite.

### Roadmap

- `0.3.0` — infrastructure management;
- `0.4.0` — economy;
- `0.5.0` — public timetable and coordination;
- `0.6.0` — passenger demand and satisfaction;
- `0.7.0` — richer physics;
- `0.8.0` — failures, weather and crisis management;
- `0.9.0` — network and hub stations;
- `1.0.0` — full integration, balance and campaign.

### Documentation

- Added `docs/31-current-state-0.2.4.md`.
- Added `docs/changelog/0.2.4.md`.
- Added `docs/roadmap-0.3.0-to-1.0.0.md`.
- Updated `docs/00-index.md`, `docs/03-architecture.md`, `docs/15-ai-context.md` and `docs/16-screens-and-ui.md`.

### Verification

No Windows build or live gameplay verification was performed in this change. The 0.2.4 verification target is F8 infrastructure editing, new-track defaults, map schema 2 save/load, locomotive traction metadata and unchanged block/signal/F6 behaviour.

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
