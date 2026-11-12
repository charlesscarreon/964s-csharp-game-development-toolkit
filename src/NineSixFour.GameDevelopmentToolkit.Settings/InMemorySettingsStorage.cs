using System.Collections.Generic;

namespace NineSixFour.GameDevelopmentToolkit.Settings
{
    public class InMemorySettingsStorage : ISettingsStorage
    {
        private Dictionary<string, string> _data = new Dictionary<string, string>();
        public void Save(Dictionary<string, string> data)
        {
            _data = new Dictionary<string, string>(data);
        }
        public Dictionary<string, string> Load()
        {
            return new Dictionary<string, string>(_data);
        }
    }
}
