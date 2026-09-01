# AI context packet

## Current release

**RailDispatchMono `0.1.2pre`** is the current Myra UI stabilization preview following the immutable `0.1.2a`–`0.1.2k` stages. The `0.1.0` gameplay baseline and `0.1.1` documentation restructuring remain historical baselines.

## One-paragraph context

RailDispatchMono is a C#/.NET 9 MonoGame project with shared Core code and platform hosts. `RailDispatchMonoGame` configures the game and delegates screen lifecycle/update/draw work to `ScreenManager`. `MyraUIManager` is the single integration boundary for standard Myra UI: it assigns `MyraEnvironment.Game`, owns one shared `Desktop`, and manages one active root widget tree. Main Menu, Settings, About and Pause Menu use standard Myra widgets. Save/Load are gameplay actions exposed only by the Myra pause surface. The gameplay HUD and gameplay-specific radial menus remain non-Myra rendering/input surfaces by design.

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

## Myra contract

- Package: `Myra` 1.6.5.
- Initialization is idempotent and happens before any Myra widget is constructed.
- `MyraUIManager` owns exactly one shared `Desktop`.
- Only one Myra root is active at a time.
- `Desktop` is the widget input/render boundary.
- Migrated surfaces: Main Menu, Settings, About and Pause Menu.
- `MyraPauseView` contains Resume, Save, Load and Quit.
- Startup Main Menu does not expose Load Game.
- Gameplay HUD has no second visible Save/Load surface.
- `ScreenManager` remains the lifecycle owner for registered screens.

## Pause lifecycle — current architecture

Pause is a **gameplay state**, not a popup screen.

`GameplayScreen` owns `_isPaused` and is the only authoritative owner of entering/leaving pause. Entering pause activates `MyraPauseView` as the shared Myra root. While paused, gameplay simulation updates are skipped but the Myra desktop remains interactive. Resuming clears `_isPaused` and clears the Myra root.

There is no runtime `PauseScreen` in the current architecture. Do not reintroduce one merely to host the UI. `MyraPauseView` is presentation and action dispatch; `GameplayScreen` owns the actual state transition and persistence operations.

Myra callbacks that change gameplay/screen state must execute through the normal game update boundary, not by mutating the screen stack from `Desktop.Render()`.

## Persistence placement

Save and Load are owned logically by `GameplayScreen` and exposed through `MyraPauseView`. `MapSaveService` is the persistence boundary. The UI does not perform file I/O directly.

## Hard constraints

- Do not invent missing classes or APIs.
- Do not create a parallel screen manager.
- Do not create a second Myra `Desktop`.
- Do not add a second visible UI surface for an existing action.
- Do not use a popup `GameScreen` for pause unless a future release explicitly changes the pause architecture.
- Do not move shared gameplay into a platform host.
- Do not store authoritative simulation state only in a screen.
- Do not change existing shared APIs without searching all usages.
- Do not treat stale comments as executable behavior.
- Historical `0.1.2a`–`0.1.2k` stages are immutable.
- If a historical commit has no reliable release description, document it as `bugfix` rather than inventing details.
- Keep Myra initialization before any Myra widget construction.
- Keep gameplay rendering/UI concerns separate: Myra is for standard application/menu UI, not an automatic replacement for railway rendering or radial gameplay tools.

## Debugging rule

Repeated or duplicated logs do not by themselves prove duplicated simulation updates. Inspect logger subscriptions/call sites and the screen/update traversal before changing game-loop logic.
