# AI context packet

## Current development line

**RailDispatchMono `0.1.6pre`** is the current consolidated development snapshot. The previous consolidated milestone is `0.1.5pre`.

Preserve the rigid-consist, movement, F6/F7, curve and coupling contracts. The 0.1.6 line adds wagon-owned passenger state and stabilises coupling/decoupling, runtime safety and diagnostics without introducing automatic transfers or economy.

## Passenger/station contract

`StationController` owns station lifecycle, train stop/dwell coordination and passenger-generation timing. It composes `PassengerManager`, `IPassengerService`, `IPassengerDemandProvider` and `ITrainStopDecision`.

`Passenger` has fixed origin/destination and runtime state. A boarded passenger belongs to a concrete `Wagon` and keeps `CurrentWagonId`. `PassengerManager.GetOnBoard(Train)` is an operational view, not an ownership boundary.

`Wagon` owns its passenger list, capacity and optional `TrainRoute`. Boarding must satisfy the wagon's route/destination invariant. `Wagon.CanContinueJourneyTo(...)` is the explicit continuity check.

`PassengerManager.GetTransferCandidates(Train)` is diagnostic/future-system infrastructure only. It does not select trains or move passengers automatically.

Runtime load restores an onboard passenger directly into its saved wagon. It must not re-run ordinary station boarding validation.

## Coupling contract

`CouplingService` is authoritative. `Composition.Vehicles` is physical order and is never reversed by coupling or decoupling.

Current `0.1.6pre` contract:

- locomotive insertion/replacement rebuilds adjacent runtime connections;
- merge clears stale runtime connections and rebuilds the chain from vehicle order;
- candidates use order-preserving `Rear → Front` outer boundaries;
- decoupling uses adjacent vehicle indices plus the actual runtime connection;
- locomotive–wagon and wagon–wagon compatible couplers use the same runtime contract;
- passengers remain with their wagons;
- coupling/decoupling preserve exact vehicle world positions at the operation instant;
- `CouplingGeometry` accounts for intrinsic `VehicleOrientation`;
- bounded coupling diagnostics capture the state required to diagnose post-merge movement.

## Movement controls

F6 is manual shunting toward `3 km/h` and bypasses automatic RadioStop/collision stopping for the targeted train while held. F7 changes travel direction only at `0 km/h`; it does not reverse the vehicle list or teleport vehicles.

The active travel head is derived from direction/reversal state. Vehicle movement distance and trajectory sampling use that head and the physical distance of each vehicle behind it.

`RadioStop` is a hard guard for normal automatic `Train.Update(...)` movement. Signal limits are separate from the consist capability and do not overwrite `TrainComposition.EffectiveMaxSpeed`.

Physical metres/grid conversion is centralised through `SimulationScale`.

## Architecture rules

- One authoritative train lifecycle owner: `TrainManager`.
- One authoritative ordered consist: `TrainComposition`.
- One coupling mutation boundary: `CouplingService`.
- One station lifecycle coordinator: `StationController`.
- One active passenger collection owner: `PassengerManager`.
- One shared Myra `Desktop`: `MyraUIManager`.
- One `TrackConnections.GetOppositeDirection()` extension for opposite-cardinal lookup.
- UI requests domain operations; it does not own domain state.

## Persistence

Runtime save schema remains version `1`. Rolling-stock short labels are persisted. Runtime coupling connections are not persisted as a runtime graph. Onboard passenger restoration targets the concrete saved wagon.

## Platform hosts

Android, DesktopGL, WindowsDX and iOS remain repository platform hosts. The checked-in `.slnx`/`.sln` currently enumerate Core + DesktopGL only; do not remove the other host projects solely because they are not solution members.

## Verification

Live gameplay has verified F7 reversal without locomotive/wagon interpenetration and stable wagon+locomotive coupling state before/after runtime connection rebuild. There is no dedicated automated Core test project and no CI check run establishing build success.

## AI rule

Before passenger/station changes inspect `StationController`, `Station`, `PassengerManager`, `Passenger`, `Wagon`, `TrainRoute`, `ITrainStopDecision`, `IPassengerService` and `IPassengerDemandProvider` together. Before coupling changes inspect `TrainComposition`, `CouplingService`, vehicle-end connection state and passenger ownership together. Before movement/geometry changes inspect `Train`, `TrainMovement`, `TrainGeometry`, `TrainDirection`, `SimulationScale`, `VehicleOrientation` and all coupling geometry consumers together.
