# Display Name (`NineSixFour.GameDevelopmentToolkit.DisplayName`)

Configurable, Unicode-aware display name validation for usernames, NPC names, item names, and other in-game labels.

> **Scope:** format/character/script/length validation and normalization support. This module does **not** perform content moderation, profanity filtering, uniqueness checks, confusable/homoglyph detection, or reserved-name enforcement.

## Key types

- `DisplayNameOptions` — validation configuration (length, allowed character classes, scripts, blacklists, safety defaults)
- `DisplayNameValidator` — validates input against options
- `DisplayNameValidationResult` — `IsValid`, `SanitizedDisplayName`, `Errors`
- `DisplayNameValidationError` — error codes
- `DisplayNameConfiguration` — global `Default` options and `Profiles`
- `DisplayNameProfileManager` — named profiles (e.g. Username, NPCName)

## Quick start

```csharp
using NineSixFour.GameDevelopmentToolkit.DisplayName;

// Basic validation (uses DisplayNameConfiguration.Default)
var validator = new DisplayNameValidator();
var result = validator.Validate("PlayerOne");

if (result.IsValid)
{
    string name = result.SanitizedDisplayName ?? "";
    // use name
}
else
{
    // inspect result.Errors
}

// TryValidate
if (validator.TryValidate("PlayerOne", out var r) && r.IsValid) { /* ... */ }
```

## Custom options

```csharp
var options = new DisplayNameOptions
{
    MinLength = 3,
    MaxLength = 16,
    AllowLetters = true,
    AllowDigits = true,
    AllowSpaces = false,
    AllowPunctuation = false,
    AllowSymbols = false,
    TrimWhitespace = true
};

var validator = new DisplayNameValidator(options);
var result = validator.Validate("  MyName  ");
// result.SanitizedDisplayName is trimmed when TrimWhitespace=true
```

## Global defaults and profiles

```csharp
// Universal default used by parameterless validator
DisplayNameConfiguration.Default.MinLength = 3;
DisplayNameConfiguration.Default.MaxLength = 20;

// Named profiles
var npcOptions = new DisplayNameOptions { MinLength = 1, MaxLength = 24, AllowSpaces = true };
DisplayNameConfiguration.Profiles.Register("NPCName", npcOptions);

var npcValidator = DisplayNameValidator.FromProfile("NPCName");
// or: DisplayNameConfiguration.Profiles.GetValidator("NPCName")
```

## Options reference (high level)

- **Length/whitespace:** `MinLength` (>= 0), `MaxLength`, `TrimWhitespace`
- **Character classes:** `AllowLetters`, `AllowDigits`, `AllowSpaces`, `AllowPunctuation`, `AllowSymbols`, `AllowedSpecialCharacters`
- **Scripts/Unicode:** `AllowAnyLetterScript`, `AllowedScripts` (allowlist mode when populated), `ExcludedScripts`, `AllowCombiningCharacters`, `AllowOnlyBasicMultilingualPlane`
- **Blacklists:** `ExcludedCharacters`, `ExcludedStrings`, `ExcludedUnicodeCategories`
- **Safety defaults:** `DenyControlCharacters`, `DenyFormatCharacters`, `DenyPrivateUseCharacters`, `DenySurrogateCharacters`, `DenyInvisibleCharacters` (all default `true`)
- **Clone:** `DisplayNameOptions.Clone()` creates a deep copy for safe reuse

## Validation behavior & errors

- Input is normalized (Unicode NFC) where possible; normalization issues may be reported.
- `SanitizedDisplayName` reflects trimming (when enabled) and the processed value.
- `Errors` contains `DisplayNameValidationError` values such as `Empty`, `TooShort`, `TooLong`, `InvalidCharacter`, `ForbiddenCharacter`, `ForbiddenScript`, control/format/private-use/invisible/combining/surrogate issues, whitespace position errors (when not trimmed), and `NormalizationIssue`.
- Duplicate errors are de-duplicated while preserving order.

## Notes

- “Valid” means the name satisfies the configured structural/character rules; it does **not** imply appropriateness, uniqueness, or safety against spoofing/confusables.
- In multiplayer contexts, **revalidate on the server**; do not trust client-side results.
- See automated/manual testers for examples: `tests/NineSixFour.GameDevelopmentToolkit.DisplayName.AutomatedTests`, `samples/NineSixFour.GameDevelopmentToolkit.DisplayName.ManualTester`.
