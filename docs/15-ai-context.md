# AI context packet

## One-paragraph context

RailDispatchMono is a C#/.NET 9 MonoGame project with a shared Core implementation and platform hosts. The central runtime class is `RailDispatchMonoGame`, which configures graphics/game-loop defaults and delegates update/draw work to `ScreenManager`. `ScreenManager` owns the collection of `GameScreen` layers, shared input state, shared drawing resources, screen lifecycle, transitions, input routing, touch gestures and presentation scaling. Gameplay/domain code is under `Game`, including railway and train subsystems. Settings are represented by `RailDispatchMonoSettings` and persisted through platform-specific storage classes.

## Mental model

```text
APPLICATION HOST
    |
    +--> RailDispatchMonoGame
            |
            +--> GraphicsDeviceManager
            +--> ScreenManager
                    |
                    +--> InputState
                    +--> GameScreen(s)
                    |      |
                    |      +--> gameplay/menu/UI behavior
                    |
                    +--> SpriteBatch / Font / blank texture
                    +--> logical-to-physical presentation transform

GAME DOMAIN
    |
    +--> Railway
    |      +--> BlockController
    |      +--> Junction
    |      +--> TrackRoute
    |
    +--> Train
           +--> TrainManager
           +--> Train
           +--> Vehicle
           +--> Locomotive
           +--> Wagon
```

## First files to inspect for common tasks

- Startup/game loop: `RailDispatchMono.Core/RailDispatchMonoGame.cs`
- Screen behavior: `RailDispatchMono.Core/Screens/GameScreen.cs`
- Screen orchestration: `RailDispatchMono.Core/ScreenManagers/ScreenManager.cs`
- Input: `RailDispatchMono.Core/Inputs/InputState.cs`
- Settings: `RailDispatchMono.Core/Settings/RailDispatchMonoSettings.cs`
- Train behavior: `RailDispatchMono.Core/Game/Train/`
- Railway behavior: `RailDispatchMono.Core/Game/Railway/`
- Rendering/camera: `RailDispatchMono.Core/Game/Rendering/`

## Hard constraints

- Do not invent missing classes or APIs.
- Do not create a parallel screen manager.
- Do not bypass `InputState` for shared input semantics.
- Do not move shared gameplay into a platform host.
- Do not store authoritative simulation state only in a screen.
- Do not change shared APIs without searching all usages.
- Do not treat stale comments as executable behavior.

## Coordinate systems

The screen manager uses a logical base size of 800x480 and calculates a scale/offset transformation for the physical backbuffer. Input uses the inverse transformation so pointer positions correspond to logical game coordinates.

## Current implementation warnings

The current source contains hard-coded desktop/mobile flags and multiple content-loading touchpoints. These are existing implementation characteristics and should be preserved unless a task explicitly changes them.
