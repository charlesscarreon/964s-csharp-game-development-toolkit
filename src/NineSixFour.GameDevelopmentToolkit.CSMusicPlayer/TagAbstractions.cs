namespace NineSixFour.GameDevelopmentToolkit.CSMusicPlayer
{
    public class TrackMetadata
    {
        public string? Title { get; set; }
        public string? Artist { get; set; }
        public string? Album { get; set; }
        public byte[]? AlbumArt { get; set; }
        public string? AlbumArtMimeType { get; set; }
        public System.TimeSpan? Duration { get; set; }
    }

    public interface IMusicTagReader
    {
        TrackMetadata Read(string source);
    }

    public interface IMusicTagWriter
    {
        /// <summary>Write metadata back to the source (e.g. file tags). Return true on success.</summary>
        bool Write(string source, TrackMetadata metadata);
    }
}
