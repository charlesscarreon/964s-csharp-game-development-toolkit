# Timer (`NineSixFour.GameDevelopmentToolkit.Timer`)

Configurable, `Update()`-driven timers for cooldowns, countdowns, count-ups, and stopwatches.

> **Tip:** alias the toolkit timer to avoid confusion with `System.Threading.Timer`:  
> `using GameTimer = NineSixFour.GameDevelopmentToolkit.Timer.Timer;`

## Key types

- `Timer` — core timer (countdown or count-up)
- `TimerOptions` — duration, direction, precision, limits, completion/adjustment behavior
- `TimerState` (`Idle`, `Running`, `Paused`, `Completed`), `TimerDirection` (`CountDown`, `CountUp`), `TimerPrecision`
- `ITimeProvider`, `SystemTimeProvider`, `ManualTimeProvider`
- `TimerManager` — create/manage/update multiple named timers
- `Stopwatch` — helpers to create count-up timers
- Event args: `TimerEventArgs`, `TimerTickEventArgs`, `TimerTimeAdjustedEventArgs`, `TimerCompletedEventArgs`

## Quick start

```csharp
using System;
using NineSixFour.GameDevelopmentToolkit.Timer;
using GameTimer = NineSixFour.GameDevelopmentToolkit.Timer.Timer;

// Countdown
var timer = new GameTimer(new TimerOptions
{
    Duration = TimeSpan.FromSeconds(10),
    Direction = TimerDirection.CountDown,
    AutoStart = true
});

timer.Completed += (s, e) => { /* finished */ };

// In your game loop:
timer.Update();

// Inspect
var remaining = timer.Remaining;
var progress = timer.Progress; // 0..1
```

## Count-up / stopwatch

```csharp
using NineSixFour.GameDevelopmentToolkit.Timer;

// Unlimited stopwatch
var sw = Stopwatch.CreateUnlimited(autoStart: true);

// Limited stopwatch (completes/stops at max)
var limited = Stopwatch.CreateWithLimit(TimeSpan.FromSeconds(30), autoStart: true);

// Manual options
var timer = new Timer(new TimerOptions
{
    Direction = TimerDirection.CountUp,
    MaxTime = TimeSpan.FromMinutes(2),
    StopAtMaxTime = true,
    AutoStart = true
});
```

## Lifecycle and control

- `Start()`, `Pause()`, `Resume()`, `Stop()`, `Reset()`, `Restart()`
- State/properties: `State`, `IsRunning`, `IsPaused`, `IsIdle`, `IsCompleted`, `IsStopwatch`
- Time: `Elapsed`, `Remaining`, `Value` (`Remaining` for countdown, `Elapsed` for count-up), `Progress`, `Duration`, `PendingTime`
- Call `Update()` regularly (e.g. each frame/tick) to advance completion/tick evaluation.

## Runtime adjustments & pending time

```csharp
timer.AddSeconds(5);
timer.SubtractMilliseconds(500);
timer.SetDuration(TimeSpan.FromSeconds(20));
timer.SetRemaining(TimeSpan.FromSeconds(8)); // meaningful for countdown

// Stack adjustments and apply later
timer.AddPendingSeconds(2);
timer.AddPendingMilliseconds(500);
timer.ApplyPending();
timer.ClearPending();
```

Behavior (per options/implementation): supports pending-time stacking, revive-on-time-add (`AllowReviveOnTimeAdd`), over-subtract/clamp handling (`ClampToZero`, `ClampToZeroOnOverSubtract`, `AllowNegativeRemaining`), and configurable completion firing (`FireCompletedOnlyOnce`).

## TimerManager

```csharp
var manager = new TimerManager();
manager.DefaultTimeProvider = new SystemTimeProvider();

var cooldown = manager.Create("cooldown", TimeSpan.FromSeconds(3), o => o.AutoStart = true);
manager.TimerCompleted += (s, e) => { /* e.Timer */ };

// Each tick/frame
manager.Update();

// Bulk controls: StartAll/PauseAll/ResumeAll/StopAll/ResetAll, Get/GetAll/Remove/Clear
```

## Events

- `Started`, `Paused`, `Resumed`, `Stopped`, `Completed` (`TimerEventArgs`)
- `Tick` (`TimerTickEventArgs`, includes `Precision`)
- `DurationChanged` (`TimerEventArgs`)
- `TimeAdjusted` (`TimerTimeAdjustedEventArgs`: `Adjustment`, `NewDuration`)
- `Revived` (`TimerEventArgs`)

## Time providers (deterministic testing)

```csharp
var time = new ManualTimeProvider();
var timer = new Timer(new TimerOptions { Duration = TimeSpan.FromSeconds(5), AutoStart = true }, time);

time.AdvanceSeconds(2);
timer.Update();
// timer.Elapsed ~ 2s
```

`ITimeProvider` exposes `GetTicks()`; `SystemTimeProvider` uses `DateTime.UtcNow.Ticks`.

## Notes

- `TimerOptions` defaults: 1s countdown, `Seconds` precision, not auto-started, clamping enabled, revive-on-add enabled, complete-once, `StopAtMaxTime` enabled.
- Count-up timers can be unlimited (`MaxTime == null`) or limited (`MaxTime` + `StopAtMaxTime`).
- See: `tests/NineSixFour.GameDevelopmentToolkit.Timer.AutomatedTests`, `samples/NineSixFour.GameDevelopmentToolkit.Timer.ManualTester`.
