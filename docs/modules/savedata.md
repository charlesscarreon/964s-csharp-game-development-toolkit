# Save Data (`NineSixFour.GameDevelopmentToolkit.SaveData`)

Save-slot management (metadata + payload envelope), load limits, rename/delete, and operation results/events.

> **Important:** this module does **not** handle persistent/permanent data. You provide `ISaveStorage` and `ISaveSerializer` implementations and control when save/load operations are invoked.

## Key types

- `SaveManager` — main API for slot operations
- `ISaveStorage` — slot storage abstraction (exists/read/write/delete/list)
- `ISaveSerializer` — serialize/deserialize payloads and envelopes (`byte[]`)
- `SaveDataOptions` — slot/rename/policy/load-limit options
- `SaveSlotMetadata` — slot id, display name, timestamps, load counts
- `SaveEnvelope` — stored per slot (metadata + payload bytes)
- `SaveOperationResult`, `LoadOperationResult<T>`, `SaveFailureReason`
- `InMemorySaveStorage` — in-memory `ISaveStorage` for testing/dev
- Events/args: `SaveSlotEventArgs`, `SaveSlotRenamedEventArgs`, `LoadLimitReachedEventArgs`

## Quick start

```csharp
using NineSixFour.GameDevelopmentToolkit.SaveData;

ISaveStorage storage = new InMemorySaveStorage();
ISaveSerializer serializer = /* your serializer (e.g. JSON/binary wrapper) */;

var manager = new SaveManager(storage, serializer, new SaveDataOptions
{
    MaxSlots = 10,
    AllowRenaming = true,
    DefaultMaxLoadCount = -1, // unlimited
    AutoDeleteOnLoadLimitReached = true
});

// Save (throws on failure)
manager.Save("slot1", myGameState, displayName: "My Save");

// Load (throws on failure)
var state = manager.Load<MyGameState>("slot1");

// Metadata / listing
SaveSlotMetadata? meta = manager.GetMetadata("slot1");
IReadOnlyList<SaveSlotMetadata> slots = manager.GetAllSlots();

// Rename / delete
manager.Rename("slot1", "Chapter 2");
manager.Delete("slot1");
```

## `Try*` and result-based APIs

```csharp
var save = manager.TrySave("slot1", myGameState, "My Save");
if (!save.Success) { /* save.Reason, save.Message */ }

var load = manager.TryLoad<MyGameState>("slot1");
if (load.Success)
{
    var data = load.Data;
    var metadata = load.Metadata;
}
else
{
    // load.Reason (e.g. SlotNotFound, LoadLimitReached), load.SlotDeletedDueToLoadLimit
}
```

Available operations include `TrySave/Save`, `TryLoad/Load`, `TryRename/Rename`, `TryDelete/Delete`, `TrySetMaxLoadCount/SetMaxLoadCount`, plus `Exists`, `GetMetadata`, `GetAllSlots`.

## Options & policy

- `MaxSlots` — max number of slots (`-1` = unlimited; `0` invalid). Applies to creating new slots (overwriting an existing slot is allowed).
- `AllowRenaming` — enables/disables rename operations.
- `DefaultMaxLoadCount` — default per-slot load limit for new saves (`-1` = unlimited).
- `AutoDeleteOnLoadLimitReached` — delete slot when a load attempt hits the limit.
- `AllowFreeSave` / `AllowFreeLoad` — **advisory flags only** (not enforced by `SaveManager`); use in your own game flow/UX rules.

## Behavior details

- **Envelope storage:** each slot stores a `SaveEnvelope` containing `SaveSlotMetadata` and serialized payload bytes (persisted via `ISaveStorage`/`ISaveSerializer`).
- **Load limits:** if `MaxLoadCount >= 0` and `LoadCount >= MaxLoadCount`, load fails with `LoadLimitReached`; if `AutoDeleteOnLoadLimitReached` is true the slot is deleted and `LoadLimitReached` is raised (`SlotDeleted` indicated).
- **Load counting:** successful loads increment `LoadCount` and persist updated metadata (best-effort write-back).
- **Metadata:** includes `SlotId`, `DisplayName`, `CreatedAtUtc`, `LastSavedAtUtc`, `LoadCount`, `MaxLoadCount`.

## Events

- `Saved`, `Loaded`, `Deleted` — `SaveSlotEventArgs` (`SlotId`, `Metadata`)
- `Renamed` — `SaveSlotRenamedEventArgs` (`SlotId`, `OldDisplayName`, `NewDisplayName`, `Metadata`)
- `LoadLimitReached` — `LoadLimitReachedEventArgs` (`SlotId`, `Metadata`, `SlotDeleted`)

## Abstractions to implement

```csharp
public interface ISaveStorage
{
    bool Exists(string slotId);
    Stream OpenRead(string slotId);
    Stream OpenWrite(string slotId);
    void Delete(string slotId);
    IEnumerable<string> ListSlotIds();
}

public interface ISaveSerializer
{
    byte[] Serialize<T>(T data);
    T Deserialize<T>(byte[] data);
}
```

Your serializer must be able to round-trip `SaveEnvelope` as well as your payload types (as used by `SaveManager`).

## Testers

- Automated: `tests\NineSixFour.GameDevelopmentToolkit.SaveData.AutomatedTests`
- Manual: `samples\NineSixFour.GameDevelopmentToolkit.SaveData.ManualTester`
 (uses in-memory storage/serializer patterns for demonstration)
