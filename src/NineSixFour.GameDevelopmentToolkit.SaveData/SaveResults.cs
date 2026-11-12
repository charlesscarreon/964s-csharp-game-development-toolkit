using System;

namespace NineSixFour.GameDevelopmentToolkit.SaveData
{
    public enum SaveFailureReason
    {
        None = 0,
        SlotNotFound,
        MaxSlotsReached,
        RenamingDisabled,
        LoadLimitReached,
        InvalidSlotId,
        StorageError,
        SerializationError
    }

    public class SaveOperationResult
    {
        public bool Success { get; set; }
        public SaveFailureReason Reason { get; set; }
        public string Message { get; set; } = "";
        public SaveSlotMetadata? Metadata { get; set; }

        public static SaveOperationResult Ok(SaveSlotMetadata? metadata = null) => new SaveOperationResult { Success = true, Reason = SaveFailureReason.None, Metadata = metadata };
        public static SaveOperationResult Fail(SaveFailureReason reason, string message = "") => new SaveOperationResult { Success = false, Reason = reason, Message = message ?? "" };
    }

    public class LoadOperationResult<T>
    {
        public bool Success { get; set; }
        public SaveFailureReason Reason { get; set; }
        public string Message { get; set; } = "";
        public T Data { get; set; } = default!;
        public SaveSlotMetadata? Metadata { get; set; }
        public bool SlotDeletedDueToLoadLimit { get; set; }

        public static LoadOperationResult<T> Ok(T data, SaveSlotMetadata metadata) => new LoadOperationResult<T> { Success = true, Reason = SaveFailureReason.None, Data = data, Metadata = metadata };
        public static LoadOperationResult<T> Fail(SaveFailureReason reason, string message = "", bool deleted = false) => new LoadOperationResult<T> { Success = false, Reason = reason, Message = message ?? "", SlotDeletedDueToLoadLimit = deleted };
    }
}
