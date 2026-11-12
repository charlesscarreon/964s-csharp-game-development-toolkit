using System.Collections.Generic;
using NineSixFour.GameDevelopmentToolkit.DisplayName;

namespace NineSixFour.GameDevelopmentToolkit.DisplayName.AutomatedTests
{
    public class Exclusions
    {
        public static List<TestResult> Run()
        {
            var results = new List<TestResult>();
            var v = new DisplayNameValidator(new DisplayNameOptions { MinLength=1, MaxLength=50, ExcludedCharacters=new char[]{'"','\\','{','}','[',']',':',','} });
            Test(results, "Exclusions", "BlockJsonBreakingChars", "{\"name\"}", false, v);
            Test(results, "Exclusions", "BlockBraces", "{test}", false, v);
            return results;
        }
        static void Test(List<TestResult> results, string scenario, string name, string? input, bool expectedValid, DisplayNameValidator v)
        {
            var r = v.Validate(input);
            results.Add(new TestResult { Scenario=scenario, Name=name, Input=input, ExpectedValid=expectedValid, ActualValid=r.IsValid, Errors=r.Errors.ConvertAll(e=>e.ToString()), Sanitized=r.SanitizedDisplayName, Passed=r.IsValid==expectedValid, Details="" });
        }
    }
}
