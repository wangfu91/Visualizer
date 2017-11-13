using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Visualizer.UI.DSP;
using Visualizer.UI.Spectrum;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Visualizer.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private LineSpectrum _lineSpectrum;

        private IAudioProvider _audioProvider;

        private const FftSize FftSize = DSP.FftSize.Fft4096;




        public MainPage()
        {
            InitializeComponent();

            animatedControl.ClearColor = Colors.Transparent;
            animatedControl.TargetElapsedTime = TimeSpan.FromMilliseconds(16); // Make sure there are at least 60 frame per second.
            _audioProvider = new BassAudioPlayer();
            //_audioProvider = new AudioGraphProvider();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            _audioProvider.CurrentPlayingFile = await SelectPlaybackFile();

            if (_audioProvider.IsPlaying)
                _audioProvider.Stop();

            await _audioProvider.Play();

            //linespectrum and voiceprint3dspectrum used for rendering some fft data
            //in oder to get some fft data, set the previously created spectrumprovider 
            _lineSpectrum = new LineSpectrum(FftSize)
            {
                SpectrumProvider = _audioProvider,
                UseAverage = true,
                BarCount = 19980,
                BarSpacing = 0,
                IsXLogScale = false,
                ScalingStrategy = ScalingStrategy.Sqrt,
                MinimumFrequency = 20,
                MaximumFrequency = 20000
            };
        }

        private void OnCreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {

        }

        private void OnDraw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            if (_lineSpectrum != null && _audioProvider.IsPlaying)
            {
                _lineSpectrum.CreateSpectrumLine(sender, args.DrawingSession);
            }
        }

        private void OnUpdate(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedUpdateEventArgs args)
        {

        }

        private async Task<IStorageFile> SelectPlaybackFile()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".aac");
            picker.FileTypeFilter.Add(".wav");

            var file = await picker.PickSingleFileAsync();
            return file;
        }


    }
}
