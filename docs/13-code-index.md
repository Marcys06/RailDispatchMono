# Code index

This index points to the current implementation boundaries. Read callers and consumers before modifying a listed component.

## Application and screens

- `RailDispatchMono/RailDispatchMono.Core/RailDispatchMonoGame.cs` — shared MonoGame game loop.
- `RailDispatchMono/RailDispatchMono.Core/ScreenManagers/ScreenManager.cs` — screen lifecycle, input routing and drawing.
- `RailDispatchMono/RailDispatchMono.Core/Screens/GameplayScreen.cs` — primary gameplay screen; owns map/simulation services and gameplay pause/persistence flow.
- `RailDispatchMono/RailDispatchMono.Core/Screens/DepotScreen.cs` — full-screen Depot builder.

## Train domain

- `Game/Train/Train.cs` — train state, speed, direction and gameplay-facing train model.
- `Game/Train/TrainMovement.cs` — acceleration/braking and movement integration.
- `Game/Train/TrainManualShunting.cs` — F6 manual shunting.
- `Game/Train/TrainDirection.cs` — F7 travel-direction reversal.
- `Game/Train/TrainGeometry.cs` — rigid offsets, trajectory/curve movement and per-vehicle transform calculation.
- `Game/Train/TrainManager.cs` — authoritative train lifecycle.
- `Game/Train/TrainManager.Coupling.cs` — C/X/F6 command path and candidate handling.
- `Game/Train/TrainComposition.cs` — authoritative ordered vehicle collection and derived consist statistics.
- `Game/Train/Vehicle.cs` — base vehicle and static/runtime coupling state.
- `Game/Train/Wagon.cs` — wagon state including passenger collection, capacity and service route.

## Coupling

- `Game/Train/CouplingService.cs` — authoritative coupling/decoupling validation and mutation.
- `Game/Train/CouplingConnection.cs` — concrete vehicle-end connection.
- `Game/Train/CouplingSpecification.cs` — physical coupler compatibility.
- `Game/Train/VehicleCouplingState.cs` — runtime per-end connection state.

## Railway and stations

- `Game/Railway/Station.cs` — station identity, geometry and passenger/stop parameters.
- `Game/Railway/StationController.cs` — station lifecycle, train stop/dwell and passenger-generation coordinator.
- `Game/Railway/ITrainStopDecision.cs` — stop decision boundary.
- `Game/Railway/DefaultTrainStopDecision.cs` — default stop policy.
- `Game/Railway/TrackRoute.cs` — railway route representation.
- `Game/Railway/BlockController.cs` — block occupancy/authority.
- `Game/Railway/SignalController.cs` — signal state/safety coordination.
- `Game/Railway/DepotController.cs` — depot ownership.

## Passenger subsystem

- `Game/Passengers/Passenger.cs` — fixed origin/destination passenger model.
- `Game/Passengers/PassengerState.cs` — waiting/on-board/arrived state enum.
- `Game/Passengers/PassengerManager.cs` — active passenger collection, boarding, alighting and exchange notifications.
- `Game/Passengers/IPassengerService.cs` — station service boundary.
- `Game/Passengers/DefaultPassengerService.cs` — alight-before-board implementation.
- `Game/Passengers/IPassengerDemandProvider.cs` — destination-demand abstraction.
- `Game/Passengers/RandomPassengerDemandProvider.cs` — temporary random destination provider.

## Rendering and UI

- `Game/Rendering/TrainRenderer.cs` — rolling-stock rendering and per-vehicle transforms.
- `Game/Rendering/StationRenderer.cs` — station world rendering.
- `Game/Rendering/FloatingTextManager.cs` — passenger exchange feedback among other floating notifications.
- `UI/Myra/MyraUIManager.cs` — single shared Myra Desktop/root boundary.
- `UI/Myra/MyraGameplayView.cs` — gameplay HUD including train/station information.
- `UI/Myra/MyraDepotView.cs` — Depot train builder.

## Persistence

- `Game/Save/RuntimeSaveService.cs` — runtime train/map save-load boundary.
- Passenger runtime state and individual coupling connections are not currently persisted.

## AI workflow

For passenger/station changes, inspect the full chain:

`StationController → PassengerManager → PassengerService/DemandProvider → Wagon/TrainRoute → Gameplay HUD`.

For movement/coupling changes, inspect `TrainManager.Coupling.cs`, `CouplingService`, `TrainComposition`, `TrainGeometry`, `TrainMovement`, `VehicleCouplingState` and affected safety controllers together.
