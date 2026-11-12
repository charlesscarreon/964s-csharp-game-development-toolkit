using System.Collections.Generic;

namespace NineSixFour.GameDevelopmentToolkit.Settings
{
    public interface ISettingsStorage
    {
        void Save(Dictionary<string, string> data);
        Dictionary<string, string> Load();
    }
}
