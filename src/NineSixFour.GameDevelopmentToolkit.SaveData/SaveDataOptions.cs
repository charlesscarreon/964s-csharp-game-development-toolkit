namespace NineSixFour.GameDevelopmentToolkit.SaveData
{
    public class SaveDataOptions
    {
        /// <summary>Maximum number of save slots. -1 means unlimited.</summary>
        public int MaxSlots { get; set; } = 10;
        /// <summary>Whether renaming save slots is allowed.</summary>
        public bool AllowRenaming { get; set; } = true;
        /// <summary>Advisory policy flag: game may allow saving freely. Not enforced by SaveManager.</summary>
        public bool AllowFreeSave { get; set; } = true;
        /// <summary>Advisory policy flag: game may allow loading freely. Not enforced by SaveManager.</summary>
        public bool AllowFreeLoad { get; set; } = true;
        /// <summary>Default max load count per slot. -1 means unlimited.</summary>
        public int DefaultMaxLoadCount { get; set; } = -1;
        /// <summary>If true, slot is deleted when load limit is reached (on load attempt).</summary>
        public bool AutoDeleteOnLoadLimitReached { get; set; } = true;
    }
}
