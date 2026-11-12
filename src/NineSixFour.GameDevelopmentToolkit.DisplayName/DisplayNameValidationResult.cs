using System.Collections.Generic;

namespace NineSixFour.GameDevelopmentToolkit.DisplayName
{
    /// <summary>
    /// Result of a display name validation operation.
    /// </summary>
    public class DisplayNameValidationResult
    {
        /// <summary>
        /// Gets or sets whether the display name is valid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the sanitized display name (e.g., trimmed if enabled).
        /// </summary>
        public string? SanitizedDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the list of validation errors.
        /// </summary>
        public List<DisplayNameValidationError> Errors { get; set; } = new List<DisplayNameValidationError>();
    }
}
