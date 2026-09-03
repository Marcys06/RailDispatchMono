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

At `0.1.4pre`, Myra surfaces include Main Menu, Settings, About, Pause, gameplay HUD and the full-screen Depot builder. The gameplay HUD includes the clock, `GameDay`, speed controls, build tools and the train/station information panel.

## ScreenManager

`ScreenManager` remains the authoritative owner of registered screen lifecycle, update, input routing and drawing. Pause is not a popup screen: `GameplayScreen` owns pause state and activates `MyraPauseView` through `MyraUIManager`. `DepotScreen` is a real full-screen `GameScreen` and therefore follows normal screen lifecycle.

## Ownership

- simulation/domain state belongs to `Game/` subsystems;
- camera state belongs to the rendering/camera subsystem;
- screen lifecycle belongs to `ScreenManager`/`GameScreen`;
- Myra presentation belongs to Myra views and `MyraUIManager`;
- persistence remains behind the existing save services and gameplay-owned actions;
- train lifecycle belongs to `TrainManager`;
- ordered consist state and derived consist statistics belong to `TrainComposition`;
- rolling-stock catalogue data belongs to `Game/RollingStock`;
- depot ownership belongs to `DepotController`.

## Train diagnostics boundary

Train movement establishes a temporary per-thread diagnostic context containing the train GUID. `DebugManager` uses that context to normalize raw messages beginning with `[TRAIN]` to `[TRAIN:<first-8-guid-chars>]`. This is a logging concern only; it does not change train identity or simulation state.

## Dependency discipline

1. Find the existing owner of state.
2. Reuse existing managers/models rather than parallel globals.
3. Keep platform APIs out of Core unless already abstracted.
4. Keep presentation in the UI/screen layer.
5. Keep simulation/domain behavior in game subsystems.
6. Use the shared input and Myra boundaries rather than creating parallel routing systems.
7. When changing movement/safety calculations, audit all consumers of braking, mass and Vmax.
8. When changing a public constructor or data contract, inspect save/load and catalogue factories together.
