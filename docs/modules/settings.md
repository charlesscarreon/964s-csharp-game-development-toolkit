# Settings (`NineSixFour.GameDevelopmentToolkit.Settings`)

A lightweight, engine-agnostic settings store with typed access, defaults, change events, and pluggable storage (no UI).

## Key types

- `SettingsStore` — in-memory settings with defaults, typed getters/setters, save/load
- `ISettingsStorage` — persistence abstraction (`Save`/`Load` of `Dictionary<string,string>`)
- `InMemorySettingsStorage` — reference storage implementation
- `SettingChangedEventArgs` — `Key`, `OldValue`, `NewValue`

## Quick start

```csharp
using NineSixFour.GameDevelopmentToolkit.Settings;

var store = new SettingsStore();

// Defaults (applied if key not yet set)
store.SetDefault("MasterVolume", 1.0f);
store.SetDefault("Fullscreen", true);

// Typed access
float volume = store.GetFloat("MasterVolume", 1.0f);
bool fullscreen = store.GetBool("Fullscreen", true);

store.SetFloat("MasterVolume", 0.8f);
store.SetBool("Fullscreen", false);

// Generic get/set
int quality = store.Get<int>("Quality", 2);
store.Set("Quality", 3);
```

## Change events

```csharp
store.SettingChanged += (sender, e) =>
{
    // e.Key, e.OldValue, e.NewValue
};
```

`SettingChanged` is raised when `Set(...)` is called.

## Persistence (storage abstraction)

```csharp
var storage = new InMemorySettingsStorage();
store.Storage = storage;

store.Save(); // writes current settings via ISettingsStorage
store.Load(); // loads values from storage into the store
```

`ISettingsStorage` contract:

```csharp
public interface ISettingsStorage
{
    void Save(Dictionary<string, string> data);
    Dictionary<string, string> Load();
}
```

Implement this interface to persist to files, PlayerPrefs, cloud, etc. (engine-specific code belongs outside the core module).

## API highlights

- Defaults: `SetDefault(key, value)`
- Typed helpers: `Get/SetBool`, `Get/SetInt`, `Get/SetFloat`, `Get/SetDouble`, `Get/SetString`
- Generic: `Get<T>(key, defaultValue)`, `Set(key, value)`
- Management: `HasKey`, `Remove`, `Clear`, `ResetToDefaults`
- Persistence: `Storage`, `Save()`, `Load()`

## Behavior notes

- Keys are compared case-insensitively.
- `Get<T>` attempts direct cast and `Convert.ChangeType` (invariant culture), falling back to the provided default on failure; it also falls back to registered defaults when the key is not present in current values.
- `Save()` serializes stored values using `ToString()` into `Dictionary<string,string>`; `Load()` stores loaded values as strings. Use typed getters (e.g. `GetInt`, `GetBool`) to retrieve converted values after load.
- `ResetToDefaults()` clears current values and reapplies defaults.

## Examples & testers

- Automated: `tests\NineSixFour.GameDevelopmentToolkit.Settings.AutomatedTests`
- Manual: `samples\NineSixFour.GameDevelopmentToolkit.Settings.ManualTester`
