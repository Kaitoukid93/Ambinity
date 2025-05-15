using System.Runtime.InteropServices;
using AmbinityCore.CapturingService;
using AmbinityCore.ColorEngines;
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

    private byte[] _reusableRow;
    private IAnimation _animation;
    private AnimationConfiguration _config;
    private bool _loadingAnimation;
    private int _frameRate = 1;
    private readonly AnimationsRepository _animationRepository;
    private AnimationsRepository _currentWorkingRepository;
    private IAnimationStreamDecoder _streamDecoder;

    // Class-level fields for SKImageInfo and SKBitmap
    private SKImageInfo _imageInfo;
    private SKBitmap _bitmap;

    public AnimationDecodeEngine(FrameBuffer buffer, AnimationsRepository repository)
    {
        _animationRepository = repository;
        _buffer = buffer;
    }

    public void Render()
    {
        if (_loadingAnimation)
            return;

        try
        {
            int width = (int)_zone.Width;
            int height = (int)_zone.Height;
            var dst = new SKRect(0, 0, width, height);

            lock (_buffer.FrameLock)
            {
                // Use the pre-initialized bitmap
                var result = _streamDecoder.TryDecodeNextFrame(_bitmap, dst);
                if (result == false)
                    return;
                var pixelData = _bitmap.Bytes;
                int length = (int)_bitmap.Width * 4;

                for (int i = 0; i < _bitmap.Height; i++)
                {
                    int start = (_buffer.FrameWidth * 4) * (i + (int)_zone.Y) + (int)_zone.X * 4;
                    int startSource = i * _bitmap.Width * 4;
                    Array.Copy(pixelData, startSource, _buffer.PixelData, start, length);
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while rendering animation");
        }
    }

    public void Init(LightingZone zone)
    {
        _zone = zone;
        _config = zone.LightingConfiguration as AnimationConfiguration;
        if (_zone.ParentProfile.Assets.Count == 0 || _zone.ParentProfile.AnimationRepository == null)
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

        // Resolve animation from repository
        var animation = _currentWorkingRepository.FindAnimation(_config.AnimationUID);
        if (animation == null)
        {
            Log.Error("Resource not found in Animation Repository");
            return;
        }

        animation.LoadAnimation();

        // Dispose of the previous stream decoder and bitmap
        _streamDecoder?.Dispose();
        _bitmap?.Dispose();

        // Initialize a stream decoder based on the animation type
        if (animation is LottieJsonAnimation)
        {
            _streamDecoder = new LottieAnimationStreamDecoder(animation);
        }
        else if (animation is GifAnimation)
        {
            _streamDecoder = new GifAnimationStreamDecoder(animation, _zone);
        }
        int width = (int)_zone.Width;
        int height = (int)_zone.Height;
        _imageInfo = new SKImageInfo(width, height, SKColorType.Bgra8888);
        _bitmap = new SKBitmap(_imageInfo);
        _loadingAnimation = false;
    }

    public void Dispose()
    {
        _config.AnimationChanged -= OnAnimationChanged;
        _streamDecoder?.Dispose();
        _bitmap?.Dispose();
    }

    public bool IsDisposed { get; }
}
