# Screen system

## `GameScreen`

`GameScreen` is the reusable screen/lifecycle abstraction used by the application. It models presentation state, transitions and ownership by `ScreenManager`.

### Important properties

- `IsPopup` — whether the screen allows screens underneath it to remain uncovered.
- `TransitionOnTime` / `TransitionOffTime` — transition durations.
- `TransitionPosition` / `TransitionAlpha` — transition progress and opacity.
- `ScreenState` — transition/active/hidden state.
- `IsExiting` — whether the screen is leaving permanently.
- `IsActive` — whether the screen currently has focus/active lifecycle state.
- `ScreenManager` — owning manager.
- `ControllingPlayer` — optional controlling player.

## Screen state machine

```text
TransitionOn --> Active --> TransitionOff --> Hidden
                      \\--> ExitScreen --> TransitionOff --> removed
```

A newly registered screen starts in its transition-on state. A non-popup screen can cover screens below it. Exiting screens are removed after their transition lifecycle completes.

## Input routing

`ScreenManager` remains the authoritative router for legacy/gameplay screen input. Migrated Myra widgets receive pointer/keyboard input through the single shared Myra `Desktop`.

Do not create a second global input dispatcher for pause, Depot or application menus.

## Popup behavior

Legacy popup screens remain supported for dialogs and other cases that genuinely need `ScreenManager` layering. **Pause is not implemented as a popup `GameScreen`.**

This distinction is important: the current pause system uses `GameplayScreen` as the lifecycle owner and changes its simulation state instead of adding/removing another screen from the stack.

## Pause architecture

At `0.1.4pre`, pause ownership is:

```text
ScreenManager
    |
    +--> GameplayScreen
            |
            +--> _isPaused
            +--> MyraPauseView
            +--> SaveMap / LoadMap / ResumeGame
```

`GameplayScreen.TogglePause()` is the authoritative entry point for changing pause state. `ESC` uses the same gameplay path. The visible Myra pause view invokes callbacks/actions owned by `GameplayScreen`.

When entering pause, the simulation is stopped and the Myra pause root becomes active. When resuming, `_isPaused` is cleared and the Myra root is cleared. No `PauseScreen` is inserted into `ScreenManager`.

## Depot screen

`DepotScreen` is a full-screen `GameScreen`, not a popup and not a second UI host. It owns temporary builder state while `TrainManager` remains the authoritative owner of the created train.

Opening the Depot covers gameplay, so gameplay simulation does not continue underneath the builder. Closing the screen restores the gameplay Myra root and normal gameplay operation.

## Loading and unloading

`ScreenManager.AddScreen` assigns manager/player references and loads content when appropriate. `RemoveScreen` unloads and removes registered screens.

Pause and Depot Myra views do not create their own `Desktop`; their roots are owned by `MyraUIManager` and are explicitly replaced/cleared by their owning lifecycle.

## Presentation scaling

`GameScreen.LoadContent()` and the existing presentation infrastructure remain authoritative for screen rendering. Myra menus use the host viewport and established presentation semantics.

## Implementation caution

Executable behavior and current call sites are authoritative. Historical comments describing the old `PauseScreen` popup architecture must not be copied into new code.
