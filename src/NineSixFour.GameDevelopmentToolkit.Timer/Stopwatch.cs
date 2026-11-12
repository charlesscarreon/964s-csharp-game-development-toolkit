using System;

namespace NineSixFour.GameDevelopmentToolkit.Timer
{
    public static class Stopwatch
    {
        public static Timer Create(TimeSpan? maxTime = null, bool stopAtMaxTime = true, ITimeProvider? timeProvider = null, bool autoStart = false)
        {
            var options = new TimerOptions
            {
                Direction = TimerDirection.CountUp,
                MaxTime = maxTime,
                StopAtMaxTime = stopAtMaxTime,
                AutoStart = autoStart,
                Duration = TimeSpan.Zero
            };
            return new Timer(options, timeProvider);
        }

        public static Timer CreateWithLimit(TimeSpan maxTime, bool autoStart = false, ITimeProvider? timeProvider = null)
        {
            return Create(maxTime, true, timeProvider, autoStart);
        }

        public static Timer CreateUnlimited(bool autoStart = false, ITimeProvider? timeProvider = null)
        {
            return Create(null, false, timeProvider, autoStart);
        }
    }
}
