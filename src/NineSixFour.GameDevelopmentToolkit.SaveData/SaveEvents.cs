using System;

namespace NineSixFour.GameDevelopmentToolkit.SaveData
{
    public class SaveSlotEventArgs : EventArgs
    {
        public string SlotId { get; }
        public SaveSlotMetadata? Metadata { get; }

        public SaveSlotEventArgs(string slotId, SaveSlotMetadata? metadata)
        {
            SlotId = slotId;
            Metadata = metadata;
        }
    }

    public class SaveSlotRenamedEventArgs : EventArgs
    {
        public string SlotId { get; }
        public string OldDisplayName { get; }
        public string NewDisplayName { get; }
        public SaveSlotMetadata Metadata { get; }

        public SaveSlotRenamedEventArgs(string slotId, string oldDisplayName, string newDisplayName, SaveSlotMetadata metadata)
        {
            SlotId = slotId;
            OldDisplayName = oldDisplayName;
            NewDisplayName = newDisplayName;
            Metadata = metadata;
        }
    }

    public class LoadLimitReachedEventArgs : EventArgs
    {
        public string SlotId { get; }
        public SaveSlotMetadata Metadata { get; }
        public bool SlotDeleted { get; }

        public LoadLimitReachedEventArgs(string slotId, SaveSlotMetadata metadata, bool slotDeleted)
        {
            SlotId = slotId;
            Metadata = metadata;
            SlotDeleted = slotDeleted;
        }
    }
}
