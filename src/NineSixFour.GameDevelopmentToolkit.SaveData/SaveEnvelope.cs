namespace NineSixFour.GameDevelopmentToolkit.SaveData
{
    /// <summary>Internal envelope stored per slot: metadata + serialized payload bytes.</summary>
    public class SaveEnvelope
    {
        public SaveSlotMetadata Metadata { get; set; } = new SaveSlotMetadata();
        public byte[] Payload { get; set; } = new byte[0];
    }
}
