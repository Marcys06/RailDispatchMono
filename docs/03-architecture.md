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

This diagram describes the current central flow. Individual gameplay classes can participate below a screen without becoming part of the screen-management contract.

## `RailDispatchMonoGame`

Responsibilities currently visible in source:

1. Construct `GraphicsDeviceManager`.
2. Configure the Content root and mouse visibility.
3. Configure the preferred 1280x720 backbuffer.
4. Enable vertical synchronization.
5. Configure a fixed timestep of 1/60 second.
6. During `Initialize`, construct `ScreenManager` and `GameplayScreen` and add the gameplay screen.
7. During `LoadContent`, load the gameplay screen's content.
8. During `Update`, delegate to `ScreenManager.Update`.
9. During `Draw`, delegate to `ScreenManager.Draw`.

Do not duplicate this lifecycle in individual screens.

## `ScreenManager`

`ScreenManager` is the central coordinator for screen instances. It owns two screen lists: the active collection and a temporary collection used while updating from top to bottom.

It also owns the shared `InputState` and drawing resources (`SpriteBatch`, `SpriteFont`, and a blank texture).

The manager controls:

- screen registration/removal;
- content loading/unloading;
- input routing;
- screen coverage semantics;
- screen drawing order;
- presentation scaling;
- touch gesture configuration;
- optional screen tracing.

## `GameScreen`

`GameScreen` is the base lifecycle contract for a screen. A screen exposes state such as popup status, transition timings, transition position, active/hidden/transition states, controlling player and enabled touch gestures.

A screen has four principal overridable operations:

- `LoadContent()`
- `UnloadContent()`
- `Update(...)`
- `HandleInput(...)`
- `Draw(...)`

`HandleInput` is deliberately separate from `Update`: the manager decides which screen receives user input.

## Dependency discipline

When implementing a feature:

1. Find the existing owner of the relevant state.
2. Reuse the existing manager/model rather than creating a parallel global mechanism.
3. Keep platform APIs out of Core unless the existing architecture already abstracts them.
4. Keep screen-specific presentation in the screen layer.
5. Keep simulation/domain behavior in the relevant `Game` subsystem.
6. Keep shared input transformation in `InputState`/`ScreenManager` instead of performing ad-hoc coordinate conversion in every control.

## Architectural caution

The repository is an evolving codebase. Some implementation details are transitional (for example, comments in source contain migration notes). Documentation describes actual current behavior and should not be interpreted as proof that every subsystem is already cleanly separated.
