using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NineSixFour.GameDevelopmentToolkit.DisplayName
{
    /// <summary>
    /// Validates display names according to configured rules.
    /// </summary>
    public class DisplayNameValidator
    {
        /// <summary>
        /// Gets the options used by this validator.
        /// </summary>
        public DisplayNameOptions Options { get; }

        /// <summary>
        /// Creates a validator with explicit options.
        /// </summary>
        public DisplayNameValidator(DisplayNameOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            ValidateOptions();
        }

        /// <summary>
        /// Creates a validator using the global default options.
        /// </summary>
        public DisplayNameValidator() : this(DisplayNameConfiguration.Default)
        {
        }

        /// <summary>
        /// Creates a validator from a named profile.
        /// </summary>
        public static DisplayNameValidator FromProfile(string profileName)
        {
            return DisplayNameConfiguration.Profiles.GetValidator(profileName);
        }

        /// <summary>
        /// Validates the input display name.
        /// </summary>
        public DisplayNameValidationResult Validate(string? input)
        {
            var result = new DisplayNameValidationResult();
            if (input == null)
            {
                result.IsValid = false;
                result.Errors.Add(DisplayNameValidationError.Empty);
                result.SanitizedDisplayName = string.Empty;
                return result;
            }

            string normalized = input;
            try
            {
                normalized = input.Normalize(NormalizationForm.FormC);
            }
            catch
            {
                result.Errors.Add(DisplayNameValidationError.NormalizationIssue);
            }

            string sanitized = normalized;
            if (Options.TrimWhitespace)
            {
                sanitized = sanitized.Trim();
            }

            if (string.IsNullOrEmpty(sanitized))
            {
                result.IsValid = false;
                result.Errors.Add(DisplayNameValidationError.Empty);
                result.SanitizedDisplayName = sanitized;
                return result;
            }

            if (sanitized.Length < Options.MinLength)
            {
                result.Errors.Add(DisplayNameValidationError.TooShort);
            }
            if (sanitized.Length > Options.MaxLength)
            {
                result.Errors.Add(DisplayNameValidationError.TooLong);
            }

            if (Options.TrimWhitespace)
            {
                // When trimming, don't treat leading/trailing whitespace as errors
            }
            else
            {
                if (normalized.StartsWith(" ") || normalized.StartsWith("\t"))
                    result.Errors.Add(DisplayNameValidationError.StartsWithWhitespace);
                if (normalized.EndsWith(" ") || normalized.EndsWith("\t"))
                    result.Errors.Add(DisplayNameValidationError.EndsWithWhitespace);
                if (sanitized.IndexOf(' ') >= 0 && !Options.AllowSpaces)
                {
                }
            }
            {
                // If not trimming, check original spacing rules as appropriate
            }

            // Check forbidden strings
            foreach (var fs in Options.ExcludedStrings)
            {
                if (!string.IsNullOrEmpty(fs) && sanitized.IndexOf(fs, StringComparison.Ordinal) >= 0)
                {
                    result.Errors.Add(DisplayNameValidationError.ForbiddenCharacter);
                    break;
                }
            }

            // Iterate by text elements/code points
            var enumerator = StringInfo.GetTextElementEnumerator(sanitized);
            while (enumerator.MoveNext())
            {
                string element = enumerator.GetTextElement();
                if (element.Length == 0) continue;

                // Check for surrogate pairs
                if (element.Length >= 2)
                {
                    // Could be surrogate pair or combining sequence
                    // Check if contains surrogates
                    for (int i = 0; i < element.Length; i++)
                    {
                        char c = element[i];
                        if (char.IsSurrogate(c))
                        {
                            if (Options.DenySurrogateCharacters)
                                result.Errors.Add(DisplayNameValidationError.ContainsSurrogateCharacter);
                        }
                    }
                }

                // Check each char in element
                foreach (char c in element)
                {
                    if (Options.DenySurrogateCharacters && char.IsSurrogate(c))
                    {
                        result.Errors.Add(DisplayNameValidationError.ContainsSurrogateCharacter);
                        continue;
                    }
                    if (Options.DenyControlCharacters && char.IsControl(c))
                    {
                        result.Errors.Add(DisplayNameValidationError.ContainsControlCharacter);
                        continue;
                    }

                    UnicodeCategory cat = char.GetUnicodeCategory(c);

                    if (Options.DenyFormatCharacters && cat == UnicodeCategory.Format)
                    {
                        result.Errors.Add(DisplayNameValidationError.ContainsFormatCharacter);
                        continue;
                    }
                    if (Options.DenyPrivateUseCharacters && cat == UnicodeCategory.PrivateUse)
                    {
                        result.Errors.Add(DisplayNameValidationError.ContainsPrivateUseCharacter);
                        continue;
                    }
                    if (Options.DenyInvisibleCharacters && IsInvisible(c))
                    {
                        result.Errors.Add(DisplayNameValidationError.ContainsInvisibleCharacter);
                        continue;
                    }
                    if (!Options.AllowCombiningCharacters && (cat == UnicodeCategory.NonSpacingMark || cat == UnicodeCategory.SpacingCombiningMark || cat == UnicodeCategory.EnclosingMark))
                    {
                        result.Errors.Add(DisplayNameValidationError.ContainsCombiningCharacter);
                        continue;
                    }

                    // Check excluded categories
                    foreach (var ec in Options.ExcludedUnicodeCategories)
                    {
                        if (cat == ec)
                        {
                            result.Errors.Add(DisplayNameValidationError.InvalidCharacter);
                            goto nextElement;
                        }
                    }

                    // Check excluded characters
                    foreach (var exc in Options.ExcludedCharacters)
                    {
                        if (c == exc)
                        {
                            result.Errors.Add(DisplayNameValidationError.ForbiddenCharacter);
                            goto nextElement;
                        }
                    }

                    // Check if character is allowed
                    if (!IsCharacterAllowed(c, cat))
                    {
                        result.Errors.Add(DisplayNameValidationError.InvalidCharacter);
                        goto nextElement;
                    }

                    // Script checks for letters
                    if (cat == UnicodeCategory.LowercaseLetter || cat == UnicodeCategory.UppercaseLetter || cat == UnicodeCategory.TitlecaseLetter || cat == UnicodeCategory.OtherLetter || cat == UnicodeCategory.ModifierLetter)
                    {
                        string script = GetScriptName(c);
                        if (Options.AllowedScripts.Count > 0)
                        {
                            if (!Options.AllowedScripts.Contains(script))
                            {
                                result.Errors.Add(DisplayNameValidationError.ForbiddenScript);
                                goto nextElement;
                            }
                        }
                        else if (Options.ExcludedScripts.Count > 0)
                        {
                            if (Options.ExcludedScripts.Contains(script))
                            {
                                result.Errors.Add(DisplayNameValidationError.ForbiddenScript);
                                goto nextElement;
                            }
                        }
                    }

                    if (Options.AllowOnlyBasicMultilingualPlane && c > 0xFFFF)
                    {
                        result.Errors.Add(DisplayNameValidationError.InvalidCharacter);
                        goto nextElement;
                    }
                nextElement:;
                }
            }

            // Remove duplicate errors while preserving order
            var uniqueErrors = new List<DisplayNameValidationError>();
            var seen = new HashSet<DisplayNameValidationError>();
            foreach (var e in result.Errors)
            {
                if (seen.Add(e))
                    uniqueErrors.Add(e);
            }
            result.Errors = uniqueErrors;

            result.IsValid = result.Errors.Count == 0;
            result.SanitizedDisplayName = sanitized;
            return result;
        }

        /// <summary>
        /// Tries to validate the input.
        /// </summary>
        public bool TryValidate(string? input, out DisplayNameValidationResult result)
        {
            result = Validate(input);
            return result.IsValid;
        }

        private void ValidateOptions()
        {
            if (Options.MinLength < 0)
                throw new ArgumentException("MinLength cannot be less than 0", nameof(Options));
            if (Options.MaxLength < Options.MinLength)
                throw new ArgumentException("MaxLength must be >= MinLength", nameof(Options));
        }

        private bool IsInvisible(char c)
        {
            // Common invisible characters
            if (c == '\u200B') return true; // Zero width space
            if (c == '\u200C') return true; // Zero width non-joiner
            if (c == '\u200D') return true; // Zero width joiner
            if (c == '\u2060') return true; // Word joiner
            if (c == '\uFEFF') return true; // Zero width no-break space/BOM
            if (c == '\u00AD') return true; // Soft hyphen (often treated as invisible in some contexts)
            return false;
        }

        private bool IsCharacterAllowed(char c, UnicodeCategory cat)
        {
            // Check if explicitly allowed as special char
            foreach (var sc in Options.AllowedSpecialCharacters)
            {
                if (c == sc) return true;
            }

            // Digits
            if (char.IsDigit(c))
            {
                return Options.AllowDigits;
            }

            // Letters
            if (cat == UnicodeCategory.LowercaseLetter || cat == UnicodeCategory.UppercaseLetter || cat == UnicodeCategory.TitlecaseLetter || cat == UnicodeCategory.OtherLetter || cat == UnicodeCategory.ModifierLetter)
            {
                if (!Options.AllowLetters) return false;
                if (Options.AllowAnyLetterScript) return true;
                return true;
            }

            // Space
            if (char.IsWhiteSpace(c))
            {
                if (c == ' ' || c == '\t')
                    return Options.AllowSpaces;
                return false;
            }

            // Punctuation
            if (cat == UnicodeCategory.ConnectorPunctuation || cat == UnicodeCategory.DashPunctuation || cat == UnicodeCategory.OpenPunctuation ||
                cat == UnicodeCategory.ClosePunctuation || cat == UnicodeCategory.InitialQuotePunctuation || cat == UnicodeCategory.FinalQuotePunctuation ||
                cat == UnicodeCategory.OtherPunctuation)
            {
                return Options.AllowPunctuation;
            }

            // Symbols
            if (cat == UnicodeCategory.MathSymbol || cat == UnicodeCategory.CurrencySymbol || cat == UnicodeCategory.ModifierSymbol || cat == UnicodeCategory.OtherSymbol)
            {
                return Options.AllowSymbols;
            }

            return false;
        }

        private string GetScriptName(char c)
        {
            // Basic script detection for common scripts
            if (c >= 0x4E00 && c <= 0x9FFF) return "Han";
            if ((c >= 0xAC00 && c <= 0xD7AF) || (c >= 0x1100 && c <= 0x11FF) || (c >= 0x3130 && c <= 0x318F)) return "Hangul";
            if (c >= 0x3040 && c <= 0x309F) return "Hiragana";
            if (c >= 0x30A0 && c <= 0x30FF) return "Katakana";
            if (c >= 0x0E00 && c <= 0x0E7F) return "Thai";
            if ((c >= 0x0400 && c <= 0x04FF) || (c >= 0x0500 && c <= 0x052F)) return "Cyrillic";
            if ((c >= 0x0600 && c <= 0x06FF) || (c >= 0x0750 && c <= 0x077F) || (c >= 0x08A0 && c <= 0x08FF) || (c >= 0xFB50 && c <= 0xFDFF) || (c >= 0xFE70 && c <= 0xFEFF)) return "Arabic";
            if (c >= 0x0370 && c <= 0x03FF) return "Greek";
            if (c >= 0x0590 && c <= 0x05FF) return "Hebrew";
            if ((c >= 0x0900 && c <= 0x097F)) return "Devanagari";
            // Default to Latin-like or general
            if ((c >= 0x0041 && c <= 0x007A) || (c >= 0x00C0 && c <= 0x024F)) return "Latin";
            return "Unknown";
        }
    }
}
