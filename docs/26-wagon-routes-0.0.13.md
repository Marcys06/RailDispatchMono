# Wagon routes — 0.0.13

## Purpose

`TrainRoute` describes the service route of one wagon. It is deliberately separate from `Train`: two wagons in the same consist may have different routes, which is required for future coupling/decoupling.

## Model

A route contains:

- `Version` — JSON schema/version marker.
- `StationIds` — ordered station GUIDs.
- `CurrentStopIndex` — current point in the route.

The route can be empty. An empty route means that the wagon has not yet been configured; current test consists retain their legacy passenger-boarding behavior in that case.

## Passenger behavior

Once a route is configured, a passenger boards a passenger wagon only when the passenger's destination station is included in that wagon's route. Each wagon performs this decision independently.

When passengers alight, the wagon advances its current route point when the station is present in the route.

The route does not decide when the train stops. Physical train movement remains governed by semaphores and switch settings. This separation is intentional and leaves room for future timetable/dispatch events.

## UI

Hovering over a wagon shows its route summary. LPM opens the wagon route editor. The editor is screen-space UI generated with the existing `SpriteBatch`/`SpriteFont` approach and allows adding, removing and clearing stations.

## JSON preparation

`TrainRoute.ToJson()` and `TrainRoute.FromJson()` provide a stable starting point for future save files and timetable files. `0.0.13` does not yet write routes automatically to disk.
