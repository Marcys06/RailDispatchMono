# Changelog

This file contains the high-level release history. The `0.1.6` lettered development stages have been consolidated into `0.1.6pre`; the historical lettered notes remain immutable.

## [0.1.6pre] — Consolidated 0.1.6 pre-release
**Data:** 2026-09-04

The `0.1.6` development line is consolidated here after stabilisation of passenger ownership, coupling/decoupling, consist ordering, movement geometry, direction reversal, runtime safety and diagnostics.

### Passenger and journey model

- Boarding is performed against a concrete `Wagon`.
- Passenger state keeps `CurrentWagonId` and remains owned by the concrete wagon rather than the operational `Train` grouping.
- `Wagon.CanContinueJourneyTo(...)` defines route continuity for boarding/journey continuation.
- `PassengerManager.GetOnBoard(Train)` is an operational view over passengers in the train's current wagons.
- `PassengerManager.GetTransferCandidates(Train)` is a future transfer/diagnostic seam; it does not select trains or move passengers automatically.
- Runtime load restores onboard passengers directly into their saved wagon.
- Coupling and decoupling never migrate passengers between wagons.

### Consist ordering and coupling

- `TrainComposition` is the single authoritative ordered vehicle container.
- `Vehicle.CompositionOrder` explicitly records physical composition order without becoming a second source of truth.
- F7, coupling and decoupling never reverse `Composition.Vehicles`.
- `TrainComposition.SetLocomotive(...)` rebuilds adjacent runtime coupling connections.
- Coupling candidates are restricted to compatible order-preserving `Rear → Front` outer boundaries.
- Coupling clears stale runtime connections and rebuilds the complete runtime coupling chain from physical vehicle order.
- Decoupling uses adjacent vehicle indices and the actual runtime connection.
- Coupling/decoupling preserve exact vehicle world positions at the operation instant.
- `CouplingGeometry` consistently accounts for intrinsic `VehicleOrientation`, including `Reverse`.
- Coupling remains limited to `6 km/h`; decoupling remains below `6 km/h`.

### Train direction, movement and geometry

- F7 changes travel direction only at `0 km/h` without reversing or reordering the physical vehicle list.
- The travel head is determined from direction/reversal state rather than assuming composition index `0`.
- Vehicle positions and inter-vehicle spacing are preserved across direction changes and coupling operations.
- Movement distance is measured from the active travel head.
- Trajectory history is seeded from exact world positions and ordered according to travel direction.
- Vehicles sample their own position from trajectory history using their physical distance behind the travel head.
- Vehicle rotation follows the local trajectory tangent; the train head uses the exact active curve-arc tangent.
- Vehicle lengths and movement use `SimulationScale` as the physical metres/grid conversion boundary.
- `Speed` remains a non-negative magnitude; `IsReversed` remains travel-state information rather than a vehicle-list mutation.

### Runtime safety and speed state

- `RadioStop` is a hard guard against normal automatic movement in `Train.Update(...)`.
- F6 manual shunting is the explicit exception and can clear/bypass automatic RadioStop/collision stopping for the targeted train while held.
- `TrainComposition.EffectiveMaxSpeed` is the authoritative consist Vmax capability.
- Signal speed restrictions are separate runtime limits and do not overwrite the consist capability.
- Duplicate signal-speed state was removed.
- `TrackConnections.GetOppositeDirection()` is centralised as one railway extension method.

### Diagnostics and maintenance

- Coupling diagnostics capture bounded before/after snapshots of train state, vehicle order, positions, distances, transforms and trajectory state.
- High-value coupling diagnostics use the dedicated train diagnostic path so they are not lost behind the normal global log-rate limiter.
- Stale `.bak`, debug-log, empty particle and obsolete direction-preservation artifacts were removed.
- Local `*_log.txt` and `.sln.bak` files are ignored.
- Android, WindowsDX and iOS host projects remain in the repository and are not removed merely because the checked-in solution files enumerate Core + DesktopGL.

### Scope and verification

The 0.1.6pre milestone remains a rigid consist model. Slack action, impact forces, coupling delay/animation, brake-pipe propagation, automatic passenger transfers, passenger train selection, fares/economy and full longitudinal vehicle dynamics remain outside scope.

There is no dedicated automated Core test project and no CI check run establishing build success for this snapshot. Required validation is a normal solution build followed by live gameplay verification, including F7 reversal, coupling/decoupling and movement immediately after coupling.

Detailed consolidated notes: `docs/changelog/0.1.6pre.md`.

## [0.1.6g] — Runtime safety and geometry cleanup
**Data:** 2026-09-04

Historical development stage. See `docs/changelog/0.1.6g.md`.

## [0.1.6f] — Explicit consist ordering

Historical development stage. See `docs/changelog/0.1.6f.md`.

## [0.1.6e] — Coupling/decoupling stabilisation

Historical development stage. See `docs/changelog/0.1.6e.md`.

## [0.1.6d] — Passenger journey continuity
**Data:** 2026-09-04

Historical development stage. See `docs/changelog/0.1.6d.md`.

## [0.1.6c] — Wagon-aware passenger boarding

Historical development stage. See `docs/changelog/0.1.6c.md`.

## [0.1.5pre] — Final 0.1.5 pre-release
**Data:** 2026-09-03

Historical consolidated milestone. Detailed notes: `docs/changelog/0.1.5pre.md`.
