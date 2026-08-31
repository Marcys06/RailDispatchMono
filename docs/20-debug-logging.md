# Debug logging

`DebugManager` is the central diagnostic entry point. All categories share one global rolling-window budget of **30 emitted messages per second**. Excess messages are dropped before console, Trace and file output.

Existing categories, history and enable/disable controls remain available. The limiter is a safety cap; callers should still prefer state-change and event logging over per-frame diagnostics.
