using System;

namespace NineSixFour.GameDevelopmentToolkit.Timer
{
    public class TimerEventArgs : EventArgs
    {
        public Timer Timer { get; }
        public TimeSpan Elapsed { get; }
        public TimeSpan Remaining { get; }
        public TimerEventArgs(Timer timer, TimeSpan elapsed, TimeSpan remaining)
        {
            Timer = timer;
            Elapsed = elapsed;
            Remaining = remaining;
        }
    }

    public class TimerTickEventArgs : TimerEventArgs
    {
        public TimerPrecision Precision { get; }
        public TimerTickEventArgs(Timer timer, TimeSpan elapsed, TimeSpan remaining, TimerPrecision precision) : base(timer, elapsed, remaining)
        {
            Precision = precision;
        }
    }

    public class TimerTimeAdjustedEventArgs : TimerEventArgs
    {
        public TimeSpan Adjustment { get; }
        public TimeSpan NewDuration { get; }
        public TimerTimeAdjustedEventArgs(Timer timer, TimeSpan elapsed, TimeSpan remaining, TimeSpan adjustment, TimeSpan newDuration) : base(timer, elapsed, remaining)
        {
            Adjustment = adjustment;
            NewDuration = newDuration;
        }
    }

    public class TimerCompletedEventArgs : EventArgs
    {
        public Timer Timer { get; }
        public TimerCompletedEventArgs(Timer timer) { Timer = timer; }
    }
}
