# Input system

## `InputState`

`InputState` is the shared input snapshot/action layer used by the screen system.

It tracks current and previous state for:

- keyboard, up to four player slots;
- gamepads, up to four player slots;
- mouse;
- touch;
- queued touch gestures.

The constant `MaxInputs` is currently 4.

## Frame model

At the start of each `ScreenManager.Update`, `InputState.Update` copies current keyboard/gamepad/mouse/touch state into the previous-state fields and reads fresh device state.

This enables edge-triggered actions such as `IsNewKeyPress` and `IsNewButtonPress`.

## High-level actions

The class provides semantic actions including:

- menu select;
- menu cancel;
- menu up/down;
- pause;
- select next/previous.

Prefer these semantic methods in screens instead of duplicating raw key/button combinations.

## Supported interaction paths

The current implementation combines keyboard, gamepad, mouse and touch. Mouse clicks are also interpreted as touch-like interaction for selected operations.

The cursor can be moved with:

- gamepad 0 left thumbstick;
- keyboard arrow keys.

Cursor movement is clamped to the configured base screen dimensions.

## Coordinate transformation

Input coordinates are transformed from physical screen coordinates into game/presentation coordinates. `ScreenManager.ScalePresentationArea()` calculates the rendering transformation and passes its inverse to `InputState`.

When adding clickable UI, use the transformed cursor coordinates rather than comparing raw window coordinates against logical game rectangles.

## Touch gestures

`ScreenManager` updates `TouchPanel.EnabledGestures` from the current screen's `EnabledGestures`. `GameScreen` also updates the touch panel when its gesture configuration changes while active.

A screen should request only the gestures it actually needs.

## Platform flags

`InputState` checks `RailDispatchMonoGame.IsMobile` and `IsDesktop` during construction. The current shared `RailDispatchMonoGame` implementation reports `IsMobile == false` and `IsDesktop == true`.

If platform behavior changes, review these flags and all consumers together; they are part of the input initialization contract.

## AI rule

Do not introduce a second input singleton, a second cursor coordinate system, or per-screen device polling unless the existing input abstraction is demonstrably insufficient. First inspect `InputState` and its semantic action methods.
