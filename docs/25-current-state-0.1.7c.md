# Current State — 0.1.7c

## Gameplay HUD

The right side of the gameplay HUD now contains three operational lists:

- `POCIĄGI` — trains with current speed;
- `WAGONY` — individual wagons with timetable delay and passenger occupancy;
- `STACJE` — stations with waiting passenger count.

Wagon entries use the same clickable button presentation as train entries. Clicking a wagon focuses its containing train.

## Wagon status

For wagons with an enabled/assigned timetable, the HUD displays the current `WagonScheduleRuntime.DelaySeconds` value. Delay is formatted as seconds or minutes/seconds and preserves the sign for early/late arrivals.

Passenger occupancy is calculated from `Wagon.PassengerCount` and `Wagon.PassengerCapacity`, displayed as current passengers, capacity and percentage.

Wagons without a timetable display no delay value (`—`).

## Version

The runtime save metadata version is `0.1.7c`. Save schema remains `2`.
