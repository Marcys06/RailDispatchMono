# Architecture

## Core rule

`RailDispatchMono.Core` is the shared application/game layer. Platform projects host the application. Dependency direction points from platform hosts toward Core.

## Main runtime components

```text
Platform host
    |
    v
RailDispatchMonoGame
    |
    +--> GraphicsDeviceManager
    +--> MyraUIManager
    |       +--> MyraEnvironment.Game
    |       +--> one shared Desktop
    |       +--> one active Root
    |
    v
ScreenManager
    |
    +--> GameScreen instances
    +--> InputState
    +--> SpriteBatch / shared resources
```

Myra is a UI/presentation layer inside this architecture. It does not replace `ScreenManager`, gameplay/domain ownership or the legacy MonoGame renderer where that renderer is still authoritative.

## MyraUIManager

`MyraUIManager` is the single Myra integration boundary. It initializes `MyraEnvironment.Game`, owns the shared `Desktop` and manages the active Myra root. Migrated views must use this boundary and must not create independent desktops.

At `0.1.3pre`, Myra surfaces include Main Menu, Settings, About, Pause and the gameplay HUD. The gameplay HUD includes the clock, `GameDay`, speed controls, build tools and the train/station information panel.

## ScreenManager

`ScreenManager` remains the authoritative owner of registered screen lifecycle, update, input routing and drawing. Pause is no longer a popup screen: `GameplayScreen` owns pause state and activates `MyraPauseView` through `MyraUIManager`.

## Ownership

- simulation/domain state belongs to `Game/` subsystems;
- camera state belongs to the rendering/camera subsystem;
- screen lifecycle belongs to `ScreenManager`/`GameScreen`;
- Myra presentation belongs to Myra views and `MyraUIManager`;
- persistence remains behind `MapSaveService` and gameplay-owned actions.

## Dependency discipline

1. Find the existing owner of state.
2. Reuse existing managers/models rather than parallel globals.
3. Keep platform APIs out of Core unless already abstracted.
4. Keep presentation in the UI/screen layer.
5. Keep simulation/domain behavior in game subsystems.
6. Use the shared input and Myra boundaries rather than creating parallel routing systems.
