# Screens and UI inventory

## Current development line

`0.1.6a` starts from the completed `0.1.5pre` screen/UI architecture. Myra remains the single migrated UI integration boundary through `MyraUIManager`.

## Concrete screens

- `GameplayScreen` — primary gameplay screen and authoritative pause/persistence owner.
- `DepotScreen` — full-screen Depot train builder.
- `MenuScreen`, `MenuEntry`, `MessageBoxScreen` — legacy infrastructure retained where required.
- `SettingsScreen`, `AboutScreen`, `LoadingScreen`, `BackgroundScreen` — application/game screens.

Pause is gameplay state owned by `GameplayScreen`; presentation is `MyraPauseView`, not a separate popup screen.

## Myra surfaces

- `MyraMainMenuView`
- `MyraSettingsView`
- `MyraAboutView`
- `MyraPauseView`
- `MyraGameplayView`
- `MyraDepotView`
- `MyraUIManager`

There is one shared Myra `Desktop` and one active root. Depot temporarily replaces the gameplay root and restores it on close.

## Gameplay HUD

The gameplay HUD exposes the clock/GameDay, simulation controls, build tools and train/station information. Selecting a train or station can request camera navigation to its world-space position.

Passenger exchange currently also has domain-level feedback through `FloatingTextManager` when `PassengerManager` reports a boarding/alighting exchange. This is not a full passenger visualization system.

## Depot

Depot selection is raised by `InputManager.DepotSelected` and opens `DepotScreen`. The builder uses `RollingStockCatalog` definitions and creates the final train only through `TrainManager.CreateTrainFromComposition()`.

## Train controls relevant to UI

- `C` — couple nearest valid boundary candidate, fixed `6 km/h` coupling limit.
- `X` — decouple wagon under cursor, only below `6 km/h`.
- `F6` — manual shunting toward `3 km/h` for the train under cursor.
- `F7` — reverse travel direction at `0 km/h`; does not reorder/reposition the consist.

The former F6/F7/F8 coupling-speed selector no longer exists.

## Station/passenger UI boundary

The current HUD may present train/station information, but passenger domain state is not owned by the UI. `StationController`, `PassengerManager`, `PassengerService` and `Wagon` remain authoritative.

There is currently no dedicated passenger window, platform crowd visualization, passenger route-choice UI, fare/economy UI or transfer UI.

## Rendering boundary

`TrainRenderer` renders rolling stock and uses each vehicle's trajectory-derived transform on curves. `StationRenderer` and `DepotRenderer` render world objects. Railway/world rendering is not a Myra responsibility.

## Input ownership

- Myra Desktop handles migrated widget interaction.
- `GameplayScreen` owns pause state.
- `DepotScreen` owns temporary builder state; train ownership remains in `TrainManager`.
- `InputManager` owns world input and cursor selection.
- Domain managers own simulation state.

## AI rule

Before adding a passenger/station UI surface, inspect existing Myra gameplay views and the domain chain first. Do not duplicate domain state in UI models or create a second Myra `Desktop`.
