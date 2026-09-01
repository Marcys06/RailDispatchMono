# AI agent rules

This file is the mandatory starting point for an AI coding agent working on RailDispatchMono. These rules describe the current repository contract at `0.1.2i`.

## 1. Never invent architecture

The source code is authoritative. Do not infer a subsystem contract from a directory name, class name, old comment, changelog entry, or generic MonoGame template.

## 2. Core is shared

`RailDispatchMono.Core` is shared by platform hosts. Avoid platform-specific APIs in Core unless they are already abstracted or explicitly required.

## 3. Do not bypass `ScreenManager`

Screens are registered, updated, drawn and removed through `ScreenManager`. Do not create a parallel global screen stack. Myra is a rendering/UI layer, not a replacement for `ScreenManager`.

## 4. Respect screen lifecycle

Understand `GameScreen`, `TransitionOn`, `Active`, `TransitionOff`, `Hidden`, popup behavior and input routing before changing visibility behavior.

## 5. Input ownership

Legacy screens continue to use `InputState`. A migrated Myra surface may use the shared Myra `Desktop` input processing, but must not also process the same pointer/keyboard action through the legacy surface.

## 6. Preserve state ownership

Authoritative game/domain state belongs to the relevant game subsystem, not to a screen merely because the screen displays it.

## 7. Preserve settings notifications

`RailDispatchMonoSettings` uses `INotifyPropertyChanged`. A setting setter must not silently bypass the existing notification contract.

## 8. Do not duplicate content infrastructure

Reuse existing Content loading and asset paths. Do not copy Myra source or library assets into Core when the NuGet integration is sufficient.

## 9. Search before changing APIs

Before changing a constructor, method, property, event or type used outside its file, search the repository for all references. Existing public/internal APIs are presumed intentional. Do not change existing APIs unless explicitly required.

## 10. Platform changes require cross-platform review

A change to Core can affect desktop and Android hosts. Do not leak desktop-only assumptions into shared Core code.

## 11. Do not delete existing files for cleanup

Preserve historical changelog files and unrelated repository files.

## 12. Prefer small changes

`0.1.2` is implemented as lettered incremental stages. Do not rewrite an earlier stage after it has been committed. A defect found in one stage is corrected in the next lettered stage.

## 13. Validate assumptions against code

If documentation says class A owns behavior B, verify the implementation and call sites before relying on it.

## 14. Update documentation with architecture changes

Changes to lifecycle, ownership, dependencies, input, settings, content, platform behavior, persistence or screen orchestration require corresponding documentation updates under `docs/`.

## 15. Save-system contract

The current save system uses separate JSON files inside each save directory. Do not collapse it into a single JSON document unless explicitly requested.

## 16. Startup contract

The application enters through the Main Menu. New Game creates a new empty game state immediately without an additional confirmation step.

## 17. Current gameplay contract

- `GameDay` and `GameTime` represent simulation time.
- x1/x2/x5 are simulation-speed controls.
- Pause stops simulation progression and is toggled with `ESC`.
- Depots are world objects and the entry point for train creation.
- Wagon routes describe passenger-service destinations; they do not directly control locomotive movement.
- Passenger exchange may emit floating `+X` / `-X` notifications.

## 18. Myra integration contract

- Myra is consumed through the standard `Myra` NuGet package (`1.6.5`).
- `MyraUIManager` is the single Core integration boundary for `MyraEnvironment.Game` and the shared `Desktop`.
- The desktop bounds follow the current graphics viewport.
- `ScreenManager` remains the lifecycle owner.
- The shared desktop is rendered once by the game host after the ScreenManager stack; `Desktop.Render()` also performs Myra widget input processing.
- Main Menu, Settings, About and Pause are Myra presentation surfaces.
- `PauseScreen` is a `GameScreen`, not a legacy `MenuScreen`, and contains no `MenuEntry` instances.
- Save/Load are exposed only by `MyraPauseView`; Gameplay HUD must not provide a second visible Save/Load surface.
- A migrated screen must clear the shared desktop when it leaves ownership.
- Do not create separate Myra desktops from unrelated screens.
- Myra is not a blanket replacement for railway rendering, gameplay HUD or radial gameplay tools.

## 19. Release contract

`0.1.2x` stages are incremental commits. A failed stage is not amended or rewritten; its correction belongs to the next lettered stage.

## 20. Documentation version discipline

Current-state documents describe the latest committed `0.1.2x` stage. Historical documents and changelog entries retain their original version identifiers.
