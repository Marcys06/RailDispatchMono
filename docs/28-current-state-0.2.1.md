# Current state — 0.2.1

## HUD architecture

`MyraGameplayView` is the single gameplay HUD surface. It is mounted by `RailDispatchMonoGame` into the existing shared `MyraUIManager.Desktop`.

The previous independent fixed-width information groups were replaced by one dashboard hierarchy:

- top bar: game time, day, simulation speed and operating mode;
- operations rail: infrastructure tools, wagon-route entry point and simulation controls;
- operational centre: selected train, locomotive timetable and diagnostics;
- traffic rail: trains, stations and dispatcher status;
- bottom bar: wagon occupancy/delay summary and the common keyboard operating contract.

The centre column is flexible while side rails have stable widths. Long labels wrap and lists scroll instead of forcing neighbouring controls to resize or overlap.

## Operational information

Train entries explicitly identify automatic timetable operation versus manual operation. Selecting a train focuses the camera and shows:

- current speed;
- consist Vmax;
- effective signal speed target;
- direction;
- dispatcher/block status;
- locomotive timetable point and delay;
- planned departure;
- RadioStop state in diagnostics.

Station entries remain compact and can expand into destination passenger groups. Wagon entries show occupancy, timetable delay and route information through the existing tooltip mechanism.

## Player responsibility

The HUD is intentionally informative rather than autonomous. It does not operate switches, change signal aspects, couple or decouple vehicles, clear RadioStop, select passenger transfers or solve infrastructure conflicts.

F6, F7, C, X and S retain their established gameplay meaning. The player remains responsible for dispatching decisions and infrastructure configuration.

## Rendering boundary

The Myra HUD does not replace world rendering. `TrackRenderer`, `TrainRenderer`, `StationRenderer`, `SignalRenderer` and other world renderers continue to own their respective visual domains.

## Compatibility

The old HUD layout is not a compatibility target. No persistence schema change is required for the HUD itself.

## Verification

The new HUD should be verified with a Windows build and live gameplay at the repository's normal 1600x900 window size and at resized window dimensions. In particular, verify long station names, multiple trains/wagons, expanded passenger summaries and the diagnostic panel.
