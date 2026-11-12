using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using NineSixFour.GameDevelopmentToolkit.Settings;

namespace NineSixFour.GameDevelopmentToolkit.Settings.ManualTester
{
    class Program
    {
        static bool developerMode = true;
        static SettingsStore store = new SettingsStore();
        static InMemorySettingsStorage storage = new InMemorySettingsStorage();
        static List<SessionEntry> sessionLog = new List<SessionEntry>();

        static void Main()
        {
            store.Storage = storage;
            store.SettingChanged += (s, e) =>
            {
                var msg = "[EVENT] SettingChanged: " + e.Key + " '" + (e.OldValue?.ToString() ?? "null") + "' -> '" + (e.NewValue?.ToString() ?? "null") + "'";
                Console.WriteLine("\n" + msg + "\n");
                sessionLog.Add(new SessionEntry { Action = "Event", Details = msg, Timestamp = DateTime.Now });
            };

            Console.WriteLine("=== 964's Settings Manual Tester ===");
            Console.WriteLine("F1: Toggle Developer/Gamer Mode | ESC: Exit");
            Console.WriteLine();

            while (true)
            {
                ShowMenu();
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape) break;
                if (key.Key == ConsoleKey.F1) { developerMode = !developerMode; Console.WriteLine("\nSwitched to " + (developerMode ? "DEVELOPER" : "GAMER") + " mode\n"); continue; }
                if (key.Key == ConsoleKey.D1) SetString();
                if (key.Key == ConsoleKey.D2) SetInt();
                if (key.Key == ConsoleKey.D3) SetFloat();
                if (key.Key == ConsoleKey.D4) SetBool();
                if (key.Key == ConsoleKey.G) GetValue();
                if (key.Key == ConsoleKey.D) SetDefault();
                if (key.Key == ConsoleKey.R) { store.ResetToDefaults(); Log("ResetToDefaults", "ok"); Console.WriteLine("\nReset to defaults.\n"); }
                if (key.Key == ConsoleKey.K) { Console.Write("Key to remove: "); var k = Console.ReadLine() ?? ""; store.Remove(k); Log("Remove", k); }
                if (key.Key == ConsoleKey.C) { store.Clear(); Log("Clear", "ok"); Console.WriteLine("\nCleared.\n"); }
                if (key.Key == ConsoleKey.S) { store.Save(); Log("Save", "ok"); Console.WriteLine("\nSaved to InMemorySettingsStorage.\n"); }
                if (key.Key == ConsoleKey.L) { store.Load(); Log("Load", "ok"); Console.WriteLine("\nLoaded from InMemorySettingsStorage.\n"); }
                if (key.Key == ConsoleKey.V) SaveSessionLog();
            }
        }

        static void ShowMenu()
        {
            Console.WriteLine("Mode: " + (developerMode ? "DEVELOPER" : "GAMER"));
            Console.WriteLine("1. SetString  2. SetInt  3. SetFloat  4. SetBool");
            Console.WriteLine("G. Get value  D. SetDefault");
            Console.WriteLine("R. ResetToDefaults  K. Remove key  C. Clear");
            Console.WriteLine("S. Save  L. Load  V. Save Session Log");
            Console.WriteLine("F1. Toggle Mode  ESC. Exit");
            Console.WriteLine();
        }

        static void SetString()
        {
            Console.Write("Key: "); var k = Console.ReadLine() ?? "";
            Console.Write("Value: "); var v = Console.ReadLine() ?? "";
            store.SetString(k, v);
            Log("SetString", k + "=" + v);
        }

        static void SetInt()
        {
            Console.Write("Key: "); var k = Console.ReadLine() ?? "";
            Console.Write("Value (int): "); if (int.TryParse(Console.ReadLine(), out int v)) { store.SetInt(k, v); Log("SetInt", k + "=" + v); }
            else Console.WriteLine("Invalid int.");
        }

        static void SetFloat()
        {
            Console.Write("Key: "); var k = Console.ReadLine() ?? "";
            Console.Write("Value (float): "); if (float.TryParse(Console.ReadLine(), out float v)) { store.SetFloat(k, v); Log("SetFloat", k + "=" + v); }
            else Console.WriteLine("Invalid float.");
        }

        static void SetBool()
        {
            Console.Write("Key: "); var k = Console.ReadLine() ?? "";
            Console.Write("Value (true/false): "); if (bool.TryParse(Console.ReadLine(), out bool v)) { store.SetBool(k, v); Log("SetBool", k + "=" + v); }
            else Console.WriteLine("Invalid bool.");
        }

        static void SetDefault()
        {
            Console.Write("Key: "); var k = Console.ReadLine() ?? "";
            Console.Write("Default string value: "); var v = Console.ReadLine() ?? "";
            store.SetDefault(k, v);
            Log("SetDefault", k + "=" + v);
            Console.WriteLine("Default set (stored as string).");
        }

        static void GetValue()
        {
            Console.Write("Key: "); var k = Console.ReadLine() ?? "";
            if (!store.HasKey(k))
            {
                Console.WriteLine(developerMode ? "Key not found." : "Setting not found.");
                Log("Get", k + " (missing)");
                return;
            }
            var sVal = store.GetString(k, "");
            if (developerMode)
            {
                Console.WriteLine("\n--- DEVELOPER VIEW ---");
                Console.WriteLine("Key: " + k);
                Console.WriteLine("AsString: " + sVal);
                Console.WriteLine("AsInt(default -1): " + store.GetInt(k, -1));
                Console.WriteLine("AsFloat(default -1): " + store.GetFloat(k, -1));
                Console.WriteLine("AsBool(default false): " + store.GetBool(k, false));
                Console.WriteLine("----------------------\n");
            }
            else
            {
                Console.WriteLine("Value: " + sVal);
            }
            Log("Get", k + "=" + sVal);
        }

        static void Log(string action, string details)
        {
            sessionLog.Add(new SessionEntry { Action = action, Details = details, Timestamp = DateTime.Now });
        }

        static void SaveSessionLog()
        {
            string logDir = GetProjectLogDirectory();
            Directory.CreateDirectory(logDir);
            string baseName = "settings-manual-session-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string jsonPath = Path.GetFullPath(Path.Combine(logDir, baseName + ".json"));
            string txtPath = Path.GetFullPath(Path.Combine(logDir, baseName + ".txt"));

            File.WriteAllText(jsonPath, JsonSerializer.Serialize(new { timestamp = DateTime.Now, module = "SettingsManual", entries = sessionLog }, new JsonSerializerOptions { WriteIndented = true }));

            var sb = new StringBuilder();
            sb.AppendLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] Settings Manual Session");
            foreach (var e in sessionLog)
                sb.AppendLine("[" + e.Timestamp.ToString("HH:mm:ss") + "] " + e.Action + " | " + e.Details);
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
