using System;

namespace NineSixFour.GameDevelopmentToolkit.CSMusicPlayer
{
    public enum TrackSourceType
    {
        Local,
        BuiltIn,
        Custom
    }

    public enum LoopMode
    {
        None,
        One,
        All
    }

    public enum PlaybackState
    {
        Stopped,
        Playing,
        Paused
    }
}
