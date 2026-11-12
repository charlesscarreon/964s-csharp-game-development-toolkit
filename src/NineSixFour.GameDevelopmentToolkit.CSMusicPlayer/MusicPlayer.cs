using System;
using System.Collections.Generic;
using System.Linq;

namespace NineSixFour.GameDevelopmentToolkit.CSMusicPlayer
{
    public class GameStateMusicMapping
    {
        public string StateKey { get; set; } = "";
        /// <summary>Playlist to play for this state (optional if TrackId is set).</summary>
        public string? PlaylistId { get; set; }
        /// <summary>Specific track to play for this state (optional).</summary>
        public string? TrackId { get; set; }
        public bool Shuffle { get; set; }
        public LoopMode LoopMode { get; set; } = LoopMode.All;
    }

    public class MusicPlayerEventArgs : EventArgs
    {
        public Track? Track { get; }
        public Playlist? Playlist { get; }
        public string? StateKey { get; }
        public MusicPlayerEventArgs(Track? track, Playlist? playlist = null, string? stateKey = null)
        {
            Track = track; Playlist = playlist; StateKey = stateKey;
        }
    }

    public class MusicPlayer
    {
        private readonly Dictionary<string, Track> _tracks = new Dictionary<string, Track>(StringComparer.Ordinal);
        private readonly Dictionary<string, Playlist> _playlists = new Dictionary<string, Playlist>(StringComparer.Ordinal);
        private readonly Dictionary<string, GameStateMusicMapping> _stateMappings = new Dictionary<string, GameStateMusicMapping>(StringComparer.OrdinalIgnoreCase);
        private readonly Random _random;
        private readonly List<int> _playOrder = new List<int>(); // indices into active playlist.Tracks
        private int _playOrderPos = -1;
        private bool _suppressBackendEnded;

        public MusicPlayerOptions Options { get; }
        public IAudioBackend Backend { get; }
        public IMusicTagReader? TagReader { get; set; }
        public IMusicTagWriter? TagWriter { get; set; }

        public Playlist? ActivePlaylist { get; private set; }
        public Track? CurrentTrack { get; private set; }
        public PlaybackState State { get; private set; } = PlaybackState.Stopped;
        public string? CurrentStateKey { get; private set; }

        public event EventHandler<MusicPlayerEventArgs>? TrackChanged;
        public event EventHandler<MusicPlayerEventArgs>? PlaybackStateChanged;
        public event EventHandler<PlaybackErrorEventArgs>? PlaybackError;

        public MusicPlayer(IAudioBackend backend, MusicPlayerOptions? options = null, int? shuffleSeed = null)
        {
            Backend = backend ?? throw new ArgumentNullException(nameof(backend));
            Options = options ?? new MusicPlayerOptions();
            _random = shuffleSeed.HasValue ? new Random(shuffleSeed.Value) : new Random();
            Backend.PlaybackEnded += Backend_PlaybackEnded;
            Backend.PlaybackError += Backend_PlaybackError;
            Backend.PlaybackStarted += (s, e) => { /* state driven by core */ };
        }

        // --- Track library ---
        public Track AddTrack(Track track, bool readTags = false)
        {
            if (track == null) throw new ArgumentNullException(nameof(track));
            if (string.IsNullOrEmpty(track.Id)) throw new ArgumentException("Track.Id required", nameof(track));
            if (track.SourceType == TrackSourceType.Local && !Options.AllowLocalMusic)
                throw new InvalidOperationException("Local music is not allowed.");
            if (track.SourceType == TrackSourceType.BuiltIn && !Options.AllowBuiltInMusic)
                throw new InvalidOperationException("Built-in music is not allowed.");
            var t = track.Clone();
            if (readTags && TagReader != null && !string.IsNullOrEmpty(t.Source))
            {
                try { ApplyMetadata(t, TagReader.Read(t.Source)); } catch { }
            }
            _tracks[t.Id] = t;
            return t.Clone();
        }

        public bool TryAddTrack(Track track, bool readTags = false)
        {
            try { AddTrack(track, readTags); return true; } catch { return false; }
        }

        public bool RemoveTrack(string trackId)
        {
            if (!_tracks.Remove(trackId)) return false;
            foreach (var pl in _playlists.Values)
                pl.Tracks.RemoveAll(t => t.Id == trackId);
            if (CurrentTrack?.Id == trackId) Stop();
            RebuildPlayOrder();
            return true;
        }

        public Track? GetTrack(string trackId) => _tracks.TryGetValue(trackId, out var t) ? t.Clone() : null;
        public IReadOnlyCollection<Track> GetAllTracks() => _tracks.Values.Select(t => t.Clone()).ToArray();
        public IReadOnlyCollection<Track> GetFavorites() => _tracks.Values.Where(t => t.IsFavorite).Select(t => t.Clone()).ToArray();

        public bool SetFavorite(string trackId, bool favorite)
        {
            if (!_tracks.TryGetValue(trackId, out var t)) return false;
            t.IsFavorite = favorite;
            SyncTrackInPlaylists(t);
            if (CurrentTrack?.Id == trackId) CurrentTrack.IsFavorite = favorite;
            return true;
        }

        public bool EditTrackMetadata(string trackId, string? title = null, string? album = null, byte[]? albumArt = null, string? albumArtMimeType = null, bool writeBack = false)
        {
            if (!Options.AllowMetadataEditing) throw new InvalidOperationException("Metadata editing is not allowed.");
            if (!_tracks.TryGetValue(trackId, out var t)) return false;
            if (title != null) t.Title = title;
            if (album != null) t.Album = album;
            if (albumArt != null) { t.AlbumArt = (byte[])albumArt.Clone(); t.AlbumArtMimeType = albumArtMimeType; }
            SyncTrackInPlaylists(t);
            if (CurrentTrack?.Id == trackId) { if (title != null) CurrentTrack.Title = title; if (album != null) CurrentTrack.Album = album; if (albumArt != null) { CurrentTrack.AlbumArt = (byte[])albumArt.Clone(); CurrentTrack.AlbumArtMimeType = albumArtMimeType; } }
            if (writeBack && TagWriter != null && !string.IsNullOrEmpty(t.Source))
            {
                try
                {
                    TagWriter.Write(t.Source, new TrackMetadata { Title = t.Title, Artist = t.Artist, Album = t.Album, AlbumArt = t.AlbumArt, AlbumArtMimeType = t.AlbumArtMimeType, Duration = t.Duration });
                }
                catch { }
            }
            return true;
        }

        private void ApplyMetadata(Track t, TrackMetadata? m)
        {
            if (m == null) return;
            if (!string.IsNullOrEmpty(m.Title)) t.Title = m.Title!;
            if (!string.IsNullOrEmpty(m.Artist)) t.Artist = m.Artist!;
            if (!string.IsNullOrEmpty(m.Album)) t.Album = m.Album!;
            if (m.AlbumArt != null) t.AlbumArt = m.AlbumArt;
            if (!string.IsNullOrEmpty(m.AlbumArtMimeType)) t.AlbumArtMimeType = m.AlbumArtMimeType;
            if (m.Duration.HasValue) t.Duration = m.Duration;
        }

        private void SyncTrackInPlaylists(Track t)
        {
            foreach (var pl in _playlists.Values)
                for (int i = 0; i < pl.Tracks.Count; i++)
                    if (pl.Tracks[i].Id == t.Id) pl.Tracks[i] = t.Clone();
        }

        // --- Playlists ---
        public Playlist CreatePlaylist(string id, string name)
        {
            if (!Options.AllowPlaylistCreation) throw new InvalidOperationException("Playlist creation is not allowed.");
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("id required", nameof(id));
            if (_playlists.ContainsKey(id)) throw new InvalidOperationException("Playlist already exists.");
            var pl = new Playlist { Id = id, Name = string.IsNullOrEmpty(name) ? id : name, LoopMode = Options.DefaultLoopMode, ShuffleEnabled = Options.DefaultShuffleEnabled };
            _playlists[id] = pl;
            return ClonePlaylist(pl);
        }

        public bool TryCreatePlaylist(string id, string name)
        {
            try { CreatePlaylist(id, name); return true; } catch { return false; }
        }

        public bool RenamePlaylist(string id, string newName)
        {
            if (!Options.AllowPlaylistRenaming) return false;
            if (!_playlists.TryGetValue(id, out var pl)) return false;
            pl.Name = newName ?? id;
            return true;
        }

        public bool DeletePlaylist(string id)
        {
            if (!Options.AllowPlaylistDeletion) return false;
            if (!_playlists.Remove(id)) return false;
            if (ActivePlaylist?.Id == id) { Stop(); ActivePlaylist = null; _playOrder.Clear(); _playOrderPos = -1; }
            return true;
        }

        public Playlist? GetPlaylist(string id) => _playlists.TryGetValue(id, out var pl) ? ClonePlaylist(pl) : null;
        public IReadOnlyCollection<Playlist> GetAllPlaylists() => _playlists.Values.Select(ClonePlaylist).ToArray();

        public bool AddTrackToPlaylist(string playlistId, string trackId)
        {
            if (!_playlists.TryGetValue(playlistId, out var pl)) return false;
            if (!_tracks.TryGetValue(trackId, out var t)) return false;
            if (t.SourceType == TrackSourceType.Local && !Options.AllowLocalMusic) return false;
            if (t.SourceType == TrackSourceType.BuiltIn && !Options.AllowBuiltInMusic) return false;
            pl.Tracks.Add(t.Clone());
            if (ActivePlaylist?.Id == playlistId) RebuildPlayOrder(keepCurrent: true);
            return true;
        }

        public bool RemoveTrackFromPlaylist(string playlistId, string trackId)
        {
            if (!_playlists.TryGetValue(playlistId, out var pl)) return false;
            int removed = pl.Tracks.RemoveAll(t => t.Id == trackId);
            if (removed > 0 && ActivePlaylist?.Id == playlistId) RebuildPlayOrder(keepCurrent: true);
            return removed > 0;
        }

        public bool SetPlaylistLoopMode(string playlistId, LoopMode mode)
        {
            if (!_playlists.TryGetValue(playlistId, out var pl)) return false;
            pl.LoopMode = mode;
            return true;
        }

        public bool SetPlaylistShuffle(string playlistId, bool enabled)
        {
            if (!_playlists.TryGetValue(playlistId, out var pl)) return false;
            pl.ShuffleEnabled = enabled;
            if (ActivePlaylist?.Id == playlistId) RebuildPlayOrder(keepCurrent: true);
            return true;
        }

        private static Playlist ClonePlaylist(Playlist pl)
        {
            return new Playlist { Id = pl.Id, Name = pl.Name, LoopMode = pl.LoopMode, ShuffleEnabled = pl.ShuffleEnabled, Tracks = pl.Tracks.Select(t => t.Clone()).ToList() };
        }

        // --- Game state mappings ---
        public void SetGameStateMapping(GameStateMusicMapping mapping)
        {
            if (mapping == null || string.IsNullOrEmpty(mapping.StateKey)) throw new ArgumentException("StateKey required");
            _stateMappings[mapping.StateKey] = mapping;
        }

        public bool RemoveGameStateMapping(string stateKey) => _stateMappings.Remove(stateKey);
        public GameStateMusicMapping? GetGameStateMapping(string stateKey) => _stateMappings.TryGetValue(stateKey, out var m) ? m : null;

        public bool PlayForState(string stateKey)
        {
            if (!_stateMappings.TryGetValue(stateKey, out var m)) return false;
            CurrentStateKey = stateKey;
            if (!string.IsNullOrEmpty(m.TrackId))
            {
                if (!string.IsNullOrEmpty(m.PlaylistId))
                {
                    PlayPlaylist(m.PlaylistId!, startTrackId: m.TrackId);
                    return true;
                }
                // play single track (optionally in ad-hoc context)
                return PlayTrack(m.TrackId!);
            }
            if (!string.IsNullOrEmpty(m.PlaylistId))
            {
                if (_playlists.TryGetValue(m.PlaylistId!, out var pl))
                {
                    pl.ShuffleEnabled = m.Shuffle;
                    pl.LoopMode = m.LoopMode;
                }
                return PlayPlaylist(m.PlaylistId!);
            }
            return false;
        }

        // --- Playback control (core drives backend) ---
        public bool PlayTrack(string trackId)
        {
            if (!_tracks.TryGetValue(trackId, out var t)) return false;
            if (!CanPlayTrack(t)) return false;
            ActivePlaylist = null;
            _playOrder.Clear();
            _playOrderPos = -1;
            StartTrackInternal(t.Clone());
            return true;
        }

        public bool PlayPlaylist(string playlistId, string? startTrackId = null)
        {
            if (!_playlists.TryGetValue(playlistId, out var pl)) return false;
            ActivePlaylist = pl;
            RebuildPlayOrder();
            if (_playOrder.Count == 0) return false;
            int startPos = 0;
            if (!string.IsNullOrEmpty(startTrackId))
            {
                for (int i = 0; i < _playOrder.Count; i++)
                {
                    if (pl.Tracks[_playOrder[i]].Id == startTrackId) { startPos = i; break; }
                }
            }
            _playOrderPos = startPos;
            var track = pl.Tracks[_playOrder[_playOrderPos]].Clone();
            if (!CanPlayTrack(track)) return TryAdvanceToPlayable(+1, allowWrap: true);
            StartTrackInternal(track);
            return true;
        }

        public void Pause()
        {
            if (State != PlaybackState.Playing) return;
            try { Backend.Pause(); } catch (Exception ex) { RaiseBackendError(CurrentTrack, ex.Message, ex); }
            State = PlaybackState.Paused;
            PlaybackStateChanged?.Invoke(this, new MusicPlayerEventArgs(CurrentTrack, ActivePlaylist != null ? ClonePlaylist(ActivePlaylist) : null, CurrentStateKey));
        }

        public void Resume()
        {
            if (State != PlaybackState.Paused) return;
            try { Backend.Resume(); } catch (Exception ex) { RaiseBackendError(CurrentTrack, ex.Message, ex); return; }
            State = PlaybackState.Playing;
            PlaybackStateChanged?.Invoke(this, new MusicPlayerEventArgs(CurrentTrack, ActivePlaylist != null ? ClonePlaylist(ActivePlaylist) : null, CurrentStateKey));
        }

        public void Stop()
        {
            _suppressBackendEnded = true;
            try { Backend.Stop(); } catch { }
            _suppressBackendEnded = false;
            State = PlaybackState.Stopped;
            PlaybackStateChanged?.Invoke(this, new MusicPlayerEventArgs(CurrentTrack, ActivePlaylist != null ? ClonePlaylist(ActivePlaylist) : null, CurrentStateKey));
        }

        public bool Next()
        {
            if (ActivePlaylist == null) return false;
            if (ActivePlaylist.LoopMode == LoopMode.One)
            {
                // manual next breaks repeat-one typical behavior? go to next track
                return TryAdvanceToPlayable(+1, allowWrap: ActivePlaylist.LoopMode == LoopMode.All);
            }
            return TryAdvanceToPlayable(+1, allowWrap: ActivePlaylist.LoopMode == LoopMode.All);
        }

        public bool Previous()
        {
            if (ActivePlaylist == null) return false;
            // If playing for a while, some players restart track; core keeps simple: go to previous
            return TryAdvanceToPlayable(-1, allowWrap: ActivePlaylist.LoopMode == LoopMode.All);
        }

        public void Seek(TimeSpan position)
        {
            try { Backend.Seek(position); } catch (Exception ex) { RaiseBackendError(CurrentTrack, ex.Message, ex); }
        }

        private void StartTrackInternal(Track track)
        {
            CurrentTrack = track;
            try
            {
                Backend.Play(track.Clone());
            }
            catch (Exception ex)
            {
                State = PlaybackState.Stopped;
                RaiseBackendError(track, ex.Message, ex);
                return;
            }
            State = PlaybackState.Playing;
            TrackChanged?.Invoke(this, new MusicPlayerEventArgs(track.Clone(), ActivePlaylist != null ? ClonePlaylist(ActivePlaylist) : null, CurrentStateKey));
            PlaybackStateChanged?.Invoke(this, new MusicPlayerEventArgs(track.Clone(), ActivePlaylist != null ? ClonePlaylist(ActivePlaylist) : null, CurrentStateKey));
        }

        private bool TryAdvanceToPlayable(int dir, bool allowWrap)
        {
            if (ActivePlaylist == null || _playOrder.Count == 0) return false;
            int start = _playOrderPos;
            int pos = _playOrderPos;
            for (int attempts = 0; attempts < _playOrder.Count; attempts++)
            {
                pos += dir;
                if (pos < 0 || pos >= _playOrder.Count)
                {
                    if (!allowWrap) return false;
                    pos = pos < 0 ? _playOrder.Count - 1 : 0;
                }
                var track = ActivePlaylist.Tracks[_playOrder[pos]].Clone();
                if (CanPlayTrack(track))
                {
                    _playOrderPos = pos;
                    StartTrackInternal(track);
                    return true;
                }
            }
            return false;
        }

        private bool CanPlayTrack(Track t)
        {
            if (t.SourceType == TrackSourceType.Local && !Options.AllowLocalMusic) return false;
            if (t.SourceType == TrackSourceType.BuiltIn && !Options.AllowBuiltInMusic) return false;
            return true;
        }

        private void RebuildPlayOrder(bool keepCurrent = false)
        {
            string? currentId = (keepCurrent && ActivePlaylist != null && _playOrderPos >= 0 && _playOrderPos < _playOrder.Count) ? ActivePlaylist.Tracks[_playOrder[_playOrderPos]].Id : null;
            _playOrder.Clear();
            _playOrderPos = -1;
            if (ActivePlaylist == null) return;
            for (int i = 0; i < ActivePlaylist.Tracks.Count; i++) _playOrder.Add(i);
            if (ActivePlaylist.ShuffleEnabled && _playOrder.Count > 1)
            {
                // Fisher-Yates
                for (int i = _playOrder.Count - 1; i > 0; i--)
                {
                    int j = _random.Next(i + 1);
                    int tmp = _playOrder[i]; _playOrder[i] = _playOrder[j]; _playOrder[j] = tmp;
                }
            }
            if (currentId != null)
            {
                for (int i = 0; i < _playOrder.Count; i++)
                    if (ActivePlaylist.Tracks[_playOrder[i]].Id == currentId) { _playOrderPos = i; break; }
            }
        }

        private void Backend_PlaybackEnded(object? sender, TrackEventArgs e)
        {
            if (_suppressBackendEnded) return;
            if (ActivePlaylist == null)
            {
                State = PlaybackState.Stopped;
                PlaybackStateChanged?.Invoke(this, new MusicPlayerEventArgs(CurrentTrack, null, CurrentStateKey));
                return;
            }
            if (ActivePlaylist.LoopMode == LoopMode.One && CurrentTrack != null)
            {
                StartTrackInternal(CurrentTrack.Clone());
                return;
            }
            bool advanced = TryAdvanceToPlayable(+1, allowWrap: ActivePlaylist.LoopMode == LoopMode.All);
            if (!advanced)
            {
                State = PlaybackState.Stopped;
                PlaybackStateChanged?.Invoke(this, new MusicPlayerEventArgs(CurrentTrack, ClonePlaylist(ActivePlaylist), CurrentStateKey));
            }
        }

        private void Backend_PlaybackError(object? sender, PlaybackErrorEventArgs e)
        {
            RaiseBackendError(e.Track ?? CurrentTrack, e.Message, e.Exception);
        }

        private void RaiseBackendError(Track? track, string message, Exception? ex = null)
        {
            PlaybackError?.Invoke(this, new PlaybackErrorEventArgs(track, message, ex));
        }
    }
}
