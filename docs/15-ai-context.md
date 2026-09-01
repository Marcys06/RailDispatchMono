# AI context packet

## Current release

**RailDispatchMono `0.1.2f`** is the accelerated Myra UI integration stage. The `0.1.0` gameplay baseline and `0.1.1` documentation restructuring remain historical baselines.

## One-paragraph context

RailDispatchMono is a C#/.NET 9 MonoGame project with shared Core code and platform hosts. `RailDispatchMonoGame` configures the game and delegates screen lifecycle/update/draw work to `ScreenManager`. `MyraUIManager` is the shared boundary for the standard Myra UI library: it assigns `MyraEnvironment.Game`, owns one shared `Desktop`, and manages the active root widget. The Main Menu and Pause Menu now use standard Myra widget surfaces. The startup menu contains New Game, Settings, About and Quit; Save/Load are grouped under the gameplay pause menu. Settings, About, dialogs and gameplay HUD remain on the legacy UI until explicitly migrated.

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
- Initialization: idempotent; the Game instance is assigned before any Myra screen/widget is constructed.
- Shared desktop owner: `MyraUIManager`.
- Screen lifecycle owner: `ScreenManager`.
- Migrated surfaces: Main Menu and Pause Menu.
- `MyraMainMenuView` is centered in the current viewport.
- `MyraPauseView` is centered in the current viewport and exposes Resume, Save, Load and Quit.
- The startup Main Menu does not expose Load Game.
- The host renders the shared Myra desktop once after the ScreenManager stack.

## Persistence placement

Save and Load are gameplay actions owned by `GameplayScreen` through `PauseScreen` callbacks. They are not startup-menu actions. `PauseScreen` remains the owner of the pause lifecycle while the Myra view owns only presentation and pointer interaction.

## Hard constraints

- Do not invent missing classes or APIs.
- Do not create a parallel screen manager.
- A migrated Myra screen must not also process the same UI action through a second visible legacy surface.
- Do not move shared gameplay into a platform host.
- Do not store authoritative simulation state only in a screen.
- Do not change existing shared APIs without searching all usages.
- Do not treat stale comments as executable behavior.
- Do not amend a completed `0.1.2x` stage. Corrections belong to the next letter.
- Keep Myra initialization before any Myra widget construction.
- When migrating a screen, clear the shared Myra root during that screen's unload lifecycle.

## Debugging rule

Repeated or duplicated logs do not by themselves prove duplicated simulation updates. Inspect logger subscriptions/call sites and the screen/update traversal before changing game-loop logic.
