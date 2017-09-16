using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Visualizer.UI.DSP;
using Microsoft.Graphics.Canvas.Brushes;

namespace Visualizer.UI.Spectrum
{
    public class LineSpectrum : SpectrumBase
    {
        private int _barCount;
        private double _barSpacing;
        private double _barWidth;
        private Size _currentSize;

        public LineSpectrum(FftSize fftSize)
        {
            FftSize = fftSize;
        }

        public double BarWidth => _barWidth;

        public double BarSpacing
        {
            get => _barSpacing;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _barSpacing = value;
                UpdateFrequencyMapping();

                RaisePropertyChanged("BarSpacing");
                RaisePropertyChanged("BarWidth");
            }
        }

        public int BarCount
        {
            get => _barCount;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _barCount = value;
                SpectrumResolution = value;
                UpdateFrequencyMapping();

                RaisePropertyChanged("BarCount");
                RaisePropertyChanged("BarWidth");
            }
        }

        public Size CurrentSize
        {
            get => _currentSize;
            protected set
            {
                _currentSize = value;
                RaisePropertyChanged("CurrentSize");
            }
        }

        public void CreateSpectrumLine(Size size, CanvasDrawingSession ds)
        {
            if (!UpdateFrequencyMappingIfNessesary(size))
                return;

            var fftBuffer = new float[(int)FftSize];

            //get the fft result from the spectrum provider
            if (SpectrumProvider.GetFftData(fftBuffer))
            {
                CreateSpectrumLineInternal(ds, fftBuffer, size);
            }
        }

        private void CreateSpectrumLineInternal(CanvasDrawingSession ds, float[] fftBuffer, Size size)
        {
            var height = (float)size.Height;
            var width = (float)size.Width;
            //prepare the fft result for rendering 
            SpectrumPointData[] spectrumPoints = CalculateSpectrumPoints(height, fftBuffer);

            using (var brush = CanvasLinearGradientBrush.CreateRainbow(ds, 0))
            {
                brush.StartPoint = new Vector2(0, height);
                brush.EndPoint = new Vector2(width, 0);

                //connect the calculated points with lines
                for (int i = 0; i < spectrumPoints.Length; i++)
                {
                    SpectrumPointData p = spectrumPoints[i];
                    int barIndex = p.SpectrumPointIndex;
                    double xCoord = BarSpacing * (barIndex + 1) + (_barWidth * barIndex) + _barWidth / 2;

                    var p1 = new Vector2((float)xCoord, height);
                    var p2 = new Vector2((float)xCoord, height - (float)p.Value - 1);

                    ds.DrawLine(p1, p2, brush, strokeWidth: 20.0F);
                }
            }
        }

        public override void UpdateFrequencyMapping()
        {
            _barWidth = Math.Max(((_currentSize.Width - (BarSpacing * (BarCount + 1))) / BarCount), 0.00001);
            base.UpdateFrequencyMapping();
        }

        public bool UpdateFrequencyMappingIfNessesary(Size newSize)
        {
            if (newSize != CurrentSize)
            {
                CurrentSize = newSize;
                UpdateFrequencyMapping();
            }

            return newSize.Width > 0 && newSize.Height > 0;
        }

    }

}
