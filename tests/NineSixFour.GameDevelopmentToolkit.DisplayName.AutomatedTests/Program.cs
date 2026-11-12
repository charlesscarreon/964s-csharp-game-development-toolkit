using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using NineSixFour.GameDevelopmentToolkit.DisplayName;

namespace NineSixFour.GameDevelopmentToolkit.DisplayName.AutomatedTests
{
    class Program
    {
        static int Main()
        {
            Console.WriteLine("=== 964's C# Game Development Toolkit - Display Name Automated Tester ===");
            Console.WriteLine();
            
            var runId = Guid.NewGuid().ToString();
            var timestamp = DateTime.Now;
            var results = new List<TestResult>();
            
            // Run all scenarios
            results.AddRange(BasicValidation.Run());
            results.AddRange(CharacterRules.Run());
            results.AddRange(Exclusions.Run());
            results.AddRange(ScriptFiltering.Run());
            results.AddRange(SafetyDefaults.Run());
            results.AddRange(Normalization.Run());
            results.AddRange(UniversalConfig.Run());
            results.AddRange(Profiles.Run());
            results.AddRange(EdgeCases.Run());
            
            // Calculate summary
            int total = results.Count;
            int passed = 0;
            int failed = 0;
            foreach (var r in results)
            {
                if (r.Passed) passed++;
                else failed++;
            }
            
            // Create logs directory (inside project)
            string logDir = GetProjectLogDirectory();
            Directory.CreateDirectory(logDir);
            string baseName = $"displayname-tests-{timestamp:yyyyMMdd-HHmmss}";
            
            // Write JSON log (machine readable)
            string jsonPath = Path.GetFullPath(Path.Combine(logDir, baseName + ".json"));
            var logData = new
            {
                runId = runId,
                timestamp = timestamp.ToString("o"),
                module = "DisplayName",
                results = results,
                summary = new { total = total, passed = passed, failed = failed }
            };
            string json = JsonSerializer.Serialize(logData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonPath, json);
            Console.WriteLine($"JSON log written: {jsonPath}");
            
            // Write TXT log (human readable)
            string txtPath = Path.GetFullPath(Path.Combine(logDir, baseName + ".txt"));
            using (var sw = new StreamWriter(txtPath))
            {
                sw.WriteLine($"[ {timestamp:yyyy-MM-dd HH:mm:ss} ] RUN START - Display Name Automated Tests");
                sw.WriteLine($"Run ID: {runId}");
                sw.WriteLine();
                
                string currentScenario = "";
                foreach (var r in results)
                {
                    if (r.Scenario != currentScenario)
                    {
                        currentScenario = r.Scenario;
                        sw.WriteLine($"[SCENARIO] {currentScenario}");
                    }
                    string status = r.Passed ? "PASS" : "FAIL";
                    sw.WriteLine($"  [{status}] {r.Name} | input=\"{r.Input?.Replace("\"", "\\\"")}\" | expected={r.ExpectedValid} got={r.ActualValid} | errors=[{string.Join(",", r.Errors)}]");
                    if (!r.Passed)
                    {
                        sw.WriteLine($"         Details: {r.Details}");
                    }
                }
                
                sw.WriteLine();
                sw.WriteLine($"SUMMARY: {passed}/{total} PASSED ({failed} FAILED)");
                sw.WriteLine($"[ {DateTime.Now:yyyy-MM-dd HH:mm:ss} ] RUN END");
            }
            Console.WriteLine($"TXT log written: {txtPath}");
            
            Console.WriteLine();
            Console.WriteLine($"SUMMARY: {passed}/{total} PASSED ({failed} FAILED)");
            Console.WriteLine();
            
            return failed == 0 ? 0 : 1;
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
    
    public class TestResult
    {
        public string Scenario { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Input { get; set; }
        public bool ExpectedValid { get; set; }
        public bool ActualValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string? Sanitized { get; set; }
        public bool Passed { get; set; }
        public string Details { get; set; } = "";
    }
}
