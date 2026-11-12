using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using NineSixFour.GameDevelopmentToolkit.Settings;

namespace NineSixFour.GameDevelopmentToolkit.Settings.AutomatedTests
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
            Console.WriteLine("=== 964's Settings Automated Tester ===");
            var results = new List<TestResult>();
            results.AddRange(DefaultsTests());
            results.AddRange(TypedGetSetTests());
            results.AddRange(EventTests());
            results.AddRange(KeyManagementTests());
            results.AddRange(StorageTests());
            results.AddRange(EdgeCaseTests());

            int pass = 0, fail = 0;
            foreach (var r in results)
            {
                if (r.Passed) pass++; else fail++;
                Console.WriteLine((r.Passed ? "PASS" : "FAIL") + " " + r.Scenario + "/" + r.Name + " exp=" + r.Expected + " got=" + r.Actual);
            }

            string logDir = GetProjectLogDirectory();
            Directory.CreateDirectory(logDir);
            string baseName = "settings-tests-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string jsonPath = Path.GetFullPath(Path.Combine(logDir, baseName + ".json"));
            string txtPath = Path.GetFullPath(Path.Combine(logDir, baseName + ".txt"));

            File.WriteAllText(jsonPath, JsonSerializer.Serialize(new
            {
                timestamp = DateTime.Now,
                module = "Settings",
                results = results,
                summary = new { total = results.Count, passed = pass, failed = fail }
            }, new JsonSerializerOptions { WriteIndented = true }));

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] RUN START - Settings Automated Tests");
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

        static List<TestResult> DefaultsTests()
        {
            var results = new List<TestResult>();
            var s = new SettingsStore();
            s.SetDefault("Volume", 0.8f);
            float v = s.GetFloat("Volume", 1.0f);
            results.Add(new TestResult { Scenario = "Defaults", Name = "GetDefaultValue", Expected = "0.8", Actual = v.ToString("0.0"), Passed = Math.Abs(v - 0.8f) < 0.001f });
            s.ResetToDefaults();
            float v2 = s.GetFloat("Volume", 1.0f);
            results.Add(new TestResult { Scenario = "Defaults", Name = "ResetToDefaults", Expected = "0.8", Actual = v2.ToString("0.0"), Passed = Math.Abs(v2 - 0.8f) < 0.001f });
            return results;
        }

        static List<TestResult> TypedGetSetTests()
        {
            var results = new List<TestResult>();
            var s = new SettingsStore();
            s.SetBool("B", true);
            s.SetInt("I", 42);
            s.SetFloat("F", 1.5f);
            s.SetDouble("D", 2.5);
            s.SetString("S", "hello");
            results.Add(new TestResult { Scenario = "Typed", Name = "Bool", Expected = "True", Actual = s.GetBool("B").ToString(), Passed = s.GetBool("B") == true });
            results.Add(new TestResult { Scenario = "Typed", Name = "Int", Expected = "42", Actual = s.GetInt("I").ToString(), Passed = s.GetInt("I") == 42 });
            results.Add(new TestResult { Scenario = "Typed", Name = "Float", Expected = "1.5", Actual = s.GetFloat("F").ToString("0.0"), Passed = Math.Abs(s.GetFloat("F") - 1.5f) < 0.001f });
            results.Add(new TestResult { Scenario = "Typed", Name = "Double", Expected = "2.5", Actual = s.GetDouble("D").ToString("0.0"), Passed = Math.Abs(s.GetDouble("D") - 2.5) < 0.001 });
            results.Add(new TestResult { Scenario = "Typed", Name = "String", Expected = "hello", Actual = s.GetString("S"), Passed = s.GetString("S") == "hello" });
            return results;
        }

        static List<TestResult> EventTests()
        {
            var results = new List<TestResult>();
            var s = new SettingsStore();
            string key = ""; object? oldV = null; object? newV = null; int count = 0;
            s.SettingChanged += (sender, e) => { key = e.Key; oldV = e.OldValue; newV = e.NewValue; count++; };
            s.SetInt("X", 1);
            s.SetInt("X", 2);
            results.Add(new TestResult { Scenario = "Events", Name = "SettingChangedFired", Expected = "2", Actual = count.ToString(), Passed = count == 2 && key == "X" });
            results.Add(new TestResult { Scenario = "Events", Name = "NewValue", Expected = "2", Actual = newV?.ToString() ?? "", Passed = (newV?.ToString() ?? "") == "2" });
            return results;
        }

        static List<TestResult> KeyManagementTests()
        {
            var results = new List<TestResult>();
            var s = new SettingsStore();
            s.SetString("A", "1");
            results.Add(new TestResult { Scenario = "Keys", Name = "HasKey", Expected = "True", Actual = s.HasKey("A").ToString(), Passed = s.HasKey("A") });
            s.Remove("A");
            results.Add(new TestResult { Scenario = "Keys", Name = "Remove", Expected = "False", Actual = s.HasKey("A").ToString(), Passed = !s.HasKey("A") });
            s.SetString("B", "2"); s.SetString("C", "3");
            s.Clear();
            results.Add(new TestResult { Scenario = "Keys", Name = "Clear", Expected = "False", Actual = (s.HasKey("B") || s.HasKey("C")).ToString(), Passed = !s.HasKey("B") && !s.HasKey("C") });
            return results;
        }

        static List<TestResult> StorageTests()
        {
            var results = new List<TestResult>();
            var storage = new InMemorySettingsStorage();
            var s = new SettingsStore { Storage = storage };
            s.SetString("Name", "Player");
            s.SetInt("Score", 100);
            s.Save();

            var s2 = new SettingsStore { Storage = storage };
            s2.Load();
            results.Add(new TestResult { Scenario = "Storage", Name = "SaveLoadString", Expected = "Player", Actual = s2.GetString("Name"), Passed = s2.GetString("Name") == "Player" });
            // Note: Load stores values as strings; typed getter should convert
            results.Add(new TestResult { Scenario = "Storage", Name = "SaveLoadInt", Expected = "100", Actual = s2.GetInt("Score").ToString(), Passed = s2.GetInt("Score") == 100 });
            return results;
        }

        static List<TestResult> EdgeCaseTests()
        {
            var results = new List<TestResult>();
            var s = new SettingsStore();
            results.Add(new TestResult { Scenario = "Edge", Name = "MissingKeyDefault", Expected = "99", Actual = s.GetInt("Missing", 99).ToString(), Passed = s.GetInt("Missing", 99) == 99 });
            s.Set("Weird", "not-an-int");
            int fallback = s.GetInt("Weird", 7);
            results.Add(new TestResult { Scenario = "Edge", Name = "TypeConversionFallback", Expected = "7", Actual = fallback.ToString(), Passed = fallback == 7, Details = "stored as string 'not-an-int'" });
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
