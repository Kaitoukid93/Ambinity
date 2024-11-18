using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AmbinityCore.DataBase;
using AmbinityCore.LightingEngines;
using AmbinityCore.Models.GeneralSetting;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Repositories;
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
    public event Action<LightingProfile> CurrentPlayingProfileChanged;
    public event Action FrameUpdate;
    private float _bitmapDimFactor;
    private int _framerate;


    public LightingProfileDecoder(LightingProfileRepository repository, ColorEngineProvider colorEngineProvider,
        FrameBuffer buffer, GeneralSettingsManager generalSettingsManager)
    {
        _colorEngineProvider = colorEngineProvider;
        _repository = repository;
        _buffer = buffer;
        _generalSettings = generalSettingsManager?.Settings;
        _engines = new List<IColorEngine>();
    }

    private LightingProfileRepository _repository;
    private ColorEngineProvider _colorEngineProvider;
    private CancellationTokenSource _tokenSource;
    private IGeneralSettings _generalSettings;
    private bool _isRendering;
    public bool IsRendering => _isRendering;
    public LightingProfile CurrentPlayingProfile => _currentPlayingProfile;
    private LightingProfile _currentPlayingProfile;
    private FrameBuffer _buffer;
    private List<IColorEngine> _engines;
    private readonly AmbinityDeviceRepository _deviceRepository;
    public double[] FramesTime;

    private void LoadLastProfile()
    {
        if (_repository.Items == null || _repository.Items.Count == 0)
            return;
        if (_generalSettings.LastPlayedProfileID == null)
            return;
        var lastPlayedProfile =
            _repository.Items.Where(i => (i as LightingProfile).ID == _generalSettings.LastPlayedProfileID)
                .FirstOrDefault() as LightingProfile;
        if (lastPlayedProfile == null)
            lastPlayedProfile = _repository.Items.First() as LightingProfile;
        _currentPlayingProfile = lastPlayedProfile;
    }

    /// <summary>
    /// Init a profle ready to play
    /// </summary>
    /// <param name="profile"></param>
    // public void Init(LightingProfile profile)
    // {
    //     //create list engines for managing
    //     if (profile == null)
    //         return;
    //     _currentPlayingProfile = profile;
    //     Log.Information("Init profile: " + profile.Name);
    //     if (profile.Zones.Count == 0)
    //     {
    //         Log.Warning("Profile contains 0 zones!");
    //         //return;
    //     }
    //
    //     Resume();
    // }
    public void Init()
    {
        //only play if app tour is not activated, app tour is designed to work with nothing is playing,
        //so it can show user how to press the play button to render a profile
        if (!_generalSettings.ShowAppTour)
        {
            LoadLastProfile();
            Resume();
        }
    }

    /// <summary>
    /// Replay the profile
    /// </summary>
    private void Resume()
    {
        //todo reuse engines
        _framerate = _generalSettings.TargetFramerate;
        if (_framerate < 24)
            _framerate = 24;
        if (_currentPlayingProfile == null)
            return;
        var isRunning = _tokenSource != null && _isRendering;
        if (isRunning)
            return;
        Log.Information("Start rendering for profile: " + _currentPlayingProfile.Name);
        _tokenSource = new CancellationTokenSource();
        //clear buffer
        lock (_buffer.FrameLock)
        {
            _buffer.PixelData = new byte[_buffer.FrameWidth * _buffer.FrameHeight * 4];
        }

        _currentPlayingProfile.LoadAssets();
        FramesTime = new Double[_currentPlayingProfile.Zones.Count];
        int count = 0;
        foreach (var zone in _currentPlayingProfile.Zones)
        {
            zone.ParentProfile = _currentPlayingProfile;
            RegisterZone(zone, count++);
        }

        _currentPlayingProfile.IsPlaying = true;
        _isRendering = true;
        _generalSettings.LastPlayedProfileID = _currentPlayingProfile.ID;
        RenderingStatusChanged?.Invoke();
    }

    public async Task Toggle()
    {
        if (_isRendering)
            await Stop();
        else
        {
            Resume();
        }
    }

    /// <summary>
    /// Toggle specific profile
    /// </summary>
    /// <param name="profileID"></param>
    public async Task Toggle(Guid profileID)
    {
        if (_isRendering && _currentPlayingProfile.ID == profileID)
            await Stop();
        else
        {
            await Stop();
            if (_currentPlayingProfile != null && _currentPlayingProfile.ID == profileID)
            {
            }
            else
            {
                if (_repository.Items
                        .FirstOrDefault(i => (i as LightingProfile).ID == profileID) is not LightingProfile
                    targetProfile)
                    return;
                _currentPlayingProfile = targetProfile;
            }

            Resume();
            CurrentPlayingProfileChanged?.Invoke(_currentPlayingProfile);
        }
    }

    /// <summary>
    /// stop signal, call init to play again
    /// </summary>
    public async Task Stop()
    {
        if (_currentPlayingProfile == null)
            return;
        if (!_isRendering)
            return;
        //dim the bitmap 


        // for (int j = 0; j < 100; j++)
        // {
        //     lock (_buffer.FrameLock)
        //     {
        //         _bitmapDimFactor -= 0.01f;
        //         for (int i = 0; i < _buffer.PixelData.Length; i += 4)
        //         {
        //             _buffer.PixelData[i + 0] = (byte)(_buffer.PixelData[i + 0] * _bitmapDimFactor);
        //             _buffer.PixelData[i + 1] = (byte)(_buffer.PixelData[i + 1] * _bitmapDimFactor);
        //             _buffer.PixelData[i + 2] = (byte)(_buffer.PixelData[i + 2] * _bitmapDimFactor);
        //             _buffer.PixelData[i + 3] = 255;
        //         }
        //     }
        //
        //     await Task.Delay(10);
        // }


        //clear buffer

        _engines.Clear();
        _currentPlayingProfile.IsPlaying = false;
        await _tokenSource?.CancelAsync();
        _tokenSource = null;
        _isRendering = false;
        // lock (_buffer.FrameLock)
        // {
        //     _buffer.PixelData = new byte[_buffer.FrameWidth * _buffer.FrameHeight * 4];
        // }

        RenderingStatusChanged?.Invoke();
    }

    /// <summary>
    /// get corresponding lighting engine
    /// </summary>
    /// <param name="zone"></param>
    private void RegisterZone(LightingZone zone, int index)
    {
        var engine = _colorEngineProvider.GetEngine(zone);
        engine.Init(zone);
        var thread = new Thread(() => Render(engine, _tokenSource.Token, index))
        {
            IsBackground = true,
            Priority = ThreadPriority.BelowNormal,
            Name = "colorsweep"
        };
        thread.Start();
        _engines.Add(engine);
    }

    public void UnregisterZone(LightingZone zone)
    {
        var engine = _engines.Where(e => e.Zone == zone).FirstOrDefault();
        if (engine == null)
            return;
        engine.Dispose();
    }

    /// <summary>
    /// render activated child to canvas
    /// </summary>
    private void Render(IColorEngine engine, CancellationToken token, int index)
    {
        try
        {
            if (!engine.IsAvailable)
                return;
            Stopwatch sw = new Stopwatch();
            Stopwatch reportStopwatch = new Stopwatch();
            reportStopwatch.Start();
            long totalFrameTime = 0;
            while (!token.IsCancellationRequested)
            {
                sw.Restart();
                var brightness = _currentPlayingProfile.Brightness / 100d;
                _buffer.BrightnessFactor = brightness;
                engine.Render();
                sw.Stop();
                totalFrameTime = sw.ElapsedMilliseconds; // Accumulate the elapsed time
                if (reportStopwatch.ElapsedMilliseconds >= 100)
                {
                    reportStopwatch.Restart();
                    FramesTime[index] = Math.Round((double)totalFrameTime, 5);
                }

                FrameUpdate?.Invoke();
                Thread.Sleep(1000 / _framerate);
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
}