using System;
using System.Collections.Generic;
using System.Globalization;

namespace NineSixFour.GameDevelopmentToolkit.DisplayName
{
    /// <summary>
    /// Configuration options for display name validation.
    /// </summary>
    public class DisplayNameOptions
    {
        // Length
        /// <summary>Minimum length. Must be >= 0.</summary>
        public int MinLength { get; set; } = 1;
        /// <summary>Maximum length.</summary>
        public int MaxLength { get; set; } = 16;
        /// <summary>Whether to trim whitespace from input.</summary>
        public bool TrimWhitespace { get; set; } = true;

        // Allowlists
        /// <summary>Allow letters from any Unicode letter category.</summary>
        public bool AllowLetters { get; set; } = true;
        /// <summary>Allow digits.</summary>
        public bool AllowDigits { get; set; } = true;
        /// <summary>Allow spaces.</summary>
        public bool AllowSpaces { get; set; } = false;
        /// <summary>Allow punctuation.</summary>
        public bool AllowPunctuation { get; set; } = false;
        /// <summary>Allow symbols.</summary>
        public bool AllowSymbols { get; set; } = false;
        /// <summary>Explicitly allowed special characters.</summary>
        public char[] AllowedSpecialCharacters { get; set; } = Array.Empty<char>();

        // Script/Unicode control
        /// <summary>Allow letters from any script via Unicode categories.</summary>
        public bool AllowAnyLetterScript { get; set; } = true;
        /// <summary>Scripts to exclude (e.g., "Han", "Hangul", "Thai", "Cyrillic", "Arabic").</summary>
        public HashSet<string> ExcludedScripts { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        /// <summary>If populated, only these scripts are allowed (allowlist mode).</summary>
        public HashSet<string> AllowedScripts { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        /// <summary>Allow combining characters/diacritics.</summary>
        public bool AllowCombiningCharacters { get; set; } = false;
        /// <summary>Restrict to Basic Multilingual Plane if needed.</summary>
        public bool AllowOnlyBasicMultilingualPlane { get; set; } = false;

        // Blacklists
        /// <summary>Explicitly forbidden characters.</summary>
        public char[] ExcludedCharacters { get; set; } = Array.Empty<char>();
        /// <summary>Explicitly forbidden strings.</summary>
        public string[] ExcludedStrings { get; set; } = Array.Empty<string>();
        /// <summary>Explicitly forbidden Unicode categories.</summary>
        public UnicodeCategory[] ExcludedUnicodeCategories { get; set; } = Array.Empty<UnicodeCategory>();

        // Safety defaults
        /// <summary>Deny control characters (default: true).</summary>
        public bool DenyControlCharacters { get; set; } = true;
        /// <summary>Deny format characters (default: true).</summary>
        public bool DenyFormatCharacters { get; set; } = true;
        /// <summary>Deny private use characters (default: true).</summary>
        public bool DenyPrivateUseCharacters { get; set; } = true;
        /// <summary>Deny surrogate characters (default: true).</summary>
        public bool DenySurrogateCharacters { get; set; } = true;
        /// <summary>Deny invisible characters (default: true).</summary>
        public bool DenyInvisibleCharacters { get; set; } = true;

        /// <summary>
        /// Creates a deep clone of the options.
        /// </summary>
        public DisplayNameOptions Clone()
        {
            return new DisplayNameOptions
            {
                MinLength = MinLength,
                MaxLength = MaxLength,
                TrimWhitespace = TrimWhitespace,
                AllowLetters = AllowLetters,
                AllowDigits = AllowDigits,
                AllowSpaces = AllowSpaces,
                AllowPunctuation = AllowPunctuation,
                AllowSymbols = AllowSymbols,
                AllowedSpecialCharacters = (char[])AllowedSpecialCharacters.Clone(),
                AllowAnyLetterScript = AllowAnyLetterScript,
                ExcludedScripts = new HashSet<string>(ExcludedScripts, StringComparer.OrdinalIgnoreCase),
                AllowedScripts = new HashSet<string>(AllowedScripts, StringComparer.OrdinalIgnoreCase),
                AllowCombiningCharacters = AllowCombiningCharacters,
                AllowOnlyBasicMultilingualPlane = AllowOnlyBasicMultilingualPlane,
                ExcludedCharacters = (char[])ExcludedCharacters.Clone(),
                ExcludedStrings = (string[])ExcludedStrings.Clone(),
                ExcludedUnicodeCategories = (UnicodeCategory[])ExcludedUnicodeCategories.Clone(),
                DenyControlCharacters = DenyControlCharacters,
                DenyFormatCharacters = DenyFormatCharacters,
                DenyPrivateUseCharacters = DenyPrivateUseCharacters,
                DenySurrogateCharacters = DenySurrogateCharacters,
                DenyInvisibleCharacters = DenyInvisibleCharacters
            };
        }
    }
}
