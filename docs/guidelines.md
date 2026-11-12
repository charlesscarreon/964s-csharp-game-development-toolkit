# Guidelines & design principles

## Core principles

- **No UI/presentation:** modules provide logic, data, events, and abstractions only.
- **Engine-agnostic core:** works in Unity and any C# environment targeting `netstandard2.0`. Engine-specific adapters belong in `integrations/` (e.g. `integrations/Unity/`).
- **Configurable via options:** behavior is controlled through options/config objects rather than hardcoded rules.
- **Testable via abstractions:** storage, serialization, time, audio, and tag I/O are injected through interfaces where appropriate.
- **Namespaces:** `NineSixFour.GameDevelopmentToolkit.<Module>`.

## Module constraints & responsibilities

### Display Name
- Provides **format/character/script/length** validation and normalization support.
- **No** content moderation, profanity filtering, uniqueness checks, confusable/homoglyph detection, or reserved-name enforcement. These are the consumer’s responsibility.

### Multiplayer / trust
- **Never trust client-side validation.** Modules can be used on clients and servers, but authoritative servers must revalidate inputs and enforce rules.

### Save Data
- **Does not handle persistent/permanent data.** The consumer provides `ISaveStorage` and `ISaveSerializer` implementations and decides when/how data is persisted.
- Manages slot mechanics (metadata envelope, limits, rename/delete, results/events) over the provided abstractions.
- `AllowFreeSave` / `AllowFreeLoad` are **advisory policy flags** (for your game flow); `SaveManager` does not enforce call timing.
- Use throwing APIs (`Save`/`Load`/`Rename`/`Delete`/`SetMaxLoadCount`) for hard failures or `Try*`/result APIs for expected outcomes.

### CSMusicPlayer
- Core is **playback/playlist management logic**; it does not implement audio decoding/output.
- **Core drives the backend:** playback state/advancement (including auto-advance on `PlaybackEnded`, loop/shuffle behavior) is coordinated by `MusicPlayer` against `IAudioBackend`.
- Track `Source` is an identifier/path/URI understood by your backend/tag components; core does not scan the filesystem.
- Metadata editing can be applied in-memory and optionally written back via `IMusicTagWriter`.

## API & error-handling conventions

- Invalid arguments typically throw (`ArgumentException`, `ArgumentNullException`, etc.).
- Expected/operational outcomes may be exposed as `Try*` methods and/or result objects with reason codes (e.g. Save Data).
- Prefer events/callbacks for state changes (timers, settings, save operations, playback).

## Timer specifics

- Timers are **`Update()`-driven** (call `Update()` from your game loop) and use `ITimeProvider` for time.
- Use `NineSixFour.GameDevelopmentToolkit.Timer.Timer` (or alias as `GameTimer`) to avoid confusion with `System.Threading.Timer`.
- `ManualTimeProvider` enables deterministic testing.

## Testing & logs

- Automated and manual testers are included per module.
- Logs are written as **JSON + TXT** into the repository `logs\` directory for traceability and AI/human review.
- Favor deterministic inputs/time providers in tests where applicable.

## Versioning & distribution

- UPM package: `com.964.gamedevelopmenttoolkit` (see `package.json`).
- Semantic versioning intended; see `CHANGELOG.md` for changes.

## Attribution

- Licensed under MIT (see `LICENSE`).
- Please retain attribution as described in `NOTICE` when distributing or using the toolkit/modules.
