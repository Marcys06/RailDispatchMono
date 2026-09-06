# Current state — 0.2.2

## Locomotive timetable authoring

The locomotive-owned `LocomotiveSchedule` is now editable in-game through `MyraLocomotiveScheduleView`.

The editor is opened with **F9** after selecting a train. It works on a draft copy and commits only after `LocomotiveSchedule.IsValid()` succeeds.

Supported operations:

- add timetable point;
- remove timetable point;
- reorder points;
- cycle station selection;
- change arrival time;
- change departure time;
- enable/disable timetable;
- save or cancel.

Wagon timetables remain separate and are not modified by this editor.

## Timetable runtime

`LocomotiveScheduleRuntime` now keeps:

- scheduled arrival/departure;
- actual arrival;
- actual departure;
- observed travel duration;
- current delay;
- propagated positive delay;
- expected arrival/departure for future points.

Early arrival remains compatible with the rule that the train waits until the required departure time. Positive delay is propagated to subsequent timetable points.

## Station execution

`StationController` remains the integration point between locomotive timetable runtime and station dwell. The existing station braking/stop logic is used for the next timetable station. Timetable timing does not take switch or signal control away from the player.

## Dispatcher / blocks

`RailwayDispatcher` now maintains an explicit first-come-first-served request queue.

The dispatcher refuses entry when the next block is:

- occupied by another train;
- reserved by another train;
- still inside the block release cooldown;
- behind an earlier FCFS request.

The existing `BlockController` and `Block` models remain authoritative; no parallel reservation model was introduced.

## Diagnostics

**F10** opens `MyraRailwayDiagnosticsView`.

It shows:

- all block states: `FREE`, `RESERVED`, `OCCUPIED`;
- reservation/occupying train;
- block cooldown;
- dispatcher FCFS queue;
- every signal aspect and associated block state;
- timetable runtime state for every train;
- dispatcher state for every train.

## HUD timetable information

The existing gameplay HUD continues to expose the selected train's timetable state. The extended runtime state is additionally available in the full F10 diagnostics panel.

## Player responsibility

The system remains deliberately non-autonomous:

- no automatic switch operation;
- no automatic signal-aspect setting by dispatcher;
- no automatic coupling/decoupling;
- RadioStop remains a hard stop;
- F6/manual shunting remains available;
- the player resolves infrastructure situations the timetable cannot solve.

## Compatibility

0.2.2 does not target backward compatibility with older saves.

## Verification

The implementation was updated directly in the repository. A Windows build and live gameplay verification are still required; this environment cannot honestly claim a successful local Windows build or visual runtime test.
