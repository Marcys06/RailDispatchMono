# AI context packet

## Current release

**RailDispatchMono `0.1.2a`** is the first infrastructure stage of the Myra UI integration series. The `0.1.0` gameplay baseline and `0.1.1` documentation restructuring remain historical baselines.

## One-paragraph context

RailDispatchMono is a C#/.NET 9 MonoGame project with shared Core code and platform hosts. `RailDispatchMonoGame` configures the game and delegates screen lifecycle/update/draw work to `ScreenManager`. `MyraUIManager` is now the shared boundary for the standard Myra UI library: it assigns `MyraEnvironment.Game` and owns a shared Myra `Desktop`. Existing screens have not yet been migrated to Myra at `0.1.2a`.

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
- Input owner: existing `InputState`/screen routing architecture.
- `0.1.2a` does not globally render the Myra desktop and does not migrate existing screens.

## Hard constraints

- Do not invent missing classes or APIs.
- Do not create a parallel screen manager.
- Do not bypass the established input architecture.
- Do not move shared gameplay into a platform host.
- Do not store authoritative simulation state only in a screen.
- Do not change existing shared APIs without searching all usages.
- Do not treat stale comments as executable behavior.
- Do not amend a completed `0.1.2x` stage. Corrections belong to the next letter.

## Debugging rule

Repeated or duplicated logs do not by themselves prove duplicated simulation updates. Inspect logger subscriptions/call sites and the screen/update traversal before changing game-loop logic.
