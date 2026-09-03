# RailDispatchMono — current state `0.1.4pre`

**Date:** 2026-09-03  
**Status:** consolidated development/pre-release milestone

This document is the authoritative current-state snapshot for the `0.1.4` line. The lettered `0.1.4a`–`0.1.4i` stages remain historical development records in `CHANGELOG.md` and `docs/changelog/`.

## 1. Technology and repository structure

- C# / .NET 9 shared Core project.
- MonoGame is the shared game framework.
- Myra `1.6.5` is the shared UI framework.
- `RailDispatchMono/RailDispatchMono.Core` is the active shared Core project referenced by the solution.
- Platform hosts include Android, DesktopGL, WindowsDX and iOS.
- Shared gameplay/domain code remains in Core; platform bootstrap/lifecycle code remains in platform projects.

## 2. Runtime and screen architecture

`RailDispatchMonoGame` owns the main game loop and shared graphics/UI infrastructure. `ScreenManager` owns screen lifecycle, update/input routing and drawing. `GameplayScreen` is the primary gameplay screen.

`MyraUIManager` is the single Myra integration boundary. It owns one shared `Desktop` and one active root. Main Menu, Settings, About, Pause, gameplay HUD and Depot builder use this infrastructure.

Pause is a gameplay state owned by `GameplayScreen`, not a popup `GameScreen`. `MyraPauseView` is presentation only. `DepotScreen` is a full-screen `GameScreen`; opening it covers gameplay, while closing it restores the gameplay Myra root.

## 3. Input and Depot workflow

`InputState` and `InputManager` remain the shared gameplay input path. Depot mode is selected with `9` / NumPad `9`. Selecting an existing Depot raises `InputManager.DepotSelected` and opens `DepotScreen`.

The Depot builder supports:

- one locomotive selection;
- zero or more passenger wagons;
- adding/removing individual wagons;
- clearing wagons;
- live Vmax, mass, length and wagon count;
- locomotive-only or locomotive-plus-wagons creation;
- cancellation without creating a train.

Train creation uses the single authoritative `TrainManager.CreateTrainFromComposition()` path and places the consist on an adjacent free track cell when available.

## 4. Train and rolling stock domain

`TrainManager` owns train lifecycle. `TrainComposition` is the authoritative ordered vehicle collection and owns derived consist statistics. `Vehicle` is the base rolling-stock abstraction.

Current locomotives:

| Type | Traction | Vmax | Mass | Power | Label |
|---|---|---:|---:|---:|---|
| EP07 | electric | 125 km/h | 80 t | 2.0 MW | EP07 |
| EU200 — Newag Griffin E4ACP | electric AC | 200 km/h | 84 t | 5.5 MW | EU200 |
| SU42 | diesel | 90 km/h | 74 t | 1.2 MW | SU42 |

Passenger coach labels are `1KL`, `2KL`, `3KL`. Their current catalogue Vmax is 200 km/h.

## 5. Train performance model

Loaded consist acceleration and braking use the locomotive capability multiplied by the non-linear mass factor:

`factor = 1 / (totalMass / locomotiveMass)^1.30`

Locomotive power independently limits Vmax above supported mass. Current calibration uses `0.006 MW/t` and exponent `0.55`.

The intended calibration is:

- EU200 + ten 40 t passenger wagons: 200 km/h;
- SU42 + five 40 t passenger wagons: approximately 75 km/h;
- SU42 + ten 40 t passenger wagons: approximately 55 km/h.

## 6. Signal and collision safety

Signal `Stop` / `StopStation` and restricted-aspect calculations use the same effective consist braking capability as train movement. The stopping model uses `v = sqrt(2*a*d)`, the existing reaction allowance, a `0.8`-cell stopping offset and the leading-vehicle physical half-length correction.

`TrainCollisionController` keeps a minimum RadioStop distance of 3 cells and expands it at higher speed using effective braking distance, `0.15 s` reaction distance and a `0.8`-cell buffer. RadioStop remains a fallback rather than a replacement for signal/block authority.

Spatial scale remains `1 map cell = 10 m` for the physical/gameplay model.

## 7. Rolling stock rendering

`TrainRenderer` draws differentiated rolling stock in world space. Electric locomotives are red, diesel locomotives are black, passenger coaches use distinct blue shades, and every visible unit receives a centered white short label.

Labels are:

- locomotives: `EP07`, `EU200`, `SU42`;
- passenger coaches: `1KL`, `2KL`, `3KL`.

Label rotation is normalized for both travel directions. `DepotRenderer` also uses world-space coordinates under the existing camera transform and renders a visible 1x1-cell Depot footprint plus placement preview.

## 8. Diagnostics

During `Train.Update`, a temporary train-scoped diagnostic context is established. `DebugManager` normalizes messages beginning with `[TRAIN]` to `[TRAIN:<first-8-guid-chars>]` while that train update is active.

Example:

```text
[13:10:30.449] [General] [TRAIN:de148bda] START - Pos: (...), Dir: East, Speed: ...
```

The identifier is derived from `Train.Id` and is diagnostic only.

## 9. Persistence

`RuntimeSaveService` persists rolling-stock `ShortName` values and loads the current `Locomotive` / `Wagon` constructor contracts. Runtime save schema remains version `1`; saves without the newer `ShortName` field remain deserializable.

## 10. Coupling boundary

Static coupling metadata is available through `Vehicle.Coupling` / `CouplingSpecification`. Default rolling stock exposes screw couplers at both ends.

Runtime coupling is intentionally not implemented in `0.1.4pre`:

- no runtime coupled/uncoupled state;
- no coupling-distance detection;
- no coupling/decoupling commands;
- no consist merge/split as a coupling action;
- no compatibility checks;
- no coupling forces/slack dynamics;
- no persistence of individual coupler connections.

These are the planned `0.1.5` implementation boundary.

## 11. Architectural ownership rules

- `TrainManager` owns train lifecycle.
- `TrainComposition` owns ordered vehicles and derived consist statistics.
- `Game/RollingStock` owns catalogue definitions/factories.
- `DepotController` owns Depot buildings.
- `GameplayScreen` owns gameplay pause state.
- `MyraUIManager` owns the shared Myra `Desktop` and active root.
- `ScreenManager` owns screen lifecycle.
- `InputManager` owns world-input interpretation but not train lifecycle.
- UI requests domain operations; it does not become a second owner of domain collections.

## 12. Deferred 0.1.5 work

The next implementation line may add runtime coupling/decoupling on top of the existing ordered `TrainComposition`. It should keep train lifecycle in `TrainManager`, connection state in the train/consist domain and interaction in UI/input callers.

No `0.1.5` coupling mechanics are included in this snapshot.
