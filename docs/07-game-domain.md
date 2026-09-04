# Game domain

## Current development line: `0.1.6pre`

The domain combines the wagon-aware passenger model with the stabilised rigid-consist, movement, F6/F7, curve and coupling/decoupling model.

## Railway subsystem

`Game/Railway` contains blocks, junctions, signals, track routes, stations and depots. Railway infrastructure remains domain-owned rather than screen-owned.

### Stations

`Station` is a world-domain object with identity, name, position/size and passenger-service parameters:

- `StopRadius`;
- `DwellTimeSeconds`;
- `PassengerServiceEnabled`;
- `PassengerGenerationEnabled`;
- `PassengerGenerationIntervalSeconds`;
- `PassengerGenerationBatchSize`;
- `PassengerWaitingCapacity`.

`StationController` owns station lifecycle, train stop/dwell coordination, passenger-generation timing and station service. `ITrainStopDecision` remains the stop-decision boundary.

## Passenger subsystem

The passenger subsystem is an implemented operational vertical slice, not a complete transport-economy simulation.

### Passenger ownership

`PassengerManager` owns the active passenger collection. A `Passenger` has fixed origin/destination and runtime state. When onboard, the passenger is associated with a concrete `Wagon` through `CurrentWagonId`.

`Train` is only the current operational grouping of wagons. `PassengerManager.GetOnBoard(Train)` is therefore a view over passengers in the train's current wagons, not the ownership model.

### Boarding

Boarding is performed against a concrete `Wagon`. The wagon checks capacity and, when a `TrainRoute` is configured, verifies that the route can serve the passenger's destination. `Wagon.CanContinueJourneyTo(...)` is the explicit route-continuity invariant.

`DefaultPassengerService` performs alighting before boarding. Coupling and decoupling never migrate passengers between wagons.

### Journey continuity and transfers

`PassengerManager.GetTransferCandidates(Train)` is a future transfer-system/diagnostic seam. It does not select a train and does not move passengers automatically.

Not implemented:

- automatic transfers;
- passenger train selection;
- timetable-aware route choice;
- population/city demand model;
- fares, revenue or operating economics;
- satisfaction/wait-time scoring;
- visual platform crowds.

### Save/load

Runtime restoration of an already-onboard passenger targets its saved concrete wagon directly. It does not run normal station-boarding validation against the current station.

## Train and rolling stock

`TrainManager` owns train lifecycle. `TrainComposition` owns the authoritative ordered vehicle collection and derived consist statistics. `Vehicle` owns static coupling metadata and runtime coupling state. `Wagon` additionally owns its passenger collection and service route.

### Ordering

`Composition.Vehicles` is the only authoritative physical vehicle order. `Vehicle.CompositionOrder` is metadata assigned and normalised by `TrainComposition`; it is not a second collection and is independent from travel direction.

### Performance

Acceleration and braking use the non-linear consist mass factor. Train Vmax is derived from `TrainComposition.EffectiveMaxSpeed`; signal restrictions are maintained as a separate runtime target and never overwrite the composition capability.

Physical distance conversion is centralised through `SimulationScale`: train speed is maintained in metres/second and movement distance is converted with `SimulationScale.MetersToGrid(...)`. Legacy grid-length conversion to metres also uses `SimulationScale`.

### F6/F7 and movement

- `F6` is manual shunting toward a fixed `3 km/h` for the train under the cursor and bypasses automatic RadioStop/collision stopping for that targeted train while held.
- `F7` changes travel `Direction` only at `0 km/h`.
- F7 never reverses `Composition.Vehicles`, never reorders vehicles and does not teleport the locomotive.
- The active travel head is selected from direction/reversal state; movement distance is measured from that head rather than assuming vehicle index `0` is always the head.
- Vehicle positions and spacing are preserved at the reversal instant.
- Trajectory history is seeded from exact world positions and ordered according to travel direction.
- Curve movement uses trajectory history and each vehicle's physical distance behind the head to derive position and local tangent.
- `RadioStop` is a hard movement guard for normal automatic updates; manual F6 shunting explicitly clears/bypasses it for the targeted train.

## Coupling and decoupling

`CouplingService` is the authoritative mutation boundary.

- Coupling is limited to compatible outer train boundaries and a fixed `6 km/h` limit.
- Candidates are order-preserving `Rear → Front` boundaries.
- Merge clears stale runtime connections and rebuilds the full connection chain from the new vehicle order.
- Locomotive insertion/replacement rebuilds adjacent runtime connections.
- Decoupling identifies the split from adjacent vehicle indices and the actual runtime connection.
- `Composition.Vehicles` is never reversed by coupling or decoupling.
- Coupling/decoupling preserve the exact world position of every vehicle at the operation instant.
- `CouplingGeometry` applies `VehicleOrientation` consistently when deriving physical front/rear directions and endpoints.
- Coupling diagnostics can snapshot train state, vehicle order, positions, distances, transforms and trajectory around the merge to diagnose post-coupling movement without per-frame log spam.
- Coupling and decoupling do not migrate passengers.

No slack action, impact forces, coupling animation, brake-pipe propagation or full longitudinal vehicle dynamics are implemented.

## Persistence

Runtime save schema remains version `1`. Rolling-stock short labels are persisted. Runtime coupling connections are not persisted as a runtime graph. Current passenger restoration uses the concrete saved wagon when onboard state is represented in the save.

## Domain ownership rule

Do not create parallel managers for state already owned by `TrainManager`, `TrainComposition`, `StationController`, `PassengerManager`, `CouplingService` or `DepotController`. Screens and UI request domain operations; they do not mutate authoritative collections directly.
