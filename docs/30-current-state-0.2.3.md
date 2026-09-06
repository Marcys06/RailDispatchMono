# Current state — 0.2.3

## Operational HUD

`MyraGameplayView` is the primary operational dashboard. The train list now exposes:

- `AUTO` / `RĘCZ` mode;
- current locomotive timetable point `n/m`;
- timetable delay;
- speed;
- selected-train marker;
- dispatcher state.

The selected-train panel exposes current and next timetable points, ETA, required departure, delay and dispatcher state. It also provides explicit manual controls:

- `WYMUSZENIE [F6]` — dispatcher override for the selected train;
- `ZWOLNIJ TRASĘ` — releases dispatcher reservation/request through the dispatcher boundary;
- `AUTO / RĘCZ` — toggles locomotive timetable automation without deleting the timetable.

F6 does not clear physical block occupancy and does not change signal aspects or switches.

## Dispatcher diagnostics

F10 is the infrastructure diagnostic surface. It shows:

- FCFS pending requests;
- target block and request wait time;
- every block as `FREE`, `RESERVED` or `OCCUPIED` with owner information;
- entry/exit signal aspects associated with blocks;
- train timetable/runtime state.

FCFS arbitration is local to the requested block: a train waiting for block B does not delay a request for unrelated block C.

The diagnostic release action calls `RailwayDispatcher.NotifyReleased`; it does not directly mutate block occupancy.

## Dispatcher override

`RailwayDispatcher.ForceProceed` is the F6 manual override. It removes the train from the dispatcher queue and bypasses dispatcher arbitration until the dispatcher releases the train. It does not mutate `Block.IsOccupied`, signal aspects or switch state.

This preserves the game's player-first rule: the player can intervene, but the HUD does not become an autonomous infrastructure controller.

## Notifications

The gameplay HUD presents compact operational notifications for the selected train, including waiting for a route, route granted and timetable delay. Notifications are derived from domain state and do not own simulation state.

## Timetable runtime

Locomotive timetable runtime retains planned versus observed timing, actual travel duration and propagated delay. The timetable remains the plan; runtime state represents what is currently expected/observed.

## F9 editor

The locomotive timetable editor remains the authoritative UI for editing locomotive-owned cyclic schedules. Save validation rejects invalid times and invalid chronological ordering before replacing the locomotive schedule.

## Player responsibility

The system does not automatically operate switches or signal aspects. Coupling/decoupling remains manual. F6 is an explicit player intervention rather than an automatic recovery mechanism.

## Compatibility

Backward compatibility with previous UI layouts and saves is not a requirement for this development line.

## Verification

A Windows build and live gameplay verification remain required. The critical checks for 0.2.3 are F6 override behaviour, F10 dispatcher/block diagnostics, route release, AUTO/RĘCZ switching, timetable delay display and multiple trains competing for the same block.
