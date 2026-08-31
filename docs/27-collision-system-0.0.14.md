# Collision system — 0.0.14

## Responsibility

`TrainCollisionController` provides the minimal emergency protection required before 0.1.0. It does not replace the signal/block system.

## Detection rule

For the train's current direction, the controller follows the currently selected track path through cells and junctions. It checks for other train vehicles in front of the train.

- Safety distance: 2 cells.
- If another train is inside that distance and no matching signal was encountered first, `RadioStop()` is issued.
- If the next matching signal occurs before the other train, the signal system owns the decision and the collision controller does not stop the train.

This deliberately follows the current switch state instead of using a simple Euclidean radius, so trains on unrelated branches are not treated as obstacles.

## Spawn protection

`TrainManager.Add()` rejects a new train when any vehicle of the candidate consist would occupy a map cell already occupied by another train vehicle.

## RadioStop

`Train.RadioStop()` is currently a simple zero-speed command. It is intentionally separated from the movement algorithm so a future event/command dispatcher can replace it without redesigning collision detection.

## Priority

The effective order is:

`Signal > Collision > Station`

A station stop does not override a signal decision, and collision protection only intervenes when no protecting signal exists before the obstacle.

## Future work

The system is not a railway safety system yet. Interlocking, block reservation, braking-distance calculation, signal aspect propagation, route conflicts and realistic train protection are future work.
