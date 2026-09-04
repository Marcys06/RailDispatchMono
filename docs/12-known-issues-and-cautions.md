# Known issues and cautions

## Current development line: `0.1.6a`

The previous consolidated milestone is `0.1.5pre`. The 0.1.6 line currently starts with documentation alignment; no new passenger gameplay feature is claimed by `0.1.6a` itself.

## Passenger system — current boundary

The station/passenger foundation is implemented and operational at the domain level:

- `Station` stores stop, dwell and passenger-generation parameters;
- `StationController` detects station visits, controls dwell and invokes passenger service;
- `PassengerManager` owns active passengers;
- `Passenger` has fixed origin/destination and three states;
- `Wagon` owns boarded passengers and enforces capacity/route acceptance;
- `DefaultPassengerService` performs alighting before boarding;
- `RandomPassengerDemandProvider` supplies temporary random destinations.

Do not describe this as a complete passenger simulation. Transfers, timetable-aware route choice, population/demand modelling, fares/revenue, satisfaction, passenger persistence and visual platform crowds are not implemented.

## Station cautions

Station stopping is currently based on `ITrainStopDecision`, train position/cell geometry, speed and the station's configured dwell time. The controller uses a station-visit guard so a train is not repeatedly serviced while remaining inside the same station area.

Passenger generation is timer-based and independent per station. Waiting passengers are bounded by `PassengerWaitingCapacity`.

## Train and rolling stock

- `TrainManager` is the authoritative train lifecycle owner.
- `TrainComposition` owns ordered vehicles and derived consist statistics.
- Acceleration/braking use the non-linear total-mass factor with exponent `1.30`.
- Locomotive power can reduce Vmax for heavy consists.
- Signal stopping and RadioStop use effective consist braking.
- `F6` manual shunting targets the train under the cursor and moves toward `3 km/h` while bypassing automatic RadioStop/collision stopping for that train.
- `F7` reverses travel direction only at `0 km/h`; it does not reorder or reposition vehicles.
- Curves use trajectory history and per-vehicle tangents.

## Coupling and decoupling

Rigid runtime coupling/decoupling is implemented through `CouplingService`.

- `C` couples the nearest valid outer-boundary candidate at a fixed `6 km/h` limit.
- `X` acts on a wagon under the cursor and requires train speed `< 6 km/h`.
- Vehicle order is preserved across merge/split operations.
- Compatible adjacent vehicles in a composition receive runtime coupling connections automatically.
- Individual coupling connections are not persisted.
- Dynamic slack, impact forces, coupling animation, brake-pipe propagation and full longitudinal dynamics are not implemented.

## Persistence

Runtime save schema remains version `1`. Rolling-stock `ShortName` is persisted and older saves without it remain compatible. Passenger state and runtime coupling connections are not currently persisted.

## UI

Myra uses one shared `Desktop` through `MyraUIManager`. Gameplay HUD, station/train information and Depot builder are presentation surfaces; domain state remains owned by the corresponding game subsystems.

## Verification

There is no dedicated automated Core test project. Runtime changes require the normal solution build and live gameplay verification in the user's .NET/MonoGame environment.

## Rule for future work

When extending passengers or stations, inspect `StationController`, `PassengerManager`, `Passenger`, `Wagon`, `TrainRoute`, `ITrainStopDecision`, `IPassengerService` and `IPassengerDemandProvider` together before adding new ownership or parallel managers.
