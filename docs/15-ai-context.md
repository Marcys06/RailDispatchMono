# AI context packet

## Current development line

**RailDispatchMono `0.2.4`** is the current development snapshot. The 0.2.4 line adds infrastructure metadata and electrical traction compatibility while preserving the timetable, block, signal and player-control contracts.

## Infrastructure contract

`TrackCell` separates geometric shape (`TrackGeometry`) from operational infrastructure:

- `TrackType`: `Mainline`, `Secondary`, `Siding`, `Platform`;
- `LineClass`: `Local`, `Regional`, `Mainline`, `Magistral`;
- `TractionSystem`: `None`, `DC`, `AC`;
- `WearPercent` / `ConditionPercent` for future infrastructure wear.

`LineClassProfile` currently exposes prepared Vmax and axle-load limits. These are data for future infrastructure/physics rules; 0.2.4 does not replace the existing movement model with them.

## Traction contract

`TractionType` remains propulsion category (`Electric` / `Diesel`). Electric locomotives additionally declare a set of supported `TractionSystem` values. EP07 is DC, EU200 is AC. Diesel locomotives are independent of track electrification. The model supports multi-system electric locomotives.

0.2.4 does not automatically reroute trains, replace locomotives or solve incompatible infrastructure. No automatic “route infeasible” planning layer was added.

## Infrastructure editing

`TrackBuilder` carries selected infrastructure defaults for newly built track. It also exposes metadata-only configuration methods for existing cells.

F8 opens `MyraTrackInfrastructureView` for the track under the cursor. The view can change track type, line class and traction without changing geometry, connections, blocks, signals or switches.

## Persistence

`MapSaveData` schema is `2`. Track persistence includes geometry, connections, switch position, track type, line class, traction and wear. Old saves are not a compatibility target.

## Dispatcher/block contract

`RailwayDispatcher` still arbitrates the next connected existing `Block` on a first-come-first-served basis. F6 calls `RailwayDispatcher.ForceProceed`; it does not clear physical occupancy, alter signals or move switches. Route release operates through `RailwayDispatcher.NotifyReleased`.

The dispatcher does not set switches and does not change signal aspects. Infrastructure metadata is not a replacement for the block/signal layer.

## Coupling and movement contract

`CouplingService` is authoritative. `Composition.Vehicles` is physical order and is never reversed by coupling or decoupling. Coupling/decoupling remain manual. Existing rigid-consist, trajectory, acceleration, braking and Vmax contracts remain unchanged.

## UI contract

- F6 — dispatcher override;
- F8 — track infrastructure editor under cursor;
- F9 — locomotive timetable editor;
- F10 — railway diagnostics.

There is one shared Myra `Desktop`. UI requests domain operations; it does not own domain state.

## Roadmap contract

0.3.0 activates infrastructure management (electrification, classes, maintenance, wear), then 0.4.0 economy, 0.5.0 public timetable, 0.6.0 passenger demand, 0.7.0 physics, 0.8.0 crises, 0.9.0 network and 1.0.0 integrated release.

## Verification

No automated Core test project or CI build establishes compilation for this snapshot. A local Windows solution build and live UI verification remain required after pulling changes. For 0.2.4 verify F8 editing, new-track defaults, map save/load schema 2, EP07/EU200/SU42 traction metadata and unchanged block/signal/F6 behaviour.

## AI rule

Before infrastructure changes inspect `TrackCell`, `TrackBuilder`, `GameMap`, `MapSaveData`, `MapSaveService`, `LocomotiveDefinition`, `Locomotive`, `RollingStockCatalog`, `Block`, `BlockController`, `RailwayDispatcher`, `SignalController`, `TrainMovement` and `StationController` together. Before UI changes inspect `MyraGameplayView`, `MyraUIManager`, `RailDispatchMonoGame` and the relevant domain owner. Every runtime or UI contract change must update maintained architecture/current-state documentation and the relevant changelog.
