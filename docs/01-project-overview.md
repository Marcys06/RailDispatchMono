# Project overview

## Identity

RailDispatchMono is a MonoGame-based railway dispatch/game project implemented in C#.

The repository is organized as a solution with a shared `RailDispatchMono.Core` project and platform-specific projects, including Android and desktop targets.

## Technology baseline

The shared core project currently declares:

- SDK-style .NET project format.
- Target framework: `net9.0`.
- Nullable reference types enabled.
- `MonoGame.Framework.Native` version `3.8.*` as a private package dependency.
- `Myra` version `1.6.5` as the UI library used by the shared Core layer.

The Myra dependency is the standard MonoGame integration package. The integration is initialized by `RailDispatchMonoGame` and exposed through the shared `MyraUIManager`; it does not replace `ScreenManager`.

These values come from `RailDispatchMono.Core.csproj` and should be treated as the current baseline rather than inferred from older documentation.

## High-level responsibility split

### Core

`RailDispatchMono.Core` contains the reusable game implementation: the MonoGame `Game` entry abstraction, screen system, input system, game-domain classes, settings, effects, rendering helpers, Myra UI integration and content.

### Platform projects

Platform projects provide the platform-specific application host and configuration while consuming the shared core. Do not move platform-specific bootstrapping into Core unless the code is genuinely platform-independent.

## Game loop model

The central game object is `RailDispatchMonoGame`. It owns a `GraphicsDeviceManager`, creates a `ScreenManager`, initializes the shared Myra UI infrastructure and delegates update/draw work to the screen manager.

The current implementation uses a fixed 60 Hz timestep and a preferred 1600x900 backbuffer in the shared game class.

## Design intent

The project uses a layered screen architecture. A screen is a self-contained update/draw/input layer. Multiple screens can coexist, with the manager determining which screens receive input and how underlying screens are covered or transitioned.

Myra is a UI/rendering layer inside this architecture. It is not a replacement for screen lifecycle, domain ownership or the shared input-routing contract.
