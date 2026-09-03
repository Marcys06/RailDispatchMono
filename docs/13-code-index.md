# Code index

This index is a navigation aid based on source files confirmed in the repository. It should be expanded when additional source areas are audited.

## Application/game root

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/RailDispatchMonoGame.cs` | Main shared MonoGame `Game` implementation and game-loop delegation. |
| `RailDispatchMono/RailDispatchMono.Core/RailDispatchMono.Core.csproj` | Shared Core project configuration; currently `net9.0` with MonoGame and Myra dependencies. |
| `RailDispatchMono/RailDispatchMono.Core/UI/Myra/MyraUIManager.cs` | Shared Myra initialization boundary and single `Desktop` owner. |
| `RailDispatchMono/RailDispatchMono.Core/DebugManager.cs` | Central diagnostics logger; train-scoped `[TRAIN]` messages receive a short GUID prefix during train updates. |

## Screen architecture

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/Screens/GameScreen.cs` | Base screen lifecycle, transition state and input/draw hooks. |
| `RailDispatchMono/RailDispatchMono.Core/Screens/GameplayScreen.cs` | Primary gameplay screen; owns map/simulation services and opens Depot. Hardcoded test-train creation is not part of the current gameplay flow. |
| `RailDispatchMono/RailDispatchMono.Core/Screens/DepotScreen.cs` | Full-screen depot train-builder screen; creates player consists through `TrainManager`. |
| `RailDispatchMono/RailDispatchMono.Core/ScreenManagers/ScreenManager.cs` | Screen collection, lifecycle orchestration, input routing and drawing. |
| `RailDispatchMono/RailDispatchMono.Core/Screens/UI/InputManager.cs` | UI/world input management; emits depot selection events and does not directly own train creation. |

## Myra UI

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/UI/Myra/MyraUIManager.cs` | Single shared Myra `Desktop` and active-root owner. |
| `RailDispatchMono/RailDispatchMono.Core/UI/Myra/MyraGameplayView.cs` | Gameplay HUD. |
| `RailDispatchMono/RailDispatchMono.Core/UI/Myra/MyraDepotView.cs` | Depot train-builder presentation. |
| `RailDispatchMono/RailDispatchMono.Core/UI/Myra/MyraPauseView.cs` | Gameplay pause presentation. |

## Train and railway domain

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/Train.cs` | Train state, speed and gameplay-facing train model; exposes effective braking capability for safety-facing calculations. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/TrainMovement.cs` | Train acceleration/braking and movement integration. Consist mass applies a non-linear `1.30` exponent and establishes train-scoped diagnostics during updates. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/TrainManager.cs` | Authoritative train lifecycle and composition-based creation. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/TrainManager.Coupling.cs` | Runtime coupling/decoupling command path, candidate discovery and split-train registration. Current commands: `C`, `X`, `F6`, `F7`, `F8`. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/CouplingService.cs` | Authoritative coupling/decoupling validation and runtime composition mutation. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/CouplingConnection.cs` | Concrete connection between two vehicle ends. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/VehicleCouplingState.cs` | Runtime per-vehicle coupling-end state. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/TrainComposition.cs` | Ordered vehicle list and derived composition statistics, including total mass and power/load-limited Vmax. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/Vehicle.cs` | Base railway vehicle abstraction and static coupling metadata plus runtime coupling state. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/VehicleParameters.cs` | Internal physics parameters plus physical mass/length metadata. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/LocomotiveParameters.cs` | Locomotive-specific physical parameters, including `PowerMW`. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/Locomotive.cs` | Locomotive vehicle and short display label. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/Wagon.cs` | Passenger/freight/service wagon, passenger state and short display label. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Railway/Depot.cs` | Depot building domain object. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Railway/DepotController.cs` | Depot collection/ownership. |

## Rendering

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/Game/Rendering/TrainRenderer.cs` | Top-down rolling-stock rendering: electric/diesel locomotive colors, wagon colors and centered white vehicle labels normalized for both travel directions. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Rendering/DepotRenderer.cs` | World-space Depot rendering and placement preview; follows the camera/world coordinate contract used by railway rendering. |

## Rolling stock catalogue

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/Game/RollingStock/RollingStockCatalog.cs` | Registered locomotive/wagon definitions, power values and short labels. |
| `RailDispatchMono/RailDispatchMono.Core/Game/RollingStock/LocomotiveDefinition.cs` | Locomotive catalogue definition, power and vehicle factory. |
| `RailDispatchMono/RailDispatchMono.Core/Game/RollingStock/WagonDefinition.cs` | Wagon catalogue definition, short label and vehicle factory. |
| `RailDispatchMono/RailDispatchMono.Core/Game/RollingStock/TractionType.cs` | Electric/diesel classification. |

## Input and settings

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/Inputs/InputState.cs` | Device snapshots, edge detection and semantic input. |
| `RailDispatchMono/RailDispatchMono.Core/Settings/RailDispatchMonoSettings.cs` | Observable game settings model. |

## Persistence

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/Game/Save/RuntimeSaveService.cs` | Runtime train/map save-load boundary; persists rolling-stock short labels while retaining schema version 1 compatibility. |

## Platform hosts

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Android/MainActivity.cs` | Android application entry/activity. |
| `RailDispatchMono/RailDispatchMono.Android/AndroidManifest.xml` | Android application manifest. |
| `RailDispatchMono/RailDispatchMono.Android/RailDispatchMono.Android.csproj` | Android target project configuration. |

## How to use this index

Use it to choose where to start reading, not as a substitute for reading dependencies. Before modifying a listed component, search for its callers and consumers. For coupling/decoupling changes, inspect `TrainManager.Coupling.cs`, `CouplingService`, `TrainComposition`, vehicle coupling state and the active input path together.
