# Screens and UI inventory

## Current development line

`0.1.6e` is the current documented baseline. Myra remains the single migrated UI integration boundary through `MyraUIManager`.

## Concrete screens

- `GameplayScreen` — primary gameplay screen and authoritative pause/persistence owner;
- `DepotScreen` — full-screen Depot train builder;
- `MenuScreen`, `MenuEntry`, `MessageBoxScreen` — legacy infrastructure retained where required;
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

The HUD exposes clock/GameDay, simulation controls, build tools and train/station information. Passenger exchange may also produce domain-level feedback through `FloatingTextManager`.

Passenger domain state remains outside the UI. There is no dedicated passenger transfer, route-choice, fare/economy or platform-crowd UI.

## Depot

Depot selection is raised by `InputManager.DepotSelected` and opens `DepotScreen`. The builder uses `RollingStockCatalog` definitions and creates the final train through `TrainManager.CreateTrainFromComposition()`.

## Train controls relevant to UI

- `C` — couple nearest valid order-preserving boundary candidate; fixed `6 km/h` limit;
- `X` — decouple wagon under cursor; only below `6 km/h`;
- `F6` — manual shunting toward `3 km/h` for the train under cursor;
- `F7` — reverse travel direction at `0 km/h` without reordering/repositioning the consist.

The former F6/F7/F8 coupling-speed selector no longer exists.

## Passenger ownership boundary

`StationController`, `PassengerManager`, `DefaultPassengerService` and `Wagon` remain authoritative. A passenger belongs to a concrete wagon, not to a UI model or directly to a train. Coupling/decoupling therefore must not trigger UI-owned passenger migration.

## Rendering boundary

`TrainRenderer` renders rolling stock using the train/trajectory transforms. `StationRenderer` and `DepotRenderer` render world objects. Railway/world rendering is not a Myra responsibility.

## Input ownership

- Myra Desktop handles migrated widget interaction;
- `GameplayScreen` owns pause state;
- `DepotScreen` owns temporary builder state; train ownership remains in `TrainManager`;
- `InputManager` owns world input and cursor selection;
- domain managers own simulation state.

## AI rule

Before adding passenger/station UI, inspect the existing Myra views and the domain chain first. Do not duplicate passenger, wagon, train or station state in UI models and do not create a second Myra `Desktop`.
