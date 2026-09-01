# Code index

This index is a navigation aid. It is intentionally based on files confirmed in the repository tree/search and should be expanded when additional source areas are audited.

## Application/game root

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Core/RailDispatchMonoGame.cs` | Main shared MonoGame `Game` implementation and game-loop delegation. |
| `RailDispatchMono/RailDispatchMono.Core/RailDispatchMono.Core.csproj` | Shared Core project configuration; currently `net9.0` with MonoGame and Myra dependencies. |
| `RailDispatchMono/RailDispatchMono.Core/UI/Myra/MyraUIManager.cs` | Shared Myra initialization boundary and Desktop owner. |

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

## Railway and train domain

Use the existing `Game/Railway/`, `Game/Train/` and passenger/station subsystems. Before changing types, search their usages; `Train` is also a namespace in the codebase.

## Android host

| Path | Role |
|---|---|
| `RailDispatchMono/RailDispatchMono.Android/MainActivity.cs` | Android application entry/activity. |
| `RailDispatchMono/RailDispatchMono.Android/AndroidManifest.xml` | Android application manifest. |
| `RailDispatchMono/RailDispatchMono.Android/RailDispatchMono.Android.csproj` | Android target project configuration. |

## How to use this index

Use it to choose where to start reading, not as a substitute for reading dependencies. Before modifying a listed component, search for its callers and consumers.
