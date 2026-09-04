# AI agent rules

This file is the mandatory starting point for an AI coding agent working on RailDispatchMono. The current repository contract is `0.1.6g`.

## 1. Never invent architecture

Source code and current call sites are authoritative. Do not infer behavior from stale comments or historical stages.

## 2. Core is shared

`RailDispatchMono.Core` is shared by platform hosts. Keep platform-specific APIs in hosts unless explicitly abstracted.

## 3. Do not bypass ScreenManager

Registered screens remain managed by `ScreenManager`. Myra is not a parallel screen stack.

## 4. Respect screen lifecycle

Understand `GameScreen`, transitions, popup behavior and input routing before changing visibility behavior.

## 5. Input ownership

A migrated Myra surface uses the shared Myra `Desktop`; it must not duplicate the same interaction through a legacy UI path. The current coupling command path is the temporary `TrainManager.HandleCouplingHotkeys()` implementation; do not duplicate it in another owner.

## 6. Preserve state ownership

Authoritative simulation/domain state belongs to the relevant game subsystem, not the UI.

## 7. Preserve settings notifications

`RailDispatchMonoSettings` uses `INotifyPropertyChanged`; preserve that contract.

## 8. Do not duplicate content infrastructure

Reuse existing Content loading and Myra NuGet integration.

## 9. Search before changing APIs

Search all callers before changing shared constructors, methods, properties, events or types.

## 10. Platform changes require cross-platform review

Core changes can affect desktop and Android hosts.

## 11. Preserve historical changelog

Do not rewrite historical release notes. Current-state snapshots exist only for `pre` milestones.

## 12. Prefer incremental changes

Historical lettered stages remain immutable. Corrections belong to a later stage or the current `pre` development line.

## 13. Validate assumptions against code

If documentation and code disagree, inspect implementation and call sites before changing either.

## 14. Update documentation with architecture changes

Update the relevant maintained documentation and changelog when behavior or architecture changes. Do not rewrite historical lettered stages.

## 15. Save-system contract

Save data uses separate JSON files inside save directories, with metadata and schema versioning. `MapSaveService` remains the persistence boundary.

## 16. Startup contract

The application enters through Main Menu. New Game creates a new empty game state without an additional confirmation step.

## 17. Current gameplay contract

- `GameDay` and `GameTime` represent simulation time.
- x1/x2/x5 control simulation speed.
- Pause is owned by `GameplayScreen` and toggled with `ESC` or the Myra pause action.
- Depots are world objects and the entry point for train creation.
- Wagon routes describe passenger-service destinations and do not directly control locomotive movement.
- Rigid coupling/decoupling is a domain operation owned by `CouplingService` and exposed through the current `TrainManager` command path.
- `TrainComposition` is the single authoritative ordered vehicle container; `CompositionOrder` is metadata only.
- `TrainComposition.EffectiveMaxSpeed` is the authoritative consist Vmax capability; signal limits are runtime restrictions layered on top.
- `SimulationScale` is the single conversion boundary between physical metres and map-grid cells.
- `RadioStop` must prevent normal automatic movement while active; F6 manual shunting is the explicit exception.

## 18. Myra contract

- Myra `1.6.5` is consumed through the shared Core project.
