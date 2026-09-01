# Current State — 0.1.2j

`0.1.2j` is the final Myra integration stage of the `0.1.2x` series.

## Current UI state

- Main Menu is rendered with standard Myra widgets and centered in the viewport.
- Main Menu actions are `NOWA GRA`, `USTAWIENIA`, `O GRZE` and `WYJDŹ`.
- `WCZYTAJ GRĘ` is not exposed by the startup menu.
- Settings and About use Myra presentation.
- Pause Menu is rendered exclusively with standard Myra widgets and centered in the viewport.
- Pause Menu actions are `WZNÓW GRĘ`, `ZAPISZ GRĘ`, `WCZYTAJ GRĘ` and `WYJDŹ`.
- `PauseScreen` is a `GameScreen` popup and no longer derives from legacy `MenuScreen`.
- Gameplay HUD contains no visible Save/Load controls; persistence is exposed through the pause menu.
- Save/Load callbacks reach `GameplayScreen` and the active save-slot persistence stack.

## Pause lifecycle

`GameplayScreen` owns the authoritative `_isPaused` state and the active `PauseScreen` reference. Resume calls `GameplayScreen.ResumeGame()`, which clears `_isPaused`, detaches the reference and removes that exact popup from `ScreenManager`. `PauseScreen` only raises `OnResume` and does not call `ExitScreen()` itself. This prevents a double lifecycle transition when Resume originates in a Myra widget callback.

## Myra architecture

`MyraUIManager` owns one `Desktop` and one active root. Migrated screens install the root during `LoadContent()` and clear it during `UnloadContent()`. The host renders the desktop after the `ScreenManager` stack. `MyraEnvironment.Game` is assigned before any Myra widget is constructed. `Desktop.Render()` performs the Myra widget input/render pass.

## Persistence

`MapSaveService` remains the persistence boundary for map/infrastructure data and, when a save slot is active, runtime train/passenger/clock persistence. The pause UI only dispatches actions; it does not perform file operations itself.

## Deliberately non-Myra gameplay UI

The railway renderer, gameplay HUD, radial build/signal/junction menus and floating gameplay text remain direct MonoGame rendering. This is intentional: Myra migration is for standard application/menu UI, not a blanket replacement of gameplay rendering.

## Verification target

The final `0.1.2j` integration is considered correct when:

1. gameplay has no legacy Save/Load buttons;
2. ESC opens the centered Myra pause menu;
3. `ZAPISZ GRĘ` writes the active slot;
4. `WCZYTAJ GRĘ` is enabled only when a save exists and restores the saved state;
5. Resume clears the pause state and removes the pause screen/root;
6. no duplicate Myra desktop/root or legacy pause menu is visible;
7. Main Menu, Settings, About and Pause all use the shared Myra integration boundary.

## Release boundary

`0.1.2j` closes the Myra integration scope. Further Myra migration or redesign is outside the `0.1.2x` line and requires an explicit later release goal.

Historical `0.1.2a` through `0.1.2i` release notes remain immutable in `docs/changelog/`.
