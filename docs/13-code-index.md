# Code index

This index is a navigation aid. It is intentionally based on files confirmed in the repository tree/search and should be expanded when additional source areas are audited.

## Application/game root

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/RailDispatchMonoGame.cs` | Main shared MonoGame `Game` implementation and game-loop delegation. |
| `RailDispatchMono/RailDispatchMono.Core/RailDispatchMono.Core.csproj` | Shared Core project configuration; currently `net9.0` with MonoGame dependency. |

## Screen architecture

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/Screens/GameScreen.cs` | Base screen lifecycle, transition state and input/draw hooks. |
| `RailDispatchMono/RailDispatchMono.Core/ScreenManagers/ScreenManager.cs` | Screen collection, lifecycle orchestration, input routing, drawing resources and presentation scaling. |
| `RailDispatchMono/RailDispatchMono.Core/Screens/UI/InputManager.cs` | UI-oriented input management; inspect before introducing parallel UI input code. |

## Input

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/Inputs/InputState.cs` | Device snapshots, edge detection, semantic actions, gestures and cursor transformation. |

## Settings

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/Settings/RailDispatchMonoSettings.cs` | Observable game settings model. |
| `RailDispatchMono/RailDispatchMono.Core/Settings/DesktopSettingsStorage.cs` | Desktop settings persistence implementation. |
| `RailDispatchMono/RailDispatchMono.Core/Settings/MobileSettingsStorage.cs` | Mobile settings persistence implementation. |
| `RailDispatchMono/RailDispatchMono.Core/Settings/ConsoleSettingsStorage.cs` | Console-oriented settings persistence implementation. |

## Railway domain

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/Game/Railway/BlockController.cs` | Railway block control/domain component. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Railway/Junction.cs` | Railway junction domain component. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Railway/TrackRoute.cs` | Track route domain component. |

## Train domain

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/Train.cs` | Train domain object. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/TrainManager.cs` | Train coordination/management component. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/Vehicle.cs` | Vehicle domain base/component. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/VehicleParameters.cs` | Vehicle parameter data. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/Locomotive.cs` | Locomotive domain type. |
| `RailDispatchMono/RailDispatchMono.Core/Game/Train/Wagon.cs` | Wagon domain type. |

## Rendering/effects

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/Game/Rendering/Camera.cs` | Camera/rendering support. |
| `RailDispatchMono/RailDispatchMono.Core/Effects/ParticleManager.cs` | Particle effect management. |

## Android host

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Android/MainActivity.cs` | Android application entry/activity. |
| `RailDispatchMono/RailDispatchMono.Android/AndroidManifest.xml` | Android application manifest. |
| `RailDispatchMono/RailDispatchMono.Android/RailDispatchMono.Android.csproj` | Android target project configuration. |

## How to use this index

Use it to choose where to start reading, not as a substitute for reading dependencies. Before modifying a listed component, search for its callers and consumers.
