using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using NineSixFour.GameDevelopmentToolkit.DisplayName;

namespace NineSixFour.GameDevelopmentToolkit.DisplayName.ManualTester
{
    class Program
    {
        static bool developerMode = true;
        static DisplayNameOptions currentOptions = new DisplayNameOptions();
        static DisplayNameValidator? currentValidator = null;
        static List<SessionEntry> sessionLog = new List<SessionEntry>();

        static void Main()
        {
            Console.WriteLine("=== 964's Display Name Manual Tester ===");
            Console.WriteLine("Toggle between DEVELOPER and GAMER modes");
            Console.WriteLine("Press F1 to toggle mode, ESC to exit, or use menu");
            Console.WriteLine();
            
            LoadQuickProfile("Default");
            currentValidator = new DisplayNameValidator(currentOptions);
            
            while (true)
            {
                ShowMenu();
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape) break;
                if (key.Key == ConsoleKey.F1) { developerMode = !developerMode; Console.WriteLine($"\nSwitched to {(developerMode?"DEVELOPER":"GAMER")} mode\n"); continue; }
                if (key.Key == ConsoleKey.D1) LoadQuickProfile("Default");
                if (key.Key == ConsoleKey.D2) LoadQuickProfile("Username");
                if (key.Key == ConsoleKey.D3) LoadQuickProfile("ItemName");
                if (key.Key == ConsoleKey.D4) LoadQuickProfile("NPCName");
                if (key.Key == ConsoleKey.D5) LoadQuickProfile("Description");
                if (key.Key == ConsoleKey.C) ConfigureOptions();
                if (key.Key == ConsoleKey.T) TestMode();
                if (key.Key == ConsoleKey.S) SaveSessionLog();
                if (key.Key == ConsoleKey.U) UseUniversalDefault();
            }
        }
        
        static void ShowMenu()
        {
            Console.WriteLine($"Mode: {(developerMode ? "DEVELOPER" : "GAMER")}");
            Console.WriteLine($"Current: Min={currentOptions.MinLength} Max={currentOptions.MaxLength} AllowSpaces={currentOptions.AllowSpaces} AllowLetters={currentOptions.AllowLetters} AllowDigits={currentOptions.AllowDigits}");
            Console.WriteLine();
            Console.WriteLine("1. Default Profile");
            Console.WriteLine("2. Username Profile (strict)");
            Console.WriteLine("3. ItemName Profile (lenient)");
            Console.WriteLine("4. NPCName Profile");
            Console.WriteLine("5. Description Profile");
            Console.WriteLine("C. Configure Options Manually");
            Console.WriteLine("T. Test Input");
            Console.WriteLine("U. Use Universal Default (DisplayNameConfiguration.Default)");
            Console.WriteLine("S. Save Session Log");
            Console.WriteLine("F1. Toggle Mode (Developer/Gamer)");
            Console.WriteLine("ESC. Exit");
            Console.WriteLine();
        }
        
        static void LoadQuickProfile(string profile)
        {
            if (profile == "Username")
                currentOptions = new DisplayNameOptions { MinLength=3, MaxLength=16, AllowSpaces=false, AllowPunctuation=false, AllowLetters=true, AllowDigits=true };
            else if (profile == "ItemName")
                currentOptions = new DisplayNameOptions { MinLength=1, MaxLength=32, AllowSpaces=true, AllowPunctuation=true, AllowLetters=true, AllowDigits=true, AllowedSpecialCharacters=new char[]{'-','_','.'} };
            else if (profile == "NPCName")
                currentOptions = new DisplayNameOptions { MinLength=1, MaxLength=24, AllowSpaces=true, AllowPunctuation=false, AllowLetters=true, AllowDigits=true };
            else if (profile == "Description")
                currentOptions = new DisplayNameOptions { MinLength=0, MaxLength=256, AllowSpaces=true, AllowPunctuation=true, AllowLetters=true, AllowDigits=true, AllowSymbols=false };
            else
                currentOptions = new DisplayNameOptions();
            currentValidator = new DisplayNameValidator(currentOptions);
            Console.WriteLine($"\nLoaded {profile} profile\n");
        }
        
        static void UseUniversalDefault()
        {
            currentOptions = DisplayNameConfiguration.Default.Clone();
            currentValidator = new DisplayNameValidator(currentOptions);
            Console.WriteLine("\nUsing Universal Default (DisplayNameConfiguration.Default)\n");
        }
        
        static void ConfigureOptions()
        {
            Console.WriteLine("\n=== Configure Options ===");
            Console.Write("MinLength (current {0}): ", currentOptions.MinLength);
            string s = Console.ReadLine();
            if (int.TryParse(s, out int min)) currentOptions.MinLength = min;
            Console.Write("MaxLength (current {0}): ", currentOptions.MaxLength);
            s = Console.ReadLine();
            if (int.TryParse(s, out int max)) currentOptions.MaxLength = max;
            Console.Write("AllowSpaces (y/n, current {0}): ", currentOptions.AllowSpaces);
            s = Console.ReadLine();
            if (s?.ToLower() == "y") currentOptions.AllowSpaces = true;
            if (s?.ToLower() == "n") currentOptions.AllowSpaces = false;
            Console.Write("AllowPunctuation (y/n, current {0}): ", currentOptions.AllowPunctuation);
            s = Console.ReadLine();
            if (s?.ToLower() == "y") currentOptions.AllowPunctuation = true;
            if (s?.ToLower() == "n") currentOptions.AllowPunctuation = false;
            Console.Write("AllowDigits (y/n, current {0}): ", currentOptions.AllowDigits);
            s = Console.ReadLine();
            if (s?.ToLower() == "y") currentOptions.AllowDigits = true;
            if (s?.ToLower() == "n") currentOptions.AllowDigits = false;
            Console.Write("ExcludedCharacters (comma separated, e.g. \",\\,{,}): ");
            s = Console.ReadLine();
            if (!string.IsNullOrEmpty(s))
            {
                var list = new List<char>();
                foreach (var part in s.Split(','))
                {
                    if (part.Length == 1) list.Add(part[0]);
                    else if (part == "\\\"" || part == "\"") list.Add('"');
                    else if (part == "\\\\") list.Add('\\');
                }
                currentOptions.ExcludedCharacters = list.ToArray();
            }
            currentValidator = new DisplayNameValidator(currentOptions);
            Console.WriteLine("\nOptions updated\n");
        }
        
        static void TestMode()
        {
            Console.WriteLine($"\n=== Test Mode ({(developerMode?"DEVELOPER":"GAMER")}) ===");
            Console.WriteLine("Type input to validate (empty to exit):");
            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input) && input == null) break;
                if (input == "") break;
                
                var result = currentValidator!.Validate(input);
                if (developerMode)
                {
                    Console.WriteLine("\n--- DEVELOPER VIEW ---");
                    Console.WriteLine($"Input: {input}");
                    Console.WriteLine($"Normalized: {input.Normalize(System.Text.NormalizationForm.FormC)}");
                    Console.WriteLine($"Sanitized: {result.SanitizedDisplayName}");
                    Console.WriteLine($"IsValid: {result.IsValid}");
                    Console.WriteLine($"Errors ({result.Errors.Count}):");
                    foreach (var e in result.Errors) Console.WriteLine($"  - {e}");
                    Console.WriteLine($"Options: Min={currentOptions.MinLength} Max={currentOptions.MaxLength} AllowSpaces={currentOptions.AllowSpaces} AllowLetters={currentOptions.AllowLetters} AllowDigits={currentOptions.AllowDigits}");
                    Console.WriteLine("----------------------\n");
                }
                else
                {
                    Console.WriteLine("\n--- GAMER VIEW ---");
                    if (result.IsValid)
                        Console.WriteLine($"✓ Valid! Your display name: {result.SanitizedDisplayName}");
                    else
                    {
                        Console.WriteLine("✗ Invalid display name");
                        Console.WriteLine("Please try again with different characters.");
                    }
                    Console.WriteLine("------------------\n");
                }
                sessionLog.Add(new SessionEntry { Input = input, IsValid = result.IsValid, Errors = result.Errors.ConvertAll(e=>e.ToString()), Sanitized = result.SanitizedDisplayName, Mode = developerMode?"Developer":"Gamer", Timestamp = DateTime.Now });
            }
            Console.WriteLine("\nExited test mode\n");
        }
        
        static void SaveSessionLog()
        {
            string logDir = GetProjectLogDirectory();
            Directory.CreateDirectory(logDir);
            string path = Path.Combine(logDir, $"manual-session-{DateTime.Now:yyyyMMdd-HHmmss}.json");
            string json = JsonSerializer.Serialize(new { timestamp = DateTime.Now, entries = sessionLog }, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
            Console.WriteLine($"\nSession log saved: {Path.GetFullPath(path)} ({sessionLog.Count} entries)\n");
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
        public string? Input { get; set; }
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string? Sanitized { get; set; }
        public string Mode { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
