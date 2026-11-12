using System;
using System.Collections.Generic;
using System.Globalization;

namespace NineSixFour.GameDevelopmentToolkit.Settings
{
    public class SettingsStore
    {
        private readonly Dictionary<string, object> _settings = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, object> _defaults = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        public ISettingsStorage? Storage { get; set; }

        public event EventHandler<SettingChangedEventArgs>? SettingChanged;

        public void SetDefault(string key, object value)
        {
            _defaults[key] = value;
            if (!_settings.ContainsKey(key))
                _settings[key] = value;
        }

        public void Set(string key, object value)
        {
            var oldValue = _settings.ContainsKey(key) ? _settings[key] : null;
            _settings[key] = value;
            SettingChanged?.Invoke(this, new SettingChangedEventArgs(key, oldValue, value));
        }

        public T Get<T>(string key, T defaultValue)
        {
            if (_settings.TryGetValue(key, out var val))
            {
                try
                {
                    if (val is T t) return t;
                    return (T)Convert.ChangeType(val, typeof(T), CultureInfo.InvariantCulture);
                }
                catch
                {
                    return defaultValue;
                }
            }
            if (_defaults.TryGetValue(key, out var def))
            {
                try
                {
                    if (def is T t2) return t2;
                    return (T)Convert.ChangeType(def, typeof(T), CultureInfo.InvariantCulture);
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false) => Get(key, defaultValue);
        public int GetInt(string key, int defaultValue = 0) => Get(key, defaultValue);
        public float GetFloat(string key, float defaultValue = 0f) => Get(key, defaultValue);
        public double GetDouble(string key, double defaultValue = 0.0) => Get(key, defaultValue);
        public string GetString(string key, string defaultValue = "") => Get(key, defaultValue);

        public void SetBool(string key, bool value) => Set(key, value);
        public void SetInt(string key, int value) => Set(key, value);
        public void SetFloat(string key, float value) => Set(key, value);
        public void SetDouble(string key, double value) => Set(key, value);
        public void SetString(string key, string value) => Set(key, value);

        public void ResetToDefaults()
        {
            _settings.Clear();
            foreach (var kv in _defaults)
                _settings[kv.Key] = kv.Value;
        }

        public void Save()
        {
            if (Storage == null) return;
            var dict = new Dictionary<string, string>();
            foreach (var kv in _settings)
                dict[kv.Key] = kv.Value?.ToString() ?? "";
            Storage.Save(dict);
        }

        public void Load()
        {
            if (Storage == null) return;
            var dict = Storage.Load();
            foreach (var kv in dict)
            {
                // Try to store as string; consumers can use typed getters
                _settings[kv.Key] = kv.Value;
            }
        }

        public bool HasKey(string key) => _settings.ContainsKey(key) || _defaults.ContainsKey(key);
        public void Remove(string key) => _settings.Remove(key);
        public void Clear() => _settings.Clear();
    }
}
