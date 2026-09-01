# Current State — 0.1.2e

`0.1.2e` is the current executable baseline of the Myra UI integration series.

## Implemented

- Added standard `Myra` NuGet package version `1.6.5` to `RailDispatchMono.Core`.
- Added `MyraUIManager` as the shared Myra integration boundary.
- `MyraEnvironment.Game` is initialized before any Myra widget can be constructed.
- Shared Myra `Desktop` uses the current viewport for its bounds.
- Main Menu visual presentation and pointer/keyboard interaction use standard Myra widgets.
- `ScreenManager` remains the owner of screen lifecycle, transitions and screen ownership.
- Existing `MenuScreen` and `MenuEntry` contracts remain intact.
- Myra is rendered once by the game host after the `ScreenManager` draw pass.
- `MyraUIManager.Initialize()` remains idempotent.

## Runtime correction

The `0.1.2e` stage fixes the startup crash caused by `MyraEnvironment.Game` being null while `MyraMainMenuView` constructed its first widget. Myra initialization now occurs at the beginning of `RailDispatchMonoGame.Initialize()`, before `ScreenManager` is created and before `MainMenuScreen` is added.

## Remaining migration

The following surfaces still use the existing UI implementation and are candidates for later `0.1.2x` stages:

- Load Game screen.
- Settings.
- About.
- Pause UI.
- Message boxes/dialogs.
- Other gameplay HUD and radial menus where migration is beneficial.

No second Myra desktop should be introduced for these screens; the shared `MyraUIManager` remains the integration boundary.

## Stage discipline

Each `0.1.2x` stage is an immutable incremental commit. If a build or runtime test exposes a defect, do not rewrite the failed stage; implement the correction in the next lettered stage.
