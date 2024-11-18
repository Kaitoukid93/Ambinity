using System.Runtime.InteropServices;
using AmbinityCore.CapturingService;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Repositories;
using Draw2D.Core.Graphic;
using Serilog;
using SkiaSharp;
using Animation = SkiaSharp.Skottie.Animation;

namespace AmbinityCore.LightingEngines;

public class AnimationDecodeEngine : IColorEngine
{
    private int _startIndex = 0;
    public LightingZone Zone => _zone;
    private LightingZone _zone;
    public bool IsAvailable { get; private set; } = true;
    private FrameBuffer _buffer;

    // private string _animationFilePath = "C:\\Users\\AMBINO\\Downloads\\moving circle.json";
    private byte[] _reusableRow;
    private Animation _animation;
    private AnimationConfiguration _config;
    private bool _loadingAnimation;
    private int _frameRate = 1;
    private readonly AnimationsRepository _animationRepository;
    private  AnimationsRepository _currentWorkingRepository;

    public AnimationDecodeEngine(FrameBuffer buffer, AnimationsRepository repository)
    {
        _animationRepository = repository;
        _buffer = buffer;
    }

    public void Render()
    {
        if (_loadingAnimation)
            return;
        int width = (int)_zone.Width;
        int height = (int)_zone.Height;
        int frameCount = (int)(_animation.Fps * _animation.Duration.TotalMilliseconds / 1000);
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
        _startIndex += _config.FrameRate;
        ;
        if (_startIndex >= frameCount)
            _startIndex = 0;
        //update frame if needed
        // _zone.UpdateFrame();
    }

    public void Init(LightingZone zone)
    {
        _zone = zone;
        _config = zone.LightingConfiguration as AnimationConfiguration;
        if (_zone.ParentProfile.Assets.Count == 0 || _zone.ParentProfile.AnimationRepository==null)
        {
            _currentWorkingRepository = _animationRepository;
        }
        else
        {
            _currentWorkingRepository = _zone.ParentProfile.AnimationRepository;
        }
        _config.AnimationChanged += OnAnimationChanged;
        OnAnimationChanged();
    }

    private void OnAnimationChanged()
    {
        if (_config.AnimationUID == null || _config.AnimationUID == Guid.Empty)
            return;
        _loadingAnimation = true;
        //resolve animation from repo
        var animation = _currentWorkingRepository.FindAnimation(_config.AnimationUID);
        if (animation == null)
        {
            Log.Error("Resource not found in Animation Repository");
            return;
        }

        animation.LoadAnimation();
        _animation = animation.SkottieAnimation;
        _loadingAnimation = false;
    }

    public void Dispose()
    {
        _config.AnimationChanged -= OnAnimationChanged;
    }

    public bool IsDisposed { get; }
}