# AI context packet

## Current development line

**RailDispatchMono `0.2.0`** is the current development snapshot. The 0.2.0 line connects the existing station, signal and block systems with an operational locomotive timetable while preserving the player's infrastructure-control role.

## Passenger/station contract

`StationController` owns station lifecycle, train stop/dwell coordination and passenger-generation timing. In 0.2.0 it also gates automatic locomotive departures and records scheduled locomotive arrivals.

`Passenger` has fixed origin/destination and runtime state. A boarded passenger belongs to a concrete `Wagon` and keeps `CurrentWagonId`. `PassengerManager.GetOnBoard(Train)` is an operational view, not an ownership boundary.

`Wagon` owns its passenger list, capacity, service route and wagon timetable. Automatic transfers remain outside the 0.2.0 contract.

## Locomotive timetable contract

`Locomotive.Schedule` owns an optional `LocomotiveSchedule`. `LocomotiveSchedulePoint` contains station, expected arrival and required departure. `LocomotiveScheduleRuntime` stores current point, cycle, actual arrival, delay and required departure.

Early arrival is allowed but does not produce an early departure. The train waits until the required departure time. The timetable is cyclic.

`TrainSchedule`/`ScheduleStorage` schema is `2` and can persist the optional locomotive schedule alongside wagon schedule definitions.

## Dispatcher/block contract

`RailwayDispatcher` arbitrates the next connected existing `Block` on a first-come-first-served basis. It can hold automatic movement when the next block is occupied or reserved by another train.

The dispatcher does not set switches and does not change signal aspects. The player remains responsible for infrastructure configuration and for situations where the desired route cannot be operated.

## Coupling contract

`CouplingService` is authoritative. `Composition.Vehicles` is physical order and is never reversed by coupling or decoupling. Passengers remain with wagons. Coupling/decoupling remain manual.

## Movement controls

F6 is manual shunting. F7 changes travel direction only at `0 km/h`. RadioStop remains a hard guard for normal automatic movement. The established rigid-consist, trajectory, acceleration, braking and Vmax contracts remain unchanged.

## Architecture rules

- One authoritative train lifecycle owner: `TrainManager`.
- One authoritative ordered consist: `TrainComposition`.
- One coupling mutation boundary: `CouplingService`.
- One station lifecycle/timetable gate: `StationController`.
- One block occupancy owner: `BlockController`/`Block`.
- One block arbitration service: `RailwayDispatcher`.
- One active passenger collection owner: `PassengerManager`.
- One shared Myra `Desktop`: `MyraUIManager`.
- UI requests domain operations; it does not own domain state.

## Platform scope

0.2.0 implementation target is Windows. Other host projects remain in the repository unless a later cleanup explicitly removes them.

## Verification

No automated Core test project or CI build establishes compilation for this snapshot. A local Windows solution build and live gameplay verification remain required after pulling the changes.

## AI rule

Before timetable changes inspect `Locomotive`, `LocomotiveSchedule`, `TrainScheduleAutomation`, `TrainSchedule`, `ScheduleStorage`, `StationController`, `TrainManager` and `WagonSchedule` together. Before block/safety changes inspect `Block`, `BlockController`, `RailwayDispatcher`, `SignalController`, `TrainMovement`, `TrainCollisionController` and `StationController` together. Before movement/coupling changes inspect the existing `TrainComposition`, `CouplingService`, `TrainGeometry`, `TrainMovement`, `TrainDirection`, `SimulationScale` and vehicle-end connection contracts together.

Every change to the runtime contract must update the maintained architecture/current-state documentation and the relevant changelog.
