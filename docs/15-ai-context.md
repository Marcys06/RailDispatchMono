# AI context packet

## Current development line

**RailDispatchMono `0.2.3`** is the current development snapshot. The 0.2.3 line adds an operational dispatcher HUD and explicit player intervention while preserving the timetable, block, signal and player-control contracts from 0.2.2.

## HUD contract

`MyraGameplayView` is the single gameplay HUD surface mounted by `MyraUIManager`. It presents global status, infrastructure tools, selected-train operations, dispatcher state, traffic, stations and wagons in one hierarchy.

Selecting a train focuses the camera and exposes timetable point, next point, ETA, required departure, delay, dispatcher state and RadioStop diagnostics. The HUD provides explicit dispatcher actions but does not directly mutate physical infrastructure.

## Passenger/station contract

`StationController` owns station lifecycle, train stop/dwell coordination, passenger generation and locomotive timetable gating. `Passenger` remains owned by a concrete `Wagon` while onboard. Automatic passenger transfers remain outside the contract.

## Locomotive timetable contract

`Locomotive.Schedule` owns an optional `LocomotiveSchedule`. `LocomotiveSchedulePoint` contains station, expected arrival and required departure. `LocomotiveScheduleRuntime` stores current point, cycle, actual arrival/departure, travel duration, delay and required departure.

Early arrival does not produce early departure. The timetable is cyclic. `TrainSchedule`/`ScheduleStorage` schema is `2`.

## Dispatcher/block contract

`RailwayDispatcher` arbitrates the next connected existing `Block` on a first-come-first-served basis. Requests retain target block and request timestamp; FCFS ordering is local to the requested block.

F6 calls `RailwayDispatcher.ForceProceed`. This bypasses dispatcher arbitration for the selected train only. It does not clear physical occupancy, alter signals or move switches. Route release operates through `RailwayDispatcher.NotifyReleased`.

The dispatcher does not set switches and does not change signal aspects. The player remains responsible for infrastructure configuration and unresolved operational situations.

## Coupling and movement contract

`CouplingService` is authoritative. `Composition.Vehicles` is physical order and is never reversed by coupling or decoupling. Coupling/decoupling remain manual.

F6 is now the explicit dispatcher override. F7 changes travel direction only at `0 km/h`. RadioStop remains a hard guard for normal automatic movement. Existing rigid-consist, trajectory, acceleration, braking and Vmax contracts remain unchanged.

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

No automated Core test project or CI build establishes compilation for this snapshot. A local Windows solution build and live UI verification remain required after pulling changes. For 0.2.3 specifically, verify F6 override, F9 timetable editing, F10 diagnostics, route release, AUTO/RĘCZ switching, multiple trains competing for one block, delay display and resized Myra layouts.

## AI rule

Before HUD changes inspect `MyraGameplayView`, `MyraUIManager`, `RailDispatchMonoGame`, `GameplayScreen`, `TrainManager`, `StationController`, `RailwayDispatcher`, `LocomotiveSchedule` and `WagonSchedule` together. Before timetable changes inspect the timetable runtime/persistence chain. Before block/safety changes inspect `Block`, `BlockController`, `RailwayDispatcher`, `SignalController`, `TrainMovement`, `TrainCollisionController` and `StationController` together. Before movement/coupling changes inspect `TrainComposition`, `CouplingService`, `TrainGeometry`, `TrainMovement`, `TrainDirection`, `SimulationScale` and vehicle-end connection contracts.

Every UI or runtime contract change must update the maintained architecture/current-state documentation and the relevant changelog.
