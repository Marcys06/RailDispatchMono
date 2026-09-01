# Screens and UI inventory

The Core screen area contains reusable screen infrastructure and concrete UI/game screens. Standard Myra widgets are the presentation layer for migrated application menus.

## Confirmed concrete screens

- `GameplayScreen` — primary gameplay screen and authoritative pause/persistence owner.
- `MenuScreen` — legacy menu abstraction retained for compatibility.
- `MenuEntry` — legacy menu item/support object retained where older screens still require it.
- `SettingsScreen` — settings logic owner with Myra presentation.
- `AboutScreen` — about logic owner with Myra presentation.
- `LoadingScreen` — loading-stage screen.
- `BackgroundScreen` — background presentation layer.
- `MessageBoxScreen` — legacy dialog infrastructure retained for compatibility.

`PauseScreen` is no longer part of the current runtime architecture. The visible pause surface is `MyraPauseView`, owned by the gameplay pause state.

## Myra surfaces

- `MyraMainMenuView` — centered startup menu containing New Game, Settings, About and Quit.
- `MyraSettingsView` — settings presentation.
- `MyraAboutView` — about presentation.
- `MyraPauseView` — centered pause menu containing Resume, Save, Load and Quit.
- `MyraUIManager` — owns the shared Myra `Desktop` and active root widget.

The startup Main Menu does not expose Load Game. Save/Load exist only in the pause menu.

## Pause lifecycle

`GameplayScreen` is the sole authoritative owner of pause state. Pause is represented by `_isPaused`, not by a second `GameScreen` popup.

Entering pause activates `MyraPauseView` through the shared `MyraUIManager`. While paused, simulation updates are skipped but the shared Myra desktop remains interactive. Resume clears the gameplay pause state and clears the Myra root.

This architecture avoids competing lifecycle owners and prevents a pause popup from blocking or altering `ScreenManager` input routing.

## Screen layering model

```text
BackgroundScreen
       |
       +--> Gameplay/Menu screen
                 |
                 +--> optional legacy popup/dialog

Gameplay pause is a state of GameplayScreen, not a popup.
```

## Input ownership

- Myra Desktop handles pointer interaction for migrated widgets.
- `GameplayScreen` owns the authoritative ESC pause/resume path.
- `InputManager` does not contain a second pause menu.
- Pause actions are dispatched to gameplay-owned operations.
- Gameplay Save/Load is not part of `GameplayScreen.DrawHud()`.
- Gameplay HUD, railway rendering and radial gameplay tools remain outside Myra by design.

## Persistence UI

`MyraPauseView` dispatches Save and Load to `GameplayScreen`. `MapSaveService` is the persistence boundary; the UI does not implement file I/O.

## Myra completion/stabilization boundary

The immutable `0.1.2a`–`0.1.2k` stages contain the incremental Myra migration and pause fixes. `0.1.2pre` is the current stabilization preview and records the rebuilt pause architecture together with the final small stabilization bugfixes.

## AI rule for UI changes

Before adding a button, menu, dialog or overlay:

1. inspect existing lifecycle and persistence contracts;
2. inspect the current Myra view pattern before creating another widget tree;
3. reuse the existing `ScreenManager` and input architecture;
4. preserve established presentation semantics;
5. keep one clear owner for each visible UI action;
6. do not add a legacy fallback UI for a surface already migrated to Myra;
7. do not reintroduce `PauseScreen` as a pause popup unless a future release explicitly changes the architecture.
