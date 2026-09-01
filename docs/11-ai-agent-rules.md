# AI agent rules

This file is the mandatory starting point for an AI coding agent working on RailDispatchMono. These rules describe the current repository contract at `0.1.2a`.

## 1. Never invent architecture

The source code is authoritative. Do not infer a subsystem contract from a directory name, class name, old comment, changelog entry, or generic MonoGame template.

## 2. Core is shared

`RailDispatchMono.Core` is shared by platform hosts. Avoid platform-specific APIs in Core unless they are already abstracted or explicitly required.

## 3. Do not bypass `ScreenManager`

Screens are registered, updated, drawn and removed through `ScreenManager`. Do not create a parallel global screen stack or dispatch input independently. Myra does not replace `ScreenManager`.

## 4. Respect screen lifecycle

Understand `GameScreen`, `TransitionOn`, `Active`, `TransitionOff`, `Hidden`, popup behavior and input routing before changing visibility behavior. Pause is an overlay, not a second game loop.

## 5. Input has one shared source

Use the existing `InputState`/input routing architecture for shared input semantics. Do not compare raw physical mouse coordinates with logical game coordinates without applying the established presentation transformation. Myra controls must integrate with this model rather than inventing a second input coordinate system.

## 6. Preserve state ownership

Authoritative game/domain state belongs to the relevant game subsystem, not to a screen merely because the screen displays it.

## 7. Preserve settings notifications

`RailDispatchMonoSettings` uses `INotifyPropertyChanged`. A setting setter must not silently bypass the existing notification contract.

## 8. Do not duplicate content infrastructure

Reuse existing Content loading and asset paths. Search current load sites before adding or renaming assets. Do not copy Myra source or library assets into Core when the NuGet integration is sufficient.

## 9. Search before changing APIs

Before changing a constructor, method, property, event or type used outside its file, search the repository for all references. Existing public/internal APIs are presumed intentional.

**Do not change existing APIs unless the task explicitly requires it and all usages have been reviewed.**

## 10. Platform changes require cross-platform review

A change to Core can affect desktop and Android hosts. A platform-host change must not leak platform assumptions into shared Core code.

## 11. Do not delete existing files for cleanup

Documentation and implementation cleanup must not delete unrelated repository files. Preserve historical changelog files.

## 12. Prefer small changes

`0.1.2` is being implemented as lettered incremental stages. Do not rewrite an earlier stage after it has been committed. If a stage has a defect, document the failure and implement the correction in the next lettered stage.

## 13. Validate assumptions against code

If documentation says class A owns behavior B, verify the implementation and call sites. If behavior is not verified, do not present it as fact.

## 14. Update documentation with architecture changes

If a code change modifies lifecycle, ownership, dependencies, input, settings, content, platform behavior, persistence or screen orchestration, update the corresponding document under `docs/`.

## 15. Save-system contract

The current save system uses separate JSON files inside each save directory. Do not collapse it into a single JSON document unless a future task explicitly changes the format.

## 16. Startup contract

The application enters through the Main Menu. New Game intentionally creates a new empty game state immediately without an additional confirmation step.

## 17. Current gameplay contract

- `GameDay` and `GameTime` represent simulation time.
- x1/x2/x5 are simulation-speed controls.
- Pause stops simulation progression and is toggled with `ESC`.
- Depots are world objects and the entry point for train creation.
- Wagon routes describe passenger-service destinations; they do not directly control locomotive movement.
- Passenger exchange is wagon-specific and may emit floating `+X` / `-X` notifications.

## 18. Myra integration contract

- Myra is consumed through the standard `Myra` NuGet package.
- `MyraUIManager` is the single Core integration boundary for `MyraEnvironment.Game` and the shared `Desktop`.
- `ScreenManager` remains the screen/lifecycle owner.
- At `0.1.2a`, existing screens are not migrated; later stages may migrate them one at a time.
- Do not initialize separate Myra desktops from unrelated screens without an explicit architectural decision.

## 19. Release contract

`0.1.2x` stages are incremental commits. A failed stage is not amended or rewritten; its correction belongs to the next lettered stage.

## 20. Documentation version discipline

Current-state documents must describe the latest committed `0.1.2x` stage. Historical documents and changelog entries retain their original version identifiers.
