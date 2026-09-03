# Development workflows

## Before changing code

1. Identify the owning subsystem.
2. Read the target class completely enough to understand lifecycle and state ownership.
3. Search for all call sites of the members being changed.
4. Check platform consumers if the change touches startup, graphics, input or storage.
5. Check existing documentation before introducing a new abstraction.

## Adding a new screen

1. Derive the screen from `GameScreen`.
2. Put screen-specific presentation/input logic in the screen.
3. Register the screen through `ScreenManager.AddScreen`.
4. Set transition/popup/gesture behavior through the existing protected properties as required.
5. Load screen-owned content through the screen lifecycle.
6. Use `InputState` semantic actions instead of directly polling devices where possible.
7. Use `ScreenManager` for screen removal and transitions.

## Adding a gameplay feature

1. Find the domain owner in `Game/`.
2. Extend the existing manager/model when appropriate.
3. Keep rendering and UI concerns outside domain classes.
4. Expose only the state required by screens.
5. Search for all consumers after changing domain APIs.

## Adding input

Prefer existing semantic methods in `InputState`. If a new semantic action is needed by more than one screen, add it to `InputState` rather than duplicating the raw keyboard/gamepad logic in each screen.

The current `0.1.5` coupling commands are a temporary exception: `TrainManager.HandleCouplingHotkeys()` owns the `C`/`X`/`F6`/`F7`/`F8` command path. Do not duplicate those commands in another input or UI owner until the planned vehicle/end selection UI replaces the temporary path.

For pointer/touch UI, use `CurrentCursorLocation`, which is already transformed into the presentation coordinate system.

## Adding a setting

1. Extend `RailDispatchMonoSettings`.
2. Preserve `INotifyPropertyChanged` semantics.
3. Locate the existing storage implementation(s) that need persistence.
4. Locate UI/localization consumers.
5. Update documentation if the setting changes runtime behavior or introduces a new platform-specific path.

## Adding an asset

1. Put the source asset in the appropriate Content tree.
2. Follow the project's existing content-pipeline conventions.
3. Use the generated/runtime asset path expected by MonoGame.
4. Search all content loads before renaming or relocating an asset.

## Changing screen behavior

Understand these three concepts separately:

- `ScreenState` — lifecycle/visibility state;
- `IsPopup` — whether lower screens are considered covered;
- `IsExiting` — permanent removal path.

Do not implement custom flags that duplicate these concepts without a specific need.

## Safe refactoring

Avoid broad refactors mixed with feature work. This repository has multiple platform targets and shared Core code; an apparently local signature change can affect several projects.

Prefer small, mechanically verifiable changes. After a public/member signature change, search the entire repository for the old signature and usages.

## Verification

The repository currently has no dedicated automated Core test project. Normal verification is therefore:

1. build the solution;
2. run the DesktopGL game when the change affects runtime behavior;
3. exercise the affected gameplay flow manually;
4. inspect diagnostics when relevant;
5. check affected platform consumers for shared Core changes.

Do not reintroduce a test project solely to validate a small gameplay change unless the project structure and documentation explicitly call for one again.

## Documentation update rule

Any change to architecture, lifecycle, file ownership, public contracts, settings, input semantics, content paths or platform responsibilities requires an update to the corresponding `docs/` file.
