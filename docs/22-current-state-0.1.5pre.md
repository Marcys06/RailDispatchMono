# RailDispatchMono — current state `0.1.5pre`

**Date:** 2026-09-03  
**Status:** consolidated development/pre-release milestone

This document is the authoritative current-state snapshot for the `0.1.5` line. Lettered `0.1.5a`–`0.1.5i` stages remain historical development records in `CHANGELOG.md` and `docs/changelog/`.

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

Train coupling/decoupling commands are handled by the existing `TrainManager.HandleCouplingHotkeys()` path. `F6` is now reserved for manual shunting and is no longer a coupling-speed selector.

## 4. Train and rolling stock domain

`TrainManager` owns train lifecycle. `TrainComposition` is the authoritative ordered vehicle collection and owns derived consist statistics. `Vehicle` owns static coupling metadata and runtime coupling state.

When vehicles are added sequentially to a `TrainComposition`, the composition automatically initializes a runtime coupling between each adjacent pair when their physical coupling ends are free and their `CouplerType` values are compatible. This applies equally to mixed wagon formations such as `1KL <-> 2KL <-> 3KL`.

Runtime coupling state is represented by `VehicleCouplingState` and `CouplingConnection`. `CouplingService` is the authoritative operation/validation boundary for coupling and decoupling.

## 5. Rigid coupling and decoupling

Coupling is currently restricted to compatible outer vehicle ends. Validation covers:

- different trains and vehicles;
- unoccupied coupling ends;
- compatible coupler types;
- train-boundary position;
- maximum endpoint distance;
- end-facing/alignment geometry.

`C` uses a fixed `6 km/h` shunting limit. There are no longer any runtime commands for selecting a different coupling speed.

A successful coupling stops both participating trains through the existing `RadioStop` mechanism, creates the runtime connection, merges the two compositions while preserving vehicle order, leaves the merged train stopped and removes the trailing runtime train from the manager.

A successful decoupling:

1. requires the target train to be moving below `6 km/h`;
2. resolves the concrete `CouplingConnection` from the wagon selected by the cursor;
3. verifies both connected vehicles belong to the same runtime train and form an adjacent train boundary;
4. stops the train through `RadioStop`;
5. splits `TrainComposition` at the connected boundary;
6. clears the connection from both vehicle ends;
7. initializes the detached train's spatial state from the split position and direction, preserving each vehicle's physical distance behind the head instead of collapsing the consist onto one spawn point;
8. creates a new stopped `Train` for the detached section;
9. registers that train through `TrainManager`.

The `< 6 km/h` decoupling rule is enforced inside `CouplingService`, so direct domain calls cannot bypass the speed restriction.

`X` only acts on a wagon actually under the cursor. It no longer falls back to the last `C` coupling or the oldest available runtime connection. When the hovered wagon has both front and rear runtime connections, the rear connection is preferred; otherwise its available connection is used.

Vehicle transforms normally use trajectory history after movement. For a newly created or repositioned train with no accumulated travel history, vehicle positions are derived from the head position, train direction and each vehicle's distance behind the head. This prevents a detached consist from visually respawning with all vehicles at the same position after `X`.

A complete pre-built multi-vehicle composition already contains runtime connections between adjacent vehicles. Therefore `X` does not require a preceding `C` command to split such a formation.

No dynamic coupler forces, slack, impact shock, coupling animation, brake-pipe propagation or persistence of individual connections are implemented.

## 6. Gameplay commands

The current keyboard command contract is:

| Key | Action |
|---|---|
| `C` | Couple nearest valid boundary candidate, limited to `6 km/h` |
| `X` | Decouple the wagon under the cursor, only when its train is below `6 km/h` |
| `F6` | Manual shunting: while held over a train, accelerate it toward `3 km/h` and bypass automatic RadioStop/collision stopping |

`F6` manual shunting is cursor-targeted. Only the train under the cursor receives the manual movement update; other trains continue through the normal station, collision, signal and RadioStop processing path.

Manual shunting uses the same consist acceleration/mass model as normal movement, but its target speed is fixed at `3 km/h` and it does not apply the automatic RadioStop/collision stop path while the key is held.

`C` delegates authoritative structural and geometric checks to `CouplingService` and allows the fixed `6 km/h` shunting limit.

The previous `F6/F7/F8` coupling-speed selection mechanism has been removed.

## 7. Train performance and safety

Loaded consist acceleration/braking use the locomotive capability multiplied by the non-linear mass factor:

`factor = 1 / (totalMass / locomotiveMass)^1.30`

Locomotive power independently limits Vmax above supported mass. Signal stopping and RadioStop use the same effective consist braking capability as movement.

Manual `F6` shunting intentionally bypasses the automatic RadioStop/collision stop path for the selected train while held.

## 8. Persistence

`RuntimeSaveService` retains runtime save schema version `1` and persists rolling-stock `ShortName` values. Coupling connections are not currently persisted.

## 9. Diagnostics

Coupling operations write `[COUPLING]` diagnostics. Train movement diagnostics continue to use train-scoped `[TRAIN:<first-8-guid-chars>]` correlation.

## 10. Verification boundary

The repository no longer contains a dedicated automated Core test project. Validation of current runtime coupling/decoupling behavior therefore relies on the normal application build plus live gameplay verification in the user's .NET/MonoGame environment.

The `0.1.5i` changes were inspected against the current train movement, RadioStop, collision, coupling, decoupling and cursor/renderer paths. A live build was not run in the current environment because NuGet/package restore requires unavailable network access.

## 11. Ownership rules

- `TrainManager` owns train lifecycle and exposes the coupling/manual-shunting command path.
- `TrainComposition` owns vehicle order, composition statistics and initialization of adjacent runtime couplings when vehicles are added.
- `CouplingService` owns coupling/decoupling validation and state mutation for explicit operations, including the `< 6 km/h` decoupling restriction.
- `Vehicle` owns its static coupling specification and runtime end connections.
- `InputManager`/`InputState` own shared gameplay input architecture and provide the cursor world position to gameplay train control.
- Screens/UI request domain operations and do not mutate `TrainComposition.Vehicles` directly.

## 12. Deferred work

- vehicle/end selection UI for `C` instead of nearest-candidate selection;
- user-facing coupling failure messages;
- coupling connection persistence;
- integration verification across curves, cell boundaries, signals, blocks and stations;
- dynamic coupling physics, animation, slack and brake-pipe behavior.
