# Game domain

## Current development line: `0.1.6a`

The current implementation builds on the completed `0.1.5pre` train/movement line. The domain now includes rolling-stock definitions, mass/power performance, station handling, a basic passenger vertical slice and rigid consist coupling/decoupling.

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

`StationController` owns the station collection and coordinates train detection, stop decisions, dwell state, passenger generation and station service. It uses `ITrainStopDecision` to determine whether a train should stop.

A train is serviced only after it has reached the station and is sufficiently slow. During dwell, the train is held at zero speed. A completed station visit is tracked so the same train is not serviced repeatedly until it leaves the station area.

## Passenger subsystem

The passenger subsystem is implemented under `Game/Passengers` and currently represents a basic operational vertical slice rather than a full passenger/economy simulation.

### Passenger model

`Passenger` stores:

- unique ID;
- origin `Station`;
- destination `Station`;
- state;
- current station ID;
- current train ID;
- creation timestamp.

`PassengerState` currently has three states:

- `WaitingAtStation`;
- `OnBoard`;
- `Arrived`.

Passengers have fixed origin and destination. Transfers are not implemented.

### Passenger manager

`PassengerManager` owns the active passenger collection and provides waiting/on-board queries plus boarding and alighting operations. It also raises `PassengerExchange` events and forwards exchange notifications to `FloatingTextManager`.

Completed passengers can be removed with `RemoveCompletedPassengers()`.

### Station passenger generation

`StationController.Update()` advances a generation timer independently for each station. When the timer expires, it requests destinations from `IPassengerDemandProvider` and creates passengers up to the station's configured batch size and waiting capacity.

The default `RandomPassengerDemandProvider` chooses uniformly from all other registered stations. This is deliberately isolated behind the interface so a future city/population/demand model can replace it without changing station or wagon ownership.

### Wagon passenger state

`Wagon` owns its concrete passenger list and exposes capacity/count information. A passenger can board only a passenger wagon with free capacity and, when a service route is configured, a route that can serve the passenger's destination.

At a station, `DefaultPassengerService` performs alighting first and boarding second. The resulting exchange is reported as `PassengerServiceResult`.

### Current passenger limitations

Not implemented yet:

- transfers;
- route choice between competing trains;
- timetable-aware passenger decisions;
- population/city demand model;
- fares, revenue or operating economics;
- passenger satisfaction/wait-time scoring;
- persistent passenger state in runtime saves;
- passenger-specific classes/types beyond the current fixed model;
- visual crowds/individual passenger entities on platforms.

## Train subsystem

`TrainManager` owns train lifecycle. `TrainComposition` is the authoritative ordered vehicle collection and owns derived mass, length, wagon count and Vmax data. `Vehicle` owns static coupling metadata and runtime coupling state.

## 0.1.5 consist and movement contract

### Mass-dependent performance

Acceleration and braking use locomotive capability multiplied by the non-linear consist mass factor:

`factor = 1 / (totalMass / locomotiveMass)^1.30`

Locomotive power can independently reduce Vmax for heavy consists; wagon Vmax remains an additional cap.

### Rigid coupling/decoupling

`CouplingService` is authoritative for validation and mutation. Connections link concrete vehicle ends. Coupling is limited to compatible outer train boundaries and a fixed `6 km/h` coupling/shunting limit. `X` targets a wagon under the cursor and decouples only below `6 km/h`.

Vehicle order is preserved when consists are merged or split. Adjacent compatible vehicles in a newly built composition automatically receive runtime coupling connections.

No slack, impact forces, coupling animation, brake-pipe propagation or persistence of individual coupling connections is implemented.

### F6/F7

`F6` is manual shunting toward a fixed `3 km/h` for the train under the cursor and bypasses automatic RadioStop/collision stopping for that targeted train while held.

`F7` reverses only the train travel direction and is accepted at `0 km/h`. `Composition.Vehicles`, world positions and inter-vehicle spacing are unchanged at the reversal instant.

Curve following uses trajectory history. Each vehicle obtains its own historical position and local tangent, so vehicles enter and leave curves sequentially rather than rotating simultaneously with the head.

## Save/load

Runtime save schema remains version `1`. Rolling-stock short labels are persisted and old saves without the field remain compatible. Runtime coupling connections and passenger state are not currently persisted.

## Domain ownership rule

Do not create parallel managers for state already owned by `TrainManager`, `TrainComposition`, `StationController`, `PassengerManager`, `CouplingService` or `DepotController`. Screens and UI request domain operations; they do not mutate authoritative collections directly.
