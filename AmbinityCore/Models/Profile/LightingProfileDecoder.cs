using System.Buffers;
using System.Runtime.InteropServices;
using AmbinityCore.LightingEngines;
using AmbinityCore.Models.Lighting.Zone;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Draw2D.Core.Graphic;
using Serilog;

namespace AmbinityCore.Models.Profile;

/// <summary>
/// Represent a working class that decode a profile, render all the zones in the canvas, managing
/// play, pause, resume of the profile and child zones
/// this class is singleton and re-init everytime user play a profile
/// </summary>
public class LightingProfileDecoder
{
    public event Action RenderingStatusChanged;
    public LightingProfileDecoder(LightingProfileRepository repository, ColorEngineProvider colorEngineProvider, FrameBuffer buffer)
    {
        _colorEngineProvider = colorEngineProvider;
        _repository = repository;
        var lastPlayedProfile =
            repository.Items.Where(i => (i as LightingProfile).IsPlaying == true).FirstOrDefault() as LightingProfile;
        if (lastPlayedProfile != null)
            Init(lastPlayedProfile);
        _buffer = buffer;
    }

    private LightingProfileRepository _repository;
    private ColorEngineProvider _colorEngineProvider;
    private CancellationTokenSource _tokenSource;
    private bool _isRendering;
    public bool IsRendering => _isRendering;
    public LightingProfile CurrentPlayingProfile => _currentPlayingProfile;
    private LightingProfile _currentPlayingProfile;
    private FrameBuffer _buffer;

    public void Init(LightingProfile profile)
    {
        var isRunning = _tokenSource != null && _isRendering;
        if (isRunning)
            return;
        if (profile == null)
            return;
        Log.Information("Start Rendering");
        _tokenSource = new CancellationTokenSource();
        _currentPlayingProfile = profile;
        _currentPlayingProfile.IsPlaying = true;
        foreach (var zone in profile.Zones)
        {
            RegisterZone(zone);
        }

        _isRendering = true;
        RenderingStatusChanged?.Invoke();
    }

    /// <summary>
    /// Replay the profile
    /// </summary>
    public void Resume()
    {
        if (CurrentPlayingProfile == null)
            return;
        var isRunning = _isRendering;
        if (isRunning)
            return;
        Log.Information("Start Rendering");
        _tokenSource = new CancellationTokenSource();
        //clear buffer
        lock (_buffer.FrameLock)
        {
            _buffer.PixelData = new byte[_buffer.FrameWidth * _buffer.FrameHeight * 4];
        }
        foreach (var zone in CurrentPlayingProfile.Zones)
        {
            RegisterZone(zone);
        }

        _currentPlayingProfile.IsPlaying = true;
        _isRendering = true;
        RenderingStatusChanged?.Invoke();
    }

    /// <summary>
    /// stop signal, call init to play again
    /// </summary>
    public void Stop()
    {
        if(_currentPlayingProfile==null)
            return;
        //clear buffer
        lock (_buffer.FrameLock)
        {
            _buffer.PixelData = new byte[_buffer.FrameWidth * _buffer.FrameHeight * 4];
        }
        _currentPlayingProfile.IsPlaying = false;
        if (!_isRendering)
            return;
        _tokenSource?.Cancel();
        _tokenSource = null;
        _isRendering = false;
        RenderingStatusChanged?.Invoke();
    }

    /// <summary>
    /// get corresponding lighting engine
    /// </summary>
    /// <param name="zone"></param>
    public void RegisterZone(LightingZone zone)
    {
        var engine = _colorEngineProvider.GetEngine(zone);
        engine.Init(zone);
        var thread = new Thread(() => Render(engine, _tokenSource.Token))
        {
            IsBackground = true,
            Priority = ThreadPriority.BelowNormal,
            Name = "colorsweep"
        };
        thread.Start();
    }

    /// <summary>
    /// render activated child to canvas
    /// </summary>
    public void Render(IColorEngine engine, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                engine.Render();
                Thread.Sleep(1000 / 30);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
        }
        finally
        {
            engine.Dispose();
            GC.Collect();
        }
    }


    public WriteableBitmap CreateBitmapFromPixelData(
        byte[] buffer,
        int pixelWidth,
        int pixelHeight)
    {
        // Standard may need to change on some devices 
        Vector dpi = new Vector(96, 96);

        var bitmap = new WriteableBitmap(
            new PixelSize(pixelWidth, pixelHeight),
            dpi,
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);

        using (var frameBuffer = bitmap.Lock())
        {
            Marshal.Copy(buffer, 0, frameBuffer.Address, buffer.Length);
        }

        return bitmap;
    }
}