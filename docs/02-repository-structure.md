# Repository structure

## Top level

The repository contains the solution file `RailDispatchMono.slnx`, repository metadata, `CHANGELOG.md`, and the `RailDispatchMono` source directory.

## Main source projects

### `RailDispatchMono/RailDispatchMono.Core`

Shared game implementation. Important areas currently present include:

- `Game/` — simulation/game-domain code.
- `Game/Train/` — train, locomotive, wagon, vehicle and train-management code.
- `Game/Railway/` — railway infrastructure and routing code such as blocks, junctions and track routes.
- `Game/Rendering/` — rendering helpers such as the camera.
- `Screens/` — screen implementations and screen UI code.
- `ScreenManagers/` — screen orchestration.
- `Inputs/` — shared input state handling.
- `Settings/` — settings model and platform storage implementations.
- `Effects/` — visual effects infrastructure such as `ParticleManager`.
- `Content/` — game assets and content-pipeline inputs.

The project file also contains an empty `Docs` folder declaration. The repository-level documentation added by this work lives in `docs/` at the repository root and must not be confused with that existing project-folder declaration.

### `RailDispatchMono/RailDispatchMono.Android`

Android host/application project. It contains `MainActivity.cs`, Android manifest/resources and the Android project file.

### Other platform projects

The repository tree contains additional platform targets. Platform-specific startup code should remain in its respective project. Shared gameplay code belongs in Core unless a dependency genuinely requires platform separation.

## Naming orientation

When locating code, start from the responsibility rather than guessing a filename:

- game lifecycle → `RailDispatchMonoGame.cs`
- screen lifecycle → `Screens/GameScreen.cs`
- screen orchestration → `ScreenManagers/ScreenManager.cs`
- input state → `Inputs/InputState.cs`
- trains → `Game/Train/`
- railway network → `Game/Railway/`
- camera/rendering → `Game/Rendering/`
- settings → `Settings/`
- effects → `Effects/`

## Do not assume

A directory name is not proof that every class in it follows a strict architectural pattern. Read the actual class and its call sites before introducing a new abstraction. In particular, avoid assuming that all game-domain classes are independent services or that all screens own their own input infrastructure.
