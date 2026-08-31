# AI context packet

## Current release

**RailDispatchMono `0.1.0`** is the current stabilization baseline. The `0.0.16` feature scope is complete. Work under `0.1.0` should primarily fix bugs and preserve the existing architecture and APIs.

## One-paragraph context

RailDispatchMono is a C#/.NET 9 MonoGame project with shared Core code and platform hosts. `RailDispatchMonoGame` configures the game and delegates update/draw work to `ScreenManager`. `ScreenManager` owns the `GameScreen` collection, shared input state, drawing resources, lifecycle, transitions, input routing, touch gestures and presentation scaling. Gameplay/domain code is under `Game`, including railway, train, station, passenger, depot, simulation and save subsystems. The application enters through Main Menu; gameplay is a managed screen. Persistence uses separate JSON files inside save directories and includes versioned metadata. Settings remain persisted through the existing settings infrastructure.

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
                    |      +--> Main Menu / Gameplay / Pause / Settings / UI
                    |
                    +--> SpriteBatch / shared resources
                    +--> logical-to-physical presentation transform

GAME DOMAIN
    |
    +--> Map / Railway
    |      +--> GameMap
    |      +--> BlockController
    |      +--> SignalController / SignalRenderer
    |      +--> Junction / TrackRoute
    |
    +--> Train
    |      +--> TrainManager
    |      +--> Train
    |      +--> Vehicle / Locomotive / Wagon
    |
    +--> Stations / Passengers
    |
    +--> Depot
    |
    +--> Simulation
    |      +--> GameClock
    |
    +--> Save
           +--> save directories
           +--> metadata.json
           +--> separate runtime JSON files
```

## Startup flow

```text
Application host
    -> RailDispatchMonoGame
        -> ScreenManager
            -> MainMenuScreen
                -> New Game -> fresh empty gameplay state
                -> Load Game -> selected save directory -> gameplay
                -> Settings / About / Quit
```

New Game intentionally requires no confirmation.

## Save-system contract

Each save is a directory containing separate JSON files. The format is versioned with `schemaVersion`; do not invent a monolithic save file.

`metadata.json` identifies the save and contains its metadata. The runtime save implementation is responsible for serializing the supported state across the established files, including map/infrastructure, trains/vehicles, schedules/routes, passengers and simulation time (`GameDay`/`GameTime`) where implemented.

Auto-save is disabled at `0.1.0`.

If a save is corrupt or incomplete, the user must receive a notification rather than silently continuing with partial state.

## Gameplay contracts

- `GameDay` and `GameTime` are authoritative simulation-time values.
- x1/x2/x5 are simulation speed multipliers.
- Pause stops simulation progression.
- `ESC` toggles pause through the existing screen/input architecture.
- Depots are world buildings and the entry point for train creation.
- A depot may create multiple trains through the existing depot workflow.
- Wagon routes describe passenger-service destinations and do not directly control locomotive movement.
- Passenger exchange is wagon-specific.
- Passenger-count changes may produce floating `+X` / `-X` notifications at the affected wagon.

## First files to inspect for common tasks

- Startup/game loop: `RailDispatchMono.Core/RailDispatchMonoGame.cs`
- Screen behavior: `RailDispatchMono.Core/Screens/GameScreen.cs`
- Screen orchestration: `RailDispatchMono.Core/ScreenManagers/ScreenManager.cs`
- Main menu: `RailDispatchMono.Core/Screens/MainMenuScreen.cs`
- Gameplay: `RailDispatchMono.Core/Screens/GameplayScreen.cs`
- Input: `RailDispatchMono.Core/Inputs/InputState.cs` and `Screens/UI/InputManager.cs`
- Settings: `RailDispatchMono.Core/Settings/`
- Train behavior: `RailDispatchMono.Core/Game/Train/`
- Railway behavior: `RailDispatchMono.Core/Game/Railway/`
- Passenger/station behavior: `RailDispatchMono.Core/Game/`
- Save behavior: `RailDispatchMono.Core/Game/Save/`
- Rendering/camera: `RailDispatchMono.Core/Game/Rendering/`

## Hard constraints

- Do not invent missing classes or APIs.
- Do not create a parallel screen manager.
- Do not bypass the established input architecture.
- Do not move shared gameplay into a platform host.
- Do not store authoritative simulation state only in a screen.
- Do not change existing shared APIs without searching all usages.
- Do not treat stale comments as executable behavior.
- At `0.1.0`, prefer targeted fixes over feature expansion.

## Coordinate systems

The screen manager uses the established logical presentation transform for the game viewport. Input must use the inverse transformation so pointer positions correspond to logical game coordinates.

## Debugging rule

Repeated or duplicated logs do not by themselves prove duplicated simulation updates. Inspect logger subscriptions/call sites and the screen/update traversal before changing game-loop logic.
