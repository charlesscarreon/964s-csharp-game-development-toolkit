using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using GameTimer = NineSixFour.GameDevelopmentToolkit.Timer.Timer;
using TimerOptions = NineSixFour.GameDevelopmentToolkit.Timer.TimerOptions;
using TimerPrecision = NineSixFour.GameDevelopmentToolkit.Timer.TimerPrecision;
using TimerDirection = NineSixFour.GameDevelopmentToolkit.Timer.TimerDirection;
using ManualTimeProvider = NineSixFour.GameDevelopmentToolkit.Timer.ManualTimeProvider;
using StopwatchTimer = NineSixFour.GameDevelopmentToolkit.Timer.Stopwatch;
using TimerManager = NineSixFour.GameDevelopmentToolkit.Timer.TimerManager;

namespace NineSixFour.GameDevelopmentToolkit.Timer.ManualTester
{
    class Program
    {
        static bool developerMode = true;
        static ManualTimeProvider timeProvider = new ManualTimeProvider();
        static TimerOptions currentOptions = new TimerOptions { Duration = TimeSpan.FromSeconds(10), Precision = TimerPrecision.Seconds, Direction = TimerDirection.CountDown };
        static GameTimer? currentTimer = null;
        static TimerManager manager = new TimerManager();
        static List<SessionEntry> sessionLog = new List<SessionEntry>();

        static void Main()
        {
            Console.WriteLine("=== 964's Timer Manual Tester ===");
            Console.WriteLine("F1: Toggle Developer/Gamer Mode | ESC: Exit");
            Console.WriteLine("Using ManualTimeProvider - control time with keys");
            Console.WriteLine();
            manager.DefaultTimeProvider = timeProvider;
            currentTimer = new GameTimer(currentOptions, timeProvider);
            while (true)
            {
                ShowMenu();
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape) break;
                if (key.Key == ConsoleKey.F1) { developerMode = !developerMode; Console.WriteLine("\nSwitched to " + (developerMode?"DEVELOPER":"GAMER") + " mode\n"); continue; }
                if (key.Key == ConsoleKey.D1) CreateCountdown();
                if (key.Key == ConsoleKey.D2) CreateStopwatch();
                if (key.Key == ConsoleKey.S) currentTimer?.Start();
                if (key.Key == ConsoleKey.P) currentTimer?.Pause();
                if (key.Key == ConsoleKey.R) currentTimer?.Resume();
                if (key.Key == ConsoleKey.T) currentTimer?.Stop();
                if (key.Key == ConsoleKey.X) currentTimer?.Reset();
                if (key.Key == ConsoleKey.A) { Console.Write("Add seconds: "); if (double.TryParse(Console.ReadLine(), out double asv)) currentTimer?.AddSeconds(asv); }
                if (key.Key == ConsoleKey.Z) { Console.Write("Subtract seconds: "); if (double.TryParse(Console.ReadLine(), out double ssv)) currentTimer?.SubtractSeconds(ssv); }
                if (key.Key == ConsoleKey.Q) timeProvider.AdvanceSeconds(1);
                if (key.Key == ConsoleKey.W) timeProvider.AdvanceSeconds(5);
                if (key.Key == ConsoleKey.E) timeProvider.AdvanceSeconds(10);
                if (key.Key == ConsoleKey.U) { currentTimer?.Update(); LogSession("Update"); ShowStatus(); }
                if (key.Key == ConsoleKey.L) SaveSessionLog();
            }
        }

        static void ShowMenu()
        {
            Console.WriteLine("Mode: " + (developerMode?"DEVELOPER":"GAMER"));
            if (currentTimer != null)
            {
                if (developerMode)
                    Console.WriteLine("State:" + currentTimer.State + " | Elapsed:" + currentTimer.Elapsed.TotalSeconds.ToString("F2") + "s | Remaining:" + currentTimer.Remaining.TotalSeconds.ToString("F2") + "s | Progress:" + currentTimer.Progress.ToString("F2") + " | Pending:" + currentTimer.PendingTime.TotalSeconds + "s");
                else
                {
                    if (currentTimer.IsCompleted) Console.WriteLine("✗ Completed");
                    else if (currentTimer.IsRunning) Console.WriteLine("⏱ Running - " + currentTimer.Value.TotalSeconds.ToString("F1") + "s");
                    else if (currentTimer.IsPaused) Console.WriteLine("⏸ Paused - " + currentTimer.Value.TotalSeconds.ToString("F1") + "s");
                    else Console.WriteLine("⏹ Idle");
                }
            }
            Console.WriteLine();
            Console.WriteLine("1. Countdown (10s)  2. Stopwatch (unlimited)");
            Console.WriteLine("S. Start  P. Pause  R. Resume  T. Stop  X. Reset");
            Console.WriteLine("A. AddTime  Z. SubtractTime");
            Console.WriteLine("Q. +1s  W. +5s  E. +10s  U. Update()");
            Console.WriteLine("L. Save Log  F1. Toggle Mode  ESC. Exit");
            Console.WriteLine();
        }

        static void CreateCountdown()
        {
            currentOptions = new TimerOptions { Duration = TimeSpan.FromSeconds(10), Precision = TimerPrecision.Seconds, Direction = TimerDirection.CountDown, AllowReviveOnTimeAdd = true };
            currentTimer = new GameTimer(currentOptions, timeProvider);
            currentTimer.Completed += (s,e) => Console.WriteLine("\n[EVENT] Completed!\n");
            currentTimer.Revived += (s,e) => Console.WriteLine("\n[EVENT] Revived!\n");
            currentTimer.TimeAdjusted += (s,e) => Console.WriteLine("\n[EVENT] Time Adjusted\n");
            Console.WriteLine("\nCreated Countdown Timer (10s)\n");
        }

        static void CreateStopwatch()
        {
            currentTimer = StopwatchTimer.CreateUnlimited(false, timeProvider);
            Console.WriteLine("\nCreated Stopwatch (count up)\n");
        }

        static void ShowStatus() { }

        static void LogSession(string action)
        {
            if (currentTimer == null) return;
            sessionLog.Add(new SessionEntry { Action = action, State = currentTimer.State.ToString(), Elapsed = currentTimer.Elapsed.TotalSeconds, Remaining = currentTimer.Remaining.TotalSeconds, Timestamp = DateTime.Now });
        }

        static void SaveSessionLog()
        {
            string logDir = GetProjectLogDirectory();
            Directory.CreateDirectory(logDir);
            string path = Path.Combine(logDir, "timer-manual-session-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".json");
            File.WriteAllText(path, JsonSerializer.Serialize(new { timestamp = DateTime.Now, entries = sessionLog }, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine("\nSession log saved: " + Path.GetFullPath(path) + " (" + sessionLog.Count + " entries)\n");
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
        public string State { get; set; } = "";
        public double Elapsed { get; set; }
        public double Remaining { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
