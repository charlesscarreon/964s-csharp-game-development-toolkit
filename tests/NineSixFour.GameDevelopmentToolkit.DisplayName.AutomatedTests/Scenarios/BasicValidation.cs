using System.Collections.Generic;
using NineSixFour.GameDevelopmentToolkit.DisplayName;

namespace NineSixFour.GameDevelopmentToolkit.DisplayName.AutomatedTests
{
    public class BasicValidation
    {
        public static List<TestResult> Run()
        {
            var results = new List<TestResult>();
            
            // Default options
            var v = new DisplayNameValidator(new DisplayNameOptions());
            Test(results, "BasicValidation", "DefaultValid", "Player123", true, v);
            Test(results, "BasicValidation", "Empty", "", false, v);
            Test(results, "BasicValidation", "Null", null, false, v);
            
            // Min length
            var v2 = new DisplayNameValidator(new DisplayNameOptions { MinLength = 3, MaxLength = 10 });
            Test(results, "BasicValidation", "MinLengthOk", "Abc", true, v2);
            Test(results, "BasicValidation", "MinLengthTooShort", "Ab", false, v2);
            
            // Max length
            var v3 = new DisplayNameValidator(new DisplayNameOptions { MinLength = 1, MaxLength = 5 });
            Test(results, "BasicValidation", "MaxLengthOk", "Hello", true, v3);
            Test(results, "BasicValidation", "MaxLengthTooLong", "HelloWorld", false, v3);
            
            // Trimming
            var v4 = new DisplayNameValidator(new DisplayNameOptions { MinLength = 3, MaxLength = 10, TrimWhitespace = true, AllowSpaces = true });
            var r = v4.Validate("  Test  ");
            results.Add(new TestResult { Scenario = "BasicValidation", Name = "TrimmingSanitized", Input = "  Test  ", ExpectedValid = true, ActualValid = r.IsValid, Errors = r.Errors.ConvertAll(e=>e.ToString()), Sanitized = r.SanitizedDisplayName, Passed = r.IsValid && r.SanitizedDisplayName == "Test", Details = $"sanitized={r.SanitizedDisplayName}" });
            
            return results;
        }
        
        static void Test(List<TestResult> results, string scenario, string name, string? input, bool expectedValid, DisplayNameValidator v)
        {
            var r = v.Validate(input);
            results.Add(new TestResult 
            { 
                Scenario = scenario, 
                Name = name, 
                Input = input, 
                ExpectedValid = expectedValid, 
                ActualValid = r.IsValid, 
                Errors = r.Errors.ConvertAll(e=>e.ToString()), 
                Sanitized = r.SanitizedDisplayName, 
                Passed = r.IsValid == expectedValid,
                Details = r.IsValid == expectedValid ? "" : $"Got errors: {string.Join(",", r.Errors)}"
            });
        }
    }
}
