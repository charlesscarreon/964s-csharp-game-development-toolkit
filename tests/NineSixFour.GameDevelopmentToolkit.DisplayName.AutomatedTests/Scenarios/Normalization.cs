using System.Collections.Generic;
using NineSixFour.GameDevelopmentToolkit.DisplayName;

namespace NineSixFour.GameDevelopmentToolkit.DisplayName.AutomatedTests
{
    public class Normalization
    {
        public static List<TestResult> Run()
        {
            var results = new List<TestResult>();
            var v = new DisplayNameValidator(new DisplayNameOptions { MinLength=1, MaxLength=20 });
            // é can be composed differently; normalization should handle
            Test(results, "Normalization", "ComposedE", "café", true, v);
            return results;
        }
        static void Test(List<TestResult> results, string scenario, string name, string? input, bool expectedValid, DisplayNameValidator v)
        {
            var r = v.Validate(input);
            results.Add(new TestResult { Scenario=scenario, Name=name, Input=input, ExpectedValid=expectedValid, ActualValid=r.IsValid, Errors=r.Errors.ConvertAll(e=>e.ToString()), Sanitized=r.SanitizedDisplayName, Passed=r.IsValid==expectedValid, Details="" });
        }
    }
}
