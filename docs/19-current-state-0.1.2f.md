# Current State — 0.1.2f

`0.1.2f` continues the accelerated Myra UI migration.

## Current UI state

- Main Menu is rendered with standard Myra widgets.
- Main Menu is centered in the viewport.
- Main Menu actions are `NOWA GRA`, `USTAWIENIA`, `O GRZE` and `WYJDŹ`.
- `WCZYTAJ GRĘ` is no longer exposed by the startup menu.
- Pause Menu is rendered with standard Myra widgets and centered in the viewport.
- Pause Menu actions are `WZNÓW GRĘ`, `ZAPISZ GRĘ`, `WCZYTAJ GRĘ` and `WYJDŹ`.
- Save/load callbacks remain implemented by `GameplayScreen`; Myra only presents the controls.

## Architecture

`ScreenManager` remains the lifecycle owner. `PauseScreen` remains the gameplay pause owner. `MyraUIManager` owns one shared Myra `Desktop` and one active root widget tree.

Migrated screens install their Myra root during `LoadContent()` and clear it during `UnloadContent()`. The game host renders the active Myra desktop after the normal screen stack.

## Legacy UI still active

- Settings
- About
- Message boxes/dialogs
- Gameplay HUD and gameplay-specific menus

These are not part of this stage's migration.

## Stage discipline

`0.1.2a` through `0.1.2e` are immutable historical stages. If testing exposes a defect in `0.1.2f`, create `0.1.2g` rather than rewriting this stage.
