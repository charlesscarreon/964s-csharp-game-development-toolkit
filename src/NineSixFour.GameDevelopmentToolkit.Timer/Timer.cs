using System;

namespace NineSixFour.GameDevelopmentToolkit.Timer
{
    public class Timer
    {
        private long _startTicks;
        private long _pausedTicks;
        private long _totalPausedTicks;
        private bool _completedFired;
        private TimeSpan _pendingTime = TimeSpan.Zero;

        public string? Id { get; }
        public TimerOptions Options { get; }
        public ITimeProvider TimeProvider { get; }
        public TimerState State { get; private set; } = TimerState.Idle;

        public TimeSpan Duration { get; private set; }
        public TimeSpan PendingTime => _pendingTime;

        public TimeSpan Elapsed
        {
            get
            {
                if (State == TimerState.Idle) return TimeSpan.Zero;
                long now = TimeProvider.GetTicks();
                long total = now - _startTicks - _totalPausedTicks;
                if (State == TimerState.Paused) total = _pausedTicks - _startTicks - _totalPausedTicks;
                if (total < 0) total = 0;
                return TimeSpan.FromTicks(total);
            }
        }

        public TimeSpan Remaining
        {
            get
            {
                if (Options.Direction == TimerDirection.CountUp)
                {
                    if (Options.MaxTime.HasValue)
                    {
                        var rem = Options.MaxTime.Value - Elapsed;
                        if (!Options.AllowNegativeRemaining && rem < TimeSpan.Zero) rem = TimeSpan.Zero;
                        return rem;
                    }
                    return TimeSpan.Zero; // unlimited
                }
                else
                {
                    var rem = Duration - Elapsed;
                    if (Options.ClampToZero && rem < TimeSpan.Zero) rem = TimeSpan.Zero;
                    if (!Options.AllowNegativeRemaining && rem < TimeSpan.Zero) rem = TimeSpan.Zero;
                    return rem;
                }
            }
        }

        public TimeSpan Value => Options.Direction == TimerDirection.CountDown ? Remaining : Elapsed;
        public double Progress
        {
            get
            {
                if (Duration == TimeSpan.Zero) return 1.0;
                if (Options.Direction == TimerDirection.CountUp && Options.MaxTime.HasValue)
                {
                    var p = Elapsed.TotalSeconds / Options.MaxTime.Value.TotalSeconds;
                    if (p < 0) p = 0;
                    if (p > 1) p = 1;
                    return p;
                }
                var prog = Elapsed.TotalSeconds / Duration.TotalSeconds;
                if (prog < 0) prog = 0;
                if (prog > 1) prog = 1;
                return prog;
            }
        }

        public bool IsRunning => State == TimerState.Running;
        public bool IsPaused => State == TimerState.Paused;
        public bool IsIdle => State == TimerState.Idle;
        public bool IsCompleted => State == TimerState.Completed;
        public bool IsStopwatch => Options.Direction == TimerDirection.CountUp;

        public event EventHandler<TimerEventArgs>? Started;
        public event EventHandler<TimerEventArgs>? Paused;
        public event EventHandler<TimerEventArgs>? Resumed;
        public event EventHandler<TimerEventArgs>? Stopped;
        public event EventHandler<TimerEventArgs>? Completed;
        public event EventHandler<TimerTickEventArgs>? Tick;
        public event EventHandler<TimerEventArgs>? DurationChanged;
        public event EventHandler<TimerTimeAdjustedEventArgs>? TimeAdjusted;
        public event EventHandler<TimerEventArgs>? Revived;

        public Timer(TimerOptions options, ITimeProvider? timeProvider = null, string? id = null)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            TimeProvider = timeProvider ?? new SystemTimeProvider();
            Id = id;
            Duration = options.Duration;
            if (options.AutoStart) Start();
            else if (options.StartPaused) { Start(); Pause(); }
        }

        public void Start()
        {
            if (State == TimerState.Running) return;
            if (State == TimerState.Paused)
            {
                _totalPausedTicks += (TimeProvider.GetTicks() - _pausedTicks);
                State = TimerState.Running;
                Resumed?.Invoke(this, new TimerEventArgs(this, Elapsed, Remaining));
                return;
            }
            _startTicks = TimeProvider.GetTicks();
            _totalPausedTicks = 0;
            _pausedTicks = 0;
            _completedFired = false;
            State = TimerState.Running;
            Started?.Invoke(this, new TimerEventArgs(this, Elapsed, Remaining));
        }

        public void Pause()
        {
            if (State != TimerState.Running) return;
            _pausedTicks = TimeProvider.GetTicks();
            State = TimerState.Paused;
            Paused?.Invoke(this, new TimerEventArgs(this, Elapsed, Remaining));
        }

        public void Resume() => Start();
        public void Stop() { State = TimerState.Idle; Stopped?.Invoke(this, new TimerEventArgs(this, Elapsed, Remaining)); }
        public void Reset() { Stop(); _completedFired = false; }
        public void Restart() { Reset(); Start(); }

        public void AddTime(TimeSpan amount) => AdjustTime(amount);
        public void SubtractTime(TimeSpan amount) => AdjustTime(-amount);
        public void AddSeconds(double seconds) => AddTime(TimeSpan.FromSeconds(seconds));
        public void AddMilliseconds(double ms) => AddTime(TimeSpan.FromMilliseconds(ms));
        public void SubtractSeconds(double seconds) => SubtractTime(TimeSpan.FromSeconds(seconds));
        public void SubtractMilliseconds(double ms) => SubtractTime(TimeSpan.FromMilliseconds(ms));

        private void AdjustTime(TimeSpan adjustment)
        {
            Duration = Duration + adjustment;
            TimeAdjusted?.Invoke(this, new TimerTimeAdjustedEventArgs(this, Elapsed, Remaining, adjustment, Duration));
            DurationChanged?.Invoke(this, new TimerEventArgs(this, Elapsed, Remaining));
            if (State == TimerState.Completed && adjustment > TimeSpan.Zero && Options.AllowReviveOnTimeAdd)
            {
                if (Remaining > TimeSpan.Zero || (Options.Direction == TimerDirection.CountUp))
                {
                    _completedFired = false;
                    if (Options.Direction == TimerDirection.CountDown) State = TimerState.Running;
                    else State = TimerState.Running;
                    Revived?.Invoke(this, new TimerEventArgs(this, Elapsed, Remaining));
                }
            }
            if (Options.Direction == TimerDirection.CountDown && adjustment < TimeSpan.Zero && Options.ClampToZeroOnOverSubtract)
            {
                if (Remaining == TimeSpan.Zero && !_completedFired)
                {
                    Complete();
                }
            }
        }

        public void SetDuration(TimeSpan duration)
        {
            Duration = duration;
            DurationChanged?.Invoke(this, new TimerEventArgs(this, Elapsed, Remaining));
        }

        public void SetRemaining(TimeSpan remaining)
        {
            if (Options.Direction == TimerDirection.CountDown)
            {
                Duration = Elapsed + remaining;
                DurationChanged?.Invoke(this, new TimerEventArgs(this, Elapsed, Remaining));
            }
        }

        public void AddPendingTime(TimeSpan amount) => _pendingTime += amount;
        public void AddPendingSeconds(double seconds) => AddPendingTime(TimeSpan.FromSeconds(seconds));
        public void AddPendingMilliseconds(double ms) => AddPendingTime(TimeSpan.FromMilliseconds(ms));
        public void ClearPending() => _pendingTime = TimeSpan.Zero;
        public void ApplyPending()
        {
            if (_pendingTime == TimeSpan.Zero) return;
            var total = _pendingTime;
            ClearPending();
            AdjustTime(total);
        }

        public void Update()
        {
            if (State != TimerState.Running) return;
            if (Options.Direction == TimerDirection.CountDown)
            {
                if (Remaining == TimeSpan.Zero && !_completedFired)
                {
                    Complete();
                }
            }
            else // CountUp
            {
                if (Options.MaxTime.HasValue && Options.StopAtMaxTime)
                {
                    if (Elapsed >= Options.MaxTime.Value && !_completedFired)
                    {
                        Complete();
                    }
                }
            }
            Tick?.Invoke(this, new TimerTickEventArgs(this, Elapsed, Remaining, Options.Precision));
        }

        private void Complete()
        {
            if (Options.FireCompletedOnlyOnce && _completedFired) return;
            _completedFired = true;
            State = TimerState.Completed;
            Completed?.Invoke(this, new TimerEventArgs(this, Elapsed, Remaining));
        }
    }
}
