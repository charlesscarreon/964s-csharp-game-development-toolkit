using System;
using System.Collections.Generic;
using System.Linq;

namespace NineSixFour.GameDevelopmentToolkit.Timer
{
    public class TimerManager
    {
        private readonly Dictionary<string, Timer> _timers = new Dictionary<string, Timer>();
        public ITimeProvider DefaultTimeProvider { get; set; } = new SystemTimeProvider();

        public event EventHandler<TimerCompletedEventArgs>? TimerCompleted;
        public event EventHandler<TimerTimeAdjustedEventArgs>? TimerTimeAdjusted;

        public Timer Create(string id, TimeSpan duration, Action<TimerOptions>? configure = null)
        {
            var options = new TimerOptions { Duration = duration };
            configure?.Invoke(options);
            return Create(id, options);
        }

        public Timer Create(string id, TimerOptions options)
        {
            var timer = new Timer(options, DefaultTimeProvider, id);
            timer.Completed += (s, e) => TimerCompleted?.Invoke(this, new TimerCompletedEventArgs(timer));
            timer.TimeAdjusted += (s, e) => TimerTimeAdjusted?.Invoke(this, e as TimerTimeAdjustedEventArgs ?? new TimerTimeAdjustedEventArgs(timer, timer.Elapsed, timer.Remaining, TimeSpan.Zero, timer.Duration));
            _timers[id] = timer;
            return timer;
        }

        public Timer? Get(string id)
        {
            _timers.TryGetValue(id, out var timer);
            return timer;
        }

        public IEnumerable<Timer> GetAll() => _timers.Values;
        public bool Remove(string id) => _timers.Remove(id);
        public void Clear() => _timers.Clear();

        public void Update()
        {
            foreach (var timer in _timers.Values.ToList())
            {
                timer.Update();
            }
        }

        public void StartAll() { foreach (var t in _timers.Values) if (t.IsIdle) t.Start(); }
        public void PauseAll() { foreach (var t in _timers.Values) t.Pause(); }
        public void ResumeAll() { foreach (var t in _timers.Values) t.Resume(); }
        public void StopAll() { foreach (var t in _timers.Values) t.Stop(); }
        public void ResetAll() { foreach (var t in _timers.Values) t.Reset(); }
    }
}
