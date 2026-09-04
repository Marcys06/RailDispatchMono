# Current State — 0.1.7a

## Timetable ownership

`Wagon.Schedule` is the permanent schedule definition. `Train` remains an operational grouping and does not own the timetable.

## Route

The player defines a base sequence such as `A-B-C-D`. The timetable editor expands it to `A-B-C-D-C-B-A`.

## Control points

Each expanded point stores independent arrival and departure times. The arrival/departure interval is the planned dwell. Terminal manoeuvre time is therefore represented by a longer terminal dwell.

## Delay

When a train is serviced at a station, each wagon with a matching schedule records the timetable point, actual arrival time, game day and delay relative to the scheduled arrival.

## Repetition

The schedule definition represents one complete loop. Runtime cycle/point state is persisted so the service can continue after save/load. Full autonomous dispatching is intentionally not part of this milestone.

## Coupling and decoupling

Schedule ownership stays on individual wagons. Coupling/decoupling does not copy, merge or delete schedule state.

## UI

The existing `S` wagon menu is the entry point. It now edits both the base route and the full loop timetable. Time fields are edited by clicking the field and entering digits; `Enter` normalizes the value.

## Save

`RuntimeSaveService` uses schema `2` and game version `0.1.7a`. Each wagon save entry contains the schedule definition and schedule runtime state in addition to the existing rolling-stock and passenger data. Schema `1` remains loadable.

## Not yet implemented

- autonomous timetable-driven train movement;
- path finding based on schedule;
- automatic conflict resolution between scheduled services;
- coupling/decoupling as timed operational actions;
- timetable-aware passenger transfer planning;
- schedule editing with mouse-drag/grid widgets.
