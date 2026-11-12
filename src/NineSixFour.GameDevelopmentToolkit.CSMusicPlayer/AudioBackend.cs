using System;

namespace NineSixFour.GameDevelopmentToolkit.CSMusicPlayer
{
    public interface IAudioBackend
    {
        void Play(Track track);
        void Pause();
        void Resume();
        void Stop();
        void Seek(TimeSpan position);
        TimeSpan GetPosition();
        TimeSpan GetDuration();
        void SetVolume(float volume);
        float GetVolume();
        bool IsPlaying { get; }

        event EventHandler<TrackEventArgs>? PlaybackStarted;
        event EventHandler<TrackEventArgs>? PlaybackPaused;
        event EventHandler<TrackEventArgs>? PlaybackResumed;
        event EventHandler<TrackEventArgs>? PlaybackStopped;
        event EventHandler<TrackEventArgs>? PlaybackEnded;
        event EventHandler<PlaybackErrorEventArgs>? PlaybackError;
    }

    public class TrackEventArgs : EventArgs
    {
        public Track Track { get; }
        public TrackEventArgs(Track track) { Track = track; }
    }

    public class PlaybackErrorEventArgs : EventArgs
    {
        public Track? Track { get; }
        public string Message { get; }
        public Exception? Exception { get; }
        public PlaybackErrorEventArgs(Track? track, string message, Exception? exception = null)
        {
            Track = track; Message = message ?? ""; Exception = exception;
        }
    }
}
