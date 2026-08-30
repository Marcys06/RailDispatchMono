# Screens and UI inventory

The Core screen area contains both reusable screen infrastructure and concrete UI/game screens.

## Confirmed concrete screens

- `GameplayScreen` — primary gameplay screen created by `RailDispatchMonoGame` during initialization.
- `MenuScreen` — menu-oriented screen.
- `MenuEntry` — menu item/support object used by menu UI.
- `PauseScreen` — pause overlay/screen.
- `SettingsScreen` — settings UI.
- `AboutScreen` — about/information UI.
- `LoadingScreen` — loading-stage screen.
- `BackgroundScreen` — background presentation layer.
- `MessageBoxScreen` — message/dialog overlay.

## Supporting screen types

- `ScreenState` — screen lifecycle state type.
- `PlayerIndexEventArgs` — event argument carrying a player index.
- `EndOfLevelMessageState` — state associated with end-of-level messaging.
- `GameScreen` — base lifecycle abstraction.

## UI input

`Screens/UI/InputManager.cs` exists in addition to the lower-level `Inputs/InputState.cs` abstraction. Treat these as different layers until call-site analysis proves otherwise:

- `InputState` owns device snapshots and general semantic actions.
- `Screens/UI/InputManager` is UI-specific infrastructure.

Do not collapse them into one class without an explicit refactoring requirement.

## Screen layering model

The architecture supports multiple screens simultaneously. Typical layering can be represented as:

```text
BackgroundScreen
       |
       +--> Gameplay/Menu screen
                 |
                 +--> Popup / MessageBox / Pause
```

The exact combinations are controlled by runtime registration and `IsPopup` behavior; the diagram is conceptual, not a fixed startup stack.

## AI rule for UI changes

Before adding a button, menu, dialog or overlay:

1. inspect existing `MenuEntry`, `MenuScreen`, `MessageBoxScreen` and related UI code;
2. inspect how the screen is registered;
3. reuse the existing input and transition system;
4. preserve logical 800x480 presentation coordinates;
5. avoid introducing a second UI framework unless explicitly required.
