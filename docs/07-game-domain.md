# Game domain

## Current development line: `0.2.0`

The domain combines the established wagon-aware passenger model, rigid consist movement, signals, blocks, station service and timetable runtime.

## Railway subsystem

`Game/Railway` contains blocks, junctions, signals, track routes, stations and depots. Railway infrastructure remains domain-owned rather than screen-owned.

### Blocks and dispatcher

`BlockController` tracks physical train occupancy of blocks using the train head, vehicle positions and tail. `Block` stores occupancy and optional reservation state.

`RailwayDispatcher` is an operational arbitration layer over the existing block chain. It is first-come-first-served and only reserves the next already-connected block. It never changes switches or signal aspects. If the next block is occupied or reserved by another train, an automatically operated train waits.

### Signals

Signals already present in the game remain the infrastructure-control mechanism. 0.2.0 does not replace the signal system or make the dispatcher change signal aspects.

### Stations

`Station` is a world-domain object with identity, name, position/size and passenger-service parameters. `StationController` owns station lifecycle, train stop/dwell coordination, passenger-generation timing and station service.

In 0.2.0 `StationController` additionally coordinates the locomotive timetable departure gate before normal train movement and records scheduled locomotive arrivals.

## Timetables

### Locomotive

`Locomotive` can own an independent `LocomotiveSchedule`. `LocomotiveSchedulePoint` stores station identity, expected arrival and required departure. `LocomotiveScheduleRuntime` stores current point, cycle, actual arrival, delay and departure gate.

Early arrival never produces an early departure. After arrival, the train remains stopped until the required departure time. The timetable is cyclic.

### Wagon

`WagonSchedule` remains unchanged in principle: a wagon owns its repeating passenger-service timetable. Its route and runtime state remain independent of the locomotive timetable.

## Passenger subsystem

Passenger ownership remains at the concrete `Wagon` through `CurrentWagonId`. `Train` is only the current operational grouping of vehicles. Existing boarding/alighting and route-continuity rules remain active.

No automatic passenger-transfer system was added in 0.2.0. If a future transfer system is added, it must preserve wagon ownership and use the existing transfer seam rather than moving ownership to `Train`.

## Train and rolling stock

`TrainManager` owns train lifecycle. `TrainComposition` owns authoritative physical vehicle order. `Wagon` owns passenger state and wagon timetable. `Locomotive` owns locomotive timetable.

The established movement, F6, F7, coupling/decoupling, braking, Vmax and RadioStop contracts remain authoritative. 0.2.0 does not introduce full longitudinal train physics.

## Manual gameplay boundary

Automation does not remove the player's role. The player remains responsible for:

- switch/junction configuration;
- signal aspects;
- resolving a route that cannot be operated automatically;
- manual shunting;
- coupling and decoupling;
- taking over a train manually when required.

## Persistence

`TrainSchedule` is version `2` and can contain an optional locomotive timetable together with wagon schedule definitions. `ScheduleStorage` uses document schema `2`. Backward compatibility with pre-0.2.0 saves is not required.

## Domain ownership rule

Do not create parallel managers for state already owned by `TrainManager`, `TrainComposition`, `StationController`, `PassengerManager`, `CouplingService`, `BlockController` or `DepotController`. Screens and UI request domain operations; they do not mutate authoritative collections directly.
