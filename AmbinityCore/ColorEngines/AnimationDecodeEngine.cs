using System.Runtime.InteropServices;
using AmbinityCore.CapturingService;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using Draw2D.Core.Graphic;
using SkiaSharp;
using SkiaSharp.Skottie;

namespace AmbinityCore.LightingEngines;

public class AnimationDecodeEngine : IColorEngine
{
    private int _startIndex = 0;
    public LightingZone Zone => _zone;
    private LightingZone _zone;
    private FrameBuffer _buffer;
    private string _animationFilePath = "C:\\Users\\AMBINO\\Downloads\\moving circle.json";
    private byte[] _reusableRow;
    private Animation _animation;
    private AnimationConfiguration _config;

    public AnimationDecodeEngine(FrameBuffer buffer)
    {
        _buffer = buffer;
    }

    public void Render()
    {
        int width = (int)_zone.Width;
        int height = (int)_zone.Height;
        lock (_buffer.FrameLock)
        {
            using (var bitmap = new SKBitmap(width, height))
            using (var canvas = new SKCanvas(bitmap))
            {
                // Set up the Lottie animation renderer
                _animation.SeekFrame(_startIndex);
                var dst = new SKRect(0, 0, width, height);
                // Render the frame
                _animation.Render(canvas, dst);

                // Get the pixel data
                var pixelData = bitmap.Bytes;
                int length = (int)bitmap.Width * 4;
                for (int i = 0; i < bitmap.Height; i++)
                {
                    int start = (_buffer.FrameWidth * 4) * (i + (int)_zone.Y) + (int)_zone.X * 4;
                    int startSource = i * bitmap.Width * 4;
                    Array.Copy(pixelData, startSource, _buffer.PixelData, start, length);
                }
            }
        }

       // Thread.Sleep(1000 / 10);
        //increase color index
        _startIndex += 1;
        if (_startIndex >= 200)
            _startIndex = 0;
        //update frame if needed
        // _zone.UpdateFrame();
    }

    public void Init(LightingZone zone)
    {
        _zone = zone;
        _config = zone.LightingConfiguration as AnimationConfiguration;
        //do render
        // Load the Lottie animation
        var json = File.ReadAllText(_animationFilePath);
        _animation = Animation.Parse(File.ReadAllText(_animationFilePath));
        _config.Animation = _animation;
        // Create a SkiaSharp canvas

    }

    public void Dispose()
    {
        // throw new NotImplementedException();
    }

    public bool IsDisposed { get; }
    public CapturingType CaptureType => CapturingType.None;
}