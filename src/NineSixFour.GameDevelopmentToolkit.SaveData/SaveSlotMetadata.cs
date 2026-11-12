using System;

namespace NineSixFour.GameDevelopmentToolkit.SaveData
{
    public class SaveSlotMetadata
    {
        public string SlotId { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public DateTime CreatedAtUtc { get; set; }
        public DateTime LastSavedAtUtc { get; set; }
        public int LoadCount { get; set; }
        /// <summary>Maximum allowed loads for this slot. -1 means unlimited.</summary>
        public int MaxLoadCount { get; set; } = -1;

        public SaveSlotMetadata Clone()
        {
            return new SaveSlotMetadata
            {
                SlotId = SlotId,
                DisplayName = DisplayName,
                CreatedAtUtc = CreatedAtUtc,
                LastSavedAtUtc = LastSavedAtUtc,
                LoadCount = LoadCount,
                MaxLoadCount = MaxLoadCount
            };
        }
    }
}
