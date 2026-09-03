# Known issues and cautions

## Current baseline: `0.1.5pre`

`0.1.5pre` is the current consolidated 0.1.5 documentation and gameplay milestone. Lettered `0.1.5a`–`0.1.5f` entries remain historical development stages.

## Myra integration

- Myra `1.6.5` is referenced by Core.
- `MyraUIManager` owns the shared Myra `Desktop` and active root.
- Main Menu, Settings, About, Pause, gameplay HUD and Depot builder are Myra surfaces.
- The gameplay HUD's train/station information has one current Myra implementation; do not reintroduce duplicate legacy presentation.
- Remaining world-specific UI is not automatically considered migrated merely because the HUD is Myra-based.

## Persistence

- Save data uses separate JSON files inside each save directory.
- `metadata.json` identifies the save and stores metadata.
- Save schema is versioned through `schemaVersion`.
- Runtime save schema remains version `1`; `ShortName` loading is backward-compatible when the field is absent.
- Auto-save is intentionally disabled.
- Invalid/incomplete save data must produce a user-facing notification rather than silent partial loading.
- Individual runtime coupling connections are not persisted.

## Train and rolling stock

- `TrainManager` is the authoritative train lifecycle owner.
- `TrainComposition` is the authoritative ordered vehicle collection for a train.
- Locomotive acceleration/braking use total consist mass with the non-linear exponent `1.30`.
- Locomotive power can reduce Vmax for heavy consists; wagon Vmax remains an additional cap.
- Signal stopping and RadioStop must use effective consist braking, not a raw locomotive value.
- Train diagnostics beginning with `[TRAIN]` are normalized to `[TRAIN:<first-8-guid-chars>]` while a train update is active.

## Coupling and decoupling

Rigid runtime coupling and decoupling are implemented through `CouplingService`.

Current command contract:

- `C` couples the nearest valid outer-boundary candidate;
- `X` decouples the last coupling created by `C`, with fallback to the first remaining runtime connection;
- `F6` / `F7` / `F8` select `3` / `4` / `5 km/h` shunting limits, with `5 km/h` as default.

Coupling requires compatible free coupler ends, boundary positions, sufficient proximity and end alignment. Successful coupling and decoupling stop the affected consists through `RadioStop` before changing runtime composition.

The current command path is intentionally temporary. There is no vehicle/end selection UI yet, and no user-facing failure-reason panel. Dynamic coupling forces, slack, impact shock, animation/delay, brake-pipe propagation and individual connection persistence are also not implemented.

## Verification

There is no dedicated automated Core test project in the current repository. Runtime coupling/decoupling changes require solution build and live gameplay verification in the user's .NET/MonoGame environment.

## Startup and pause

- Main Menu is the application entry point.
- New Game creates a new empty game state immediately.
- Pause is a state owned by `GameplayScreen`, not a popup screen.
- `MyraPauseView` is the pause presentation and dispatch surface.
- Resume, Save, Load and Quit use the gameplay-owned action path.

## Remaining UI migration

Junction/signal radial interaction UI, legacy floating/tooltips where still used, dedicated train/station detail windows and wagon-route detail/editor UI remain separate from the consolidated Myra surfaces. Do not infer migration from the presence of the Myra gameplay HUD alone.

## Diagnostics

Duplicate log lines do not by themselves prove duplicated simulation updates. Inspect logger subscriptions/call sites and screen/update traversal before changing game-loop logic. Train movement diagnostics now carry a short train GUID prefix for correlation; coupling operations use `[COUPLING]` diagnostics.

## Rule for future agents

When a reported bug contradicts this document, inspect current source and call sites first. Historical lettered stages are immutable; corrections belong to the current development line. Update the maintained documentation and changelog whenever an architectural or behavioral contract changes.
