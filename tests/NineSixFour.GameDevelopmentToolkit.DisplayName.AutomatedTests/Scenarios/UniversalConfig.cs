using System.Collections.Generic;
using NineSixFour.GameDevelopmentToolkit.DisplayName;

namespace NineSixFour.GameDevelopmentToolkit.DisplayName.AutomatedTests
{
    public class UniversalConfig
    {
        public static List<TestResult> Run()
        {
            var results = new List<TestResult>();
            var old = DisplayNameConfiguration.Default.Clone();
            DisplayNameConfiguration.Default.ExcludedCharacters = new char[]{'"'};
            var v = new DisplayNameValidator(); // uses default
            var r = v.Validate("test\"name");
            results.Add(new TestResult { Scenario="UniversalConfig", Name="DefaultBlocksChar", Input="test\"name", ExpectedValid=false, ActualValid=r.IsValid, Errors=r.Errors.ConvertAll(e=>e.ToString()), Sanitized=r.SanitizedDisplayName, Passed=r.IsValid==false, Details="" });
            DisplayNameConfiguration.Default = old;
            return results;
        }
    }
}
