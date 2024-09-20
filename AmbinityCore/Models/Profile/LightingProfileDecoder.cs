using System.Buffers;
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
    public event Action FrameUpdate;


    public LightingProfileDecoder(LightingProfileRepository repository, ColorEngineProvider colorEngineProvider,
        FrameBuffer buffer, AmbinityDeviceRepository deviceRepository, GeneralSettingsManager generalSettingsManager)
    {
        _deviceRepository = deviceRepository;
        _colorEngineProvider = colorEngineProvider;
        _repository = repository;
        _buffer = buffer;
        _generalSettings = generalSettingsManager?.Settings;
        _engines = new List<IColorEngine>();
        //only play if app tour is not activated, app tour is designed to work with nothing is playing,
        //so it can show user how to press the play button to render a profile
        if (!_generalSettings.ShowAppTour)
        {
            LoadLastProfile();
            Resume();
        }
     
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

    private void LoadLastProfile()
    {
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
    public void Init(LightingProfile profile)
    {
        //create list engines for managing
        if (profile == null)
            return;
        _currentPlayingProfile = profile;
        Log.Information("Init profile: " + profile.Name);
        if (profile.Zones.Count == 0)
        {
            Log.Warning("Profile contains 0 zones!");
            //return;
        }
        Resume();
    }

    /// <summary>
    /// Replay the profile
    /// </summary>
    public void Resume()
    {
        //todo reuse engines
        if (CurrentPlayingProfile == null)
            return;
        _deviceRepository.UpdateDeviceTransform();
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

        foreach (var zone in CurrentPlayingProfile.Zones)
        {
            RegisterZone(zone);
        }

        _currentPlayingProfile.IsPlaying = true;
        _isRendering = true;
        _generalSettings.LastPlayedProfileID = _currentPlayingProfile.ID;
        RenderingStatusChanged?.Invoke();
    }

    public void Toggle()
    {
        if (_isRendering)
            Stop();
        else
        {
            Resume();
        }
    }

    /// <summary>
    /// stop signal, call init to play again
    /// </summary>
    public void Stop()
    {
        if (_currentPlayingProfile == null)
            return;
        //clear buffer
        lock (_buffer.FrameLock)
        {
            _buffer.PixelData = new byte[_buffer.FrameWidth * _buffer.FrameHeight * 4];
        }

        foreach (var engine in _engines)
        {
            engine.Dispose();
        }
        _engines.Clear();
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
    private void RegisterZone(LightingZone zone)
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
    private void Render(IColorEngine engine, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                engine.Render();
                Thread.Sleep(1000 / 30);
                FrameUpdate?.Invoke();
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