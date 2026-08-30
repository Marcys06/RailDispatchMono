# Runtime lifecycle

## Startup

The platform host starts the MonoGame application and constructs the shared `RailDispatchMonoGame` instance.

`RailDispatchMonoGame` constructs its `GraphicsDeviceManager` and configures basic graphics/game-loop settings in its constructor.

## Initialization

`RailDispatchMonoGame.Initialize()` creates:

1. a `ScreenManager` associated with the game;
2. a `GameplayScreen` associated with the graphics device and screen manager;
3. registration of the gameplay screen via `ScreenManager.AddScreen`.

The base MonoGame initialization is then invoked.

## Content loading

`RailDispatchMonoGame.LoadContent()` explicitly calls `_gameplay.LoadContent(Content)` in the current implementation.

Separately, `ScreenManager` has its own MonoGame content-loading override. Its implementation creates a `SpriteBatch`, loads `Fonts/Hud` and `Sprites/blank`, and calls `LoadContent()` on screens currently registered with the manager.

Because there are two visible content-loading paths in the current code, an AI must inspect the concrete `GameplayScreen` and component registration behavior before changing content loading. Do not blindly add another global `Content.Load` path.

## Update order

The top-level game delegates to `ScreenManager.Update(gameTime)`.

`ScreenManager.Update` first updates `InputState`. It then copies the current screens into a temporary update list and processes that list from the topmost screen toward the bottom.

For each screen:

1. `screen.Update(...)` is called regardless of whether it is active, hidden or transitioning.
2. A screen in `TransitionOn` or `Active` state may receive `HandleInput`.
3. Only the first eligible screen gets input focus during the traversal.
4. A non-popup screen marks lower screens as covered.

If screen tracing is enabled, the manager logs the current screen type names after the traversal.

## Draw order

`ScreenManager.Draw` iterates the registered screen collection in collection order and invokes `Draw` for every screen whose state is not `Hidden`.

The manager itself does not perform a separate scene-graph traversal. Screens are responsible for their own rendering.

## Screen removal

A screen can call `ExitScreen()`.

- If `TransitionOffTime` is zero, the screen is removed immediately.
- Otherwise it is marked as exiting.
- On a later update, it enters `TransitionOff` and is removed after the transition completes.

`ScreenManager.RemoveScreen` unloads screen content when the manager has already been initialized, removes the screen from both lists, and updates touch gesture configuration from the new topmost screen.

## Resize/scaling lifecycle

Every `GameScreen.Update` checks whether the graphics backbuffer dimensions differ from the manager's cached dimensions. If they differ, `ScalePresentationArea()` is called.

`ScreenManager.ScalePresentationArea()` computes a scale and letterbox offsets from the base presentation size and current backbuffer size. It also updates the input transformation using the inverse global transformation matrix.
