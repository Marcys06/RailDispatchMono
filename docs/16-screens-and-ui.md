# Screens and UI inventory

The Core screen area contains reusable screen infrastructure and concrete UI/game screens. Standard Myra widgets are the presentation layer for the migrated application menus.

## Confirmed concrete screens

- `GameplayScreen` — primary gameplay screen created by `RailDispatchMonoGame`.
- `MenuScreen` — legacy menu abstraction retained for compatibility; it is no longer the base class of `PauseScreen`.
- `MenuEntry` — legacy menu item/support object retained where older screens still require it.
- `PauseScreen` — pause lifecycle and action owner; visible UI is exclusively Myra.
- `SettingsScreen` — settings logic owner with Myra presentation.
- `AboutScreen` — about logic owner with Myra presentation.
- `LoadingScreen` — loading-stage screen.
- `BackgroundScreen` — background presentation layer.
- `MessageBoxScreen` — legacy dialog infrastructure retained for compatibility.

## Myra surfaces

- `MyraMainMenuView` — centered startup menu containing New Game, Settings, About and Quit.
- `MyraSettingsView` — settings presentation.
- `MyraAboutView` — about presentation.
- `MyraPauseView` — centered pause menu containing Resume, Save, Load and Quit.
- `MyraUIManager` — owns the shared Myra `Desktop` and active root widget.

The startup Main Menu does not expose Load Game. Save/Load exist only in the pause menu.

## Pause lifecycle

`GameplayScreen` is the authoritative owner of pause state. `PauseScreen` is a popup lifecycle adapter for the Myra pause surface. A Resume action raises `OnResume`; `GameplayScreen.ResumeGame()` then sets `_isPaused` to `false`, detaches the popup reference and removes that exact `PauseScreen` from `ScreenManager`. `PauseScreen` does not call `ExitScreen()` after raising `OnResume`, avoiding a second lifecycle transition during a Myra callback.

## Screen layering model

```text
BackgroundScreen
       |
       +--> Gameplay/Menu screen
                 |
                 +--> Popup / MessageBox / Pause
```

For migrated menus, the active screen installs the Myra root during `LoadContent()` and clears it during `UnloadContent()`. The game host renders the shared Myra desktop after the normal `ScreenManager` stack.

## Input ownership

- Myra Desktop handles pointer interaction for migrated widgets.
- `PauseScreen` handles the non-visual pause contract, including `ESC`/controller cancel.
- `PauseScreen` contains no `MenuEntry` instances and therefore cannot create a second visible legacy pause menu.
- Gameplay Save/Load is not part of `GameplayScreen.DrawHud()`.
- Gameplay HUD, railway rendering and radial gameplay tools remain outside Myra by design.

## Persistence UI

`MyraPauseView` dispatches Save and Load to `GameplayScreen` through `PauseScreen` callbacks. `MapSaveService` is the persistence boundary; the UI does not implement file I/O.

## Myra completion boundary

`0.1.2j` is the completion/freeze point for the current Myra integration. Main Menu, Settings, About and Pause are migrated, the shared Desktop lifecycle is centralized, and gameplay HUD/rendering remains intentionally outside Myra. Further Myra changes are not part of the `0.1.2x` release line.

## AI rule for UI changes

Before adding a button, menu, dialog or overlay:

1. inspect existing screen lifecycle and persistence contracts;
2. inspect the current Myra view pattern before creating another widget tree;
3. reuse the existing `ScreenManager` and input architecture;
4. preserve logical 800x480 presentation semantics where legacy UI still depends on them;
5. keep one clear owner for each visible UI action;
6. do not add a legacy fallback UI for a surface already migrated to Myra;
7. treat `0.1.2j` as the frozen Myra integration boundary unless a later release explicitly reopens the scope.
