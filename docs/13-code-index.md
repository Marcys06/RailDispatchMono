# Code index

This index is a navigation aid based on source files confirmed in the repository. It should be expanded when additional source areas are audited.

## Application/game root

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/RailDispatchMonoGame.cs` | Main shared MonoGame `Game` implementation and game-loop delegation. |
| `RailDispatchMono/RailDispatchMono.Core/RailDispatchMono.Core.csproj` | Shared Core project configuration; currently `net9.0` with MonoGame and Myra dependencies. |
| `RailDispatchMono/RailDispatchMono.Core/UI/Myra/MyraUIManager.cs` | Shared Myra initialization boundary and single `Desktop` owner. |

## Screen architecture

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/Screens/GameScreen.cs` | Base screen lifecycle, transition state and input/draw hooks. |
| `RailDispatchMono/RailDispatchMono.Core/Screens/GameplayScreen.cs` | Primary gameplay screen; owns map/simulation services and opens Depot. Test train creation was removed in `0.1.4e`. |
| `RailDispatchMono/RailDispatchMono.Core/Screens/DepotScreen.cs` | Full-screen depot train-builder screen; creates player consists through `TrainManager`. |
| `RailDispatchMono/RailDispatchMono.Core/ScreenManagers/ScreenManager.cs` | Screen collection, lifecycle orchestration, input routing and drawing. |
| `RailDispatchMono/RailDispatchMono.Core/Screens/UI/InputManager.cs` | UI/world input management; emits depot selection events. |

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
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/Train.cs` | Train state, speed and gameplay-facing train model. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/TrainManager.cs` | Authoritative train lifecycle and composition-based creation. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/TrainComposition.cs` | Ordered vehicle list and derived composition statistics. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/Vehicle.cs` | Base railway vehicle abstraction. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/VehicleParameters.cs` | Internal physics parameters plus physical mass/length metadata. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/Locomotive.cs` | Locomotive vehicle. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/Wagon.cs` | Passenger/freight/service wagon and passenger state. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Railway/Depot.cs` | Depot building domain object. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Railway/DepotController.cs` | Depot collection/ownership. |

## Rolling stock catalogue

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/Game/RollingStock/RollingStockCatalog.cs` | Registered locomotive and wagon definitions. |
| `RailDispatchMono/RailDispatchMono.Core/Game/RollingStock/LocomotiveDefinition.cs` | Locomotive catalogue definition and factory. |
| `RailDispatchMono/RailDispatchMono.Core/Game/RollingStock/WagonDefinition.cs` | Wagon catalogue definition and factory. |
| `RailDispatchMono/RailDispatchMono.Core/Game/RollingStock/TractionType.cs` | Electric/diesel classification. |

## Input and settings

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/Inputs/InputState.cs` | Device snapshots, edge detection and semantic input. |
| `RailDispatchMono/RailDispatchMono.Core/Settings/RailDispatchMonoSettings.cs` | Observable game settings model. |

## Android host

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Android/MainActivity.cs` | Android application entry/activity. |
| `RailDispatchMono/RailDispatchMono.Android/AndroidManifest.xml` | Android application manifest. |
| `RailDispatchMono/RailDispatchMono.Android/RailDispatchMono.Android.csproj` | Android target project configuration. |

## How to use this index

Use it to choose where to start reading, not as a substitute for reading dependencies. Before modifying a listed component, search for its callers and consumers.
