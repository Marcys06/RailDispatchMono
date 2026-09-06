# AI context packet

## Current development line

**RailDispatchMono `0.2.5`** is the current development snapshot. The 0.2.5 line makes infrastructure metadata operationally usable through multi-track selection and named railway-line grouping while preserving timetable, block, signal and player-control contracts.

## Infrastructure contract

`TrackCell` separates geometric shape (`TrackGeometry`) from operational infrastructure:

- `TrackType`: `Mainline`, `Secondary`, `Siding`, `Platform`;
- `LineClass`: `Local`, `Regional`, `Mainline`, `Magistral`;
- `TractionSystem`: `None`, `DC`, `AC`;
- `WearPercent` / `ConditionPercent` for future infrastructure wear.

`LineClassProfile` currently exposes prepared Vmax and axle-load limits. These are data for future infrastructure/physics rules; current movement remains unchanged.

## Traction contract

`TractionType` remains propulsion category (`Electric` / `Diesel`). Electric locomotives additionally declare supported `TractionSystem` values. EP07 is DC, EU200 is AC. Diesel locomotives are independent of track electrification. The model supports multi-system electric locomotives.

The project does not automatically reroute trains, replace locomotives or solve incompatible infrastructure.

## Named railway lines

`RailwayLine` is a player-defined logical collection of `MapPosition` track cells. `RailwayLineManager` owns line creation, deletion, membership, bulk infrastructure changes and connected-track discovery.

Named lines are metadata only. They do not replace `Block`, `SignalController`, junction state, route topology or timetable logic.

Each line has a stable ID, player-visible name and deterministic display color index. Removing a physical track removes that position from all lines.

## Multi-track selection and UI

`InputManager` owns the active map selection:

- LPM on existing track in `TrackBuildMode.None` replaces the selection;
- Shift+LPM adds;
- Ctrl+LPM toggles.

`TrackRenderer` visualizes named lines with colored contours and the active selection with a white contour while preserving traction color and line-class thickness.

F11 opens `MyraRailwayLineView`. It creates/selects/deletes named lines and applies `TrackType`, `LineClass` and `TractionSystem` to either the active selection or the selected line. It can also add/remove selection membership and select a connected track area.

## Modal GUI input lock

Any temporary Myra gameplay GUI is modal over the world. `MyraUIManager.IsGameplayOverlayOpen` becomes true when a temporary root replaces the gameplay root. `GameplayScreen` then skips `InputManager.Update()` entirely.

As a result, mouse input cannot fall through a GUI into the map: no track placement, track selection, object deletion, switch/signal interaction, camera movement or zoom is processed by the gameplay world while the GUI is open.

F8/F9/F10/F11 additionally reset `TrackBuilder.Mode` to `None` before opening their GUI, so closing the GUI does not leave a pending build action armed.

Any future gameplay GUI must preserve this contract.

## Persistence

`MapSaveData` schema is `3`. Named line persistence includes ID, name, color index and track positions. Old map schemas are intentionally not a compatibility target.

## Dispatcher/block contract

`RailwayDispatcher` still arbitrates the next connected existing `Block` on a first-come-first-served basis. F6 calls `RailwayDispatcher.ForceProceed`; it does not clear physical occupancy, alter signals or move switches. Route release operates through `RailwayDispatcher.NotifyReleased`.

The dispatcher does not set switches and does not change signal aspects. Infrastructure metadata and named lines are not a replacement for the block/signal layer.

## Coupling and movement contract

`CouplingService` is authoritative. `Composition.Vehicles` is physical order and is never reversed by coupling or decoupling. Coupling/decoupling remain manual. Existing rigid-consist, trajectory, acceleration, braking and Vmax contracts remain unchanged.

## UI contract

- F6 — dispatcher override;
- F8 — track infrastructure editor under cursor;
- F9 — locomotive timetable editor;
- F10 — railway diagnostics;
- F11 — named railway line and bulk infrastructure editor.

There is one shared Myra `Desktop`. UI requests domain operations; it does not own domain state.

## Roadmap contract

0.3.0 activates infrastructure management (electrification, classes, maintenance, wear), then 0.4.0 economy, 0.5.0 public timetable, 0.6.0 passenger demand, 0.7.0 physics, 0.8.0 crises, 0.9.0 network and 1.0.0 integrated release.

## Verification

No automated Core test project or CI build establishes compilation for this snapshot. A local Windows solution build and live UI verification remain required after pulling changes. For 0.2.5 verify F11 editing, Shift/Ctrl selection, named-line contours, schema 3 save/load, modal GUI input lock and unchanged block/signal/F6 behaviour.

## AI rule

Before infrastructure changes inspect `TrackCell`, `TrackBuilder`, `GameMap`, `MapSaveData`, `MapSaveService`, `RailwayLine`, `RailwayLineManager`, `LocomotiveDefinition`, `Locomotive`, `RollingStockCatalog`, `Block`, `BlockController`, `RailwayDispatcher`, `SignalController`, `TrainMovement` and `StationController` together. Before UI changes inspect `MyraGameplayView`, `MyraUIManager`, `RailDispatchMonoGame`, `GameplayScreen`, `InputManager`, `MyraRailwayLineView` and the relevant domain owner. Every runtime or UI contract change must update maintained architecture/current-state documentation and the relevant changelog.
