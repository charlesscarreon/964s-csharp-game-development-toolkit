using System;
using System.Collections.Generic;
using System.IO;

namespace NineSixFour.GameDevelopmentToolkit.SaveData
{
    /// <summary>
    /// Manages save slots, metadata, load limits, and save/load operations.
    /// Does not handle persistent data; uses ISaveStorage/ISaveSerializer abstractions.
    /// </summary>
    public class SaveManager
    {
        private readonly ISaveStorage _storage;
        private readonly ISaveSerializer _serializer;

        public SaveDataOptions Options { get; }

        public event EventHandler<SaveSlotEventArgs>? Saved;
        public event EventHandler<SaveSlotEventArgs>? Loaded;
        public event EventHandler<SaveSlotEventArgs>? Deleted;
        public event EventHandler<SaveSlotRenamedEventArgs>? Renamed;
        public event EventHandler<LoadLimitReachedEventArgs>? LoadLimitReached;

        public SaveManager(ISaveStorage storage, ISaveSerializer serializer, SaveDataOptions? options = null)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            Options = options ?? new SaveDataOptions();
            if (Options.MaxSlots == 0) throw new ArgumentException("MaxSlots cannot be 0.", nameof(options));
        }

        public bool Exists(string slotId)
        {
            ValidateSlotId(slotId);
            return _storage.Exists(slotId);
        }

        public SaveSlotMetadata? GetMetadata(string slotId)
        {
            ValidateSlotId(slotId);
            if (!_storage.Exists(slotId)) return null;
            var envelope = ReadEnvelope(slotId);
            return envelope?.Metadata?.Clone();
        }

        public IReadOnlyList<SaveSlotMetadata> GetAllSlots()
        {
            var list = new List<SaveSlotMetadata>();
            foreach (var id in _storage.ListSlotIds() ?? Array.Empty<string>())
            {
                try
                {
                    if (!_storage.Exists(id)) continue;
                    var env = ReadEnvelope(id);
                    if (env?.Metadata != null) list.Add(env.Metadata.Clone());
                }
                catch
                {
                    // Skip unreadable slots
                }
            }
            return list;
        }

        public SaveOperationResult TrySave<T>(string slotId, T data, string? displayName = null)
        {
            if (string.IsNullOrEmpty(slotId)) return SaveOperationResult.Fail(SaveFailureReason.InvalidSlotId, "SlotId is required.");
            if (data == null) return SaveOperationResult.Fail(SaveFailureReason.SerializationError, "Data cannot be null.");

            try
            {
                bool exists = _storage.Exists(slotId);
                if (!exists && Options.MaxSlots >= 0)
                {
                    int count = CountSlots();
                    if (count >= Options.MaxSlots)
                        return SaveOperationResult.Fail(SaveFailureReason.MaxSlotsReached, "Maximum number of save slots reached.");
                }

                SaveSlotMetadata meta;
                if (exists)
                {
                    var existing = ReadEnvelope(slotId);
                    meta = existing?.Metadata != null ? existing.Metadata.Clone() : CreateNewMetadata(slotId, displayName);
                    meta.LastSavedAtUtc = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(displayName)) meta.DisplayName = displayName!;
                }
                else
                {
                    meta = CreateNewMetadata(slotId, displayName);
                }

                byte[] payload = _serializer.Serialize(data);
                var envelope = new SaveEnvelope { Metadata = meta, Payload = payload ?? new byte[0] };
                WriteEnvelope(slotId, envelope);

                Saved?.Invoke(this, new SaveSlotEventArgs(slotId, meta.Clone()));
                return SaveOperationResult.Ok(meta.Clone());
            }
            catch (Exception ex)
            {
                return SaveOperationResult.Fail(SaveFailureReason.StorageError, ex.Message);
            }
        }

        public void Save<T>(string slotId, T data, string? displayName = null)
        {
            if (string.IsNullOrEmpty(slotId)) throw new ArgumentException("SlotId is required.", nameof(slotId));
            if (data == null) throw new ArgumentNullException(nameof(data));
            var result = TrySave(slotId, data, displayName);
            if (!result.Success)
            {
                if (result.Reason == SaveFailureReason.MaxSlotsReached)
                    throw new InvalidOperationException(result.Message);
                throw new InvalidOperationException(result.Message);
            }
        }

        public LoadOperationResult<T> TryLoad<T>(string slotId)
        {
            if (string.IsNullOrEmpty(slotId)) return LoadOperationResult<T>.Fail(SaveFailureReason.InvalidSlotId, "SlotId is required.");
            if (!_storage.Exists(slotId)) return LoadOperationResult<T>.Fail(SaveFailureReason.SlotNotFound, "Save slot not found.");

            try
            {
                var envelope = ReadEnvelope(slotId);
                if (envelope?.Metadata == null)
                    return LoadOperationResult<T>.Fail(SaveFailureReason.StorageError, "Invalid save data.");

                var meta = envelope.Metadata;
                // Check load limit before consuming a load
                if (meta.MaxLoadCount >= 0 && meta.LoadCount >= meta.MaxLoadCount)
                {
                    bool deleted = false;
                    if (Options.AutoDeleteOnLoadLimitReached)
                    {
                        _storage.Delete(slotId);
                        deleted = true;
                        Deleted?.Invoke(this, new SaveSlotEventArgs(slotId, meta.Clone()));
                    }
                    LoadLimitReached?.Invoke(this, new LoadLimitReachedEventArgs(slotId, meta.Clone(), deleted));
                    return LoadOperationResult<T>.Fail(SaveFailureReason.LoadLimitReached, "Load limit reached.", deleted);
                }

                // Perform load and increment count
                T data = _serializer.Deserialize<T>(envelope.Payload ?? new byte[0]);
                meta.LoadCount++;
                envelope.Metadata = meta;
                // Persist updated load count; if write fails we still return data but count may be stale - keep simple
                try { WriteEnvelope(slotId, envelope); } catch { }

                Loaded?.Invoke(this, new SaveSlotEventArgs(slotId, meta.Clone()));
                return LoadOperationResult<T>.Ok(data, meta.Clone());
            }
            catch (Exception ex)
            {
                return LoadOperationResult<T>.Fail(SaveFailureReason.SerializationError, ex.Message);
            }
        }

        public T Load<T>(string slotId)
        {
            if (string.IsNullOrEmpty(slotId)) throw new ArgumentException("SlotId is required.", nameof(slotId));
            var result = TryLoad<T>(slotId);
            if (!result.Success)
            {
                if (result.Reason == SaveFailureReason.SlotNotFound)
                    throw new FileNotFoundException(result.Message, slotId);
                if (result.Reason == SaveFailureReason.LoadLimitReached)
                    throw new InvalidOperationException(result.Message);
                throw new InvalidOperationException(result.Message);
            }
            return result.Data;
        }

        public SaveOperationResult TryRename(string slotId, string newDisplayName)
        {
            if (string.IsNullOrEmpty(slotId)) return SaveOperationResult.Fail(SaveFailureReason.InvalidSlotId, "SlotId is required.");
            if (string.IsNullOrEmpty(newDisplayName)) return SaveOperationResult.Fail(SaveFailureReason.InvalidSlotId, "New display name is required.");
            if (!Options.AllowRenaming) return SaveOperationResult.Fail(SaveFailureReason.RenamingDisabled, "Renaming is not allowed.");
            if (!_storage.Exists(slotId)) return SaveOperationResult.Fail(SaveFailureReason.SlotNotFound, "Save slot not found.");

            try
            {
                var envelope = ReadEnvelope(slotId);
                if (envelope?.Metadata == null) return SaveOperationResult.Fail(SaveFailureReason.StorageError, "Invalid save data.");
                string oldName = envelope.Metadata.DisplayName;
                envelope.Metadata.DisplayName = newDisplayName;
                WriteEnvelope(slotId, envelope);
                Renamed?.Invoke(this, new SaveSlotRenamedEventArgs(slotId, oldName, newDisplayName, envelope.Metadata.Clone()));
                return SaveOperationResult.Ok(envelope.Metadata.Clone());
            }
            catch (Exception ex)
            {
                return SaveOperationResult.Fail(SaveFailureReason.StorageError, ex.Message);
            }
        }

        public void Rename(string slotId, string newDisplayName)
        {
            var result = TryRename(slotId, newDisplayName);
            if (!result.Success)
            {
                if (result.Reason == SaveFailureReason.RenamingDisabled)
                    throw new NotSupportedException(result.Message);
                if (result.Reason == SaveFailureReason.SlotNotFound)
                    throw new FileNotFoundException(result.Message, slotId);
                throw new InvalidOperationException(result.Message);
            }
        }

        public SaveOperationResult TryDelete(string slotId)
        {
            if (string.IsNullOrEmpty(slotId)) return SaveOperationResult.Fail(SaveFailureReason.InvalidSlotId, "SlotId is required.");
            try
            {
                SaveSlotMetadata? meta = null;
                if (_storage.Exists(slotId))
                {
                    try { meta = ReadEnvelope(slotId)?.Metadata?.Clone(); } catch { }
                    _storage.Delete(slotId);
                }
                Deleted?.Invoke(this, new SaveSlotEventArgs(slotId, meta));
                return SaveOperationResult.Ok(meta);
            }
            catch (Exception ex)
            {
                return SaveOperationResult.Fail(SaveFailureReason.StorageError, ex.Message);
            }
        }

        public void Delete(string slotId)
        {
            var result = TryDelete(slotId);
            if (!result.Success) throw new InvalidOperationException(result.Message);
        }

        public SaveOperationResult TrySetMaxLoadCount(string slotId, int maxLoadCount)
        {
            if (string.IsNullOrEmpty(slotId)) return SaveOperationResult.Fail(SaveFailureReason.InvalidSlotId, "SlotId is required.");
            if (!_storage.Exists(slotId)) return SaveOperationResult.Fail(SaveFailureReason.SlotNotFound, "Save slot not found.");
            try
            {
                var envelope = ReadEnvelope(slotId);
                if (envelope?.Metadata == null) return SaveOperationResult.Fail(SaveFailureReason.StorageError, "Invalid save data.");
                envelope.Metadata.MaxLoadCount = maxLoadCount;
                WriteEnvelope(slotId, envelope);
                return SaveOperationResult.Ok(envelope.Metadata.Clone());
            }
            catch (Exception ex)
            {
                return SaveOperationResult.Fail(SaveFailureReason.StorageError, ex.Message);
            }
        }

        public void SetMaxLoadCount(string slotId, int maxLoadCount)
        {
            var result = TrySetMaxLoadCount(slotId, maxLoadCount);
            if (!result.Success)
            {
                if (result.Reason == SaveFailureReason.SlotNotFound)
                    throw new FileNotFoundException(result.Message, slotId);
                throw new InvalidOperationException(result.Message);
            }
        }

        private SaveSlotMetadata CreateNewMetadata(string slotId, string? displayName)
        {
            var now = DateTime.UtcNow;
            return new SaveSlotMetadata
            {
                SlotId = slotId,
                DisplayName = string.IsNullOrEmpty(displayName) ? slotId : displayName!,
                CreatedAtUtc = now,
                LastSavedAtUtc = now,
                LoadCount = 0,
                MaxLoadCount = Options.DefaultMaxLoadCount
            };
        }

        private int CountSlots()
        {
            int count = 0;
            foreach (var _ in _storage.ListSlotIds() ?? Array.Empty<string>()) count++;
            return count;
        }

        private SaveEnvelope ReadEnvelope(string slotId)
        {
            using (var stream = _storage.OpenRead(slotId))
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                var bytes = ms.ToArray();
                return _serializer.Deserialize<SaveEnvelope>(bytes);
            }
        }

        private void WriteEnvelope(string slotId, SaveEnvelope envelope)
        {
            byte[] bytes = _serializer.Serialize(envelope);
            using (var stream = _storage.OpenWrite(slotId))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        private static void ValidateSlotId(string slotId)
        {
            if (string.IsNullOrEmpty(slotId)) throw new ArgumentException("SlotId is required.", nameof(slotId));
        }
    }
}
