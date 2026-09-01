# Known issues and cautions

## Current baseline: `0.1.3pre`

`0.1.3pre` is the current consolidated Myra gameplay UI milestone.

## Myra integration

- Myra `1.6.5` is referenced by Core.
- `MyraUIManager` owns the shared Myra `Desktop` and active root.
- Main Menu, Settings, About, Pause and the gameplay HUD are Myra surfaces.
- The gameplay HUD's train/station information has one Myra implementation; duplicate legacy presentation was removed.
- Remaining world-specific UI is not automatically considered migrated merely because the HUD is Myra-based.

## Persistence

- Save data uses separate JSON files inside each save directory.
- `metadata.json` identifies the save and stores metadata.
- Save schema is versioned through `schemaVersion`.
- Auto-save is intentionally disabled.
- Invalid/incomplete save data must produce a user-facing notification rather than silent partial loading.

## Startup and pause

- Main Menu is the application entry point.
- New Game creates a new empty game state immediately.
- Pause is a state owned by `GameplayScreen`, not a popup screen.
- `MyraPauseView` is the pause presentation and dispatch surface.
- Resume, Save, Load and Quit must use the gameplay-owned action path.

## Remaining UI migration

The current known migration scope includes junction/signal radial interaction UI, legacy floating/tooltips where still used, dedicated train/station detail windows, wagon-route detail/editor UI and depot-specific interaction UI where applicable.

## Diagnostics

Duplicate log lines do not by themselves prove duplicated simulation updates. Inspect logger subscriptions/call sites and screen/update traversal before changing game-loop logic.

## Rule for future agents

When a reported bug contradicts this document, inspect current source and call sites first. Historical lettered stages are immutable; corrections belong to the current development line.
