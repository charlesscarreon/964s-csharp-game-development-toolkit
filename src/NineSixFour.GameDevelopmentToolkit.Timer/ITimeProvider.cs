namespace NineSixFour.GameDevelopmentToolkit.Timer
{
    /// <summary>
    /// Provides time information for timers (allows for testability).
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>
        /// Gets the current time in ticks.
        /// </summary>
        long GetTicks();
    }
}
