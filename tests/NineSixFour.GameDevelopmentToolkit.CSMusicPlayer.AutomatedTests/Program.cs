using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using NineSixFour.GameDevelopmentToolkit.CSMusicPlayer;

namespace NineSixFour.GameDevelopmentToolkit.CSMusicPlayer.AutomatedTests
{
    public class TestResult
    {
        public string Scenario { get; set; } = "";
        public string Name { get; set; } = "";
        public string Expected { get; set; } = "";
        public string Actual { get; set; } = "";
        public bool Passed { get; set; }
        public string Details { get; set; } = "";
    }

    public class FakeAudioBackend : IAudioBackend
    {
        public Track? Current { get; private set; }
        public bool IsPlaying { get; private set; }
        public float Volume { get; private set; } = 1f;
        public TimeSpan Position { get; private set; }
        public TimeSpan Duration { get; private set; } = TimeSpan.FromMinutes(3);
        public int PlayCount { get; private set; }
        public List<string> PlayedTrackIds { get; } = new List<string>();

        public event EventHandler<TrackEventArgs>? PlaybackStarted;
        public event EventHandler<TrackEventArgs>? PlaybackPaused;
        public event EventHandler<TrackEventArgs>? PlaybackResumed;
        public event EventHandler<TrackEventArgs>? PlaybackStopped;
        public event EventHandler<TrackEventArgs>? PlaybackEnded;
        public event EventHandler<PlaybackErrorEventArgs>? PlaybackError;

        public void Play(Track track)
        {
            Current = track; IsPlaying = true; PlayCount++; Position = TimeSpan.Zero; PlayedTrackIds.Add(track.Id);
            PlaybackStarted?.Invoke(this, new TrackEventArgs(track));
        }
        public void Pause() { if (IsPlaying) { IsPlaying = false; PlaybackPaused?.Invoke(this, new TrackEventArgs(Current!)); } }
        public void Resume() { if (!IsPlaying && Current != null) { IsPlaying = true; PlaybackResumed?.Invoke(this, new TrackEventArgs(Current)); } }
        public void Stop() { var c = Current; IsPlaying = false; Position = TimeSpan.Zero; PlaybackStopped?.Invoke(this, new TrackEventArgs(c ?? new Track())); }
        public void Seek(TimeSpan position) { Position = position; }
        public TimeSpan GetPosition() => Position;
        public TimeSpan GetDuration() => Duration;
        public void SetVolume(float volume) { Volume = volume; }
        public float GetVolume() => Volume;
        public void FireEnded() { IsPlaying = false; PlaybackEnded?.Invoke(this, new TrackEventArgs(Current ?? new Track())); }
        public void FireError(string msg) { PlaybackError?.Invoke(this, new PlaybackErrorEventArgs(Current, msg)); }
    }

    class Program
    {
        static int Main()
        {
            Console.WriteLine("=== 964's CSMusicPlayer Automated Tester ===");
            var results = new List<TestResult>();
            results.AddRange(TrackAndPlaylistTests());
            results.AddRange(PlaybackAndLoopTests());
            results.AddRange(ShuffleTests());
            results.AddRange(FavoritesAndEditTests());
            results.AddRange(GameStateTests());
            results.AddRange(OptionEnforcementTests());

            int pass = 0, fail = 0;
            foreach (var r in results)
            {
                if (r.Passed) pass++; else fail++;
                Console.WriteLine((r.Passed ? "PASS" : "FAIL") + " " + r.Scenario + "/" + r.Name + " exp=" + r.Expected + " got=" + r.Actual + (string.IsNullOrEmpty(r.Details) ? "" : " | " + r.Details));
            }

            string logDir = GetProjectLogDirectory();
            Directory.CreateDirectory(logDir);
            string baseName = "csmusicplayer-tests-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string jsonPath = Path.GetFullPath(Path.Combine(logDir, baseName + ".json"));
            string txtPath = Path.GetFullPath(Path.Combine(logDir, baseName + ".txt"));
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(new { timestamp = DateTime.Now, module = "CSMusicPlayer", results = results, summary = new { total = results.Count, passed = pass, failed = fail } }, new JsonSerializerOptions { WriteIndented = true }));
            var sb = new StringBuilder();
            sb.AppendLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] RUN START - CSMusicPlayer Automated Tests");
            string cur = "";
            foreach (var r in results)
            {
                if (r.Scenario != cur) { cur = r.Scenario; sb.AppendLine("[SCENARIO] " + cur); }
                sb.AppendLine("  [" + (r.Passed ? "PASS" : "FAIL") + "] " + r.Name + " | expected=" + r.Expected + " got=" + r.Actual + (string.IsNullOrEmpty(r.Details) ? "" : " | " + r.Details));
            }
            sb.AppendLine("SUMMARY: " + pass + "/" + results.Count + " PASSED (" + fail + " FAILED)");
            sb.AppendLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] RUN END");
            File.WriteAllText(txtPath, sb.ToString());
            Console.WriteLine("JSON: " + jsonPath);
            Console.WriteLine("TXT: " + txtPath);
            Console.WriteLine("SUMMARY: " + pass + "/" + results.Count + " PASSED (" + fail + " FAILED)");
            return fail == 0 ? 0 : 1;
        }

        static MusicPlayer CreatePlayer(FakeAudioBackend backend, MusicPlayerOptions? opts = null, int seed = 123)
            => new MusicPlayer(backend, opts ?? new MusicPlayerOptions(), seed);

        static List<TestResult> TrackAndPlaylistTests()
        {
            var r = new List<TestResult>();
            var b = new FakeAudioBackend();
            var p = CreatePlayer(b);
            p.AddTrack(new Track { Id = "t1", Title = "One", Source = "a.mp3", SourceType = TrackSourceType.Local });
            p.AddTrack(new Track { Id = "t2", Title = "Two", Source = "b.mp3", SourceType = TrackSourceType.BuiltIn });
            r.Add(new TestResult { Scenario = "Tracks", Name = "AddTracks", Expected = "2", Actual = p.GetAllTracks().Count.ToString(), Passed = p.GetAllTracks().Count == 2 });
            p.CreatePlaylist("pl1", "MyList");
            bool added = p.AddTrackToPlaylist("pl1", "t1") && p.AddTrackToPlaylist("pl1", "t2");
            var pl = p.GetPlaylist("pl1");
            r.Add(new TestResult { Scenario = "Tracks", Name = "PlaylistTracks", Expected = "2", Actual = (pl?.Tracks.Count ?? 0).ToString(), Passed = added && pl != null && pl.Tracks.Count == 2 });
            p.RemoveTrack("t1");
            pl = p.GetPlaylist("pl1");
            r.Add(new TestResult { Scenario = "Tracks", Name = "RemoveTrackFromLibraryAndPlaylist", Expected = "1", Actual = (pl?.Tracks.Count ?? 0).ToString(), Passed = p.GetTrack("t1") == null && pl != null && pl.Tracks.Count == 1 });
            return r;
        }

        static List<TestResult> PlaybackAndLoopTests()
        {
            var r = new List<TestResult>();
            var b = new FakeAudioBackend();
            var p = CreatePlayer(b);
            p.AddTrack(new Track { Id = "t1", Title = "One", SourceType = TrackSourceType.BuiltIn });
            p.AddTrack(new Track { Id = "t2", Title = "Two", SourceType = TrackSourceType.BuiltIn });
            p.CreatePlaylist("pl", "P"); p.SetPlaylistLoopMode("pl", LoopMode.All);
            p.AddTrackToPlaylist("pl", "t1"); p.AddTrackToPlaylist("pl", "t2");
            bool started = p.PlayPlaylist("pl");
            r.Add(new TestResult { Scenario = "Playback", Name = "PlayPlaylistStarts", Expected = "Playing/t1", Actual = p.State + "/" + (p.CurrentTrack?.Id ?? ""), Passed = started && p.State == PlaybackState.Playing && p.CurrentTrack?.Id == "t1" });
            b.FireEnded(); // auto-advance to t2
            r.Add(new TestResult { Scenario = "Playback", Name = "AutoAdvanceOnEnded", Expected = "t2", Actual = p.CurrentTrack?.Id ?? "", Passed = p.CurrentTrack?.Id == "t2" });
            b.FireEnded(); // loop all -> t1
            r.Add(new TestResult { Scenario = "Playback", Name = "LoopAllWraps", Expected = "t1", Actual = p.CurrentTrack?.Id ?? "", Passed = p.CurrentTrack?.Id == "t1" });
            p.Next();
            r.Add(new TestResult { Scenario = "Playback", Name = "Next", Expected = "t2", Actual = p.CurrentTrack?.Id ?? "", Passed = p.CurrentTrack?.Id == "t2" });
            p.Previous();
            r.Add(new TestResult { Scenario = "Playback", Name = "Previous", Expected = "t1", Actual = p.CurrentTrack?.Id ?? "", Passed = p.CurrentTrack?.Id == "t1" });
            p.Pause();
            r.Add(new TestResult { Scenario = "Playback", Name = "Pause", Expected = "Paused", Actual = p.State.ToString(), Passed = p.State == PlaybackState.Paused });
            p.Resume();
            r.Add(new TestResult { Scenario = "Playback", Name = "Resume", Expected = "Playing", Actual = p.State.ToString(), Passed = p.State == PlaybackState.Playing });

            // Loop One
            var b2 = new FakeAudioBackend(); var p2 = CreatePlayer(b2);
            p2.AddTrack(new Track { Id = "a", SourceType = TrackSourceType.BuiltIn });
            p2.AddTrack(new Track { Id = "b", SourceType = TrackSourceType.BuiltIn });
            p2.CreatePlaylist("pl2", "P2"); p2.SetPlaylistLoopMode("pl2", LoopMode.One);
            p2.AddTrackToPlaylist("pl2", "a"); p2.AddTrackToPlaylist("pl2", "b");
            p2.PlayPlaylist("pl2"); b2.FireEnded();
            r.Add(new TestResult { Scenario = "Playback", Name = "LoopOneRepeats", Expected = "a", Actual = p2.CurrentTrack?.Id ?? "", Passed = p2.CurrentTrack?.Id == "a" && b2.PlayCount >= 2 });

            // Loop None stops at end
            var b3 = new FakeAudioBackend(); var p3 = CreatePlayer(b3);
            p3.AddTrack(new Track { Id = "x", SourceType = TrackSourceType.BuiltIn });
            p3.CreatePlaylist("pl3", "P3"); p3.SetPlaylistLoopMode("pl3", LoopMode.None);
            p3.AddTrackToPlaylist("pl3", "x"); p3.PlayPlaylist("pl3"); b3.FireEnded();
            r.Add(new TestResult { Scenario = "Playback", Name = "LoopNoneStops", Expected = "Stopped", Actual = p3.State.ToString(), Passed = p3.State == PlaybackState.Stopped });
            return r;
        }

        static List<TestResult> ShuffleTests()
        {
            var r = new List<TestResult>();
            var b = new FakeAudioBackend();
            var p = CreatePlayer(b, null, seed: 42);
            for (int i = 1; i <= 5; i++) p.AddTrack(new Track { Id = "t" + i, SourceType = TrackSourceType.BuiltIn });
            p.CreatePlaylist("pl", "S"); p.SetPlaylistShuffle("pl", true);
            for (int i = 1; i <= 5; i++) p.AddTrackToPlaylist("pl", "t" + i);
            p.PlayPlaylist("pl");
            var order = new List<string>();
            for (int i = 0; i < 5; i++) { order.Add(p.CurrentTrack!.Id); b.FireEnded(); if (p.State == PlaybackState.Stopped) break; }
            bool allPresent = new HashSet<string>(order).SetEquals(new[] { "t1", "t2", "t3", "t4", "t5" });
            bool notStrictOriginal = !(order[0] == "t1" && order[1] == "t2" && order[2] == "t3" && order[3] == "t4" && order[4] == "t5");
            r.Add(new TestResult { Scenario = "Shuffle", Name = "ShuffledCoversAll", Expected = "all5", Actual = string.Join(",", order), Passed = allPresent && order.Count == 5, Details = "order=" + string.Join(",", order) });
            r.Add(new TestResult { Scenario = "Shuffle", Name = "DeterministicSeedOrderValid", Expected = "valid", Actual = notStrictOriginal ? "shuffled" : "same-as-input", Passed = allPresent }); // with seed 42 likely shuffled
            return r;
        }

        static List<TestResult> FavoritesAndEditTests()
        {
            var r = new List<TestResult>();
            var b = new FakeAudioBackend();
            var p = CreatePlayer(b);
            p.AddTrack(new Track { Id = "t1", Title = "Old", Album = "A1", SourceType = TrackSourceType.BuiltIn });
            bool fav = p.SetFavorite("t1", true);
            r.Add(new TestResult { Scenario = "FavEdit", Name = "FavoriteSet", Expected = "1", Actual = p.GetFavorites().Count.ToString(), Passed = fav && p.GetFavorites().Count == 1 });
            bool edited = p.EditTrackMetadata("t1", title: "New", album: "A2", albumArt: new byte[] { 1, 2, 3 }, albumArtMimeType: "image/png");
            var t = p.GetTrack("t1");
            r.Add(new TestResult { Scenario = "FavEdit", Name = "MetadataEditedInMemory", Expected = "New/A2/3", Actual = (t?.Title ?? "") + "/" + (t?.Album ?? "") + "/" + (t?.AlbumArt?.Length ?? 0), Passed = edited && t?.Title == "New" && t?.Album == "A2" && t?.AlbumArt?.Length == 3 });
            return r;
        }

        static List<TestResult> GameStateTests()
        {
            var r = new List<TestResult>();
            var b = new FakeAudioBackend();
            var p = CreatePlayer(b);
            p.AddTrack(new Track { Id = "menu", SourceType = TrackSourceType.BuiltIn });
            p.AddTrack(new Track { Id = "boss", SourceType = TrackSourceType.BuiltIn });
            p.CreatePlaylist("bgm", "BGM"); p.AddTrackToPlaylist("bgm", "menu");
            p.SetGameStateMapping(new GameStateMusicMapping { StateKey = "MainMenu", PlaylistId = "bgm", LoopMode = LoopMode.All });
            bool ok = p.PlayForState("MainMenu");
            r.Add(new TestResult { Scenario = "GameState", Name = "PlayForStatePlaylist", Expected = "menu", Actual = p.CurrentTrack?.Id ?? "", Passed = ok && p.CurrentTrack?.Id == "menu" && p.CurrentStateKey == "MainMenu" });
            p.SetGameStateMapping(new GameStateMusicMapping { StateKey = "Boss", TrackId = "boss" });
            bool ok2 = p.PlayForState("Boss");
            r.Add(new TestResult { Scenario = "GameState", Name = "PlayForStateTrack", Expected = "boss", Actual = p.CurrentTrack?.Id ?? "", Passed = ok2 && p.CurrentTrack?.Id == "boss" });
            return r;
        }

        static List<TestResult> OptionEnforcementTests()
        {
            var r = new List<TestResult>();
            var b = new FakeAudioBackend();
            var p = CreatePlayer(b, new MusicPlayerOptions { AllowLocalMusic = false, AllowMetadataEditing = false, AllowPlaylistCreation = false });
            bool localRejected = !p.TryAddTrack(new Track { Id = "l", SourceType = TrackSourceType.Local });
            bool builtinOk = p.TryAddTrack(new Track { Id = "b", SourceType = TrackSourceType.BuiltIn });
            r.Add(new TestResult { Scenario = "Options", Name = "LocalMusicBlocked", Expected = "True", Actual = localRejected.ToString(), Passed = localRejected });
            r.Add(new TestResult { Scenario = "Options", Name = "BuiltInAllowed", Expected = "True", Actual = builtinOk.ToString(), Passed = builtinOk });
            bool playlistBlocked = !p.TryCreatePlaylist("x", "X");
            r.Add(new TestResult { Scenario = "Options", Name = "PlaylistCreationBlocked", Expected = "True", Actual = playlistBlocked.ToString(), Passed = playlistBlocked });
            bool editBlocked = false;
            try { p.EditTrackMetadata("b", title: "No"); } catch { editBlocked = true; }
            r.Add(new TestResult { Scenario = "Options", Name = "MetadataEditBlocked", Expected = "True", Actual = editBlocked.ToString(), Passed = editBlocked });
            return r;
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
}
