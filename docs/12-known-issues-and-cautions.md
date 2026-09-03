# Known issues and cautions

## Current baseline: `0.1.4pre`

`0.1.4pre` is the current consolidated 0.1.4 documentation and gameplay milestone. Lettered `0.1.4a`–`0.1.4i` entries remain historical development stages.

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

## Train and rolling stock

- `TrainManager` is the authoritative train lifecycle owner.
- `TrainComposition` is the authoritative ordered vehicle collection for a train.
- Locomotive acceleration/braking use total consist mass with the non-linear exponent `1.30`.
- Locomotive power can reduce Vmax for heavy consists; wagon Vmax remains an additional cap.
- Signal stopping and RadioStop must use effective consist braking, not a raw locomotive value.
- Train diagnostics beginning with `[TRAIN]` are normalized to `[TRAIN:<first-8-guid-chars>]` while a train update is active.

## Coupling boundary

Static coupling metadata exists on vehicles, but runtime coupling is not implemented in `0.1.4pre`.

Not yet implemented:

- coupled/uncoupled runtime connection state;
- coupling distance detection;
- coupling/decoupling commands;
- consist merge/split as a coupling action;
- coupler compatibility checks;
- coupling forces, slack or longitudinal dynamics;
- persistence of individual coupler connections.

These belong to the planned `0.1.5` implementation line.

## Startup and pause

- Main Menu is the application entry point.
- New Game creates a new empty game state immediately.
- Pause is a state owned by `GameplayScreen`, not a popup screen.
- `MyraPauseView` is the pause presentation and dispatch surface.
- Resume, Save, Load and Quit use the gameplay-owned action path.

## Remaining UI migration

Junction/signal radial interaction UI, legacy floating/tooltips where still used, dedicated train/station detail windows and wagon-route detail/editor UI remain separate from the consolidated Myra surfaces. Do not infer migration from the presence of the Myra gameplay HUD alone.

## Diagnostics

Duplicate log lines do not by themselves prove duplicated simulation updates. Inspect logger subscriptions/call sites and screen/update traversal before changing game-loop logic. Train movement diagnostics now carry a short train GUID prefix for correlation.

## Rule for future agents

When a reported bug contradicts this document, inspect current source and call sites first. Historical lettered stages are immutable; corrections belong to the current development line. Update the maintained documentation and changelog whenever an architectural or behavioral contract changes.
