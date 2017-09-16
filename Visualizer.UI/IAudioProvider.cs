using System;
using System.Windows.Input;
using Windows.Storage;
using Visualizer.UI.Spectrum;

namespace Visualizer.UI
{
    public interface IAudioProvider : ISpectrumProvider
    {
        TimeSpan Duration { get; set; }

        TimeSpan Position { get; set; }

        double PlaybackSpeed { get; set; }

        double Volume { get; set; }

        float CurrentVolumePeek { get; set; }

        ICommand PlayCommand { get; }

        ICommand PauseCommand { get; }

        IStorageFile CurrentPlayingFile { get; set; }
    }
}