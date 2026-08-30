# Known issues and cautions

This document records implementation details that can cause an AI agent to make incorrect assumptions.

## Hard-coded platform flags

`RailDispatchMonoGame.IsMobile` currently returns `false` and `IsDesktop` currently returns `true` in the shared Core game class. `InputState` uses these values to choose initialization behavior.

Do not describe the flags as automatically detected platform information.

## Desktop-oriented graphics defaults in Core

The shared game constructor currently requests a 1280x720 backbuffer, enables VSync, and uses a fixed 60 FPS timestep. These are explicit defaults, not generic MonoGame behavior.

## Two content-loading touchpoints

`RailDispatchMonoGame.LoadContent()` explicitly calls the gameplay screen's `LoadContent(Content)`. `ScreenManager` also overrides its component `LoadContent()` and calls `LoadContent()` on registered screens.

This requires care when changing initialization. Do not assume a single conventional MonoGame screen-manager template is being used without verifying registration/lifecycle behavior.

## Screen manager initialization timing

`ScreenManager` has an `isInitialized` flag. When a screen is added after initialization, `AddScreen` immediately calls that screen's `LoadContent()`.

This means dynamic screen registration has different loading timing from registration before manager initialization.

## Base presentation size

`ScreenManager` defines a base presentation size of 800x480. The top-level game prefers a 1280x720 physical backbuffer. These are intentionally different concepts: base logical coordinates versus current backbuffer dimensions.

## Input coordinate inversion

When scaling is recalculated, the manager passes `Matrix.Invert(globalTransformation)` to `InputState`. Pointer coordinates therefore have an explicit logical-space transformation.

Do not replace this with simple width/height ratios unless the transformation semantics are intentionally changed.

## Mouse-to-touch compatibility behavior

The current `InputState` treats left mouse click as a touch-like action for selected interaction paths. Middle and right mouse clicks are also mapped to synthetic touch counts. This is existing behavior and should not be silently removed during input cleanup.

## Four-player input arrays

Keyboard and gamepad states are stored in arrays of size four. High-level methods can accept a specific `PlayerIndex` or search across players.

## Existing naming typo

The current input class contains `IsRightMoustButtonClicked()`. Do not rename a public/internal member solely for spelling without checking every call site. If correcting it, consider a compatibility-preserving migration rather than a blind rename.

## Nullable manager reference

`GameScreen` stores its manager in a nullable backing field but exposes a non-null `ScreenManager` getter using the null-forgiving operator. This reflects a lifecycle invariant: a screen is expected to be attached through `ScreenManager.AddScreen` before code requiring the manager executes.

## Documentation boundary

This file intentionally records cautions, not proposed fixes. Do not treat every caution as a bug requiring immediate refactoring.
