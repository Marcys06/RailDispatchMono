# RailDispatchMono — current state `0.1.5pre`

**Date:** 2026-09-03  
**Status:** consolidated development/pre-release milestone

This document is the authoritative current-state snapshot for the `0.1.5` line. Lettered `0.1.5a`–`0.1.5f` stages remain historical development records in `CHANGELOG.md` and `docs/changelog/`.

## 1. Technology and repository structure

- C# / .NET 9 shared Core project.
- MonoGame is the shared game framework.
- Myra `1.6.5` is the shared UI framework.
- `RailDispatchMono/RailDispatchMono.Core` is the active shared Core project referenced by the solution.
- Platform hosts include Android, DesktopGL, WindowsDX and iOS.
- The solution contains the shared Core project and DesktopGL host; there is no separate Core test project.

## 2. Runtime and screen architecture

`RailDispatchMonoGame` owns the main game loop and shared graphics/UI infrastructure. `ScreenManager` owns screen lifecycle, update/input routing and drawing. `GameplayScreen` is the primary gameplay screen.

`MyraUIManager` is the single Myra integration boundary. It owns one shared `Desktop` and one active root. Main Menu, Settings, About, Pause, gameplay HUD and Depot builder use this infrastructure.

Pause is a gameplay state owned by `GameplayScreen`, not a popup `GameScreen`. `MyraPauseView` is presentation only. `DepotScreen` is a full-screen `GameScreen`; opening it covers gameplay, while closing it restores the gameplay Myra root.

## 3. Input and Depot workflow

`InputState`/`InputManager` remain the shared input architecture for gameplay/UI. Depot mode is selected with `9` / NumPad `9`. Selecting an existing Depot raises `InputManager.DepotSelected` and opens `DepotScreen`.

The Depot builder supports one locomotive, zero or more passenger wagons, wagon editing, live composition statistics and creation through `TrainManager.CreateTrainFromComposition()`.

Coupling commands are currently handled by the existing `TrainManager.HandleCouplingHotkeys()` path. This is the current `0.1.5` command path and must not be duplicated by UI code.

## 4. Train and rolling stock domain

`TrainManager` owns train lifecycle. `TrainComposition` is the authoritative ordered vehicle collection and owns derived consist statistics. `Vehicle` owns static coupling metadata and runtime coupling state.

Runtime coupling state is represented by `VehicleCouplingState` and `CouplingConnection`. `CouplingService` is the authoritative operation/validation boundary for coupling and decoupling.

## 5. Rigid coupling and decoupling

Coupling is currently restricted to compatible outer vehicle ends. Validation covers:

- different trains and vehicles;
- unoccupied coupling ends;
- compatible coupler types;
- train-boundary position;
- maximum endpoint distance;
- end-facing/alignment geometry.

A successful coupling stops both participating trains through the existing `RadioStop` mechanism, creates the runtime connection, merges the two compositions while preserving vehicle order, leaves the merged train stopped and removes the trailing runtime train from the manager.

A successful decoupling:

1. resolves the concrete `CouplingConnection` from a vehicle end;
2. verifies both connected vehicles belong to the same runtime train and form an adjacent train boundary;
3. stops the train through `RadioStop`;
4. splits `TrainComposition` at the connected boundary;
5. clears the connection from both vehicle ends;
6. creates a new stopped `Train` for the detached section;
7. registers that train through `TrainManager`.

No dynamic coupler forces, slack, impact shock, coupling animation, brake-pipe propagation or persistence of individual connections are implemented.

## 6. Gameplay commands

The current keyboard command contract is:

| Key | Action |
|---|---|
| `C` | Couple nearest valid boundary candidate |
| `X` | Decouple the last coupling created by `C`; otherwise first available runtime connection |
| `F6` | Select 3 km/h shunting/coupling limit |
| `F7` | Select 4 km/h shunting/coupling limit |
| `F8` | Select 5 km/h shunting/coupling limit (default) |

`C` refuses a candidate if either train exceeds the selected shunting speed. It delegates all authoritative structural and geometric checks to `CouplingService`.

The command layer uses `SignalAspect.Reserve3` (`S14`, `Rezerwowy 3`) as its semantic shunting signal profile. This does not replace or bypass normal signal/block safety.

## 7. Train performance and safety

Loaded consist acceleration/braking use the locomotive capability multiplied by the non-linear mass factor:

`factor = 1 / (totalMass / locomotiveMass)^1.30`

Locomotive power independently limits Vmax above supported mass. Signal stopping and RadioStop use the same effective consist braking capability as movement.

## 8. Persistence

`RuntimeSaveService` retains runtime save schema version `1` and persists rolling-stock `ShortName` values. Coupling connections are not currently persisted.

## 9. Diagnostics

Coupling operations write `[COUPLING]` diagnostics. Train movement diagnostics continue to use train-scoped `[TRAIN:<first-8-guid-chars>]` correlation.

## 10. Verification boundary

The repository no longer contains a dedicated automated Core test project. Validation of current runtime coupling/decoupling behavior therefore relies on the normal application build plus live gameplay verification in the user's .NET/MonoGame environment.

## 11. Ownership rules

- `TrainManager` owns train lifecycle and exposes the coupling command path.
- `TrainComposition` owns vehicle order and composition statistics.
- `CouplingService` owns coupling/decoupling validation and state mutation.
- `Vehicle` owns its static coupling specification and runtime end connections.
- `InputManager`/`InputState` own shared gameplay input architecture.
- Screens/UI request domain operations and do not mutate `TrainComposition.Vehicles` directly.

## 12. Deferred work

- vehicle/end selection UI instead of nearest-candidate selection;
- user-facing coupling failure messages;
- coupling connection persistence;
- integration verification across curves, cell boundaries, signals, blocks and stations;
- dynamic coupling physics, animation, slack and brake-pipe behavior.
