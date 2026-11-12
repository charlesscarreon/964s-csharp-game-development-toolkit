namespace NineSixFour.GameDevelopmentToolkit.Timer
{
    public class ManualTimeProvider : ITimeProvider
    {
        private long _ticks;
        public ManualTimeProvider(long startTicks = 0) { _ticks = startTicks; }
        public long GetTicks() => _ticks;
        public void SetTicks(long ticks) => _ticks = ticks;
        public void Advance(long ticks) => _ticks += ticks;
        public void AdvanceSeconds(double seconds) => _ticks += (long)(seconds * 10000000.0);
        public void AdvanceMilliseconds(double ms) => _ticks += (long)(ms * 10000.0);
    }
}
