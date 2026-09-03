# Changelog

This file is the high-level release history. Detailed release notes are kept in `docs/changelog/`.

## [0.1.5x] — Train travel direction state and non-destructive F7 reversal
**Data:** 2026-09-03

- Rebuilt `F7` as a train travel-direction state change instead of a composition mutation.
- `Train.Direction` is now the authoritative current travel direction, with explicit `Train.IsReversed` runtime state.
- F7 never reorders `Composition.Vehicles` and never moves vehicle world coordinates at the reversal moment.
- Vehicle rendering now uses the same authoritative transform calculation as the train simulation.
- Locomotive `VehicleOrientation` is no longer used as the F7 travel-direction switch.
- Runtime saves persist the reversed travel state.
- Detailed notes: `docs/changelog/0.1.5x.md`.

## [0.1.5i] — Fixed shunting control and cursor-targeted decoupling
**Data:** 2026-09-03

- Replaced configurable `3 / 4 / 5 km/h` coupling command speeds with a fixed `6 km/h` shunting limit.
- Removed the `F7` and `F8` coupling-speed commands.
- `X` decoupling is now allowed only while the target train is below `6 km/h`; the limit is enforced by `CouplingService`.
- `F6` is now manual shunting control: while held over a train, it accelerates toward `3 km/h` and bypasses the automatic RadioStop/collision stop path.
- `X` now targets the wagon under the cursor instead of the last `C` coupling or first runtime connection.
- When the hovered wagon has two connections, the rear connection is preferred; otherwise its available runtime connection is used.
- `X` no longer falls back to an unrelated/oldest coupling when no wagon is under the cursor.
- Detailed notes: `docs/changelog/0.1.5i.md`.

## [0.1.5h] — Preserve detached consist positions after decoupling
**Data:** 2026-09-03

- Fixed detached vehicles being rendered/spawned at the same position after `X` decoupling.
- A newly detached `Train` now preserves the physical vehicle positions implied by the consist's position and direction instead of collapsing all vehicles onto the new train head position.
- The existing runtime coupling/split behavior is unchanged: the detached section remains an ordered composition with its internal runtime connections intact.
- Fixed-position handling also resets movement/trajectory state consistently when a train position is initialized or changed.
- Detailed notes: `docs/changelog/0.1.5h.md`.

## [0.1.5g] — Automatic runtime couplings for complete consists
**Data:** 2026-09-03

- `TrainComposition.AddVehicle()` now establishes runtime couplings between adjacent vehicles when both physical ends expose the same supported coupler type.
- Mixed passenger-wagon formations such as `1KL <-> 2KL <-> 3KL` are therefore treated as one physically connected consist instead of an unconnected list of vehicles.
- `X` decoupling now works for those pre-built multi-vehicle consists without requiring a preceding `C` command.
- `TrainComposition.Split()` preserves the existing runtime connections inside the detached section; the split boundary connection is still cleared by `CouplingService`.
- Existing explicit `C` coupling remains authoritative for connecting separate trains and does not create duplicate runtime connections when merged vehicles are appended.
- No change was made to the coupler compatibility rule: physical coupling remains based on `CouplingSpecification`, not wagon display/class names.
- Detailed notes: `docs/changelog/0.1.5g.md`.

## [0.1.5f] — Remove dedicated Core test project
**Data:** 2026-09-03

- Removed `RailDispatchMono.Core.Tests` from the repository and solution.
- Removed the dedicated test project files and solution entry.
- Kept the runtime coupling/decoupling implementation unchanged.
- Added the consolidated `0.1.5pre` current-state documentation.
- Updated maintained input, domain, workflow, AI, known-issues and code-index documentation.
- Detailed notes: `docs/changelog/0.1.5f.md`.

## [0.1.5e] — Coupling command controls
**Data:** 2026-09-03

- Added direct `Coupling` command on `C` using the nearest valid boundary candidate.
- Added separate `Decoupling` command on `X` targeting the last coupling created by the command layer, with deterministic fallback to the first remaining runtime connection.
- Added shunting speed profiles of `3 / 4 / 5 km/h` on `F6 / F7 / F8`; `5 km/h` is the default.
- Coupling command refuses to execute when either participating train exceeds the selected shunting speed.
- Associated the command semantics with signal aspect `S14 Rezerwowy 3`; its signal speed limit is now `5 km/h`.
- Actual coupling/decoupling remains delegated to the existing authoritative `CouplingService`.
- Detailed notes: `docs/changelog/0.1.5e.md`.

## [0.1.5d] — Coupling regression test foundation
**Data:** 2026-09-03

- Added `RailDispatchMono.Core.Tests` targeting .NET 9 and referencing the core project.
- Added regression tests for default screw-coupler configuration.
- Added regression tests for `CouplingConnection` matching in both directions and self-connection rejection.
- Added regression tests for typed coupling operation success/failure semantics.
- Added regression tests confirming exact vehicle order through `TrainComposition.Split()` and `Train` construction.
- Added the test project to `RailDispatchMono.slnx`.
- Runtime coupling mechanics were not executed in a live gameplay scenario as part of this milestone.
- Detailed notes: `docs/changelog/0.1.5d.md`.

## [0.1.5c] — Coupling candidate discovery
**Data:** 2026-09-03

- Added `CouplingCandidate` as a UI-neutral snapshot of a possible physical connection.
- Added `TrainManager.GetCouplingCandidates(Train)` as the authoritative discovery entry point for future UI/input code.
- Candidate data exposes both concrete vehicle ends, endpoint positions, measured distance and the authoritative `CouplingCheckResult`.
- Candidate discovery considers only outer vehicle ends, matching the current rigid coupling rule.
- Candidates are sorted by endpoint distance for deterministic selection.
