# Current State — RailDispatchMono `0.1.6pre`

**Snapshot date:** 2026-09-04  
**Status:** consolidated pre-release snapshot

## 1. Purpose

This document is the authoritative compact snapshot of the repository state at the end of the `0.1.6` development line. It supersedes the lettered `0.1.6a`–`0.1.6g` development notes for understanding the current contract. Those historical notes remain immutable.

If this document conflicts with source code or current call sites, source code and current call sites win.

## 2. Architecture

- `RailDispatchMono.Core` contains shared game/domain logic.
- Platform hosts remain separate; Android, DesktopGL, WindowsDX and iOS are retained in the repository.
- `ScreenManager` remains the screen lifecycle owner.
- Myra uses the shared `Desktop` through `MyraUIManager`; UI does not become a second domain-state owner.
- `TrainManager` owns train lifecycle.
- `TrainComposition` owns the authoritative physical vehicle order.
- `CouplingService` owns coupling/decoupling mutations.
- `StationController` owns station lifecycle and passenger service coordination.
- `PassengerManager` owns the active passenger collection.

## 3. Passenger model

A passenger belongs to a concrete `Wagon` once onboard. `Train` is an operational grouping of wagons, not the passenger ownership boundary.

Current contracts:

- boarding checks wagon capacity and configured route continuity;
- `CurrentWagonId` identifies the concrete onboard wagon;
- `Wagon.CanContinueJourneyTo(...)` is the journey-continuity invariant;
- `GetOnBoard(Train)` is an operational view;
- `GetTransferCandidates(Train)` is future/diagnostic infrastructure only;
- save/load restores an onboard passenger directly into its saved wagon;
- coupling and decoupling never migrate passengers.

Automatic transfers, passenger train selection, timetables, fares and economy are not implemented.

## 4. Consist ordering

`TrainComposition.Vehicles` is the single authoritative ordered collection of vehicles.

`Vehicle.CompositionOrder` is metadata describing physical composition order. It must not be treated as a second authoritative container and is independent of travel direction.

F7, coupling and decoupling do not reverse or reorder the vehicle list.

## 5. Direction and movement

### F7

- allowed only at `0 km/h`;
- changes travel `Direction`/reversal state without reversing the physical vehicle list;
- does not teleport the locomotive;
- preserves exact vehicle world positions and inter-vehicle spacing;
- reseeds trajectory history from the preserved positions.

### Travel head

The travel head is derived from direction/reversal state. Code must not assume that composition index `0` is always the moving head.

### Trajectory

Vehicle transforms are derived from trajectory history rather than a permanent per-vehicle offset table.

The trajectory contract is:

1. preserve exact vehicle positions when a discontinuity occurs;
2. order trajectory samples by travel distance;
3. use each vehicle's physical distance behind the active head;
4. derive vehicle rotation from the local trajectory tangent;
5. use the exact active curve tangent for the train head.

This prevents vehicles from rotating or collapsing into each other as a group on curves and during direction changes.

## 6. Coupling / decoupling

`CouplingService` is the authoritative domain operation.

### Coupling

- only compatible outer boundaries are eligible;
- candidate direction is order-preserving `Rear → Front`;
- coupling speed limit is fixed at `6 km/h`;
- stale runtime connections are cleared before rebuilding the merged chain;
- merged runtime connections are reconstructed from physical vehicle order;
- exact world positions are preserved across the operation;
- affected signal state is reset;
- the trailing train is removed from `TrainManager` after a successful merge.

### Decoupling

- allowed below `6 km/h`;
- split is determined from adjacent vehicle indices and the actual runtime connection;
- detached consists preserve physical order, runtime internal connections and positions;
- affected signal state is reset.

### Orientation

`VehicleOrientation`, including `Reverse`, is intrinsic vehicle state. Coupling geometry applies it consistently when deriving physical ends and endpoints. Travel direction is not encoded by reversing the composition list.

## 7. Safety and speed

- `RadioStop` is a hard guard for normal automatic movement in `Train.Update(...)`.
- F6 manual shunting is the explicit exception for the targeted train.
- F6 targets a fixed `3 km/h` shunting speed.
- `TrainComposition.EffectiveMaxSpeed` is the authoritative consist Vmax capability.
- Signal speed limits remain separate runtime restrictions.
- `Speed` is a non-negative magnitude.
- `TrackConnections.GetOppositeDirection()` is centralised as one extension method.
- Physical metre/grid conversion is centralised through `SimulationScale`.

## 8. Runtime diagnostics

Coupling diagnostics are event-driven and intentionally bounded. They can capture:

- both train states before merge;
- vehicle IDs/types/order/orientation/length;
- exact world positions;
- distance to travel head;
- transforms and rotations;
- state immediately after merge;
- state after runtime links are rebuilt;
- trajectory state.

`TrainDiagnostic` exists for these bounded high-value snapshots so diagnostic events are not silently discarded by the ordinary global log-rate limiter. It must not be used for per-frame logging.

## 9. Repository cleanup

Removed stale runtime/development artifacts include:

- `RailDispatchMono.sln.bak`;
- tracked `debug_log.txt` / `full_log.txt`;
- empty `Effects/Particle.cs`;
- obsolete `TrainDirectionPreservation.cs`.

Local `*_log.txt` and `*.sln.bak` are ignored.

## 10. Persistence

Runtime save schema remains version `1`. Rolling-stock short labels are persisted. Runtime coupling connections are not persisted as a runtime graph. Onboard passengers restore to their concrete saved wagons.

## 11. Known scope limits

The current rigid-consist model does not implement:

- slack action;
- impact forces;
- coupling animation/delay;
- brake-pipe propagation;
- full longitudinal vehicle dynamics;
- automatic passenger transfers;
- passenger train selection;
- fares or transport economy.

## 12. Verification state

The current runtime state has been live-tested for:

- F7 reversal without locomotive/wagon interpenetration;
- coupling of a wagon and locomotive with stable post-merge positions;
- runtime connection rebuild without changing those positions.

The latest coupling diagnostic snapshot showed a two-vehicle consist with consistent physical order and spacing before and after runtime link rebuilding. Post-coupling movement should remain part of regression testing.

There is no dedicated automated Core test project and no active CI check run establishing build success for this snapshot. A local solution build remains required before treating the snapshot as build-verified.

## 13. Rules for the next development line

- Do not restore vehicle-list reversal as a shortcut for direction handling.
- Do not introduce a second authoritative vehicle-order collection.
- Do not move coupling mutation into UI/input code.
- Do not use `CompositionOrder` as a replacement for `Composition.Vehicles`.
- Do not bypass `RadioStop` from normal automatic movement.
- Do not introduce a second metre/grid conversion boundary.
- Do not turn diagnostic logging into per-frame logging.
- Update maintained documentation and add a new changelog entry when the current contract changes.
