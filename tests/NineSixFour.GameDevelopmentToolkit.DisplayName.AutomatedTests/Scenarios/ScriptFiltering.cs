using System.Collections.Generic;
using NineSixFour.GameDevelopmentToolkit.DisplayName;

namespace NineSixFour.GameDevelopmentToolkit.DisplayName.AutomatedTests
{
    public class ScriptFiltering
    {
        public static List<TestResult> Run()
        {
            var results = new List<TestResult>();
            var v = new DisplayNameValidator(new DisplayNameOptions { MinLength=1, MaxLength=20, ExcludedScripts = new System.Collections.Generic.HashSet<string>{"Hangul"} });
            // Hangul char
            Test(results, "ScriptFiltering", "BlockHangul", "테스트", false, v);
            var v2 = new DisplayNameValidator(new DisplayNameOptions { MinLength=1, MaxLength=20 });
            Test(results, "ScriptFiltering", "AllowLatin", "Player", true, v2);
            return results;
        }
        static void Test(List<TestResult> results, string scenario, string name, string? input, bool expectedValid, DisplayNameValidator v)
        {
            var r = v.Validate(input);
            results.Add(new TestResult { Scenario=scenario, Name=name, Input=input, ExpectedValid=expectedValid, ActualValid=r.IsValid, Errors=r.Errors.ConvertAll(e=>e.ToString()), Sanitized=r.SanitizedDisplayName, Passed=r.IsValid==expectedValid, Details="" });
        }
    }
}
