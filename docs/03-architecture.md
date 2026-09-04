# Architecture

## Current development line

`0.1.6a` builds on the consolidated `0.1.5pre` architecture. The main 0.1.5 additions are now part of the normal domain contract: rolling-stock catalogue, consist performance, Depot train creation, rigid coupling/decoupling, manual shunting, F7 direction reversal and curve-aware per-vehicle transforms.

The station/passenger foundation is also an implemented domain subsystem and must not be treated as planned-only functionality.

## Core rule

`RailDispatchMono.Core` is the shared application/game layer. Platform projects host the application. Dependency direction points from platform hosts toward Core.

## Main runtime components

```text
Platform host
    |
    v
RailDispatchMonoGame
    |
    +--> GraphicsDeviceManager
    +--> MyraUIManager
    |
    v
ScreenManager
    |
    +--> GameScreen instances
    +--> InputState
    +--> SpriteBatch / shared resources
    |
    v
GameplayScreen
    |
    +--> railway/map services
    +--> TrainManager
    +--> StationController
            +--> PassengerManager
            +--> IPassengerService
            +--> IPassengerDemandProvider
```

## Ownership

- simulation/domain state belongs to `Game/` subsystems;
- train lifecycle belongs to `TrainManager`;
- ordered consist state/statistics belong to `TrainComposition`;
- coupling validation and mutation belong to `CouplingService`;
- station lifecycle, stop/dwell coordination and passenger-generation timing belong to `StationController`;
- passenger collection/state belongs to `PassengerManager`;
- passenger exchange policy belongs to `IPassengerService` / `DefaultPassengerService`;
- passenger destination demand belongs to `IPassengerDemandProvider` / `RandomPassengerDemandProvider`;
- rolling-stock catalogue data belongs to `Game/RollingStock`;
- depot ownership belongs to `DepotController`;
- presentation belongs to screens/Myra/renderers;
- persistence remains behind the existing save services.

## Passenger domain boundary

`StationController` coordinates station detection and train dwell. When a train reaches a serviceable station and `ITrainStopDecision` says it should stop, the controller invokes `IPassengerService.ServiceTrainAtStation()` and starts the configured dwell period.

`PassengerManager` owns the active passenger collection. `Passenger` stores origin, destination and runtime state (`WaitingAtStation`, `OnBoard`, `Arrived`). `Wagon` owns the concrete passenger list for that wagon and enforces capacity and route acceptance.

The default demand provider selects destinations randomly from other stations. This is explicitly a replaceable placeholder for a future population/city/demand model.

Transfers are not implemented. Passenger generation is time-based per station and bounded by station waiting capacity. Completed passengers can be removed from `PassengerManager`.

## 0.1.5 movement/consist contract

F7 changes only `Train.Direction`; it does not reorder `Composition.Vehicles`, move vehicles at the reversal instant or teleport the locomotive. Vehicle spacing remains unchanged. Curve following uses trajectory history and derives each vehicle's position/rotation from its own distance behind the head, so vehicles enter curves sequentially.

Manual F6 shunting targets the train under the cursor and moves it toward a fixed `3 km/h`, bypassing the automatic RadioStop/collision stop path for that targeted train while held.

## Safety and dependency discipline

1. Find the existing owner of state before adding a new manager/service.
2. Reuse existing managers/models rather than parallel globals.
3. Keep presentation out of domain mutation.
4. When changing acceleration, braking or Vmax, audit signal stopping and RadioStop.
5. When changing station/passenger flow, audit `StationController`, `PassengerManager`, `Wagon`, route handling and the active HUD together.
6. When changing constructors/data contracts, inspect save/load and catalogue factories.
