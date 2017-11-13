using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Visualizer.UI.DSP;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI.Xaml;

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

        public void CreateSpectrumLine(ICanvasAnimatedControl canvas, CanvasDrawingSession ds)
        {
            var size = canvas.Size;
            if (!UpdateFrequencyMappingIfNessesary(size))
                return;

            var fftBuffer = new float[(int)FftSize];

            //get the fft result from the spectrum provider
            if (SpectrumProvider.GetFftData(fftBuffer))
            {
                CreateSpectrumLineInternal(canvas, ds, fftBuffer, size);
            }
        }

        private void CreateSpectrumLineInternal(ICanvasAnimatedControl canvas, CanvasDrawingSession ds, float[] fftBuffer, Size size)
        {
            var height = (float)size.Height;
            var width = (float)size.Width;
            //prepare the fft result for rendering 
            SpectrumPointData[] spectrumPoints = CalculateSpectrumPoints(height / 2, fftBuffer);

            using (var brush = new CanvasLinearGradientBrush(canvas, Colors.Green, Colors.Red))
            {
                //connect the calculated points with lines
                for (int i = 0; i < spectrumPoints.Length; i++)
                {
                    SpectrumPointData p = spectrumPoints[i];
                    int barIndex = p.SpectrumPointIndex;

                    double xCoord = (BarSpacing * (barIndex + 1) + (_barWidth * barIndex) + _barWidth / 2) /2;

                    {
                        float xCoordRight = width / 2 + (float)xCoord;

                        var pRight = new Vector2(xCoordRight, height / 2);
                        brush.StartPoint = pRight;

                        var pRightUp = new Vector2(xCoordRight, height / 2 - (float) p.Value);
                        var pRightDown = new Vector2(xCoordRight, height / 2 + (float) p.Value);

                        brush.EndPoint = new Vector2(xCoordRight, 0);
                        ds.DrawLine(pRight, pRightUp, brush, (float) _barWidth);

                        brush.EndPoint = new Vector2(xCoordRight, height);
                        ds.DrawLine(pRight, pRightDown, brush, (float)_barWidth);
                    }

                    {
                        float xCoordLeft = width / 2 - (float)xCoord;

                        var pLeft = new Vector2(xCoordLeft, height / 2);
                        brush.StartPoint = pLeft;

                        var pRightUp = new Vector2(xCoordLeft, height / 2 - (float)p.Value);
                        var pRightDown = new Vector2(xCoordLeft, height / 2 + (float)p.Value);

                        brush.EndPoint = new Vector2(xCoordLeft, 0);
                        ds.DrawLine(pLeft, pRightUp, brush, (float)_barWidth);

                        brush.EndPoint = new Vector2(xCoordLeft, height);
                        ds.DrawLine(pLeft, pRightDown, brush, (float)_barWidth);
                    }
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
