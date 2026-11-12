using System.Collections.Generic;
using System.IO;

namespace NineSixFour.GameDevelopmentToolkit.SaveData
{
    /// <summary>Abstraction for save slot storage (files, cloud, memory, etc.).</summary>
    public interface ISaveStorage
    {
        bool Exists(string slotId);
        Stream OpenRead(string slotId);
        Stream OpenWrite(string slotId);
        void Delete(string slotId);
        IEnumerable<string> ListSlotIds();
    }
}
