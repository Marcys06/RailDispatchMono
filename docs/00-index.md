# RailDispatchMono documentation

This directory is the canonical project documentation for humans and AI coding agents.

## Documentation map

- `01-project-overview.md` — project purpose and technology.
- `02-repository-structure.md` — repository and source tree orientation.
- `03-architecture.md` — architectural responsibilities.
- `04-runtime-lifecycle.md` — MonoGame startup/update/draw flow.
- `05-screen-system.md` — screens, pause and input routing.
- `06-input.md` — current keyboard/mouse controls and coordinate handling.
- `07-game-domain.md` — railway, train, station and passenger domain.
- `08-settings-localization.md` — persistent settings, display and localization.
- `09-content-platforms.md` — Content pipeline and supported desktop projects.
- `10-development-workflows.md` — development and build workflow.
- `11-ai-agent-rules.md` — rules for AI-assisted repository changes.
- `12-known-issues-and-cautions.md` — current limitations and traps.
- `13-code-index.md` — source-file index.
- `14-documentation-maintenance.md` — documentation synchronization rules.
- `19-current-state-0.0.12.md` — authoritative current implementation snapshot before `0.0.13`.

## Source-of-truth policy

Code is authoritative. If an older document contradicts the implementation, update the document rather than relying on the obsolete description.

## Current baseline

- Current feature line: `0.0.12a`.
- The next planned major feature line is `0.0.13` — route creation.
- Shared implementation is in `RailDispatchMono/RailDispatchMono.Core`.
- Desktop bootstrap uses a `1600x900` default window and permits user resizing.
- The game uses a fixed 60 FPS update loop while simulation time can run at `x1`, `x2` or `x5`.
- Stations support rectangular areas and passenger exchange.
- Passengers are quasi-individual entities with origin/destination and train/station state.
- Wagons manage passenger capacity independently.
- Depots are world buildings and are the planned origin point for future route creation.
- UI graphics are generated programmatically; external UI image assets are not required.
