# AI context packet

## Current development line

**RailDispatchMono `0.1.6a`** is the current development line. The previous consolidated milestone is `0.1.5pre`.

The 0.1.5 work that must be preserved includes the rolling-stock catalogue, mass/power performance, speed-aware safety, Depot builder, rigid coupling/decoupling, manual F6 shunting, F7 direction reversal, rigid vehicle spacing and curve trajectory/per-vehicle orientation.

The project also already contains a working station/passenger foundation. Do not recreate it as a new subsystem.

## Passenger/station contract

`StationController` owns station lifecycle, train stop/dwell coordination and passenger-generation timing. It composes:

- `PassengerManager` for active passenger state;
- `IPassengerService` for train/station exchange;
- `IPassengerDemandProvider` for destination demand;
- `ITrainStopDecision` for whether a train should stop.

`Station` stores geometry plus stop/dwell and passenger-generation settings.

`Passenger` has a fixed origin and destination and one of three states: `WaitingAtStation`, `OnBoard`, `Arrived`. It also tracks current station/train IDs and creation time.

`Wagon` owns its concrete passenger list. Boarding requires a passenger wagon, free capacity and, when configured, a service route that can serve the destination. At station service, alighting occurs before boarding.

The default demand model is `RandomPassengerDemandProvider`: destinations are selected uniformly from other registered stations. This is a temporary implementation behind an interface, not the final demand model.

Current passenger limitations: no transfers, no timetable-aware passenger choice, no population/city model, no fares/revenue, no satisfaction model, no persistent passenger state and no visual platform crowds.

## 0.1.5 train contracts

### F7

F7 is a direction change, not physical consist reversal. At `0 km/h` it changes `Train.Direction` only. `Composition.Vehicles`, world positions and inter-vehicle distances remain unchanged. The locomotive is not teleported to the opposite end.

### Curves

Vehicle positions and rotations on curves use the travelled trajectory and each vehicle's physical distance behind the head. Vehicles therefore enter/leave curves sequentially. With insufficient trajectory history, rigid straight-track offsets are used.

### Coupling

`CouplingService` is authoritative. `C` couples the nearest valid outer-boundary candidate using a fixed `6 km/h` limit. `X` operates on a wagon under the cursor and requires train speed `< 6 km/h`. Composition order is preserved; compatible adjacent vehicles can initialize runtime coupling automatically.

### F6

F6 is manual shunting, not coupling-speed selection. While held over a train it moves that train toward a fixed `3 km/h`, using the normal consist acceleration model but bypassing automatic RadioStop/collision stopping for that targeted train.

## Architecture rules

- One authoritative train lifecycle owner: `TrainManager`.
- One authoritative ordered consist: `TrainComposition`.
- One coupling mutation boundary: `CouplingService`.
- One station lifecycle coordinator: `StationController`.
- One active passenger collection owner: `PassengerManager`.
- One shared Myra `Desktop`: `MyraUIManager`.
- Screens/UI request domain operations; they do not mutate authoritative collections directly.

## Persistence

Runtime save schema remains version `1`. Rolling-stock short labels are persisted. Individual coupling connections and passenger runtime state are not currently persisted.

## Verification

There is no dedicated automated Core test project. Verify changes with the normal solution build and live gameplay in the user's .NET/MonoGame environment.

## AI rule

For passenger/station work, inspect `StationController`, `Station`, `PassengerManager`, `Passenger`, `Wagon`, `TrainRoute`, `ITrainStopDecision`, `IPassengerService` and `IPassengerDemandProvider` together before introducing new abstractions.
