# Runtime lifecycle

## Startup

The platform host starts the MonoGame application and constructs the shared `RailDispatchMonoGame` instance.

`RailDispatchMonoGame` constructs its `GraphicsDeviceManager` and shared `MyraUIManager`, then configures basic graphics/game-loop settings in its constructor.

## Initialization

`RailDispatchMonoGame.Initialize()` creates:

1. a `ScreenManager` associated with the game;
2. the Main Menu screen;
3. registration of the Main Menu through `ScreenManager.AddScreen`.

The base MonoGame initialization is then invoked.

## Content loading

`RailDispatchMonoGame.LoadContent()` initializes `MyraUIManager`. This assigns `MyraEnvironment.Game` and creates the shared Myra `Desktop` after MonoGame has a graphics context.

`ScreenManager` has its own MonoGame content-loading override for shared drawing resources and registered screens. Myra initialization is deliberately kept in the top-level game lifecycle and must not be repeated by individual screens.

## Update order

The top-level game delegates to `ScreenManager.Update(gameTime)` through the registered game component.

`ScreenManager.Update` first updates `InputState`, then processes screens from the topmost screen toward the bottom. Myra does not change this traversal at `0.1.2a`.

## Draw order

`ScreenManager.Draw` iterates the registered screen collection and invokes `Draw` for visible screens. Myra's shared `Desktop` is available through `MyraUIManager` for later screen integration; it is not rendered globally in `0.1.2a`.

This avoids adding a second global UI draw pass before the first Myra-backed screen defines the required layering semantics.

## Resize/scaling lifecycle

The existing `ScreenManager` presentation scaling remains authoritative. Future Myra desktops/widgets must use the established logical presentation/input model rather than introducing a separate coordinate transform.
