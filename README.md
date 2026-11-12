# 964's C# Game Development Toolkit

A collection of reusable, engine-agnostic C# game modules designed to reduce repetitive implementation work across Unity and other C#-based projects.

- **Package (UPM):** `com.964.gamedevelopmenttoolkit`
- **Version:** `0.1.0`
- **Target:** `netstandard2.0`
- **License:** MIT (see [`LICENSE`](LICENSE) and [`NOTICE`](NOTICE))

## Documentation

- [Installation](docs/installation.md)
- [Guidelines & design principles](docs/guidelines.md)
- [Changelog](CHANGELOG.md)

## Modules

| Module | Namespace | Status | Description | Docs |
| --- | --- | --- | --- | --- |
| Display Name | `NineSixFour.GameDevelopmentToolkit.DisplayName` | Implemented | Unicode-aware display name validation/configuration (usernames, NPC/item names, etc.). No content moderation/uniqueness/confusable checks. | [docs/modules/display-name.md](docs/modules/display-name.md) |
| Timer | `NineSixFour.GameDevelopmentToolkit.Timer` | Implemented | `Update()`-driven countdown/count-up timers, stopwatch helpers, adjustments, events, manager, and pluggable time providers. | [docs/modules/timer.md](docs/modules/timer.md) |
| Settings | `NineSixFour.GameDevelopmentToolkit.Settings` | Implemented | Key-value settings store with typed access, defaults, change events, and pluggable storage. | [docs/modules/settings.md](docs/modules/settings.md) |
| Save Data | `NineSixFour.GameDevelopmentToolkit.SaveData` | Implemented | Save-slot mechanics (metadata + payload envelope), limits/rename/delete, results/events. **Does not handle persistent/permanent data.** | [docs/modules/savedata.md](docs/modules/savedata.md) |
| CSMusicPlayer | `NineSixFour.GameDevelopmentToolkit.CSMusicPlayer` | Implemented | Playlist/music management logic with core-driven `IAudioBackend`, shuffle/loop, favorites, metadata abstractions, and game-state mappings. | [docs/modules/csmusicplayer.md](docs/modules/csmusicplayer.md) |

## Installation (quick)

- **Unity (UPM):** Package Manager → *Add package from git URL...* → `https://github.com/<owner>/964s-csharp-game-development-toolkit.git#v0.1.0`
- **From source:** add project references to the module `.csproj` files under `src/`.

See [docs/installation.md](docs/installation.md) for details and build/test commands.

## Testing

Automated and manual testers are provided per module.

```powershell
dotnet build .\964s-csharp-game-development-toolkit.sln

# Example: run an automated tester
dotnet run --project .\tests\NineSixFour.GameDevelopmentToolkit.Timer.AutomatedTests\NineSixFour.GameDevelopmentToolkit.Timer.AutomatedTests.csproj
```

- Logs are written to the repository `logs\` folder as **JSON + TXT** (resolved from the solution root).
- Manual testers live under `samples\NineSixFour.GameDevelopmentToolkit.<Module>.ManualTester`.

## Design notes

- **No UI/presentation** in core modules.
- **Engine-agnostic** (`netstandard2.0`); engine adapters belong in `integrations/`.
- **Abstractions-first** for storage, serialization, time, audio, and tag I/O.
- **Multiplayer:** never trust client validation; revalidate on the server.
- **Save Data:** consumer owns persistence/permanent data and call timing; `AllowFreeSave/AllowFreeLoad` are advisory.
- **Display Name:** no profanity/moderation/uniqueness/confusable/reserved-name checks.

See [docs/guidelines.md](docs/guidelines.md) for full guidelines.

## Attribution

If you use this toolkit or its modules, please retain attribution as described in [`NOTICE`](NOTICE).
