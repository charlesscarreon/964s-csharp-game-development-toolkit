using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using NineSixFour.GameDevelopmentToolkit.SaveData;

namespace NineSixFour.GameDevelopmentToolkit.SaveData.AutomatedTests
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

    public class JsonSaveSerializer : ISaveSerializer
    {
        private static readonly JsonSerializerOptions Opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IncludeFields = true };
        public byte[] Serialize<T>(T data) => JsonSerializer.SerializeToUtf8Bytes(data, Opts);
        public T Deserialize<T>(byte[] data) => JsonSerializer.Deserialize<T>(data, Opts)!;
    }

    public class PlayerData
    {
        public string Name { get; set; } = "";
        public int Level { get; set; }
        public float Hp { get; set; }
    }

    class Program
    {
        static int Main()
        {
            Console.WriteLine("=== 964's SaveData Automated Tester ===");
            var results = new List<TestResult>();
            results.AddRange(BasicSaveLoadTests());
            results.AddRange(MaxSlotsTests());
            results.AddRange(RenameTests());
            results.AddRange(LoadLimitTests());
            results.AddRange(MetadataPersistenceTests());
            results.AddRange(EventTests());

            int pass = 0, fail = 0;
            foreach (var r in results)
            {
                if (r.Passed) pass++; else fail++;
                Console.WriteLine((r.Passed ? "PASS" : "FAIL") + " " + r.Scenario + "/" + r.Name + " exp=" + r.Expected + " got=" + r.Actual + (string.IsNullOrEmpty(r.Details) ? "" : " | " + r.Details));
            }

            string logDir = GetProjectLogDirectory();
            Directory.CreateDirectory(logDir);
            string baseName = "savedata-tests-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string jsonPath = Path.GetFullPath(Path.Combine(logDir, baseName + ".json"));
            string txtPath = Path.GetFullPath(Path.Combine(logDir, baseName + ".txt"));

            File.WriteAllText(jsonPath, JsonSerializer.Serialize(new { timestamp = DateTime.Now, module = "SaveData", results = results, summary = new { total = results.Count, passed = pass, failed = fail } }, new JsonSerializerOptions { WriteIndented = true }));

            var sb = new StringBuilder();
            sb.AppendLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] RUN START - SaveData Automated Tests");
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

        static List<TestResult> BasicSaveLoadTests()
        {
            var results = new List<TestResult>();
            var storage = new InMemorySaveStorage();
            var mgr = new SaveManager(storage, new JsonSaveSerializer(), new SaveDataOptions { MaxSlots = 5 });
            var data = new PlayerData { Name = "Hero", Level = 7, Hp = 42.5f };
            var saveRes = mgr.TrySave("slot1", data, "First Save");
            results.Add(new TestResult { Scenario = "Basic", Name = "TrySaveSuccess", Expected = "True", Actual = saveRes.Success.ToString(), Passed = saveRes.Success });
            results.Add(new TestResult { Scenario = "Basic", Name = "Exists", Expected = "True", Actual = mgr.Exists("slot1").ToString(), Passed = mgr.Exists("slot1") });

            var loadRes = mgr.TryLoad<PlayerData>("slot1");
            bool ok = loadRes.Success && loadRes.Data != null && loadRes.Data.Name == "Hero" && loadRes.Data.Level == 7 && Math.Abs(loadRes.Data.Hp - 42.5f) < 0.001f;
            results.Add(new TestResult { Scenario = "Basic", Name = "TryLoadData", Expected = "Hero/7/42.5", Actual = loadRes.Success && loadRes.Data != null ? (loadRes.Data.Name + "/" + loadRes.Data.Level + "/" + loadRes.Data.Hp) : "fail", Passed = ok });
            results.Add(new TestResult { Scenario = "Basic", Name = "LoadCountIncremented", Expected = "1", Actual = (loadRes.Metadata?.LoadCount ?? -1).ToString(), Passed = (loadRes.Metadata?.LoadCount ?? -1) == 1 });

            // Overwrite existing slot
            var data2 = new PlayerData { Name = "Hero2", Level = 8, Hp = 50f };
            mgr.Save("slot1", data2);
            var loaded2 = mgr.Load<PlayerData>("slot1");
            results.Add(new TestResult { Scenario = "Basic", Name = "OverwriteSlot", Expected = "Hero2", Actual = loaded2.Name, Passed = loaded2.Name == "Hero2" && loaded2.Level == 8 });

            var missing = mgr.TryLoad<PlayerData>("nope");
            results.Add(new TestResult { Scenario = "Basic", Name = "MissingSlotFails", Expected = "SlotNotFound", Actual = missing.Reason.ToString(), Passed = !missing.Success && missing.Reason == SaveFailureReason.SlotNotFound });
            return results;
        }

        static List<TestResult> MaxSlotsTests()
        {
            var results = new List<TestResult>();
            var storage = new InMemorySaveStorage();
            var mgr = new SaveManager(storage, new JsonSaveSerializer(), new SaveDataOptions { MaxSlots = 2 });
            mgr.Save("a", new PlayerData { Name = "A" });
            mgr.Save("b", new PlayerData { Name = "B" });
            var third = mgr.TrySave("c", new PlayerData { Name = "C" });
            results.Add(new TestResult { Scenario = "MaxSlots", Name = "RejectThirdNewSlot", Expected = "MaxSlotsReached", Actual = third.Reason.ToString(), Passed = !third.Success && third.Reason == SaveFailureReason.MaxSlotsReached });
            // Overwriting existing should be allowed even at limit
            var overwrite = mgr.TrySave("a", new PlayerData { Name = "A2" });
            results.Add(new TestResult { Scenario = "MaxSlots", Name = "OverwriteAtLimitAllowed", Expected = "True", Actual = overwrite.Success.ToString(), Passed = overwrite.Success });
            return results;
        }

        static List<TestResult> RenameTests()
        {
            var results = new List<TestResult>();
            var storage = new InMemorySaveStorage();
            var mgr = new SaveManager(storage, new JsonSaveSerializer(), new SaveDataOptions { AllowRenaming = true });
            mgr.Save("s1", new PlayerData { Name = "X" }, "Old");
            var ren = mgr.TryRename("s1", "New Name");
            var meta = mgr.GetMetadata("s1");
            results.Add(new TestResult { Scenario = "Rename", Name = "RenameAllowed", Expected = "New Name", Actual = meta?.DisplayName ?? "", Passed = ren.Success && meta?.DisplayName == "New Name" });

            var mgr2 = new SaveManager(new InMemorySaveStorage(), new JsonSaveSerializer(), new SaveDataOptions { AllowRenaming = false });
            mgr2.Save("s1", new PlayerData { Name = "X" });
            var ren2 = mgr2.TryRename("s1", "Nope");
            results.Add(new TestResult { Scenario = "Rename", Name = "RenameDisabled", Expected = "RenamingDisabled", Actual = ren2.Reason.ToString(), Passed = !ren2.Success && ren2.Reason == SaveFailureReason.RenamingDisabled });
            return results;
        }

        static List<TestResult> LoadLimitTests()
        {
            var results = new List<TestResult>();
            var storage = new InMemorySaveStorage();
            var mgr = new SaveManager(storage, new JsonSaveSerializer(), new SaveDataOptions { DefaultMaxLoadCount = 2, AutoDeleteOnLoadLimitReached = true });
            mgr.Save("s1", new PlayerData { Name = "L" });
            var l1 = mgr.TryLoad<PlayerData>("s1");
            var l2 = mgr.TryLoad<PlayerData>("s1");
            var l3 = mgr.TryLoad<PlayerData>("s1"); // limit reached (2 loads already consumed)
            results.Add(new TestResult { Scenario = "LoadLimit", Name = "FirstTwoLoadsOk", Expected = "True/True", Actual = l1.Success + "/" + l2.Success, Passed = l1.Success && l2.Success });
            results.Add(new TestResult { Scenario = "LoadLimit", Name = "ThirdLoadBlocked", Expected = "LoadLimitReached", Actual = l3.Reason.ToString(), Passed = !l3.Success && l3.Reason == SaveFailureReason.LoadLimitReached });
            results.Add(new TestResult { Scenario = "LoadLimit", Name = "AutoDeleted", Expected = "True", Actual = l3.SlotDeletedDueToLoadLimit.ToString() + "/" + (!mgr.Exists("s1")), Passed = l3.SlotDeletedDueToLoadLimit && !mgr.Exists("s1") });

            // Per-slot override
            var storage2 = new InMemorySaveStorage();
            var mgr2 = new SaveManager(storage2, new JsonSaveSerializer(), new SaveDataOptions { DefaultMaxLoadCount = -1, AutoDeleteOnLoadLimitReached = false });
            mgr2.Save("s2", new PlayerData { Name = "M" });
            mgr2.SetMaxLoadCount("s2", 1);
            var a = mgr2.TryLoad<PlayerData>("s2");
            var b = mgr2.TryLoad<PlayerData>("s2");
            results.Add(new TestResult { Scenario = "LoadLimit", Name = "PerSlotLimit", Expected = "True/LoadLimitReached", Actual = a.Success + "/" + b.Reason, Passed = a.Success && !b.Success && b.Reason == SaveFailureReason.LoadLimitReached && mgr2.Exists("s2") });
            return results;
        }

        static List<TestResult> MetadataPersistenceTests()
        {
            var results = new List<TestResult>();
            var storage = new InMemorySaveStorage();
            var ser = new JsonSaveSerializer();
            var mgr = new SaveManager(storage, ser, new SaveDataOptions { DefaultMaxLoadCount = 5 });
            mgr.Save("s1", new PlayerData { Name = "P" }, "Persisted");
            mgr.Load<PlayerData>("s1"); // load count 1

            // New manager same storage/serializer should see metadata
            var mgr2 = new SaveManager(storage, ser, new SaveDataOptions { DefaultMaxLoadCount = 5 });
            var meta = mgr2.GetMetadata("s1");
            results.Add(new TestResult { Scenario = "Persistence", Name = "DisplayNamePersisted", Expected = "Persisted", Actual = meta?.DisplayName ?? "", Passed = meta?.DisplayName == "Persisted" });
            results.Add(new TestResult { Scenario = "Persistence", Name = "LoadCountPersisted", Expected = "1", Actual = (meta?.LoadCount ?? -1).ToString(), Passed = meta?.LoadCount == 1 });
            var slots = mgr2.GetAllSlots();
            results.Add(new TestResult { Scenario = "Persistence", Name = "GetAllSlots", Expected = "1", Actual = slots.Count.ToString(), Passed = slots.Count == 1 && slots[0].SlotId == "s1" });
            return results;
        }

        static List<TestResult> EventTests()
        {
            var results = new List<TestResult>();
            var storage = new InMemorySaveStorage();
            var mgr = new SaveManager(storage, new JsonSaveSerializer(), new SaveDataOptions { DefaultMaxLoadCount = 1, AutoDeleteOnLoadLimitReached = true, AllowRenaming = true });
            int saved = 0, loaded = 0, deleted = 0, renamed = 0, limited = 0;
            mgr.Saved += (s, e) => saved++;
            mgr.Loaded += (s, e) => loaded++;
            mgr.Deleted += (s, e) => deleted++;
            mgr.Renamed += (s, e) => renamed++;
            mgr.LoadLimitReached += (s, e) => limited++;
            mgr.Save("e1", new PlayerData { Name = "E" });
            mgr.Rename("e1", "RenamedE");
            mgr.Load<PlayerData>("e1");
            mgr.TryLoad<PlayerData>("e1"); // triggers limit+delete
            results.Add(new TestResult { Scenario = "Events", Name = "SavedFired", Expected = "1", Actual = saved.ToString(), Passed = saved == 1 });
            results.Add(new TestResult { Scenario = "Events", Name = "RenamedFired", Expected = "1", Actual = renamed.ToString(), Passed = renamed == 1 });
            results.Add(new TestResult { Scenario = "Events", Name = "LoadedFired", Expected = "1", Actual = loaded.ToString(), Passed = loaded == 1 });
            results.Add(new TestResult { Scenario = "Events", Name = "LoadLimitReachedFired", Expected = "1", Actual = limited.ToString(), Passed = limited == 1 });
            results.Add(new TestResult { Scenario = "Events", Name = "DeletedFired", Expected = "1", Actual = deleted.ToString(), Passed = deleted >= 1 });
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
