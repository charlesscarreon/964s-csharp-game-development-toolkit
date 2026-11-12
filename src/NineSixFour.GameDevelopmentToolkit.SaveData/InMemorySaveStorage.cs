using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NineSixFour.GameDevelopmentToolkit.SaveData
{
    public class InMemorySaveStorage : ISaveStorage
    {
        private readonly Dictionary<string, byte[]> _slots = new Dictionary<string, byte[]>(StringComparer.Ordinal);

        public bool Exists(string slotId) => _slots.ContainsKey(slotId);

        public Stream OpenRead(string slotId)
        {
            if (!_slots.TryGetValue(slotId, out var data))
                throw new FileNotFoundException("Slot not found", slotId);
            return new MemoryStream(data, writable: false);
        }

        public Stream OpenWrite(string slotId)
        {
            return new CommitOnDisposeStream(this, slotId);
        }

        public void Delete(string slotId)
        {
            _slots.Remove(slotId);
        }

        public IEnumerable<string> ListSlotIds()
        {
            return _slots.Keys.ToArray();
        }

        private sealed class CommitOnDisposeStream : MemoryStream
        {
            private readonly InMemorySaveStorage _owner;
            private readonly string _slotId;
            private bool _committed;

            public CommitOnDisposeStream(InMemorySaveStorage owner, string slotId)
            {
                _owner = owner;
                _slotId = slotId;
            }

            public override void Close()
            {
                Commit();
                base.Close();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing) Commit();
                base.Dispose(disposing);
            }

            private void Commit()
            {
                if (_committed) return;
                _committed = true;
                _owner._slots[_slotId] = ToArray();
            }
        }
    }
}
