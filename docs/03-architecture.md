# Architecture

## Current development line

`0.1.6e` is the current documented development baseline. It builds on the completed `0.1.5pre` movement/consist contract and the `0.1.6c`/`0.1.6d` passenger model.

The important 0.1.6 rule is that a passenger belongs to a concrete `Wagon`, while `Train` is only the current operational grouping of wagons. Coupling and decoupling therefore do not migrate passenger ownership.

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
- ordered physical consist state belongs to `TrainComposition`;
- coupling validation/mutation belongs to `CouplingService`;
- station lifecycle, stop/dwell and passenger generation belong to `StationController`;
- active passenger collection belongs to `PassengerManager`;
- station passenger exchange policy belongs to `IPassengerService` / `DefaultPassengerService`;
- passenger destination demand belongs to `IPassengerDemandProvider` / `RandomPassengerDemandProvider`;
- wagon passenger ownership and service-route acceptance belong to `Wagon`;
- rolling-stock catalogue data belongs to `Game/RollingStock`;
- depot ownership belongs to `DepotController`;
- presentation belongs to screens/Myra/renderers;
- persistence remains behind the existing save services.

## Passenger domain boundary

The implemented passenger flow is:

`StationController → PassengerManager → PassengerService → Wagon`

with demand supplied through `IPassengerDemandProvider`.

`Passenger` has fixed origin/destination and runtime state. A boarded passenger records the concrete wagon (`CurrentWagonId`). `PassengerManager.GetOnBoard(Train)` is an operational view over passengers in the wagons currently forming that train; it is not the ownership boundary.

A configured wagon validates its current `TrainRoute` before accepting a passenger. `Wagon.CanContinueJourneyTo(...)` is the explicit route-continuity invariant used by the 0.1.6d model.

`PassengerManager.GetTransferCandidates(Train)` is a diagnostic/future-system seam only. It does not choose a train and does not perform automatic transfers.

Runtime load restores an already-onboard passenger directly into its saved wagon instead of treating the restore as a new station boarding operation.

## Consist and movement contract

The `0.1.5pre` rigid-consist rules remain authoritative:

- `Composition.Vehicles` is the physical order and is not reversed by F7, coupling or decoupling;
- F7 changes only travel `Direction` and is accepted at `0 km/h`;
- vehicle world positions and spacing are preserved at F7;
- curve movement uses trajectory history and per-vehicle distance/tangent;
- F6 is manual shunting toward a fixed `3 km/h` and bypasses automatic RadioStop/collision stopping for the targeted train while held;
- coupling uses a fixed `6 km/h` limit;
- decoupling requires speed below `6 km/h`.

## Coupling contract in 0.1.6e

`CouplingService` is authoritative. Runtime connections are attached to concrete vehicle ends.

- locomotive insertion/replacement rebuilds adjacent runtime connections;
- merging clears stale runtime connections and rebuilds the full chain from vehicle order;
- coupling candidates use only order-preserving `Rear → Front` outer boundaries;
- decoupling finds the split from adjacent vehicle indices plus the actual runtime connection;
- locomotive–wagon coupling uses the same compatible coupler contract as wagon–wagon coupling;
- no passenger migration occurs during merge/split.

## Persistence

Runtime save schema remains version `1`. Rolling-stock short labels are persisted. Runtime coupling connections and passenger runtime state are not persisted as independent runtime graphs; onboard passengers are restored into their saved concrete wagon when represented by the current save data.

## Safety and dependency discipline

1. Find the existing owner of state before adding a new manager/service.
2. Reuse existing managers/models rather than parallel globals.
3. Keep presentation out of domain mutation.
4. When changing acceleration, braking or Vmax, audit signal stopping and RadioStop.
5. When changing station/passenger flow, audit `StationController`, `PassengerManager`, `Wagon`, `TrainRoute` and the active HUD together.
6. When changing coupling, audit `TrainComposition`, `CouplingService`, vehicle-end connections and passenger ownership together.
7. When changing constructors/data contracts, inspect save/load and catalogue factories.
