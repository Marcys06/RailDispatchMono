# Screens and UI inventory

The Core screen area contains reusable screen infrastructure and concrete UI/game screens. Myra is now the standard presentation layer for the migrated Main Menu and Pause Menu surfaces.

## Confirmed concrete screens

- `GameplayScreen` — primary gameplay screen created by `RailDispatchMonoGame` during initialization.
- `MenuScreen` — menu-oriented screen and legacy lifecycle/input abstraction.
- `MenuEntry` — menu item/support object retained for compatibility and keyboard/controller routing.
- `PauseScreen` — pause overlay/screen; its visible menu is presented by Myra.
- `SettingsScreen` — settings UI, not yet migrated to Myra.
- `AboutScreen` — about/information UI, not yet migrated to Myra.
- `LoadingScreen` — loading-stage screen.
- `BackgroundScreen` — background presentation layer.
- `MessageBoxScreen` — message/dialog overlay; still legacy.

## Myra surfaces

- `MyraMainMenuView` — centered startup menu containing New Game, Settings, About and Quit.
- `MyraPauseView` — centered pause menu containing Resume, Save, Load and Quit.
- `MyraUIManager` — owns the shared Myra `Desktop` and active root widget.

The startup Main Menu no longer exposes Load Game. Save/load actions are intentionally grouped with gameplay pause controls.

## Screen layering model

The architecture supports multiple screens simultaneously. Typical layering can be represented as:

```text
BackgroundScreen
       |
       +--> Gameplay/Menu screen
                 |
                 +--> Popup / MessageBox / Pause
```

For migrated menus, the visible Myra root is rendered by `RailDispatchMonoGame` after the `ScreenManager` stack. The active screen remains the lifecycle owner and installs/clears the root through `MyraUIManager`.

## Input ownership

- Myra Desktop handles pointer interaction for migrated menu widgets.
- Existing `MenuScreen` handling remains available for keyboard/controller semantics, including `ESC` on pause.
- A migrated surface must not create a second visible legacy menu for the same actions.

## AI rule for UI changes

Before adding a button, menu, dialog or overlay:

1. inspect existing screen lifecycle and persistence contracts;
2. inspect the current Myra view pattern before creating another widget tree;
3. reuse the existing `ScreenManager` and input architecture;
4. preserve logical 800x480 presentation semantics where legacy UI still depends on them;
5. keep one clear owner for each visible UI action.
