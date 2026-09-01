# Current State — 0.1.2pre

`0.1.2pre` is the current Myra UI stabilization preview following the immutable `0.1.2a`–`0.1.2k` stages.

## Current UI state

- Main Menu uses standard Myra widgets and is centered in the viewport.
- Main Menu actions are `NOWA GRA`, `USTAWIENIA`, `O GRZE` and `WYJDŹ`.
- `WCZYTAJ GRĘ` is not exposed by the startup menu.
- Settings and About use Myra presentation.
- Pause Menu uses `MyraPauseView` with `WZNÓW GRĘ`, `ZAPISZ GRĘ`, `WCZYTAJ GRĘ` and `WYJDŹ`.
- Gameplay HUD does not expose a second Save/Load UI.

## Pause architecture

Pause is a state of `GameplayScreen`, not a popup `GameScreen`.

`GameplayScreen` owns the authoritative `_isPaused` flag and the pause/resume lifecycle. Entering pause activates the Myra pause root. While paused, gameplay simulation is not advanced, but the shared Myra desktop remains interactive. Resuming clears the pause state and clears the Myra root.

The old `PauseScreen` popup architecture was removed because it introduced competing lifecycle/input ownership and could consume UI actions without correctly changing the gameplay state.

`ESC` and the Myra Resume action use the same authoritative resume path. Save and Load are also owned by `GameplayScreen`; the Myra view only dispatches the actions.

## Myra architecture

`MyraUIManager` owns one shared Myra `Desktop` and one active root. `MyraEnvironment.Game` is initialized before Myra widgets are constructed. Main Menu, Settings, About and Pause use this shared integration boundary.

Myra widget callbacks must not directly mutate the screen stack or perform lifecycle-sensitive work during `Desktop.Render()`. Such actions are dispatched through the normal game update lifecycle.

## Persistence

`MapSaveService` remains the persistence boundary for map/infrastructure data and runtime save-slot state. Save/Load are exposed only through the pause menu during gameplay.

After a successful save, Load can be enabled in the same pause view. Loading refreshes the dependent gameplay controllers without recreating the pause UI.

## Non-Myra gameplay UI

Railway rendering, gameplay HUD, radial build/signal/junction menus and floating gameplay text remain direct MonoGame systems. This is intentional and is not part of the application-menu Myra migration.

## Current verification

The developer has confirmed that the pause system works after the final small bugfix applied after the pause architecture rebuild.

Verified target:

1. `ESC` enters pause.
2. `WZNÓW GRĘ` exits pause and gameplay resumes.
3. `ZAPISZ GRĘ` performs the save operation.
4. `WCZYTAJ GRĘ` performs the load operation.
5. `WYJDŹ` follows the existing quit flow.
6. Main Menu remains functional.

## Version policy

`0.1.2a`–`0.1.2k` are immutable historical development stages. `0.1.2pre` is the current stabilization version. Missing historical release descriptions must be recorded as `bugfix` rather than guessed.
