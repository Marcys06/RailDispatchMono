# Game domain

## Railway subsystem

The repository currently contains a `Game/Railway` area with domain classes including:

- `BlockController`
- `Junction`
- `TrackRoute`

These names indicate infrastructure/control responsibilities, but the exact behavioral contract must be derived from the implementation and call sites. Do not infer simulation rules from class names alone.

## Train subsystem

The `Game/Train` area currently contains classes including:

- `TrainManager`
- `Train`
- `Vehicle`
- `VehicleParameters`
- `Locomotive`
- `Wagon`

A useful conceptual hierarchy is that a train is composed of railway vehicles and that locomotives/wagons are vehicle variants. This is a domain orientation, not permission to redesign the inheritance/composition model. Preserve the actual public API unless the task explicitly requires refactoring.

## Rendering subsystem

`Game/Rendering/Camera.cs` is part of the game-side rendering infrastructure. Camera behavior should remain separate from platform hosting and should not become responsible for global application lifecycle.

## Effects

The `Effects` area contains `ParticleManager` and the settings model references `ParticleEffectType`. Effects are therefore part of the shared presentation/game layer and can be exposed through settings.

## Ownership rule

Before adding a property to a domain object, determine which existing class already owns the state. For example, train collection/coordination belongs with the train-management layer rather than being silently duplicated in a screen.

## Domain vs presentation

Screens should request or present domain state. They should not become the authoritative owner of railway simulation state merely because they currently render it.

Conversely, domain classes should not gain dependencies on `SpriteBatch`, `GameScreen`, or platform application classes merely to display themselves.

## Safe extension sequence

When extending railway gameplay:

1. Locate the domain object that owns the state.
2. Locate its manager/coordinator, if one exists.
3. Search all call sites before changing a signature.
4. Check the active screen(s) that consume the state.
5. Only then implement the smallest change that preserves the existing flow.
6. Update this documentation if the ownership or lifecycle contract changes.
