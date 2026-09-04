# Known issues and cautions

## Current development line: `0.1.6g`

`0.1.6g` is the current documented baseline. Historical release notes remain in `docs/changelog/` and must not be rewritten to match later code.

## Passenger system — current boundary

Implemented passenger flow:

`StationController → PassengerManager → DefaultPassengerService → Wagon`

with destinations supplied through `IPassengerDemandProvider`.

Important invariants:

- passengers belong to concrete wagons, not directly to trains;
- boarded state keeps `CurrentWagonId`;
- `PassengerManager.GetOnBoard(Train)` is only a view over the train's current wagons;
- configured wagons validate their `TrainRoute` before boarding;
- `Wagon.CanContinueJourneyTo(...)` is the route-continuity check;
- coupling/decoupling does not migrate passengers;
- runtime load restores onboard passengers directly into their saved wagon rather than treating restore as new station boarding.

`GetTransferCandidates(Train)` is only a future transfer/diagnostic seam. Automatic transfers and passenger train selection are not implemented.

## Station cautions

Station stopping is coordinated by `StationController` and `ITrainStopDecision`. Passenger generation is timer-based per station and bounded by station waiting capacity. Dwell and exchange are domain state; UI is not authoritative.

## Train and rolling stock

- `TrainManager` owns train lifecycle.
- `TrainComposition` owns ordered vehicle state and derived consist statistics.
- `TrainComposition.EffectiveMaxSpeed` is the authoritative train Vmax capability.
- Signal restrictions are a separate runtime target and do not overwrite composition Vmax.
- Physical metre/grid conversion is centralized through `SimulationScale`.
- `F6` is manual shunting toward `3 km/h` and explicitly bypasses automatic RadioStop/collision stopping for the targeted train while held.
- `RadioStop` is a hard guard for normal `Train.Update(...)` movement.
- `F7` changes travel direction only at `0 km/h`; it does not reorder or reposition vehicles.
- Curve transforms use trajectory history and per-vehicle tangents.

## Coupling and decoupling

`CouplingService` is authoritative.

- coupling limit: `6 km/h`;
- decoupling requires speed `< 6 km/h`;
- candidate boundaries are order-preserving `Rear → Front`;
- merge clears stale runtime connections and rebuilds the chain from vehicle order;
- locomotive insertion/replacement rebuilds adjacent runtime connections;
- decoupling derives the split from adjacent vehicle indices and the actual runtime connection;
- `Composition.Vehicles` is never reversed by coupling or decoupling;
- `CompositionOrder` is metadata for that physical order, not a second vehicle container;
- coupling/decoupling preserve exact vehicle world positions at the operation instant;
- `CouplingGeometry` accounts for intrinsic `VehicleOrientation` when resolving physical front/rear geometry;
- passengers remain with their wagons.

Not implemented: slack/impact physics, coupling animation, brake-pipe propagation and full longitudinal vehicle dynamics.

## Platform solutions

The repository intentionally contains four platform hosts: Android, DesktopGL, WindowsDX and iOS. The current checked-in `.slnx` and legacy `.sln` both enumerate only Core + DesktopGL, while the Android/WindowsDX/iOS projects remain valid host projects with Core references. Do not delete the platform projects merely because they are absent from the desktop solution files.

## Persistence

Runtime save schema remains version `1`. Rolling-stock short labels are persisted. Runtime coupling connections are not persisted as a runtime graph. Current onboard passenger restoration targets the saved concrete wagon.

## UI

Myra uses one shared `Desktop` through `MyraUIManager`. Domain state remains owned by game subsystems. There is no dedicated transfer/economy/passenger-crowd UI.

## Verification

There is no dedicated automated Core test project in the repository. The normal solution build and live gameplay verification remain required after runtime changes. The repository currently has no CI check run confirming these changes.

## Rule for future work

For passenger/station changes inspect `StationController`, `PassengerManager`, `Passenger`, `Wagon`, `TrainRoute`, `ITrainStopDecision`, `IPassengerService` and `IPassengerDemandProvider` together. For coupling changes inspect `TrainComposition`, `CouplingService`, vehicle-end connection state and passenger ownership together. For movement/geometry changes inspect `Train`, `TrainMovement`, `TrainGeometry`, `TrainDirection`, `SimulationScale`, `VehicleOrientation` and all coupling geometry consumers together.
