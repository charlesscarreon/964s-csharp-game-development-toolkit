using System.Collections.Generic;

namespace NineSixFour.GameDevelopmentToolkit.CSMusicPlayer
{
    public class Playlist
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public List<Track> Tracks { get; set; } = new List<Track>();
        public LoopMode LoopMode { get; set; } = LoopMode.None;
        public bool ShuffleEnabled { get; set; }
    }
}
