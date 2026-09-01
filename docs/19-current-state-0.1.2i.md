# Current State — 0.1.2i

`0.1.2i` completes the current pause-menu/UI consolidation stage of the accelerated Myra migration.

## Current UI state

- Main Menu is rendered with standard Myra widgets and centered in the viewport.
- Main Menu actions are `NOWA GRA`, `USTAWIENIA`, `O GRZE` and `WYJDŹ`.
- `WCZYTAJ GRĘ` is not exposed by the startup menu.
- Settings and About use Myra presentation.
- Pause Menu is rendered exclusively with standard Myra widgets and centered in the viewport.
- Pause Menu actions are `WZNÓW GRĘ`, `ZAPISZ GRĘ`, `WCZYTAJ GRĘ` and `WYJDŹ`.
- `PauseScreen` no longer derives from the legacy `MenuScreen` and contains no `MenuEntry` surface.
- Gameplay HUD contains no visible Save/Load controls; persistence is exposed through the pause menu.
- Save/Load callbacks reach `GameplayScreen` and the active save-slot persistence stack.

## Myra architecture

`MyraUIManager` owns one `Desktop` and one active root. Migrated screens install the root during `LoadContent()` and clear it during `UnloadContent()`. The host renders the desktop after the `ScreenManager` stack. Myra's `Desktop.Render()` performs the widget input/render pass; the host does not maintain a second UI input loop.

## Persistence

`MapSaveService` remains the persistence boundary for map/infrastructure data and, when a save slot is active, runtime train/passenger/clock persistence. The pause UI only dispatches actions; it does not perform file operations itself.

## Deliberately non-Myra gameplay UI

The railway renderer, gameplay HUD, radial build/signal/junction menus and floating gameplay text remain direct MonoGame rendering. This is intentional: Myra migration is for standard application/menu UI, not a blanket replacement of gameplay rendering.

## Verification target

The stage is considered correct when:

1. gameplay has no legacy Save/Load buttons;
2. ESC opens the centered Myra pause menu;
3. `ZAPISZ GRĘ` writes the active slot;
4. `WCZYTAJ GRĘ` is enabled only when a save exists and restores the saved state;
5. Resume removes the pause root and returns to gameplay;
6. no duplicate Myra desktop/root or legacy pause menu is visible.

## Stage discipline

`0.1.2a` through `0.1.2h` are immutable historical stages. Any defect found in `0.1.2i` is fixed in the next lettered stage.
