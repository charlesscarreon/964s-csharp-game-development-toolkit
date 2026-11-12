# CSMusicPlayer (`NineSixFour.GameDevelopmentToolkit.CSMusicPlayer`)

Playlist and music playback **management logic** for C# games. The core coordinates playback state and track progression and drives an audio backend you provide (no audio output implementation in core).

## Key types

- `MusicPlayer` — main controller (library, playlists, playback, mappings)
- `IAudioBackend` — audio engine abstraction (play/pause/stop/seek/volume/position + events)
- `IMusicTagReader`, `IMusicTagWriter`, `TrackMetadata` — metadata read/write abstractions
- `Track`, `Playlist`, `MusicPlayerOptions`, `GameStateMusicMapping`
- Enums: `TrackSourceType` (`Local`, `BuiltIn`, `Custom`), `LoopMode` (`None`, `One`, `All`), `PlaybackState` (`Stopped`, `Playing`, `Paused`)
- Events/args: `MusicPlayerEventArgs`, `TrackEventArgs`, `PlaybackErrorEventArgs`

## Quick start

```csharp
using System;
using NineSixFour.GameDevelopmentToolkit.CSMusicPlayer;

IAudioBackend backend = /* your engine/audio backend */;
var player = new MusicPlayer(backend, new MusicPlayerOptions(), shuffleSeed: 123);

// Optional: tag read/write integrations
// player.TagReader = myTagReader;
// player.TagWriter = myTagWriter;

// Add tracks
player.AddTrack(new Track { Id = "t1", Title = "Title", Artist = "Artist", Album = "Album", Source = "path/or/id", SourceType = TrackSourceType.BuiltIn });

// Playlists
player.CreatePlaylist("bgm", "Background Music");
player.AddTrackToPlaylist("bgm", "t1");
player.SetPlaylistLoopMode("bgm", LoopMode.All);
player.SetPlaylistShuffle("bgm", false);

player.PlayPlaylist("bgm");
```

## Playback control

- `PlayTrack(trackId)` — play a single library track (clears active playlist context)
- `PlayPlaylist(playlistId, startTrackId = null)` — play within a playlist
- `Pause()`, `Resume()`, `Stop()`
- `Next()`, `Previous()` — playlist navigation
- `Seek(TimeSpan position)` — forwarded to backend
- State/properties: `State`, `CurrentTrack`, `ActivePlaylist`, `CurrentStateKey`

`MusicPlayer` **drives `IAudioBackend`** and responds to backend events (e.g. auto-advances on `PlaybackEnded` according to loop/shuffle and playlist contents).

## Options & enforcement (`MusicPlayerOptions`)

- `AllowLocalMusic`, `AllowBuiltInMusic` — enforced when adding/using tracks (and when adding to playlists)
- `AllowMetadataEditing` — required for `EditTrackMetadata`
- `AllowPlaylistCreation`, `AllowPlaylistRenaming`, `AllowPlaylistDeletion`
- `DefaultLoopMode`, `DefaultShuffleEnabled` — applied to newly created playlists

Core exposes `Try*`/bool-returning methods for some operations and throws for disallowed/invalid operations in others (e.g. `AddTrack`, `CreatePlaylist`, `EditTrackMetadata` when disabled).

## Tracks, playlists, and library

- Track fields include `Id`, `Source` (path/URI/identifier for backend/tag components), `SourceType`, `Title`, `Artist`, `Album`, `AlbumArt` (`byte[]`), `AlbumArtMimeType`, `Duration`, `IsFavorite`, `CustomMetadata`.
- Library APIs: `AddTrack`/`TryAddTrack`, `RemoveTrack`, `GetTrack`, `GetAllTracks`, `GetFavorites`, `SetFavorite`.
- Playlist APIs: `CreatePlaylist`/`TryCreatePlaylist`, `RenamePlaylist`, `DeletePlaylist`, `GetPlaylist`, `GetAllPlaylists`, `AddTrackToPlaylist`, `RemoveTrackFromPlaylist`, `SetPlaylistLoopMode`, `SetPlaylistShuffle`.
- Returned tracks/playlists are cloned snapshots; library/playlist membership is managed by the player.

## Metadata

- `IMusicTagReader.Read(source)` can be used when adding tracks (`readTags: true`).
- `EditTrackMetadata(trackId, title, album, albumArt, albumArtMimeType, writeBack)` updates in-memory metadata (and syncs playlist/current copies as applicable). If `writeBack` is true and `TagWriter` is set, a write-back attempt is made.
- Album art is handled as `byte[]` with optional MIME type.

## Game-state mappings

```csharp
player.SetGameStateMapping(new GameStateMusicMapping
{
    StateKey = "MainMenu",
    PlaylistId = "bgm",
    Shuffle = false,
    LoopMode = LoopMode.All
});

// or map a specific track:
// new GameStateMusicMapping { StateKey = "Boss", TrackId = "boss_theme" }

player.PlayForState("MainMenu");
```

`GetGameStateMapping` / `RemoveGameStateMapping` are also available.

## Events

- `MusicPlayer.TrackChanged` — `MusicPlayerEventArgs` (`Track`, `Playlist`, `StateKey`)
- `MusicPlayer.PlaybackStateChanged` — `MusicPlayerEventArgs`
- `MusicPlayer.PlaybackError` — `PlaybackErrorEventArgs` (`Track`, `Message`, `Exception`)
- Backend events (`IAudioBackend`): `PlaybackStarted/Paused/Resumed/Stopped/Ended/Error`

## `IAudioBackend` contract (summary)

Implement `Play(Track)`, `Pause`, `Resume`, `Stop`, `Seek`, `GetPosition`, `GetDuration`, `SetVolume`, `GetVolume`, `IsPlaying`, and raise the defined events so the core can coordinate progression (especially `PlaybackEnded`).

## Testers

- Automated: `tests\NineSixFour.GameDevelopmentToolkit.CSMusicPlayer.AutomatedTests`
- Manual: `samples\NineSixFour.GameDevelopmentToolkit.CSMusicPlayer.ManualTester` (includes a fake backend and simulated track-end flow)
