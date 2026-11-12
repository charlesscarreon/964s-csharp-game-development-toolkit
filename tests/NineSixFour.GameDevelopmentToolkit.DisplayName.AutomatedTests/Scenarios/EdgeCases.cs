using System.Collections.Generic;
using NineSixFour.GameDevelopmentToolkit.DisplayName;

namespace NineSixFour.GameDevelopmentToolkit.DisplayName.AutomatedTests
{
    public class EdgeCases
    {
        public static List<TestResult> Run()
        {
            var results = new List<TestResult>();
            try
            {
                var bad = new DisplayNameValidator(new DisplayNameOptions { MinLength = -1 });
                results.Add(new TestResult { Scenario="EdgeCases", Name="MinLengthNegativeThrows", Input="", ExpectedValid=false, ActualValid=false, Errors=new List<string>(), Sanitized="", Passed=false, Details="Should have thrown" });
            }
            catch (System.ArgumentException)
            {
                results.Add(new TestResult { Scenario="EdgeCases", Name="MinLengthNegativeThrows", Input="", ExpectedValid=false, ActualValid=false, Errors=new List<string>(), Sanitized="", Passed=true, Details="Threw ArgumentException as expected" });
            }
            try
            {
                var bad = new DisplayNameValidator(new DisplayNameOptions { MinLength=5, MaxLength=3 });
                results.Add(new TestResult { Scenario="EdgeCases", Name="MaxLessThanMinThrows", Input="", ExpectedValid=false, ActualValid=false, Errors=new List<string>(), Sanitized="", Passed=false, Details="Should have thrown" });
            }
            catch (System.ArgumentException)
            {
                results.Add(new TestResult { Scenario="EdgeCases", Name="MaxLessThanMinThrows", Input="", ExpectedValid=false, ActualValid=false, Errors=new List<string>(), Sanitized="", Passed=true, Details="Threw ArgumentException as expected" });
            }
            return results;
        }
    }
}
