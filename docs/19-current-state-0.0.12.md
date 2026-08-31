# RailDispatchMono — current implementation state

Last updated: 2026-08-31

This document is the authoritative snapshot of the project immediately before `0.0.13` route creation.

## Simulation

- Train movement operates on the logical track network, including straight track, curves and junctions.
- Signals are integrated with train speed control and block occupancy.
- Stop signals require braking before the signal when physically possible.
- Non-stop signal aspects provide target speed information.
- Train speed is constrained by both the signal target and the consist Vmax.
- Consist Vmax is the minimum Vmax among all vehicles in the train.
- Game time starts at `00:00` and supports `x1`, `x2`, `x5` simulation speed.
- The existing pause system stops both the simulation and game clock.

## Railway objects

### Track

- Straight track, curves and junctions are placeable.
- Junctions have switch state and determine the train's route through the cell.
- Track connections are maintained by the track builder.

### Signals

- Signals support multiple aspects including Stop, Clear and Warning.
- Signals can be placed on tracks and manually changed with the existing controls.
- Block occupancy and signal state are coordinated by `BlockController`.

### Stations

- Stations are rectangular areas rather than single cells.
- Current supported construction sizes are `1x1`, `2x2`, `3x3` and `4x4`.
- Every cell in the station area must contain track.
- Stations can have passenger service and dwell time.
- A train only receives station stopping behaviour when the stop-decision service says it should stop; a station without a required stop does not force braking.
- Station detection is latched after a completed stop so the train cannot immediately re-trigger the same station after release.

### Depots

- A depot is a world building represented by the `Depot` domain object.
- `DepotController` owns player-built depots.
- Depot rendering uses generated rectangles and symbols; no image asset is required.
- `9` / NumPad `9` activates depot placement.
- Left click places a depot; Shift + right click removes it.
- Depot is intentionally prepared as the future origin/entry point for route creation in `0.0.13`.

## Passengers

Passengers are quasi-individual entities with:

- origin station,
- destination station,
- current station identifier,
- current train identifier when on board,
- passenger state.

Passenger handling is split into:

- `ITrainStopDecision` — decides whether a train should stop at a station.
- `IPassengerService` — performs passenger exchange.
- `IPassengerDemandProvider` — supplies passenger destinations.

The default demand provider is random. The abstraction is deliberately kept so a future city/demand system can replace it without changing station or passenger core logic.

Each wagon has its own passenger capacity and handles passenger acceptance independently. This is preparation for future coupling/decoupling, where different portions of a consist can follow different routes.

## Train consist

A train contains an ordered list of individual vehicles. Wagons remain individually addressable and expose passenger information independently.

The current default spawn consist is:

- 1 locomotive,
- 2 passenger-capable wagons.

Future coupling/decoupling can therefore operate on vehicle boundaries rather than treating the train as an indivisible passenger container.

## Speed model

The train exposes:

- `TargetSpeed` — target speed derived from railway signalling/station constraints.
- `MaxSpeed` — physical consist limit, equal to the lowest vehicle Vmax.
- effective target speed — the value actually used by movement after all constraints are applied.

## UI

The game currently uses programmatically generated UI elements and `SpriteFont`; no external UI image files are required.

Current HUD includes:

- game clock,
- simulation speed controls,
- train/station object panel,
- depot/spawn workflow,
- train tooltips,
- station tooltips,
- per-wagon floating passenger notifications.

UI coordinates should be treated as viewport-relative. `SpriteFont.MeasureString` is used where text dimensions are required for tooltip/layout calculations.

## Desktop window

Default window size is `1600x900`.

The desktop game window is user-resizable. Settings provide three explicit presets:

- `1280x720`,
- `1600x900`,
- `1920x1080`.

The selected size is persisted in the existing settings storage.

## Controls — current relevant controls

- `1` — straight track
- `2` — curve
- `3` — junction
- `4` — signal
- `5` — station
- `9` — depot
- `R` — rotate/change current build element; in station mode changes station size
- `J` — signal/switch quick toggle
- `LMB` — build/select
- `PPM` — remove/open object menu depending on object
- `Shift + PPM` — explicit removal where supported
- `MMB` — camera movement
- mouse wheel — camera zoom
- `Escape` / `P` — pause

## 0.0.13 preparation

The depot is intentionally kept independent from route planning. The next major update can introduce:

1. route origin at a depot,
2. destination station selection,
3. route path validation against the actual track network,
4. signal/junction compatibility checks,
5. route assignment to a train,
6. route progress and next-stop information.

The passenger model is already structured so route planning can later determine whether a passenger can remain in its wagon, wait at a station, or use a future connection.

## Not implemented yet

- full route creation/planning,
- timetable/schedule system (`0.0.12`/`0.0.13` scope boundary),
- coupling and decoupling,
- realistic city-generated passenger demand,
- transfers between trains,
- derailment and full railway safety/interlocking,
- advanced shunting operations,
- procedural city generation.
