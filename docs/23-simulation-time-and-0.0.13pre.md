# 0.0.13pre — debug output, documentation and simulation clock

At x1, 5 seconds of game time pass during 1 second of real time. x2 and x5 multiply game-clock progression further. `GameClock.Update()` returns the normal simulation delta without the fixed 5× presentation scale, so physical train speed and distance remain unchanged at x1. x2/x5 still accelerate the normal simulation delta.

`DebugManager` globally limits emitted diagnostics to 30 messages per second.

`docs/` is the only canonical documentation tree; historical release notes are stored in `docs/changelog/`.
