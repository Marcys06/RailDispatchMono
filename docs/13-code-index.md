# Code index

This index points to the current implementation boundaries. Read callers and consumers before modifying a listed component.

## Application and screens

- `RailDispatchMono/RailDispatchMono.Core/RailDispatchMonoGame.cs` — shared MonoGame game loop.
- `RailDispatchMono/RailDispatchMono.Core/ScreenManagers/ScreenManager.cs` — screen lifecycle, input routing and drawing.
- `RailDispatchMono/RailDispatchMono.Core/Screens/GameplayScreen.cs` — primary gameplay screen and simulation service composition.
- `RailDispatchMono/RailDispatchMono.Core/Screens/DepotScreen.cs` — Depot builder.

## Train domain

- `Game/Train/Train.cs` — train state, speed, direction and gameplay-facing model.
- `Game/Train/TrainMovement.cs` — acceleration/braking and movement integration.
- `Game/Train/TrainSchedule.cs` — persistent train schedule document.
- `Game/Train/TrainScheduleAutomation.cs` — locomotive timetable runtime and station departure gating.
- `Game/Train/LocomotiveSchedule.cs` — locomotive timetable definition and runtime state.
- `Game/Train/Locomotive.cs` — locomotive rolling stock and independent timetable ownership.
- `Game/Train/WagonSchedule.cs` — existing wagon timetable model/runtime.
- `Game/Train/ScheduleStorage.cs` — locomotive/wagon timetable persistence.
- `Game/Train/TrainManualShunting.cs` — F6 manual shunting.
- `Game/Train/TrainDirection.cs` — F7 travel-direction reversal.
- `Game/Train/TrainGeometry.cs` — rigid offsets, trajectory/curve movement and transforms.
- `Game/Train/TrainManager.cs` — authoritative train lifecycle.
- `Game/Train/TrainManager.Coupling.cs` — coupling command path.
- `Game/Train/TrainComposition.cs` — authoritative ordered vehicle collection.
- `Game/Train/Wagon.cs` — wagon state, passengers and wagon timetable.

## Coupling

- `Game/Train/CouplingService.cs` — authoritative coupling/decoupling validation and mutation.
- `Game/Train/CouplingConnection.cs` — concrete vehicle-end connection.
- `Game/Train/CouplingSpecification.cs` — physical coupler compatibility.
- `Game/Train/VehicleCouplingState.cs` — runtime per-end connection state.

## Railway and stations

- `Game/Railway/Station.cs` — station identity, geometry and passenger/stop parameters.
- `Game/Railway/StationController.cs` — station lifecycle, timetable departure gate, dwell and passenger service.
- `Game/Railway/Block.cs` — block occupancy and reservation state.
- `Game/Railway/BlockController.cs` — physical block occupancy and existing block lifecycle.
- `Game/Railway/RailwayDispatcher.cs` — first-come-first-served arbitration over the next connected block.
- `Game/Railway/Signal.cs` / `SignalController.cs` — existing player-controlled signal system.
- `Game/Railway/TrackRoute.cs` — railway route representation.
- `Game/Railway/DepotController.cs` — depot ownership.

## Passenger subsystem

- `Game/Passengers/Passenger.cs` — fixed origin/destination passenger model.
- `Game/Passengers/PassengerManager.cs` — active passenger collection and station exchange.
- `Game/Passengers/IPassengerService.cs` / `DefaultPassengerService.cs` — station service boundary.
- `Game/Passengers/IPassengerDemandProvider.cs` / `RandomPassengerDemandProvider.cs` — demand generation.

## Rendering and UI

- `Game/Rendering/TrainRenderer.cs` — rolling-stock rendering.
- `Game/Rendering/StationRenderer.cs` — station rendering.
- `UI/Myra/MyraGameplayView.cs` — gameplay HUD.
- `UI/Myra/MyraUIManager.cs` — Myra root boundary.

## Persistence

- `Game/Save/RuntimeSaveService.cs` — runtime train/map save-load boundary.
- `Game/Train/ScheduleStorage.cs` — schedule-definition persistence.

## AI workflow

For timetable changes inspect `LocomotiveSchedule`, `TrainScheduleAutomation`, `StationController`, `TrainManager`, `ScheduleStorage`, `WagonSchedule` and the gameplay HUD together.

For block/safety changes inspect `Block`, `BlockController`, `RailwayDispatcher`, `SignalController`, `TrainMovement`, `TrainCollisionController` and `StationController` together.
