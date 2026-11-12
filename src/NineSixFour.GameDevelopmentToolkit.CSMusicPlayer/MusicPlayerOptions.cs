namespace NineSixFour.GameDevelopmentToolkit.CSMusicPlayer
{
    public class MusicPlayerOptions
    {
        /// <summary>Allow adding/using local music tracks. Enforced by core operations.</summary>
        public bool AllowLocalMusic { get; set; } = true;
        /// <summary>Allow adding/using built-in music tracks. Enforced by core operations.</summary>
        public bool AllowBuiltInMusic { get; set; } = true;
        /// <summary>Allow in-memory metadata edits (and write-back if writer provided). Enforced by core.</summary>
        public bool AllowMetadataEditing { get; set; } = true;
        /// <summary>Allow creating playlists. Enforced by core.</summary>
        public bool AllowPlaylistCreation { get; set; } = true;
        /// <summary>Allow renaming playlists. Enforced by core.</summary>
        public bool AllowPlaylistRenaming { get; set; } = true;
        /// <summary>Allow deleting playlists. Enforced by core.</summary>
        public bool AllowPlaylistDeletion { get; set; } = true;
        /// <summary>Default loop mode for new playlists.</summary>
        public LoopMode DefaultLoopMode { get; set; } = LoopMode.None;
        /// <summary>Default shuffle state for new playlists.</summary>
        public bool DefaultShuffleEnabled { get; set; } = false;
    }
}
