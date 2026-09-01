# Screens and UI inventory

The Core screen area contains reusable screen infrastructure and concrete UI/game screens. Standard Myra widgets are now the presentation layer for the migrated application menus.

## Confirmed concrete screens

- `GameplayScreen` — primary gameplay screen created by `RailDispatchMonoGame`.
- `MenuScreen` — legacy menu abstraction retained by other screens; it is no longer the base class of `PauseScreen`.
- `MenuEntry` — legacy menu item/support object retained where older screens still require it.
- `PauseScreen` — pause lifecycle and action owner; visible UI is exclusively Myra.
- `SettingsScreen` — settings logic owner with Myra presentation.
- `AboutScreen` — about logic owner with Myra presentation.
- `LoadingScreen` — loading-stage screen.
- `BackgroundScreen` — background presentation layer.
- `MessageBoxScreen` — message/dialog overlay; still legacy.

## Myra surfaces

- `MyraMainMenuView` — centered startup menu containing New Game, Settings, About and Quit.
- `MyraSettingsView` — settings presentation.
- `MyraAboutView` — about presentation.
- `MyraPauseView` — centered pause menu containing Resume, Save, Load and Quit.
- `MyraUIManager` — owns the shared Myra `Desktop` and active root widget.

The startup Main Menu does not expose Load Game. Save/Load exist only in the pause menu.

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
- `PauseScreen` handles only the non-visual pause contract, including `ESC`/controller cancel.
- `PauseScreen` contains no `MenuEntry` instances and therefore cannot create a second visible legacy pause menu.
- Gameplay Save/Load is not part of `GameplayScreen.DrawHud()`.
- Gameplay HUD, railway rendering and radial gameplay tools remain outside Myra unless explicitly migrated later.

## Persistence UI

`MyraPauseView` dispatches Save and Load to `GameplayScreen` through `PauseScreen` callbacks. `MapSaveService` is the persistence boundary; the UI does not implement file I/O.

## AI rule for UI changes

Before adding a button, menu, dialog or overlay:

1. inspect existing screen lifecycle and persistence contracts;
2. inspect the current Myra view pattern before creating another widget tree;
3. reuse the existing `ScreenManager` and input architecture;
4. preserve logical 800x480 presentation semantics where legacy UI still depends on them;
5. keep one clear owner for each visible UI action;
6. do not add a legacy fallback UI for a surface already migrated to Myra.
