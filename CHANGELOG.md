# Changelog

## [0.2.5a] — GUI input lock correction
**Data:** 2026-09-06

### Fixed

- Fixed the Myra temporary-root lifecycle: returning from F8/F9/F10/F11 now correctly clears the overlay state instead of leaving gameplay input blocked.
- Added same-frame consumption of queued Myra UI actions. A click on a HUD action such as `Tor prosty` can no longer change the build mode and then leak into `InputManager` as a world click in the same frame.
- The existing F8/F9/F10/F11 build-mode reset remains active.

### Verification status

No Windows build or live gameplay verification was performed. The correction was checked statically against the current Myra root/action flow.

## [0.2.5] — Named railway lines and bulk infrastructure editing
**Data:** 2026-09-06

### Named lines

- Added player-defined `RailwayLine` groups for named railway lines/sections.
- Added F11 `MyraRailwayLineView` for creating, selecting, extending, reducing and deleting named lines.
- Named lines are organizational metadata and do not replace blocks, signals, switches, routes or timetable logic.

### Multi-track selection

- LPM on an existing track in `None` build mode starts a new selection.
- Shift+LPM adds a track to the selection.
- Ctrl+LPM toggles a track in the selection.
- Connected-area selection reuses existing track topology through flood-fill.

### Bulk infrastructure editing

- Mass change of `TrackType`, `LineClass` and `TractionSystem` for the current selection.
- The same operation can be applied to a whole named line.
- Selected tracks can be added to or removed from an existing named line.
- Removing a physical track removes that position from all named lines.

### Map presentation

- Named lines are shown with colored contours.
- Active multi-track selection is shown with a white contour.
- Existing traction colors and line-class thickness remain visible underneath the line contour.

### GUI input safety

- Temporary Myra gameplay GUIs are modal with respect to the world.
- While a GUI is open, `GameplayScreen` does not pass input to `InputManager`, so map clicks cannot place/select/remove tracks or manipulate signals, switches or the camera.
- Opening F8/F9/F10/F11 resets the track build mode to `None`.
- Closing a GUI therefore cannot turn the close click into an accidental track placement.

### Persistence

- `map.json` schema moved to `3`.
- Named line ID, name, color index and track positions are persisted.
- Old map schemas are not a compatibility target.

### Scope deliberately deferred

- no economy;
- no automatic route repair;
- no automatic dispatcher/signal/switch changes;
- no movement/physics rewrite.

### Documentation

- Added `docs/changelog/0.2.5.md`.
- Updated `docs/32-current-state-0.2.5.md`, `docs/00-index.md`, `docs/06-input.md`, `docs/15-ai-context.md` and `docs/16-screens-and-ui.md`.

### Verification

No Windows build or live gameplay verification was performed. Runtime verification should cover F11 editing, Shift/Ctrl selection, line contours, schema 3 save/load, GUI input locking and unchanged dispatcher/block/signal/F6 behaviour.
