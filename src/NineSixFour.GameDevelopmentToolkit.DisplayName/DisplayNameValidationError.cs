namespace NineSixFour.GameDevelopmentToolkit.DisplayName
{
    /// <summary>
    /// Represents validation errors that can occur when validating a display name.
    /// </summary>
    public enum DisplayNameValidationError
    {
        Empty,
        TooShort,
        TooLong,
        ContainsWhitespace,
        StartsWithWhitespace,
        EndsWithWhitespace,
        InvalidCharacter,
        ForbiddenCharacter,
        ForbiddenScript,
        ContainsControlCharacter,
        ContainsFormatCharacter,
        ContainsPrivateUseCharacter,
        ContainsInvisibleCharacter,
        ContainsCombiningCharacter,
        ContainsSurrogateCharacter,
        NormalizationIssue
    }
}
