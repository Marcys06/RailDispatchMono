# Current State — 0.1.1

`0.1.1` is a documentation restructuring release. The gameplay baseline remains `0.1.0`; work after that baseline is primarily bug fixing, stabilization and polish.

## Current gameplay baseline

- Main Menu: New Game, Load Game, Settings, About and Quit.
- Gameplay and pause screens managed through `ScreenManager`.
- `ESC` toggles pause.
- Grid-based map editing and railway construction.
- Tracks, junctions, signals, blocks, stations and depots.
- Depot workflow for creating multiple trains.
- Train movement with acceleration/braking, signal look-ahead and basic collision protection.
- Wagon-level passenger service and wagon routes.
- Simulation clock with `GameDay`, `GameTime` and speed controls.
- Runtime persistence using separate JSON files inside each save directory.
- `metadata.json` and schema versioning.
- Invalid/incomplete saves are rejected with a user-facing notification.
- Auto-save is not enabled.

## Save model

A save is a directory, not one monolithic JSON document. Runtime areas remain separated into their respective JSON files. `metadata.json` identifies the save and stores metadata/schema information. The save name is derived from the project-defined creation timestamp format.

## Main Menu

`NOWA GRA` immediately creates a new empty map. `WCZYTAJ GRĘ` selects and loads an existing save. Broken or incomplete saves produce a notification rather than partially restoring state.

## Development constraints

- Inspect actual types and call sites before changing code.
- `Train` is a namespace in parts of the codebase; do not assume it is a class.
- Do not invent properties such as `Train.Id` or `Train.Composition`.
- Do not change existing APIs merely to make a new subsystem compile.
- Keep persistence separated from presentation code.
- Respect `ScreenManager` ownership of screen lifecycle and input routing.

## Documentation policy

This file is the authoritative current-state snapshot. Historical bug notes and old current-state snapshots were removed from the active documentation set because they became stale. Release history remains in `docs/changelog/` and `CHANGELOG.md`.

Known current limitations belong in `12-known-issues-and-cautions.md`; they should not be represented as historical release facts.
