# AI agent rules

This file is the mandatory starting point for an AI coding agent working on RailDispatchMono. These rules describe the current repository contract at `0.1.0`.

## 1. Never invent architecture

The source code is authoritative. Do not infer a subsystem contract from a directory name, class name, old comment, changelog entry, or generic MonoGame template.

## 2. Core is shared

`RailDispatchMono.Core` is shared by platform hosts. Avoid platform-specific APIs in Core unless they are already abstracted or explicitly required.

## 3. Do not bypass `ScreenManager`

Screens are registered, updated, drawn and removed through `ScreenManager`. Do not create a parallel global screen stack or dispatch input independently.

## 4. Respect screen lifecycle

Understand `GameScreen`, `TransitionOn`, `Active`, `TransitionOff`, `Hidden`, popup behavior and input routing before changing visibility behavior. Pause is an overlay, not a second game loop.

## 5. Input has one shared source

Use the existing `InputState`/input routing architecture for shared input semantics. Do not compare raw physical mouse coordinates with logical game coordinates without applying the established presentation transformation.

## 6. Preserve state ownership

Authoritative game/domain state belongs to the relevant game subsystem, not to a screen merely because the screen displays it.

## 7. Preserve settings notifications

`RailDispatchMonoSettings` uses `INotifyPropertyChanged`. A setting setter must not silently bypass the existing notification contract.

## 8. Do not duplicate content infrastructure

Reuse existing Content loading and asset paths. Search current load sites before adding or renaming assets.

## 9. Search before changing APIs

Before changing a constructor, method, property, event or type used outside its file, search the repository for all references. Existing public/internal APIs are presumed intentional.

**Do not change existing APIs unless the task explicitly requires it and all usages have been reviewed.**

## 10. Platform changes require cross-platform review

A change to Core can affect desktop and Android hosts. A platform-host change must not leak platform assumptions into shared Core code.

## 11. Do not delete existing files for cleanup

Documentation and implementation cleanup must not delete unrelated repository files. Preserve historical changelog files.

## 12. Prefer small changes

Do not combine unrelated refactors with a feature implementation. At `0.1.0`, the feature scope is frozen; prefer targeted bug fixes over new architecture.

## 13. Validate assumptions against code

If documentation says class A owns behavior B, verify the implementation and call sites. If behavior is not verified, do not present it as fact.

## 14. Update documentation with architecture changes

If a code change modifies lifecycle, ownership, dependencies, input, settings, content, platform behavior, persistence or screen orchestration, update the corresponding document under `docs/`.

## 15. Save-system contract

The current save system uses **separate JSON files inside each save directory**. Do not collapse it into a single JSON document unless a future task explicitly changes the format.

The save system is versioned. `metadata.json` identifies the save. Runtime state is distributed among the established save files, including map/infrastructure, trains/vehicles, schedules/routes, passengers and simulation time where supported by the current implementation.

Auto-save is intentionally disabled at `0.1.0`.

Invalid or incomplete saves must be surfaced to the user as a notification. Do not silently load partial state.

## 16. Startup contract

The application enters through the Main Menu. The current menu flow includes New Game, Load Game, Settings, About and Quit.

New Game intentionally creates a new empty game state immediately without an additional confirmation step.

## 17. Current gameplay contract

- `GameDay` and `GameTime` represent simulation time.
- x1/x2/x5 are simulation-speed controls.
- Pause stops simulation progression and is toggled with `ESC`.
- Depots are world objects and the entry point for train creation.
- A depot can be used to create multiple trains through the existing depot workflow.
- Wagon routes describe passenger-service destinations; they do not directly control locomotive movement.
- Passenger exchange is wagon-specific and may emit floating `+X` / `-X` notifications.

## 18. Current release contract

`0.1.0` is a stabilization release. Do not introduce a new feature milestone under the `0.1.0` label. New features should be planned for the next version and implemented separately unless the task explicitly states otherwise.

## 19. Comments are not necessarily current

The source contains migration notes and comments written during implementation changes. Always trust the actual signature and executable code over a stale comment.

## 20. Documentation version discipline

Current-state documents must describe `0.1.0`. Historical documents and changelog entries retain their original version identifiers. Do not rewrite historical release notes to pretend that older versions contained features added later.
