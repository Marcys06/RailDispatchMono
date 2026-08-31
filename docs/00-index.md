# RailDispatchMono documentation

This is the canonical documentation tree for the project. Code is authoritative; documents describe the current implementation and must be updated when behavior changes.

## Documentation map

- `01-project-overview.md` — project purpose and technology.
- `02-repository-structure.md` — repository and source tree orientation.
- `03-architecture.md` — architectural responsibilities.
- `04-runtime-lifecycle.md` — MonoGame startup/update/draw flow.
- `05-screen-system.md` — screens, pause and input routing.
- `06-input.md` — current keyboard/mouse controls.
- `07-game-domain.md` — railway, train, station, passenger and depot domain.
- `08-settings-localization.md` — persistent settings, display and localization.
- `09-content-platforms.md` — Content pipeline and supported hosts.
- `10-development-workflows.md` — development and build workflow.
- `11-ai-agent-rules.md` — AI-assisted repository rules.
- `12-known-issues-and-cautions.md` — current limitations and verified cautions.
- `13-code-index.md` — source-file index.
- `14-documentation-maintenance.md` — synchronization rules.
- `15-ai-context.md` — compact current architecture/context packet for AI agents.
- `19-audit-notes.md` — implementation audit notes.
- `20-debug-logging.md` — debug categories and output throttling.
- `23-simulation-time-and-0.0.13pre.md` — historical simulation-time notes.
- `24-passengers-0.0.10.md` — passenger model.
- `25-stations-0.0.10.md` — station model.
- `26-wagon-routes-0.0.13.md` — wagon route model and UI.
- `27-collision-system-0.0.14.md` — train collision protection.
- `28-current-state-0.0.14c.md` — historical implementation snapshot.
- `changelog/` — detailed versioned release notes.

## Current baseline: `0.1.0`

`0.1.0` is the first baseline release after the `0.0.16` feature series. The planned feature scope for `0.0.16` is complete; `0.1.0` is a stabilization/bugfix release rather than a new feature milestone.

### Save/load

- Save data is stored in separate save directories.
- Each save contains separate JSON files rather than one monolithic JSON document.
- `metadata.json` identifies the save; the save name is based on the creation date/time in the established timestamp format.
- Save data is versioned with `schemaVersion`.
- Runtime persistence covers the implemented map/infrastructure, trains and consists, vehicle positions, wagon routes/schedules, passenger state and simulation time (`GameDay`/`GameTime`) according to the current save implementation.
- Auto-save is intentionally disabled.
- Invalid or incomplete saves are reported to the user instead of being silently accepted.

### Startup and screens

- The application starts through the Main Menu rather than entering gameplay immediately.
- Main Menu provides New Game, Load Game, Settings, About and Quit.
- New Game immediately creates a new empty game state without an additional confirmation step.
- Load Game selects from available save directories.
- Pause is an overlay managed through `ScreenManager`; `ESC` toggles pause.

### Gameplay baseline

- Game time tracks `GameDay` and `GameTime`; x1/x2/x5 remain simulation speed multipliers.
- Pause stops simulation progression.
- Stations support passenger exchange.
- Wagons manage passenger capacity and per-wagon routes.
- Depots are world buildings and are the entry point for train creation; multiple trains may be created from a depot.
- Locomotive movement remains controlled by the railway/signalling system; wagon routes do not directly control locomotive movement.
- Signal protection takes priority over simple collision protection; collision protection uses the implemented safety-distance rule.
- Passenger exchange can display floating `+X` / `-X` notifications above/below the affected wagon.

## Release status

- `0.0.13` — wagon routes.
- `0.0.14` — collision protection.
- `0.0.15` series — persistence foundation, map save/load and pause stabilization.
- `0.0.16` series — versioned save slots, Main Menu, runtime persistence, train/depot workflow and passenger-exchange UI.
- `0.1.0` — current stabilization baseline; no planned feature expansion in this release.

## Documentation structure rule

There is one maintained documentation tree: `docs/`. Detailed release notes live in `docs/changelog/`. Historical documents may retain their original version identifiers; current-state documents must describe `0.1.0` unless explicitly marked historical.
