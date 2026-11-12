using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using NineSixFour.GameDevelopmentToolkit.SaveData;

namespace NineSixFour.GameDevelopmentToolkit.SaveData.ManualTester
{
    public class JsonSaveSerializer : ISaveSerializer
    {
        private static readonly JsonSerializerOptions Opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IncludeFields = true, WriteIndented = false };
        public byte[] Serialize<T>(T data) => JsonSerializer.SerializeToUtf8Bytes(data, Opts);
        public T Deserialize<T>(byte[] data) => JsonSerializer.Deserialize<T>(data, Opts)!;
    }

    public class PlayerData
    {
        public string Name { get; set; } = "";
        public int Level { get; set; }
        public float Hp { get; set; }
        public override string ToString() => $"Name={Name}, Level={Level}, Hp={Hp}";
    }

    class Program
    {
        static SaveDataOptions options = new SaveDataOptions { MaxSlots = 5, AllowRenaming = true, DefaultMaxLoadCount = -1, AutoDeleteOnLoadLimitReached = true, AllowFreeSave = true, AllowFreeLoad = true };
        static InMemorySaveStorage storage = new InMemorySaveStorage();
        static SaveManager manager = new SaveManager(storage, new JsonSaveSerializer(), options);
        static List<SessionEntry> sessionLog = new List<SessionEntry>();
        static bool developerMode = true;

        static void Main()
        {
            manager.Saved += (s, e) => LogEvent("Saved", e.SlotId);
            manager.Loaded += (s, e) => LogEvent("Loaded", e.SlotId);
            manager.Deleted += (s, e) => LogEvent("Deleted", e.SlotId);
            manager.Renamed += (s, e) => LogEvent("Renamed", e.SlotId + " '" + e.OldDisplayName + "'->'" + e.NewDisplayName + "'");
            manager.LoadLimitReached += (s, e) => LogEvent("LoadLimitReached", e.SlotId + " deleted=" + e.SlotDeleted);

            Console.WriteLine("=== 964's SaveData Manual Tester ===");
            Console.WriteLine("F1: Toggle Developer/Gamer Mode | ESC: Exit");
            Console.WriteLine();

            while (true)
            {
                ShowMenu();
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape) break;
                if (key.Key == ConsoleKey.F1) { developerMode = !developerMode; Console.WriteLine("\nSwitched to " + (developerMode ? "DEVELOPER" : "GAMER") + " mode\n"); continue; }
                if (key.Key == ConsoleKey.D1) DoSave();
                else if (key.Key == ConsoleKey.D2) DoLoad();
                else if (key.Key == ConsoleKey.D3) DoList();
                else if (key.Key == ConsoleKey.D4) DoRename();
                else if (key.Key == ConsoleKey.D5) DoDelete();
                else if (key.Key == ConsoleKey.D6) DoSetLoadLimit();
                else if (key.Key == ConsoleKey.D7) ShowMeta();
                else if (key.Key == ConsoleKey.O) ShowOptions();
                else if (key.Key == ConsoleKey.L) SaveSessionLog();
            }
        }

        static void ShowMenu()
        {
            Console.WriteLine("Mode: " + (developerMode ? "DEVELOPER" : "GAMER") + " | Slots: " + CountSlots() + "/" + (options.MaxSlots < 0 ? "∞" : options.MaxSlots.ToString()));
            Console.WriteLine("1.Save  2.Load  3.List  4.Rename  5.Delete  6.SetLoadLimit  7.Metadata  O.Options  L.SaveLog  F1.Mode  ESC.Exit");
            Console.WriteLine();
        }

        static int CountSlots() { int c = 0; foreach (var _ in manager.GetAllSlots()) c++; return c; }

        static void DoSave()
        {
            Console.Write("SlotId: "); var id = Console.ReadLine() ?? "";
            Console.Write("DisplayName (optional): "); var dn = Console.ReadLine();
            Console.Write("Player Name: "); var name = Console.ReadLine() ?? "";
            Console.Write("Level (int): "); int.TryParse(Console.ReadLine(), out int lvl);
            Console.Write("Hp (float): "); float.TryParse(Console.ReadLine(), out float hp);
            var data = new PlayerData { Name = name, Level = lvl, Hp = hp };
            var res = manager.TrySave(id, data, string.IsNullOrEmpty(dn) ? null : dn);
            if (res.Success)
            {
                Console.WriteLine(developerMode ? $"Saved. Meta: loads={res.Metadata?.LoadCount} max={res.Metadata?.MaxLoadCount} display='{res.Metadata?.DisplayName}'" : "✓ Saved!");
                Log("Save", id + " ok");
            }
            else
            {
                Console.WriteLine("Save failed: " + res.Reason + " - " + res.Message);
                Log("Save", id + " fail " + res.Reason);
            }
            Console.WriteLine();
        }

        static void DoLoad()
        {
            Console.Write("SlotId: "); var id = Console.ReadLine() ?? "";
            var res = manager.TryLoad<PlayerData>(id);
            if (res.Success)
            {
                if (developerMode)
                    Console.WriteLine($"Loaded: {res.Data} | LoadCount={res.Metadata?.LoadCount}/{res.Metadata?.MaxLoadCount}");
                else
                    Console.WriteLine("✓ Loaded: " + res.Data);
                Log("Load", id + " ok");
            }
            else
            {
                Console.WriteLine("Load failed: " + res.Reason + " - " + res.Message + (res.SlotDeletedDueToLoadLimit ? " (slot deleted)" : ""));
                Log("Load", id + " fail " + res.Reason);
            }
            Console.WriteLine();
        }

        static void DoList()
        {
            Console.WriteLine("\n--- Slots ---");
            foreach (var m in manager.GetAllSlots())
            {
                if (developerMode)
                    Console.WriteLine($"[{m.SlotId}] '{m.DisplayName}' loads={m.LoadCount}/{m.MaxLoadCount} saved={m.LastSavedAtUtc:o}");
                else
                    Console.WriteLine($"- {m.DisplayName} ({m.SlotId})");
            }
            Console.WriteLine("-------------\n");
            Log("List", CountSlots().ToString());
        }

        static void DoRename()
        {
            Console.Write("SlotId: "); var id = Console.ReadLine() ?? "";
            Console.Write("New Display Name: "); var nn = Console.ReadLine() ?? "";
            var res = manager.TryRename(id, nn);
            Console.WriteLine(res.Success ? "Renamed." : ("Rename failed: " + res.Reason + " - " + res.Message));
            Log("Rename", id + " " + res.Success);
            Console.WriteLine();
        }

        static void DoDelete()
        {
            Console.Write("SlotId: "); var id = Console.ReadLine() ?? "";
            var res = manager.TryDelete(id);
            Console.WriteLine(res.Success ? "Deleted (if existed)." : ("Delete failed: " + res.Reason));
            Log("Delete", id);
            Console.WriteLine();
        }

        static void DoSetLoadLimit()
        {
            Console.Write("SlotId: "); var id = Console.ReadLine() ?? "";
            Console.Write("MaxLoadCount (-1 unlimited): "); int.TryParse(Console.ReadLine(), out int c);
            var res = manager.TrySetMaxLoadCount(id, c);
            Console.WriteLine(res.Success ? "Load limit set." : ("Failed: " + res.Reason + " - " + res.Message));
            Log("SetLoadLimit", id + "=" + c + " " + res.Success);
            Console.WriteLine();
        }

        static void ShowMeta()
        {
            Console.Write("SlotId: "); var id = Console.ReadLine() ?? "";
            var m = manager.GetMetadata(id);
            if (m == null) { Console.WriteLine("Not found."); Log("Meta", id + " missing"); Console.WriteLine(); return; }
            Console.WriteLine($"SlotId={m.SlotId} DisplayName={m.DisplayName} Created={m.CreatedAtUtc:o} Saved={m.LastSavedAtUtc:o} LoadCount={m.LoadCount} MaxLoadCount={m.MaxLoadCount}");
            Log("Meta", id);
            Console.WriteLine();
        }

        static void ShowOptions()
        {
            Console.WriteLine("\n--- Options ---");
            Console.WriteLine($"MaxSlots={options.MaxSlots} AllowRenaming={options.AllowRenaming} AllowFreeSave={options.AllowFreeSave} AllowFreeLoad={options.AllowFreeLoad} DefaultMaxLoadCount={options.DefaultMaxLoadCount} AutoDeleteOnLoadLimitReached={options.AutoDeleteOnLoadLimitReached}");
            Console.WriteLine("---------------\n");
        }

        static void Log(string action, string details) => sessionLog.Add(new SessionEntry { Action = action, Details = details, Timestamp = DateTime.Now });
        static void LogEvent(string action, string details) { Console.WriteLine($"\n[EVENT] {action}: {details}\n"); Log("Event:" + action, details); }

        static void SaveSessionLog()
        {
            string logDir = GetProjectLogDirectory();
            Directory.CreateDirectory(logDir);
            string baseName = "savedata-manual-session-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string jsonPath = Path.GetFullPath(Path.Combine(logDir, baseName + ".json"));
            string txtPath = Path.GetFullPath(Path.Combine(logDir, baseName + ".txt"));

            File.WriteAllText(jsonPath, JsonSerializer.Serialize(new { timestamp = DateTime.Now, module = "SaveDataManual", entries = sessionLog }, new JsonSerializerOptions { WriteIndented = true }));
            var sb = new StringBuilder();
            sb.AppendLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] SaveData Manual Session");
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
