using System;
using System.Collections.Generic;

namespace NineSixFour.GameDevelopmentToolkit.CSMusicPlayer
{
    public class Track
    {
        public string Id { get; set; } = "";
        /// <summary>Path, URI, or source identifier understood by the backend/tag reader.</summary>
        public string Source { get; set; } = "";
        public TrackSourceType SourceType { get; set; } = TrackSourceType.Local;
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public string Album { get; set; } = "";
        public byte[]? AlbumArt { get; set; }
        public string? AlbumArtMimeType { get; set; }
        public TimeSpan? Duration { get; set; }
        public bool IsFavorite { get; set; }
        public Dictionary<string, string> CustomMetadata { get; set; } = new Dictionary<string, string>();

        public Track Clone()
        {
            return new Track
            {
                Id = Id,
                Source = Source,
                SourceType = SourceType,
                Title = Title,
                Artist = Artist,
                Album = Album,
                AlbumArt = AlbumArt == null ? null : (byte[])AlbumArt.Clone(),
                AlbumArtMimeType = AlbumArtMimeType,
                Duration = Duration,
                IsFavorite = IsFavorite,
                CustomMetadata = new Dictionary<string, string>(CustomMetadata)
            };
        }
    }
}
