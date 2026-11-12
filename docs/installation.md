# Installation

## Requirements

- **.NET SDK** (6.0+ recommended for building/running tests and samples; modules target `netstandard2.0`)
- A C# environment that can consume `netstandard2.0` libraries (Unity, .NET, Mono, etc.)
- **Unity**: Unity 2019.4+ for UPM/git package usage (see `package.json`)

## Unity (UPM via Git URL)

1. Open **Window → Package Manager**.
2. Click **+ → Add package from git URL...**
3. Enter the repository URL, optionally pinned to a tag:
   - Latest on branch: `https://github.com/<owner>/964s-csharp-game-development-toolkit.git`
   - Specific version tag: `https://github.com/<owner>/964s-csharp-game-development-toolkit.git#v0.1.0`

Notes:
- `package.json` is at the repository root (`com.964.gamedevelopmenttoolkit`).
- Tagged releases (e.g. `v0.1.0`) are recommended for reproducible installs once published.

## From source (any C# project)

1. Clone or download the repository.
2. Add project references to the modules you need, for example:
   - `src\NineSixFour.GameDevelopmentToolkit.DisplayName\NineSixFour.GameDevelopmentToolkit.DisplayName.csproj`
   - `src\NineSixFour.GameDevelopmentToolkit.Timer\NineSixFour.GameDevelopmentToolkit.Timer.csproj`
   - `src\NineSixFour.GameDevelopmentToolkit.Settings\NineSixFour.GameDevelopmentToolkit.Settings.csproj`
   - `src\NineSixFour.GameDevelopmentToolkit.SaveData\NineSixFour.GameDevelopmentToolkit.SaveData.csproj`
   - `src\NineSixFour.GameDevelopmentToolkit.CSMusicPlayer\NineSixFour.GameDevelopmentToolkit.CSMusicPlayer.csproj`
3. Build your project/solution as usual.

You can also open `964s-csharp-game-development-toolkit.sln` to build everything together.

## GitHub Releases

Prebuilt/downloadable packages may be provided via GitHub Releases in the future. Until then, use UPM git URL or source/project references.

## Build & test

From the repository root:

```powershell
dotnet build .\964s-csharp-game-development-toolkit.sln
```

Run automated testers:

```powershell
dotnet run --project .\tests\NineSixFour.GameDevelopmentToolkit.DisplayName.AutomatedTests\NineSixFour.GameDevelopmentToolkit.DisplayName.AutomatedTests.csproj
dotnet run --project .\tests\NineSixFour.GameDevelopmentToolkit.Timer.AutomatedTests\NineSixFour.GameDevelopmentToolkit.Timer.AutomatedTests.csproj
dotnet run --project .\tests\NineSixFour.GameDevelopmentToolkit.Settings.AutomatedTests\NineSixFour.GameDevelopmentToolkit.Settings.AutomatedTests.csproj
dotnet run --project .\tests\NineSixFour.GameDevelopmentToolkit.SaveData.AutomatedTests\NineSixFour.GameDevelopmentToolkit.SaveData.AutomatedTests.csproj
dotnet run --project .\tests\NineSixFour.GameDevelopmentToolkit.CSMusicPlayer.AutomatedTests\NineSixFour.GameDevelopmentToolkit.CSMusicPlayer.AutomatedTests.csproj
```

Run manual testers (interactive console):

```powershell
dotnet run --project .\samples\NineSixFour.GameDevelopmentToolkit.DisplayName.ManualTester\NineSixFour.GameDevelopmentToolkit.DisplayName.ManualTester.csproj
dotnet run --project .\samples\NineSixFour.GameDevelopmentToolkit.Timer.ManualTester\NineSixFour.GameDevelopmentToolkit.Timer.ManualTester.csproj
dotnet run --project .\samples\NineSixFour.GameDevelopmentToolkit.Settings.ManualTester\NineSixFour.GameDevelopmentToolkit.Settings.ManualTester.csproj
dotnet run --project .\samples\NineSixFour.GameDevelopmentToolkit.SaveData.ManualTester\NineSixFour.GameDevelopmentToolkit.SaveData.ManualTester.csproj
dotnet run --project .\samples\NineSixFour.GameDevelopmentToolkit.CSMusicPlayer.ManualTester\NineSixFour.GameDevelopmentToolkit.CSMusicPlayer.ManualTester.csproj
```

### Test logs

Automated and manual testers write **JSON + TXT** logs to the repository `logs\` folder (resolved by locating `964s-csharp-game-development-toolkit.sln` from the test executable/current directory), e.g.:

- `logs\displayname-tests-<timestamp>.json|.txt`
- `logs\timer-tests-<timestamp>.json|.txt`
- `logs\settings-tests-<timestamp>.json|.txt`
- `logs\savedata-tests-<timestamp>.json|.txt`
- `logs\csmusicplayer-tests-<timestamp>.json|.txt`
- `logs\*-manual-session-<timestamp>.json|.txt`
