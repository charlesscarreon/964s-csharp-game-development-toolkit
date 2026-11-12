using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using GameTimer = NineSixFour.GameDevelopmentToolkit.Timer.Timer;
using TimerOptions = NineSixFour.GameDevelopmentToolkit.Timer.TimerOptions;
using TimerPrecision = NineSixFour.GameDevelopmentToolkit.Timer.TimerPrecision;
using TimerDirection = NineSixFour.GameDevelopmentToolkit.Timer.TimerDirection;
using TimerState = NineSixFour.GameDevelopmentToolkit.Timer.TimerState;
using ManualTimeProvider = NineSixFour.GameDevelopmentToolkit.Timer.ManualTimeProvider;
using StopwatchTimer = NineSixFour.GameDevelopmentToolkit.Timer.Stopwatch;
using TimerManager = NineSixFour.GameDevelopmentToolkit.Timer.TimerManager;

namespace NineSixFour.GameDevelopmentToolkit.Timer.AutomatedTests
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

    class Program
    {
        static int Main()
        {
            Console.WriteLine("=== 964's Timer Automated Tester ===");
            var results = new List<TestResult>();
            results.AddRange(CountdownTests());
            results.AddRange(StopwatchTests());
            results.AddRange(RuntimeAdjustTests());
            results.AddRange(PendingStackingTests());
            results.AddRange(ReviveTests());
            results.AddRange(ConcurrentTests());
            results.AddRange(PreInstantiatedTests());
            results.AddRange(EdgeCaseTests());
            int pass = 0, fail = 0;
            foreach (var r in results)
            {
                if (r.Passed) pass++; else fail++;
                Console.WriteLine((r.Passed ? "PASS" : "FAIL") + " " + r.Scenario + "/" + r.Name + " exp=" + r.Expected + " got=" + r.Actual);
            }
            string logDir = GetProjectLogDirectory();
            Directory.CreateDirectory(logDir);
            string baseName = "timer-tests-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string jsonPath = Path.GetFullPath(Path.Combine(logDir, baseName + ".json"));
            string txtPath = Path.GetFullPath(Path.Combine(logDir, baseName + ".txt"));
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(new { timestamp = DateTime.Now, module = "Timer", results = results, summary = new { total = results.Count, passed = pass, failed = fail } }, new JsonSerializerOptions { WriteIndented = true }));
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] RUN START - Timer Automated Tests");
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

        static List<TestResult> CountdownTests()
        {
            var results = new List<TestResult>();
            var mp = new ManualTimeProvider();
            var t = new GameTimer(new TimerOptions { Duration = TimeSpan.FromSeconds(10), Precision = TimerPrecision.Seconds }, mp);
            t.Start();
            mp.AdvanceSeconds(3); t.Update();
            results.Add(new TestResult { Scenario = "Countdown", Name = "ElapsedAfter3s", Expected = "~3", Actual = t.Elapsed.TotalSeconds.ToString("0.0"), Passed = t.Elapsed.TotalSeconds >= 2.9 && t.Elapsed.TotalSeconds <= 3.1 });
            mp.AdvanceSeconds(7); t.Update();
            results.Add(new TestResult { Scenario = "Countdown", Name = "CompletesAtZero", Expected = "True", Actual = t.IsCompleted.ToString(), Passed = t.IsCompleted, Details = "State=" + t.State });
            return results;
        }

        static List<TestResult> StopwatchTests()
        {
            var results = new List<TestResult>();
            var mp = new ManualTimeProvider();
            var sw = StopwatchTimer.CreateUnlimited(false, mp);
            sw.Start();
            mp.AdvanceSeconds(4); sw.Update();
            results.Add(new TestResult { Scenario = "Stopwatch", Name = "CountUp", Expected = "~4", Actual = sw.Elapsed.TotalSeconds.ToString("0.0"), Passed = sw.Elapsed.TotalSeconds >= 3.9 && sw.Elapsed.TotalSeconds <= 4.1 });
            return results;
        }

        static List<TestResult> RuntimeAdjustTests()
        {
            var results = new List<TestResult>();
            var mp = new ManualTimeProvider();
            var t = new GameTimer(new TimerOptions { Duration = TimeSpan.FromSeconds(10) }, mp);
            t.Start();
            t.AddSeconds(5);
            results.Add(new TestResult { Scenario = "RuntimeAdjust", Name = "AddTime", Expected = ">=15", Actual = t.Remaining.TotalSeconds.ToString("0"), Passed = t.Remaining.TotalSeconds >= 14.9 });
            t.SubtractSeconds(3);
            results.Add(new TestResult { Scenario = "RuntimeAdjust", Name = "SubtractTime", Expected = ">=12", Actual = t.Remaining.TotalSeconds.ToString("0"), Passed = t.Remaining.TotalSeconds >= 11.9 });
            return results;
        }

        static List<TestResult> PendingStackingTests()
        {
            var results = new List<TestResult>();
            var mp = new ManualTimeProvider();
            var t = new GameTimer(new TimerOptions { Duration = TimeSpan.FromSeconds(10) }, mp);
            t.Start();
            t.AddPendingSeconds(5); t.AddPendingSeconds(5); t.AddPendingSeconds(5);
            t.ApplyPending();
            results.Add(new TestResult { Scenario = "PendingStacking", Name = "Stack3x5s", Expected = ">=25", Actual = t.Remaining.TotalSeconds.ToString("0"), Passed = t.Remaining.TotalSeconds >= 24.9 });
            return results;
        }

        static List<TestResult> ReviveTests()
        {
            var results = new List<TestResult>();
            var mp = new ManualTimeProvider();
            var t = new GameTimer(new TimerOptions { Duration = TimeSpan.FromMilliseconds(100), AllowReviveOnTimeAdd = true }, mp);
            bool revived = false;
            t.Revived += (s, e) => revived = true;
            t.Start();
            mp.AdvanceSeconds(1); t.Update();
            t.AddSeconds(2);
            results.Add(new TestResult { Scenario = "Revive", Name = "ReviveOnAdd", Expected = "True", Actual = revived.ToString(), Passed = revived && t.State == TimerState.Running, Details = "State=" + t.State });
            return results;
        }

        static List<TestResult> ConcurrentTests()
        {
            var results = new List<TestResult>();
            var mp = new ManualTimeProvider();
            var tm = new TimerManager();
            tm.DefaultTimeProvider = mp;
            var t1 = tm.Create("a", TimeSpan.FromSeconds(5));
            var t2 = tm.Create("b", TimeSpan.FromSeconds(3));
            t1.Start(); t2.Start();
            mp.AdvanceSeconds(2); tm.Update();
            results.Add(new TestResult { Scenario = "Concurrent", Name = "BothRunning", Expected = "True", Actual = (t1.IsRunning && t2.IsRunning).ToString(), Passed = t1.IsRunning && t2.IsRunning });
            return results;
        }

        static List<TestResult> PreInstantiatedTests()
        {
            var results = new List<TestResult>();
            var mp = new ManualTimeProvider();
            var t = new GameTimer(new TimerOptions { Duration = TimeSpan.FromSeconds(5) }, mp);
            results.Add(new TestResult { Scenario = "PreInstantiated", Name = "IdleBeforeStart", Expected = "Idle", Actual = t.State.ToString(), Passed = t.State == TimerState.Idle });
            t.Start(); mp.AdvanceSeconds(1); t.Update();
            results.Add(new TestResult { Scenario = "PreInstantiated", Name = "RunsAfterStart", Expected = ">0", Actual = t.Elapsed.TotalSeconds.ToString("0.0"), Passed = t.Elapsed.TotalSeconds > 0 });
            return results;
        }

        static List<TestResult> EdgeCaseTests()
        {
            var results = new List<TestResult>();
            var mp = new ManualTimeProvider();
            var t = new GameTimer(new TimerOptions { Duration = TimeSpan.FromSeconds(4), ClampToZeroOnOverSubtract = true }, mp);
            t.Start();
            t.SubtractSeconds(5);
            results.Add(new TestResult { Scenario = "EdgeCase", Name = "OverSubtract", Expected = "True", Actual = (t.IsCompleted || t.Remaining == TimeSpan.Zero).ToString(), Passed = t.IsCompleted || t.Remaining == TimeSpan.Zero, Details = "State=" + t.State });
            return results;
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
