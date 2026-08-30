# Screen system

## `GameScreen`

`GameScreen` is an abstract screen layer. It models both presentation and lifecycle state.

### Important properties

- `IsPopup` — whether the screen should allow screens underneath it to remain uncovered.
- `TransitionOnTime` — duration of activation transition.
- `TransitionOffTime` — duration of deactivation transition.
- `TransitionPosition` — normalized transition value, 0 = fully active, 1 = fully off.
- `TransitionAlpha` — `1 - TransitionPosition`.
- `ScreenState` — transition/active/hidden state.
- `IsExiting` — whether the screen is leaving permanently.
- `IsActive` — true only when no other screen has focus and the screen is transitioning on or active.
- `ScreenManager` — owning manager, assigned internally by `AddScreen`.
- `ControllingPlayer` — optional player index controlling the screen.
- `EnabledGestures` — touch gestures requested by the screen.

## Screen state machine

Conceptually:

```text
TransitionOn --> Active --> TransitionOff --> Hidden
                      \--> ExitScreen --> TransitionOff --> removed
```

A newly registered screen starts with `ScreenState.TransitionOn` and transition position 1.

If a screen is covered by a non-popup screen, it transitions off and eventually becomes `Hidden`. When uncovered, it transitions back on.

If `IsExiting` is set, the screen transitions off and is removed when the transition completes.

## Input routing

`ScreenManager` traverses screens from the topmost screen downward. Once an eligible screen receives input, lower screens do not receive input during that update.

This is an important invariant. A screen should not implement its own global input dispatch in order to compete with the manager.

## Popup behavior

A popup screen can sit above another screen without causing the underlying screen to become covered. This allows a menu/dialog overlay to preserve the underlying screen's active presentation.

## Loading and unloading

`AddScreen` assigns manager/player references and clears the exiting flag. If the manager is already initialized, the new screen's `LoadContent()` is invoked immediately.

`RemoveScreen` unloads content when appropriate and removes the screen from both the main and temporary lists.

## Presentation scaling

`GameScreen.LoadContent()` calls `ScreenManager.ScalePresentationArea()` by default.

Screens should therefore use the manager's established presentation coordinate system instead of introducing a second independent scaling model.

## Implementation caution

The source contains comments written during earlier code changes. Treat executable behavior, not migration comments, as authoritative. In particular, do not assume a method signature or loading path from a stale comment without reading the current declaration and its callers.
