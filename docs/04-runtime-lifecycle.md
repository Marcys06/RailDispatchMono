# Runtime lifecycle

## Startup

The platform host starts the MonoGame application and constructs the shared `RailDispatchMonoGame` instance.

`RailDispatchMonoGame` owns the game loop, graphics setup, shared `MyraUIManager` and `ScreenManager`.

## Initialization

`RailDispatchMonoGame.Initialize()` creates the `ScreenManager` and registers the initial application screen. Myra initialization is performed from the top-level game lifecycle before any Myra widget tree is constructed.

## Content loading

`RailDispatchMonoGame.LoadContent()` initializes the shared `MyraUIManager`. This assigns `MyraEnvironment.Game` and creates the shared Myra `Desktop` after MonoGame has a graphics context.

Individual Myra-backed views create their widget trees when their owning screen/game state activates them. The shared desktop is not recreated for each menu.

## Update order

The normal game update is authoritative for gameplay state. `ScreenManager.Update(gameTime)` handles registered screens and shared input state.

The gameplay pause state is owned by `GameplayScreen` through `_isPaused`. When paused, simulation updates are skipped while the pause UI remains interactive through the shared Myra desktop.

Myra button callbacks must not mutate the screen stack from inside the render pass. Actions that affect gameplay lifecycle are dispatched through the established Myra action/update boundary and executed during the normal game update lifecycle.

## Pause lifecycle

The current pause model deliberately does **not** insert a second pause `GameScreen` into `ScreenManager`.

```text
ESC / pause command
        |
        v
GameplayScreen.TogglePause()
        |
        +--> _isPaused = true
        |       |
        |       +--> MyraPauseView becomes active root
        |
        +<-- ResumeGame()
                |
                +--> _isPaused = false
                +--> Myra root cleared
```

`GameplayScreen` is the single owner of pause state. `MyraPauseView` is presentation only. Resume, Save and Load are gameplay operations owned by `GameplayScreen`; the UI does not own simulation or file persistence state.

While paused, `GameplayScreen` does not run train/simulation updates. It still allows the UI integration to receive pointer/keyboard input. ESC uses the same authoritative resume path as the Resume button.

## Draw order

`ScreenManager.Draw` renders the active game/screen content. The shared Myra desktop is rendered by the game host after the screen stack so the active application menu is visually on top.

Gameplay-specific rendering remains separate from Myra: railway tracks, trains, HUD elements, radial gameplay menus and floating gameplay text use their existing MonoGame rendering paths.

## Save/Load lifecycle

Pause Save/Load actions reach `GameplayScreen.SaveMap()` and `GameplayScreen.LoadMap()`. `MapSaveService` remains the persistence boundary.

Save does not leave pause. After a successful save, the Load action may become enabled immediately in the existing pause view. Load updates the active map/runtime state and refreshes dependent gameplay controllers without recreating the pause UI.

## Main Menu versus Pause

The startup Main Menu and the pause menu share the same `MyraUIManager`/`Desktop` infrastructure, but only one root is active at a time. The pause state is not represented as a competing `ScreenManager` popup.

## Resize/scaling lifecycle

The established presentation scaling remains authoritative. Myra uses the current host viewport and must not introduce a second independent logical coordinate system.
