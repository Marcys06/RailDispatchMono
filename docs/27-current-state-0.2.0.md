# Current state — 0.2.0

## Operational timetable

`LocomotiveSchedule` is an independent timetable owned by `Locomotive`. It does not replace or mutate the existing `WagonSchedule` model.

Each locomotive timetable point contains a station, expected arrival and required departure. The runtime records the actual arrival, calculates delay and keeps the required departure as an operational gate. A train arriving early remains stopped until the planned departure time. The schedule is cyclic.

## Station integration

`StationController` now coordinates the locomotive timetable before normal train movement. Existing passenger service, station detection, dwell and braking remain active. When a scheduled locomotive reaches its station, the existing station service still performs passenger handling and dwell.

## Dispatcher and blocks

`RailwayDispatcher` provides first-come-first-served arbitration for the next linked block. It refuses automatic progress into a block occupied or reserved by another train. Reservations are released after the train enters the reserved block.

The dispatcher deliberately does not set junctions or signal aspects. The player continues to operate infrastructure. Existing signals therefore remain the visible movement authority mechanism while the dispatcher prevents automatic trains from ignoring physical block conflicts.

## Manual control

Manual shunting and F6 remain outside timetable automation. F7 reversal and RadioStop retain their existing semantics. Coupling and decoupling remain manual operations.

## Passengers

Passengers remain owned by their concrete wagon while riding. Existing station boarding/alighting remains in `StationController`; no automatic transfer routing was added.

## Persistence

`TrainSchedule` version is `2`. It can contain an optional `LocomotiveSchedule` in addition to wagon schedule entries. `ScheduleStorage` writes document schema `2`. Old-save compatibility is not part of the 0.2.0 contract.

## Known boundary

The 0.2.0 dispatcher is intentionally conservative: it arbitrates existing linked blocks and does not calculate a new path through switches. A player must configure the infrastructure so that the physical route and signal aspects permit the intended movement. This preserves the game's player-driven dispatching role.
