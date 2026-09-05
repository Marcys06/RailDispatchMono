# Current State — 0.1.7d

## Gameplay HUD hover details

The right side of the gameplay HUD contains operational lists for trains, wagons and stations.

### Stations

Each station entry remains a clickable button. Hovering the entry displays a tooltip containing the waiting passengers grouped by destination station:

- destination station name;
- number of passengers waiting for that destination.

When no passengers are waiting, the tooltip explicitly reports that there are no waiting passengers.

The data comes from `PassengerManager.GetWaitingAt(station)`, so only passengers currently waiting at that station are included.

### Wagons

Each wagon entry remains a clickable button and continues to display delay and occupancy. Hovering the entry displays the ordered station stops from the wagon timetable.

When a timetable exists, the tooltip uses `Wagon.Schedule.Points`, including the return leg of the loop. For wagons without a timetable, the configured `Wagon.ServiceRoute` is used as a fallback.

Station identifiers are resolved to station names through `StationController`.

## Runtime version

The runtime save metadata version is `0.1.7d`. Save schema remains `2`; no persistence schema change was required for this UI-only feature.

## Scope

This milestone adds informational hover tooltips only. Clicking station and wagon entries retains the existing focus behavior.
