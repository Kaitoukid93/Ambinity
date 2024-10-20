using System.Drawing;
using AmbinityCore.Models.Device.LED;
using AmbinityCore.Models.Profile;
using AmbinityCore.Utils;
using Avalonia;
using Draw2D.Core.Graphic;
using Serilog;
using Point = Avalonia.Point;

namespace AmbinityCore.Models.Device.Device;

//service for capturing the great bitmap
public class AmbinityDeviceBitmapCapture
{
    public AmbinityDeviceBitmapCapture(FrameBuffer frame, AmbinityDevice device, LightingProfileDecoder decoder)
    {
        _decoder = decoder;
        _decoder.RenderingStatusChanged += OnRenderingStatusChanged;
        _frame = frame;
        _device = device;
    }

    private void OnRenderingStatusChanged()
    {
        //dim led
        _device.TransformLeds();
        _dimMode = _decoder.IsRendering ? DimMode.Up : DimMode.Down;
    }

    private FrameBuffer _frame;
    private AmbinityDevice _device;
    private CancellationTokenSource _cancellationTokenSource;
    private Thread _workerThread;
    private float _smoothFactor = 1f;
    private readonly LightingProfileDecoder _decoder;
    public AmbinityDevice Device => _device;

    public void Init()
    {
        _device?.TransformLeds();
        _cancellationTokenSource = new CancellationTokenSource();
        _workerThread = new Thread(() => Run(_cancellationTokenSource.Token))
        {
            IsBackground = true,
            Priority = ThreadPriority.BelowNormal,
            Name = "BitmapCapture"
        };
        _workerThread.Start();
    }
    private double _dimFactor;

    private enum DimMode
    {
        Up,
        Down
    };

    private DimMode _dimMode;
    private void DimLED()
    {
        if (_dimMode == DimMode.Down)
        {
            if (_dimFactor >= 0.01)
                _dimFactor -= 0.01;
            if (_dimFactor < 0.01)
                _dimFactor = 0;
            // if (_dimFactor < 0.1)
            //  _dimMode = DimMode.Up;
        }
        else if (_dimMode == DimMode.Up)
        {
            if (_dimFactor <= 0.99)
                _dimFactor += 0.01;
            //_dimMode = DimMode.Down;
        }
    }
    public void Run(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                //this indicator that user is opening this device and we need raise event when color update on each spot
                lock (_device.Lock)
                {
                    lock (_frame.FrameLock)
                    {
                        DimLED();
                        foreach (var led in _device.Leds)
                        {
                            const int numberOfSteps = 15;
                            var stepx = Math.Max(1, (int)led.TransformedRect.Width / numberOfSteps);
                            var stepy = Math.Max(1, (int)led.TransformedRect.Height / numberOfSteps);
                            GetAverageColorOfRectangularRegion(led.TransformedRect, stepy, stepx,
                                out var sumR, out var sumG, out var sumB, out var count);

                            var countInverse = 1f / count;
                            var r = sumR * countInverse;
                            var g = sumG * countInverse;
                            var b = sumB * countInverse;
                            ApplySmoothing(
                                r,
                                g,
                                b,
                                out var R,
                                out var G,
                                out var B,
                                led.LED.Red,
                                led.LED.Green,
                                led.LED.Blue);
                            if (!_device.IsIdentifying)
                                led.LED.SetColor((byte)(R*_dimFactor), (byte)(G*_dimFactor), (byte)(B*_dimFactor));
                        }
                    }
                }

                Thread.Sleep(10);
            }
        }

        catch (Exception ex)
        {
            Log.Error(ex, ToString());
        }
        finally
        {
            GC.Collect();
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = null;
        _workerThread = null;
        GC.SuppressFinalize(this);
    }
    private void ApplySmoothing(float r, float g, float b, out byte semifinalR, out byte semifinalG,
        out byte semifinalB,
        byte lastColorR, byte lastColorG, byte lastColorB)
    {
        semifinalR = (byte)((r + _smoothFactor * lastColorR) / (_smoothFactor + 1));
        semifinalG = (byte)((g + _smoothFactor * lastColorG) / (_smoothFactor + 1));
        semifinalB = (byte)((b + _smoothFactor * lastColorB) / (_smoothFactor + 1));
    }

    private unsafe void GetAverageColorOfRectangularRegion(Rect spotRectangle, int stepy, int stepx,
        out int sumR, out int sumG,
        out int sumB, out int count)
    {
        sumR = 0;
        sumG = 0;
        sumB = 0;
        count = 0;

        var stepCount = (int)spotRectangle.Width / stepx;
        var stepxTimes4 = stepx * 4;
        for (var y = (int)spotRectangle.Top; y < spotRectangle.Bottom; y += stepy)
        {
            var index = 4 * _frame.FrameWidth * y + 4 * (int)spotRectangle.Left;

            fixed (byte* ptr = _frame.PixelData)
            {
                for (var i = 0; i < stepCount; i++)
                {
                    sumB += ptr[index];
                    sumG += ptr[index + 1];
                    sumR += ptr[index + 2];
                    index += stepxTimes4;
                }
            }

            count += stepCount;
        }
    }
}