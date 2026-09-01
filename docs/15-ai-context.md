# AI context packet

## Current release

**RailDispatchMono `0.1.3pre`** is the current consolidated Myra gameplay UI pre-release after the immutable `0.1.3a`–`0.1.3e` development stages. `0.1.2pre` is the previous stabilization snapshot.

## One-paragraph context

RailDispatchMono is a C#/.NET 9 MonoGame project with shared Core code and platform hosts. `RailDispatchMonoGame` owns the game loop and delegates screen lifecycle/update/draw to `ScreenManager`. `MyraUIManager` is the single Myra integration boundary and owns one shared `Desktop` and active root. Main Menu, Settings, About, Pause and the gameplay HUD use Myra. The gameplay HUD contains the clock, GameDay, speed controls, collapsible build tools and train/station information. Train/station selection centers the camera. Pause is gameplay state owned by `GameplayScreen`; `MyraPauseView` is its presentation/action surface. Remaining world-specific radial/tooltip UI is not yet fully migrated.

## Myra contract

- Package: Myra 1.6.5.
- One shared `Desktop` owned by `MyraUIManager`.
- One active Myra root at a time.
- Migrated surfaces: Main Menu, Settings, About, Pause and gameplay HUD.
- No duplicate legacy train/station HUD.
- No duplicate legacy clock/speed HUD.
- `ScreenManager` remains lifecycle owner.
- Myra does not replace gameplay simulation or railway rendering.

## Pause lifecycle

Pause is a gameplay state, not a popup screen. `GameplayScreen` owns the pause state and activates `MyraPauseView`. While paused, simulation progression stops while Myra remains interactive. Resume clears the pause state and Myra root. Save/Load are gameplay-owned operations behind `MapSaveService`.

## Current gameplay UI

- `GameDay` and `GameTime` represent simulation time.
- x1/x2/x5 control simulation speed.
- Build tools are available in a collapsible Myra panel.
- Train/station lists are Myra-only and support camera focus.
- Station entries show waiting passenger counts.

## Remaining UI migration

- junction interaction/radial menu;
- signal interaction/radial menu;
- legacy floating/tooltips where still used;
- dedicated train/station detail windows;
- dedicated wagon-route detail/editor window;
- depot-specific interaction UI where applicable;
- richer configurable train/station state visualization.

## Hard constraints

- Do not create a parallel screen manager or Myra desktop.
- Do not duplicate a migrated UI surface.
- Do not reintroduce popup `PauseScreen` as the pause architecture.
- Preserve authoritative domain ownership.
- Search all usages before changing shared APIs.
- Keep platform-specific behavior in platform hosts.
- Treat source and current call sites as authoritative over stale documentation/comments.
- Only `0.1.2pre` and `0.1.3pre` have current-state snapshots; lettered stages belong in changelogs.
