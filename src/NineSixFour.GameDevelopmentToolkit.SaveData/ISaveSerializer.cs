namespace NineSixFour.GameDevelopmentToolkit.SaveData
{
    /// <summary>Abstraction for serializing save data and envelopes.</summary>
    public interface ISaveSerializer
    {
        byte[] Serialize<T>(T data);
        T Deserialize<T>(byte[] data);
    }
}
