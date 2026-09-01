# AI context packet

## Current release

**RailDispatchMono `0.1.2c`** is the accelerated Myra UI integration stage. The `0.1.0` gameplay baseline and `0.1.1` documentation restructuring remain historical baselines.

## One-paragraph context

RailDispatchMono is a C#/.NET 9 MonoGame project with shared Core code and platform hosts. `RailDispatchMonoGame` configures the game and delegates screen lifecycle/update/draw work to `ScreenManager`. `MyraUIManager` is the shared boundary for the standard Myra UI library: it assigns `MyraEnvironment.Game`, owns one shared `Desktop`, and manages the active root widget. The Main Menu now uses a standard Myra widget surface while Load Game, Settings, About, Pause and gameplay UI remain on the legacy screen UI.

## Mental model

```text
APPLICATION HOST
    |
    +--> RailDispatchMonoGame
            |
            +--> GraphicsDeviceManager
            +--> MyraUIManager
            |      +--> MyraEnvironment.Game
            |      +--> Desktop
            |      +--> active Root widget tree
            +--> ScreenManager
                    |
                    +--> InputState
                    +--> GameScreen(s)
                    +--> SpriteBatch / shared resources
                    +--> logical-to-physical presentation transform
```

## Myra contract

- Package: `Myra` 1.6.5.
- Initialization: once from `RailDispatchMonoGame.LoadContent()`.
- Shared desktop owner: `MyraUIManager`.
- Screen lifecycle owner: `ScreenManager`.
- Migrated surface: Main Menu.
- Main Menu installs and clears the shared root during its screen content lifecycle.
- The host renders the shared Myra desktop once after the ScreenManager stack.
- Load Game, Settings, About, Pause and gameplay UI are not yet migrated.

## Hard constraints

- Do not invent missing classes or APIs.
- Do not create a parallel screen manager.
- A migrated Myra screen must not also process the same UI action through the legacy surface.
- Do not move shared gameplay into a platform host.
- Do not store authoritative simulation state only in a screen.
- Do not change existing shared APIs without searching all usages.
- Do not treat stale comments as executable behavior.
- Do not amend a completed `0.1.2x` stage. Corrections belong to the next letter.

## Debugging rule

Repeated or duplicated logs do not by themselves prove duplicated simulation updates. Inspect logger subscriptions/call sites and the screen/update traversal before changing game-loop logic.
