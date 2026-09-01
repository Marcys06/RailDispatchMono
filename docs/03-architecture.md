# Architecture

## Core rule

`RailDispatchMono.Core` is the shared application/game layer. Platform projects host the application. The dependency direction should therefore point from platform hosts toward Core, not from Core toward a concrete platform host.

## Main runtime components

```text
Platform host
    |
    v
RailDispatchMonoGame : Microsoft.Xna.Framework.Game
    |
    +--> GraphicsDeviceManager
    +--> MyraUIManager
    |       |
    |       +--> MyraEnvironment.Game
    |       +--> Myra Desktop
    |
    v
ScreenManager : DrawableGameComponent
    |
    +--> GameScreen instances
    |       +--> Update
    |       +--> HandleInput
    |       +--> Draw
    |
    +--> InputState
    +--> SpriteBatch / SpriteFont
    +--> presentation scaling
```

Myra is intentionally below the application/game root and beside the screen infrastructure. `ScreenManager` remains the authoritative owner of screen lifecycle and input routing.

## `RailDispatchMonoGame`

Responsibilities currently visible in source:

1. Construct `GraphicsDeviceManager`.
2. Construct the shared `MyraUIManager`.
3. Configure the Content root and mouse visibility.
4. Configure the preferred 1600x900 backbuffer.
5. Enable vertical synchronization.
6. Configure a fixed timestep of 1/60 second.
7. During `Initialize`, construct `ScreenManager` and register the Main Menu.
8. During `LoadContent`, initialize `MyraUIManager` with the current MonoGame `Game` instance.
9. During `Update`, delegate to `ScreenManager` through the registered game component.
10. During `Draw`, delegate screen rendering through `ScreenManager`.

Do not duplicate this lifecycle in individual screens.

## `MyraUIManager`

`MyraUIManager` is the current integration boundary for the Myra library. It:

- assigns the active MonoGame `Game` to `MyraEnvironment.Game`;
- creates one shared Myra `Desktop`;
- exposes initialization state;
- provides the common Myra render entry point for future UI screens.

At `0.1.2a`, no existing screen has been migrated to Myra yet. This stage only establishes the dependency and runtime integration boundary.

## `ScreenManager`

`ScreenManager` remains the central coordinator for screen instances. It owns the active screen collection, shared input state and drawing resources. Myra must not introduce a parallel screen stack or bypass this lifecycle.

## Dependency discipline

When implementing a feature:

1. Find the existing owner of the relevant state.
2. Reuse the existing manager/model rather than creating a parallel global mechanism.
3. Keep platform APIs out of Core unless the existing architecture already abstracts them.
4. Keep screen-specific presentation in the screen layer.
5. Keep simulation/domain behavior in the relevant `Game` subsystem.
6. Keep shared input transformation in `InputState`/`ScreenManager` instead of performing ad-hoc coordinate conversion in every control.
7. Use `MyraUIManager` as the Myra integration boundary instead of initializing `MyraEnvironment` independently from multiple screens.
