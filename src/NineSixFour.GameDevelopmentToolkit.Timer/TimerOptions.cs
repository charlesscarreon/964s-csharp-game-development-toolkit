using System;

namespace NineSixFour.GameDevelopmentToolkit.Timer
{
    public class TimerOptions
    {
        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan? MaxTime { get; set; } = null;
        public TimerDirection Direction { get; set; } = TimerDirection.CountDown;
        public TimerPrecision Precision { get; set; } = TimerPrecision.Seconds;
        public bool AutoStart { get; set; } = false;
        public bool StartPaused { get; set; } = false;
        public bool ClampToZero { get; set; } = true;
        public bool ClampToZeroOnOverSubtract { get; set; } = true;
        public bool AllowNegativeRemaining { get; set; } = false;
        public bool AllowReviveOnTimeAdd { get; set; } = true;
        public bool FireCompletedOnlyOnce { get; set; } = true;
        public bool StopAtMaxTime { get; set; } = true;
    }
}
