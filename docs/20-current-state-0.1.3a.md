# Current State — 0.1.3a

`0.1.3a` is the first large gameplay-UI migration stage after the `0.1.2pre` Myra stabilization series.

## Myra gameplay HUD

The gameplay session now mounts `MyraGameplayView` as the active Myra root.

The HUD currently contains:

- simulation clock,
- simulation day (`GameDay`),
- speed controls `x1`, `x2`, `x5`,
- train list,
- station list with waiting passenger counts,
- building-tool panel,
- wagon-route edit control.

The train and station lists are refreshed approximately every 0.5 seconds.

## Navigation

Train and station list entries dispatch navigation actions to the active gameplay camera. The current bridge obtains the existing gameplay-owned camera and builder from `GameplayScreen` so that the UI migration does not duplicate gameplay state.

## Input

Existing keyboard shortcuts remain authoritative and continue to work alongside the Myra controls.

The wagon route edit control exposes the existing `S` workflow through the gameplay input manager rather than creating a second route-edit implementation.

## Pause interaction

The pause system remains owned by `GameplayScreen`. Myra gameplay HUD and pause UI share the same `MyraUIManager` desktop. The manager preserves the previous root while a temporary pause root is active so that Resume restores the gameplay HUD.

## Persistence

Save/Load remain owned by the existing gameplay persistence path and are not duplicated in the gameplay HUD.

## Not yet complete

The following planned `0.1.3` work remains for later stabilization/follow-up commits:

- dedicated Myra object windows for trains and stations,
- full Myra replacement of junction radial menus,
- full Myra replacement of signal radial menus,
- richer route/station information windows,
- scrollable production-quality lists,
- persistent configurable list colors.

These are intentionally left as follow-up work rather than creating additional lettered feature stages before the main `0.1.3a` integration is tested.