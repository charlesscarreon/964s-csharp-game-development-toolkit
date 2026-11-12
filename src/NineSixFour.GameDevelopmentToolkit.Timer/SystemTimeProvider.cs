using System;

namespace NineSixFour.GameDevelopmentToolkit.Timer
{
    public class SystemTimeProvider : ITimeProvider
    {
        public long GetTicks() => DateTime.UtcNow.Ticks;
    }
}
