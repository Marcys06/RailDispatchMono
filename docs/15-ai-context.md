# AI context packet

## Current development line

**RailDispatchMono `0.2.1`** is the current development snapshot. The 0.2.1 line rebuilds the Myra gameplay HUD into one coherent operational dashboard while preserving the 0.2.0 timetable, block, signal and player-control contracts.

## HUD contract

`MyraGameplayView` is the single gameplay HUD surface mounted by `MyraUIManager`. It presents global status, infrastructure tools, selected-train operations, diagnostics, traffic, stations and wagons in one hierarchy.

The centre column is flexible. Side rails have stable widths. Lists scroll and labels wrap. The UI does not own domain state.

Selecting a train focuses the camera and exposes speed, consist Vmax, effective signal target, direction, dispatcher state, locomotive timetable state and RadioStop diagnostics.

## Passenger/station contract

`StationController` owns station lifecycle, train stop/dwell coordination, passenger generation and locomotive timetable gating. `Passenger` remains owned by a concrete `Wagon` while onboard. Automatic passenger transfers remain outside the contract.

## Locomotive timetable contract

`Locomotive.Schedule` owns an optional `LocomotiveSchedule`. `LocomotiveSchedulePoint` contains station, expected arrival and required departure. `LocomotiveScheduleRuntime` stores current point, cycle, actual arrival, delay and required departure.

Early arrival does not produce early departure. The timetable is cyclic. `TrainSchedule`/`ScheduleStorage` schema is `2`.

## Dispatcher/block contract

`RailwayDispatcher` arbitrates the next connected existing `Block` on a first-come-first-served basis. It can hold automatic movement when the next block is occupied or reserved by another train.

The dispatcher does not set switches and does not change signal aspects. The player remains responsible for infrastructure configuration and unresolved operational situations.

## Coupling and movement contract

`CouplingService` is authoritative. `Composition.Vehicles` is physical order and is never reversed by coupling or decoupling. Coupling/decoupling remain manual.

F6 is manual shunting. F7 changes travel direction only at `0 km/h`. RadioStop remains a hard guard for normal automatic movement. Existing rigid-consist, trajectory, acceleration, braking and Vmax contracts remain unchanged.

## Architecture rules

- One authoritative train lifecycle owner: `TrainManager`.
- One authoritative ordered consist: `TrainComposition`.
- One coupling mutation boundary: `CouplingService`.
- One station lifecycle/timetable gate: `StationController`.
- One block occupancy owner: `BlockController`/`Block`.
- One block arbitration service: `RailwayDispatcher`.
- One active passenger collection owner: `PassengerManager`.
- One shared Myra `Desktop`: `MyraUIManager`.
- One gameplay HUD: `MyraGameplayView`.
- UI requests domain operations; it does not own domain state.

## Platform scope

0.2.x implementation target is Windows. Other host projects remain in the repository unless a later cleanup explicitly removes them.

## Verification

No automated Core test project or CI build establishes compilation for this snapshot. A local Windows solution build and live UI verification remain required after pulling changes. For 0.2.1 specifically, verify 1600x900 and resized windows, long station names, multiple trains/wagons, expanded station passenger summaries and the diagnostics panel.

## AI rule

Before HUD changes inspect `MyraGameplayView`, `MyraUIManager`, `RailDispatchMonoGame`, `GameplayScreen`, `TrainManager`, `StationController`, `RailwayDispatcher`, `LocomotiveSchedule` and `WagonSchedule` together. Before timetable changes inspect the timetable runtime/persistence chain. Before block/safety changes inspect `Block`, `BlockController`, `RailwayDispatcher`, `SignalController`, `TrainMovement`, `TrainCollisionController` and `StationController` together. Before movement/coupling changes inspect `TrainComposition`, `CouplingService`, `TrainGeometry`, `TrainMovement`, `TrainDirection`, `SimulationScale` and vehicle-end connection contracts.

Every UI or runtime contract change must update the maintained architecture/current-state documentation and the relevant changelog.
