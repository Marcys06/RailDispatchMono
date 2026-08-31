# RailDispatchMono documentation

This is the single canonical documentation tree for the project. Code is authoritative; documents describe the current implementation and must be updated when behavior changes.

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
- `12-known-issues-and-cautions.md` — current limitations.
- `13-code-index.md` — source-file index.
- `14-documentation-maintenance.md` — synchronization rules.
- `19-current-state-0.0.12.md` — previous implementation snapshot.
- `20-debug-logging.md` — debug categories and output throttling.
- `23-simulation-time-and-0.0.13pre.md` — current mini-update changes.
- `24-passengers-0.0.10.md` — passenger model.
- `25-stations-0.0.10.md` — station model.
- `changelog/` — one changelog directory containing versioned release notes.

## Current baseline

- Current line: `0.0.13pre`.
- `0.0.13` is planned for route creation.
- Shared implementation: `RailDispatchMono/RailDispatchMono.Core`.
- Game time starts at `00:00` and runs at 5× wall-clock time at x1. x2/x5 multiply that rate further.
- Pause stops both simulation and game clock.
- The 5× clock scale does not multiply train velocity, acceleration, braking or travelled distance.
- Stations support rectangular areas and passenger exchange.
- Wagons manage passenger capacity independently.
- Depots are world buildings and are the planned origin for future route creation.

## Documentation structure rule

There is no second documentation tree under `RailDispatchMono.Core`. All maintained documentation lives here. Historical changelogs and release notes live only in `docs/changelog/`.
