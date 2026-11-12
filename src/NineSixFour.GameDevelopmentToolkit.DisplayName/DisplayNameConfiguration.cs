namespace NineSixFour.GameDevelopmentToolkit.DisplayName
{
    /// <summary>
    /// Global configuration for display name validation.
    /// </summary>
    public static class DisplayNameConfiguration
    {
        /// <summary>
        /// Universal fallback options used when no options are explicitly provided.
        /// </summary>
        public static DisplayNameOptions Default { get; set; } = new DisplayNameOptions();

        /// <summary>
        /// Named profiles for different systems (e.g., Username, ItemName, NPCName).
        /// </summary>
        public static DisplayNameProfileManager Profiles { get; } = new DisplayNameProfileManager();
    }
}
