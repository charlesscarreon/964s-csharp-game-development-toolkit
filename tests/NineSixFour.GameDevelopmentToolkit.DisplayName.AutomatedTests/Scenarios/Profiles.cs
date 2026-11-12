using System.Collections.Generic;
using NineSixFour.GameDevelopmentToolkit.DisplayName;

namespace NineSixFour.GameDevelopmentToolkit.DisplayName.AutomatedTests
{
    public class Profiles
    {
        public static List<TestResult> Run()
        {
            var results = new List<TestResult>();
            DisplayNameConfiguration.Profiles.Register("Username", new DisplayNameOptions { MinLength=3, MaxLength=16, AllowSpaces=false });
            DisplayNameConfiguration.Profiles.Register("ItemName", new DisplayNameOptions { MinLength=1, MaxLength=32, AllowSpaces=true, AllowPunctuation=true, AllowedSpecialCharacters=new char[]{'-','_','.'} });
            var vu = DisplayNameValidator.FromProfile("Username");
            var vi = DisplayNameValidator.FromProfile("ItemName");
            Test(results, "Profiles", "UsernameNoSpaces", "John Doe", false, vu);
            Test(results, "Profiles", "ItemNameSpaces", "Iron Sword", true, vi);
            return results;
        }
        static void Test(List<TestResult> results, string scenario, string name, string? input, bool expectedValid, DisplayNameValidator v)
        {
            var r = v.Validate(input);
            results.Add(new TestResult { Scenario=scenario, Name=name, Input=input, ExpectedValid=expectedValid, ActualValid=r.IsValid, Errors=r.Errors.ConvertAll(e=>e.ToString()), Sanitized=r.SanitizedDisplayName, Passed=r.IsValid==expectedValid, Details="" });
        }
    }
}
