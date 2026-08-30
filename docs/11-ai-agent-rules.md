# AI agent rules

This file is intentionally explicit. It is the first document an AI coding agent should read before modifying RailDispatchMono.

## 1. Never invent architecture

The source code is authoritative. Do not infer a subsystem contract from a directory name, class name, old comment, or generic MonoGame template.

## 2. Core is shared

`RailDispatchMono.Core` is shared by platform hosts. Avoid platform-specific APIs in Core unless they are already abstracted or the project explicitly requires them.

## 3. Do not bypass `ScreenManager`

Screens are registered, updated, drawn and removed through `ScreenManager`. Do not create a parallel global screen stack or dispatch input independently.

## 4. Respect screen lifecycle

`GameScreen.Update` runs even for screens that are not currently receiving input. `HandleInput` is the input-specific hook and is selected by `ScreenManager`.

Understand `TransitionOn`, `Active`, `TransitionOff` and `Hidden` before changing visibility behavior.

## 5. Input has one shared source

Use `InputState`. It tracks current/previous keyboard and gamepad state, mouse state, touch state, gestures and logical cursor coordinates.

Do not compare raw mouse coordinates against logical game coordinates without applying the established transformation.

## 6. Preserve state ownership

Game/domain state belongs to the relevant game subsystem, not to a screen merely because the screen displays it.

## 7. Preserve settings notifications

`RailDispatchMonoSettings` uses `INotifyPropertyChanged`. A setting setter should not silently bypass this notification contract.

## 8. Do not duplicate content infrastructure

Reuse existing Content loading and asset paths. Search for current load sites before adding or renaming assets.

## 9. Search before changing APIs

Before changing a constructor, method, property or type used outside its file, search the repository for all references.

## 10. Platform changes require cross-platform review

A change to Core can affect Android and desktop targets. A change to a platform host should not leak assumptions into shared Core code.

## 11. Do not delete existing files for cleanup

Documentation work in `docs/` is additive. Existing repository files must not be deleted merely to simplify documentation.

## 12. Prefer small changes

Do not combine an unrelated refactor with a feature implementation. Preserve working behavior unless the task explicitly requests a behavior change.

## 13. Validate assumptions against code

If documentation says that class A owns behavior B, there should be an implementation or call-site basis for that statement. If not verified, document the uncertainty instead of inventing details.

## 14. Update documentation with architecture changes

If a code change modifies lifecycle, ownership, dependencies, input, settings, content, platform behavior or screen orchestration, update the corresponding document under `docs/`.

## 15. Current game-loop contract

The current `RailDispatchMonoGame` configures fixed 60 FPS timing, creates `ScreenManager`, creates the initial `GameplayScreen`, and delegates Update/Draw to the manager.

Do not add a second top-level game loop.

## 16. Current screen-manager contract

`ScreenManager` owns the screen collection, input state, shared sprite resources, presentation scaling and touch gesture configuration. Its update traversal starts at the topmost screen and routes input only to the first eligible screen.

## 17. Comments are not necessarily current

The source contains migration notes and comments written during implementation changes. Always trust the actual signature and executable code over a stale comment.
