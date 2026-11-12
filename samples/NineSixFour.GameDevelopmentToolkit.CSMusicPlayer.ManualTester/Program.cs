using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using NineSixFour.GameDevelopmentToolkit.CSMusicPlayer;

namespace NineSixFour.GameDevelopmentToolkit.CSMusicPlayer.ManualTester
{
    public class FakeAudioBackend : IAudioBackend
    {
        public Track? Current { get; private set; }
        public bool IsPlaying { get; private set; }
        public float Volume { get; private set; } = 1f;
        public TimeSpan Position { get; private set; }
        public TimeSpan Duration { get; private set; } = TimeSpan.FromMinutes(3);

        public event EventHandler<TrackEventArgs>? PlaybackStarted;
        public event EventHandler<TrackEventArgs>? PlaybackPaused;
        public event EventHandler<TrackEventArgs>? PlaybackResumed;
        public event EventHandler<TrackEventArgs>? PlaybackStopped;
        public event EventHandler<TrackEventArgs>? PlaybackEnded;
        public event EventHandler<PlaybackErrorEventArgs>? PlaybackError;

        public void Play(Track track) { Current = track; IsPlaying = true; Position = TimeSpan.Zero; PlaybackStarted?.Invoke(this, new TrackEventArgs(track)); }
        public void Pause() { if (IsPlaying) { IsPlaying = false; PlaybackPaused?.Invoke(this, new TrackEventArgs(Current!)); } }
        public void Resume() { if (!IsPlaying && Current != null) { IsPlaying = true; PlaybackResumed?.Invoke(this, new TrackEventArgs(Current)); } }
        public void Stop() { var c = Current; IsPlaying = false; Position = TimeSpan.Zero; PlaybackStopped?.Invoke(this, new TrackEventArgs(c ?? new Track())); }
        public void Seek(TimeSpan position) { Position = position; }
        public TimeSpan GetPosition() => Position;
        public TimeSpan GetDuration() => Duration;
        public void SetVolume(float volume) { Volume = volume; }
        public float GetVolume() => Volume;
        public void FireEnded() { IsPlaying = false; PlaybackEnded?.Invoke(this, new TrackEventArgs(Current ?? new Track())); }
    }

    class Program
    {
        static FakeAudioBackend backend = new FakeAudioBackend();
        static MusicPlayer player = new MusicPlayer(backend, new MusicPlayerOptions(), 42);
        static List<SessionEntry> sessionLog = new List<SessionEntry>();
        static bool developerMode = true;

        static void Main()
        {
            player.TrackChanged += (s, e) => LogEvent("TrackChanged", e.Track?.Id + " - " + e.Track?.Title);
            player.PlaybackStateChanged += (s, e) => LogEvent("PlaybackState", player.State.ToString());
            player.PlaybackError += (s, e) => LogEvent("PlaybackError", e.Message);

            Console.WriteLine("=== 964's CSMusicPlayer Manual Tester ===");
            Console.WriteLine("F1: Toggle Developer/Gamer Mode | ESC: Exit");
            Console.WriteLine();

            while (true)
            {
                ShowMenu();
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape) break;
                if (key.Key == ConsoleKey.F1) { developerMode = !developerMode; Console.WriteLine("\nSwitched to " + (developerMode ? "DEVELOPER" : "GAMER") + " mode\n"); continue; }
                try
                {
                    if (key.Key == ConsoleKey.D1) AddTrack();
                    else if (key.Key == ConsoleKey.D2) ListTracks();
                    else if (key.Key == ConsoleKey.D3) CreatePlaylist();
                    else if (key.Key == ConsoleKey.D4) AddTrackToPlaylist();
                    else if (key.Key == ConsoleKey.D5) ListPlaylists();
                    else if (key.Key == ConsoleKey.D6) PlayTrack();
                    else if (key.Key == ConsoleKey.D7) PlayPlaylist();
                    else if (key.Key == ConsoleKey.N) { player.Next(); Log("Next", player.CurrentTrack?.Id ?? ""); }
                    else if (key.Key == ConsoleKey.B) { player.Previous(); Log("Previous", player.CurrentTrack?.Id ?? ""); }
                    else if (key.Key == ConsoleKey.P) { player.Pause(); Log("Pause", ""); }
                    else if (key.Key == ConsoleKey.R) { player.Resume(); Log("Resume", ""); }
                    else if (key.Key == ConsoleKey.S) { player.Stop(); Log("Stop", ""); }
                    else if (key.Key == ConsoleKey.E) { backend.FireEnded(); Log("FireEnded", "simulated track end"); Console.WriteLine("\n[Simulated track end]\n"); }
                    else if (key.Key == ConsoleKey.F) ToggleFavorite();
                    else if (key.Key == ConsoleKey.M) EditMetadata();
                    else if (key.Key == ConsoleKey.H) ToggleShuffle();
                    else if (key.Key == ConsoleKey.L) SetLoop();
                    else if (key.Key == ConsoleKey.G) MapAndPlayState();
                    else if (key.Key == ConsoleKey.V) SaveSessionLog();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nError: " + ex.Message + "\n");
                    Log("Error", ex.Message);
                }
            }
        }

        static void ShowMenu()
        {
            Console.WriteLine("Mode: " + (developerMode ? "DEVELOPER" : "GAMER") + " | State: " + player.State + " | Track: " + (player.CurrentTrack?.Title ?? player.CurrentTrack?.Id ?? "-") + " | Playlist: " + (player.ActivePlaylist?.Name ?? "-"));
            Console.WriteLine("1.AddTrack 2.ListTracks 3.CreatePlaylist 4.AddToPlaylist 5.ListPlaylists");
            Console.WriteLine("6.PlayTrack 7.PlayPlaylist N.Next B.Prev P.Pause R.Resume S.Stop E.FireEnded");
            Console.WriteLine("F.Favorite M.EditMeta H.ToggleShuffle L.SetLoop G.GameState V.SaveLog F1.Mode ESC.Exit");
            Console.WriteLine();
        }

        static void AddTrack()
        {
            Console.Write("TrackId: "); var id = Console.ReadLine() ?? "";
            Console.Write("Title: "); var title = Console.ReadLine() ?? id;
            Console.Write("Source (path/uri): "); var src = Console.ReadLine() ?? "";
            Console.Write("Type (1=Local,2=BuiltIn): "); var t = Console.ReadLine();
            var type = t == "2" ? TrackSourceType.BuiltIn : TrackSourceType.Local;
            var track = new Track { Id = id, Title = title, Source = src, SourceType = type, Album = "Album", Artist = "Artist" };
            bool ok = player.TryAddTrack(track);
            Console.WriteLine(ok ? "Track added." : "Failed to add track (check options/type).");
            Log("AddTrack", id + " " + ok);
            Console.WriteLine();
        }

        static void ListTracks()
        {
            Console.WriteLine("\n--- Tracks ---");
            foreach (var tr in player.GetAllTracks())
                Console.WriteLine((developerMode ? $"[{tr.Id}] {tr.Title} ({tr.SourceType}) src={tr.Source} fav={tr.IsFavorite} album={tr.Album}" : $"- {tr.Title}" + (tr.IsFavorite ? " ★" : "")));
            Console.WriteLine("--------------\n");
        }

        static void CreatePlaylist()
        {
            Console.Write("PlaylistId: "); var id = Console.ReadLine() ?? "";
            Console.Write("Name: "); var name = Console.ReadLine() ?? id;
            bool ok = player.TryCreatePlaylist(id, name);
            Console.WriteLine(ok ? "Playlist created." : "Failed to create playlist.");
            Log("CreatePlaylist", id + " " + ok);
            Console.WriteLine();
        }

        static void AddTrackToPlaylist()
        {
            Console.Write("PlaylistId: "); var pid = Console.ReadLine() ?? "";
            Console.Write("TrackId: "); var tid = Console.ReadLine() ?? "";
            bool ok = player.AddTrackToPlaylist(pid, tid);
            Console.WriteLine(ok ? "Added to playlist." : "Failed.");
            Log("AddToPlaylist", pid + "<-" + tid + " " + ok);
            Console.WriteLine();
        }

        static void ListPlaylists()
        {
            Console.WriteLine("\n--- Playlists ---");
            foreach (var pl in player.GetAllPlaylists())
            {
                Console.WriteLine($"[{pl.Id}] {pl.Name} tracks={pl.Tracks.Count} loop={pl.LoopMode} shuffle={pl.ShuffleEnabled}");
                if (developerMode) foreach (var tr in pl.Tracks) Console.WriteLine("   - " + tr.Id + ": " + tr.Title);
            }
            Console.WriteLine("-----------------\n");
        }

        static void PlayTrack() { Console.Write("TrackId: "); var id = Console.ReadLine() ?? ""; bool ok = player.PlayTrack(id); Console.WriteLine(ok ? "Playing track." : "Failed."); Log("PlayTrack", id + " " + ok); Console.WriteLine(); }
        static void PlayPlaylist() { Console.Write("PlaylistId: "); var id = Console.ReadLine() ?? ""; bool ok = player.PlayPlaylist(id); Console.WriteLine(ok ? "Playing playlist." : "Failed."); Log("PlayPlaylist", id + " " + ok); Console.WriteLine(); }

        static void ToggleFavorite()
        {
            Console.Write("TrackId: "); var id = Console.ReadLine() ?? "";
            var t = player.GetTrack(id); if (t == null) { Console.WriteLine("Not found."); return; }
            player.SetFavorite(id, !t.IsFavorite);
            Console.WriteLine("Favorite=" + (!t.IsFavorite));
            Log("Favorite", id + "=" + (!t.IsFavorite));
            Console.WriteLine();
        }

        static void EditMetadata()
        {
            Console.Write("TrackId: "); var id = Console.ReadLine() ?? "";
            Console.Write("New Title (blank skip): "); var title = Console.ReadLine();
            Console.Write("New Album (blank skip): "); var album = Console.ReadLine();
            bool ok = player.EditTrackMetadata(id, string.IsNullOrEmpty(title) ? null : title, string.IsNullOrEmpty(album) ? null : album, albumArt: Encoding.UTF8.GetBytes("ART"), albumArtMimeType: "text/plain", writeBack: false);
            Console.WriteLine(ok ? "Edited (in-memory)." : "Failed.");
            Log("EditMeta", id + " " + ok);
            Console.WriteLine();
        }

        static void ToggleShuffle()
        {
            Console.Write("PlaylistId: "); var id = Console.ReadLine() ?? "";
            var pl = player.GetPlaylist(id); if (pl == null) { Console.WriteLine("Not found."); return; }
            player.SetPlaylistShuffle(id, !pl.ShuffleEnabled);
            Console.WriteLine("Shuffle=" + (!pl.ShuffleEnabled));
            Log("Shuffle", id + "=" + (!pl.ShuffleEnabled));
            Console.WriteLine();
        }

        static void SetLoop()
        {
            Console.Write("PlaylistId: "); var id = Console.ReadLine() ?? "";
            Console.Write("Loop (0=None,1=One,2=All): "); var s = Console.ReadLine();
            LoopMode mode = s == "1" ? LoopMode.One : s == "2" ? LoopMode.All : LoopMode.None;
            bool ok = player.SetPlaylistLoopMode(id, mode);
            Console.WriteLine(ok ? "Loop=" + mode : "Failed.");
            Log("Loop", id + "=" + mode);
            Console.WriteLine();
        }

        static void MapAndPlayState()
        {
            Console.Write("StateKey: "); var key = Console.ReadLine() ?? "";
            Console.Write("PlaylistId (blank for none): "); var pid = Console.ReadLine();
            Console.Write("TrackId (blank for none): "); var tid = Console.ReadLine();
            player.SetGameStateMapping(new GameStateMusicMapping { StateKey = key, PlaylistId = string.IsNullOrEmpty(pid) ? null : pid, TrackId = string.IsNullOrEmpty(tid) ? null : tid, LoopMode = LoopMode.All, Shuffle = false });
            bool ok = player.PlayForState(key);
            Console.WriteLine(ok ? "Playing for state." : "Failed to play for state.");
            Log("PlayState", key + " " + ok);
            Console.WriteLine();
        }

        static void Log(string action, string details) => sessionLog.Add(new SessionEntry { Action = action, Details = details, Timestamp = DateTime.Now });
        static void LogEvent(string action, string details) { Console.WriteLine($"\n[EVENT] {action}: {details}\n"); Log("Event:" + action, details); }

        static void SaveSessionLog()
        {
            string logDir = GetProjectLogDirectory();
            Directory.CreateDirectory(logDir);
            string baseName = "csmusicplayer-manual-session-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string jsonPath = Path.GetFullPath(Path.Combine(logDir, baseName + ".json"));
            string txtPath = Path.GetFullPath(Path.Combine(logDir, baseName + ".txt"));
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(new { timestamp = DateTime.Now, module = "CSMusicPlayerManual", entries = sessionLog }, new JsonSerializerOptions { WriteIndented = true }));
            var sb = new StringBuilder();
            sb.AppendLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] CSMusicPlayer Manual Session");
            foreach (var e in sessionLog) sb.AppendLine("[" + e.Timestamp.ToString("HH:mm:ss") + "] " + e.Action + " | " + e.Details);
            sb.AppendLine("END (" + sessionLog.Count + " entries)");
            File.WriteAllText(txtPath, sb.ToString());
            Console.WriteLine("\nSession logs saved:\n  " + jsonPath + "\n  " + txtPath + " (" + sessionLog.Count + " entries)\n");
        }

        private static string GetProjectLogDirectory()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            for (int i = 0; i < 10 && dir != null; i++)
            {
                if (File.Exists(Path.Combine(dir.FullName, "964s-csharp-game-development-toolkit.sln")))
                    return Path.Combine(dir.FullName, "logs");
                dir = dir.Parent;
            }
            dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            for (int i = 0; i < 10 && dir != null; i++)
            {
                if (File.Exists(Path.Combine(dir.FullName, "964s-csharp-game-development-toolkit.sln")))
                    return Path.Combine(dir.FullName, "logs");
                dir = dir.Parent;
            }
            return Path.Combine(Directory.GetCurrentDirectory(), "logs");
        }
    }

    class SessionEntry
    {
        public string Action { get; set; } = "";
        public string Details { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
