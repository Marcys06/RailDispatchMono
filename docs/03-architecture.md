# Architecture

## Current development line

`0.2.4` is the current documented development baseline. It connects the existing station, signal, block and timetable systems with explicit infrastructure metadata while preserving the player's infrastructure-control role.

## Core rule

`RailDispatchMono.Core` is the shared application/game layer. Platform projects host the application. Dependency direction points from platform hosts toward Core.

## Main runtime components

```text
Platform host
    |
    v
RailDispatchMonoGame
    |
    v
ScreenManager
    |
    v
GameplayScreen
    |
    +--> GameMap / TrackCell / TrackBuilder
    +--> TrainManager
    |       +--> Train / TrainMovement
    |       +--> TrainComposition
    |       +--> CouplingService
    |       +--> RailwayDispatcher
    |
    +--> StationController
            +--> Locomotive timetable execution
            +--> PassengerManager
            +--> PassengerService
            +--> station dwell
```

## Ownership

- simulation/domain state belongs to `Game/` subsystems;
- track topology belongs to `GameMap` / `TrackCell` / `TrackBuilder`;
- track geometry is `TrackGeometry`; infrastructure role is `TrackType`; line classification is `LineClass`; electrification is `TractionSystem`;
- infrastructure wear is stored by `TrackCell` and is not yet actively simulated;
- train lifecycle belongs to `TrainManager`;
- ordered physical consist state belongs to `TrainComposition`;
- coupling validation/mutation belongs to `CouplingService`;
- station lifecycle, stop/dwell and passenger generation belong to `StationController`;
- locomotive operational timetable belongs to `Locomotive.Schedule` and its `Train` runtime;
- wagon timetable remains owned by `Wagon`;
- passenger ownership while riding remains at the concrete `Wagon`;
- railway block occupancy remains owned by `BlockController`/`Block`;
- block arbitration belongs to `RailwayDispatcher`;
- signal and junction aspects remain player-controlled;
- presentation belongs to screens/Myra/renderers;
- persistence remains behind the existing save services.

## Infrastructure boundary

Infrastructure metadata is segment-local. A block may span multiple segments with different infrastructure parameters; the block system remains concerned with traffic occupancy and reservation rather than duplicating infrastructure state.

`LineClassProfile` exposes prepared Vmax and axle-load values. 0.2.4 does not automatically impose those values on the movement model; that belongs to the planned physics/infrastructure work.

Electrical traction is modelled independently from propulsion type. An electric locomotive declares supported `TractionSystem` values, while diesel locomotives are independent of electrification. The model supports multi-system locomotives. 0.2.4 does not automatically reroute an incompatible train.

## Infrastructure editing boundary

`TrackBuilder` owns construction-time infrastructure defaults and metadata-only mutation methods. `MyraTrackInfrastructureView` is a UI request surface for the same domain operations and does not own track state.

F8 edits the track under the cursor. It does not change geometry, block state, signals or switches.

## Persistence

`MapSaveData` schema `2` persists infrastructure metadata per track cell. Old map saves are not a compatibility requirement for this development line.

## Locomotive timetable

`LocomotiveSchedule` is separate from `WagonSchedule`. A locomotive timetable contains ordered station points with expected arrival and required departure times.

`TrainSchedule` persists the optional locomotive timetable alongside wagon schedule definitions. `TrainSchedule` version and `ScheduleStorage` document schema are `2`.

The train runtime records actual arrival and delay. Early arrival does not cause an early departure: the locomotive remains stopped until its required departure time. The timetable is cyclic.

## Dispatcher boundary

`RailwayDispatcher` is deliberately not a second signal system. It arbitrates the next already-connected `Block`:

- an occupied next block belonging to another train blocks automatic progress;
- a reservation belonging to another train blocks automatic progress;
- the first train processed by the simulation obtains the available reservation;
- reservations are released after the train enters the reserved block;
- no switch is changed automatically;
- no signal aspect is changed automatically by the dispatcher.

This leaves route configuration and signal operation with the player. The dispatcher prevents an automated train from ignoring physical block conflicts; it does not solve an incorrectly configured railway for the player.

## Passenger boundary

The implemented passenger flow remains:

`StationController → PassengerManager → PassengerService → Wagon`

`PassengerManager.GetOnBoard(Train)` is an operational view, not an ownership boundary. No automatic passenger-transfer system was added.

## Consist and movement contract

The established rigid-consist rules remain authoritative:

- `Composition.Vehicles` is the physical order;
- F7 changes travel direction without reversing the physical list;
- vehicle positions and trajectory handling remain under the existing movement model;
- F6 is the dispatcher override;
- coupling and decoupling remain manual;
- RadioStop remains an independent safety stop.

## Runtime ordering

For an automatically operated train, the relevant update order remains:

```text
StationController.BeforeTrainUpdate
    |
    +--> locomotive timetable departure gate
    |
    +--> RailwayDispatcher block arbitration
    |
    v
existing collision/signal safety
    |
    v
Train.Update
    |
    v
StationController.AfterTrainUpdate
    |
    +--> passenger service/dwell
    +--> locomotive actual-arrival recording
    +--> dispatcher reservation release
```

## Roadmap boundary

0.2.4 prepares infrastructure data. 0.3.0 activates infrastructure management; 0.4.0 adds economy; 0.5.0 public timetable; 0.6.0 passenger demand; 0.7.0 richer physics; 0.8.0 crises; 0.9.0 network; 1.0.0 full integration. The detailed dependency plan is in `docs/roadmap-0.3.0-to-1.0.0.md`.

## Safety and dependency discipline

1. Find the existing owner of state before adding a new manager/service.
2. Reuse existing managers/models rather than parallel systems.
3. Keep presentation out of domain mutation.
4. When changing movement, audit signals, blocks, collision safety and RadioStop.
5. When changing station/passenger flow, audit `StationController`, `PassengerManager`, `Wagon`, `TrainRoute` and HUD together.
6. When changing coupling, audit `TrainComposition`, `CouplingService`, vehicle-end connections and passenger ownership together.
7. When changing constructors/data contracts, inspect save/load and catalogue factories.
8. When changing infrastructure, audit `TrackCell`, `TrackBuilder`, `GameMap`, map persistence, rolling stock traction and block/signal ownership together.
9. Every current contract change requires a current-state snapshot and relevant changelog update.
