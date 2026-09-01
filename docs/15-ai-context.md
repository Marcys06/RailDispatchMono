# AI context packet

## Current release

**RailDispatchMono `0.1.2j`** is the final Myra UI integration stage of the `0.1.2x` series. The `0.1.0` gameplay baseline and `0.1.1` documentation restructuring remain historical baselines.

## One-paragraph context

RailDispatchMono is a C#/.NET 9 MonoGame project with shared Core code and platform hosts. `RailDispatchMonoGame` configures the game and delegates screen lifecycle/update/draw work to `ScreenManager`. `MyraUIManager` is the single integration boundary for the standard Myra UI library: it assigns `MyraEnvironment.Game`, owns one shared `Desktop`, and manages one active root widget tree. Main Menu, Settings, About and Pause Menu use standard Myra widgets. Save/Load are gameplay actions exposed only by the Myra pause surface. The gameplay HUD and gameplay-specific radial menus remain non-Myra rendering/input surfaces by design.

## Mental model

```text
APPLICATION HOST
    |
    +--> RailDispatchMonoGame
            |
            +--> GraphicsDeviceManager
            +--> MyraUIManager
            |      +--> MyraEnvironment.Game
            |      +--> one Desktop
            |      +--> one active Root
            +--> ScreenManager
                    |
                    +--> InputState
                    +--> GameScreen(s)
                    +--> SpriteBatch / shared resources
                    +--> logical-to-physical presentation transform
```

## Myra contract — frozen after 0.1.2j

- Package: `Myra` 1.6.5.
- Initialization is idempotent and happens before any Myra widget is constructed.
- `MyraUIManager` owns exactly one shared `Desktop`.
- The active screen owns installation and cleanup of the Myra root.
- `Desktop.Render()` is the Myra input/render pass; `MyraUIManager.Update()` does not duplicate widget input processing.
- Migrated surfaces: Main Menu, Settings, About and Pause Menu.
- `MyraPauseView` is centered and contains Resume, Save, Load and Quit.
- The startup Main Menu does not expose Load Game.
- There is no second visible Save/Load surface in the Gameplay HUD.
- `ScreenManager` remains the lifecycle owner; Myra does not replace the screen stack.
- No further Myra migration is planned as part of `0.1.2x`. Future UI work requires an explicit new release goal.

## Pause lifecycle

`GameplayScreen` owns `_isPaused` and the `PauseScreen` instance. Resume always calls the explicit `ResumeGame()` path, which clears the pause state and removes the exact popup from `ScreenManager`. `PauseScreen` only raises `OnResume`; it does not additionally call `ExitScreen()`. This prevents double lifecycle transitions from a Myra callback.

## Persistence placement

Save and Load are owned logically by `GameplayScreen` and exposed through `PauseScreen` callbacks. `MapSaveService` persists infrastructure and, for an active save slot, invokes runtime persistence for trains/passengers/clock. The Myra layer only presents the controls and dispatches their actions.

## Hard constraints

- Do not invent missing classes or APIs.
- Do not create a parallel screen manager.
- A migrated Myra screen must not also process the same visible UI action through a legacy surface.
- Do not move shared gameplay into a platform host.
- Do not store authoritative simulation state only in a screen.
- Do not change existing shared APIs without searching all usages.
- Do not treat stale comments as executable behavior.
- Do not amend a completed `0.1.2x` stage. Corrections belong to the next letter.
- Keep Myra initialization before any Myra widget construction.
- When a screen owns the active Myra root, clear it during that screen's unload lifecycle.
- Keep gameplay rendering/UI concerns separate: Myra is for standard application/menu UI, not an automatic replacement for railway rendering or radial gameplay tools.

## Debugging rule

Repeated or duplicated logs do not by themselves prove duplicated simulation updates. Inspect logger subscriptions/call sites and the screen/update traversal before changing game-loop logic.
