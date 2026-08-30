# Repository audit notes

## Audit scope completed

The documentation pass established the central architecture and navigation model from the current `master` source tree and inspected the key runtime contracts directly.

Directly inspected in detail:

- `RailDispatchMono.Core.csproj`
- `RailDispatchMonoGame.cs`
- `ScreenManager.cs`
- `GameScreen.cs`
- `InputState.cs`
- `RailDispatchMonoSettings.cs`
- Android `MainActivity.cs`

Repository search also confirmed the presence of the principal screen, railway, train, map, rendering, settings and effects components documented by the index.

## Important distinction

This documentation is an architectural/context layer, not generated API reference documentation. It deliberately avoids inventing method behavior for classes whose full implementation was not necessary to establish the central architecture.

For a task involving a specific subsystem, the AI must still read that subsystem's implementation and call sites before making changes.

## Current known source-tree signals

Confirmed source areas include:

- `Game/Map`
- `Game/Railway`
- `Game/Train`
- `Game/Rendering`
- `Screens`
- `Screens/UI`
- `ScreenManagers`
- `Inputs`
- `Settings`
- `Effects`
- `Content`

## Documentation objective

The main failure mode this documentation is designed to prevent is context loss: an AI should understand the project-wide contracts before editing a local file.

The most important contracts are:

1. Core is shared.
2. The top-level game delegates to the screen manager.
3. The screen manager owns screen orchestration and shared input state.
4. Game screens own presentation behavior but participate in a manager-controlled lifecycle.
5. Input coordinates are transformed into logical presentation space.
6. Domain state should remain in domain/game subsystems.
7. Platform hosts should not duplicate Core gameplay.
8. Existing implementation behavior takes precedence over generic framework templates.

## Completion criterion for future audits

A future documentation pass is considered complete for a subsystem only when its public types, state ownership, lifecycle, callers, dependencies, platform interactions and extension points have been checked against current source.
