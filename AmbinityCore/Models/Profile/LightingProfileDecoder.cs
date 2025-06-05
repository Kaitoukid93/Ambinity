
using AmbinityCore.DataBase;
using AmbinityCore.LightingEngines;
using AmbinityCore.Models.GeneralSetting;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Repositories;
using Draw2D.Core.Graphic;
using Serilog;
using Timer = System.Timers.Timer;

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
    public bool ShouldUpdateFrame { get; set; }

    public LightingProfileDecoder(LightingProfileRepository repository, ColorServiceProvider colorServiceProvider,
        FrameBuffer buffer, GeneralSettingsManager generalSettingsManager)
    {
        _colorServiceProvider = colorServiceProvider;
        _repository = repository;
        _buffer = buffer;
        _generalSettings = generalSettingsManager?.Settings;
    }

    private LightingProfileRepository _repository;
    private ColorServiceProvider _colorServiceProvider;
    private CancellationTokenSource? _tokenSource;
    private IGeneralSettings _generalSettings;
    private bool _isRendering;
    public bool IsRendering => _isRendering;
    public LightingProfile CurrentPlayingProfile => _currentPlayingProfile;
    private LightingProfile _currentPlayingProfile;
    private FrameBuffer _buffer;
    private ColorServiceCollection _colorServiceCollection = new ColorServiceCollection();
    private readonly AmbinityDeviceRepository _deviceRepository;
    public double[] FramesTime;

    private void LoadLastProfile()
    {
        if (_repository.Items == null || _repository.Items.Count == 0)
            return;

        else if (_generalSettings.LastPlayedProfileID == Guid.Empty || _generalSettings.LastPlayedProfileID == Guid.Empty)
        {
            //this happens when application is installed for the first time
            var neon = _repository.Items
                 .Where(i => (i as LightingProfile).Name == "Neon")
                 .FirstOrDefault() as LightingProfile;
            if (neon == null) // something seriously wrong with the databasse
                return;
            _generalSettings.LastPlayedProfileID = neon.ID;

        }
        var lastPlayedProfile =
            _repository.Items.Where(i => (i as LightingProfile).ID == _generalSettings.LastPlayedProfileID)
                .FirstOrDefault() as LightingProfile;
        if (lastPlayedProfile == null)
            lastPlayedProfile = _repository.Items.First() as LightingProfile;
        _currentPlayingProfile = lastPlayedProfile;
    }

    public void Init()
    {
        //only play if app tour is not activated, app tour is designed to work with nothing is playing,
        //so it can show user how to press the play button to render a profile
        // if (!_generalSettings.ShowAppTour)
        // {
        LoadLastProfile();
        Resume();
        // }
    }

    /// <summary>
    /// Resume the profile, only work if there is a previous profile already loaded
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
            Thread.Sleep(5);
        }
        var thread = new Thread(() => Render(_tokenSource.Token))
        {
            IsBackground = true,
            Priority = ThreadPriority.BelowNormal,
            Name = "Profile Decoder"
        };
        thread.Start();
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

        _colorServiceCollection.Clear();
        _currentPlayingProfile.IsPlaying = false;

        // Clear the buffer
        lock (_buffer.FrameLock)
        {
            Array.Clear(_buffer.PixelData, 0, _buffer.PixelData.Length);
            if (ShouldUpdateFrame)
                FrameUpdate?.Invoke();
        }

        if (_tokenSource != null)
        {
            await _tokenSource.CancelAsync();
            _tokenSource = null;
        }
        _isRendering = false;
        RenderingStatusChanged?.Invoke();
    }

    /// <summary>
    /// get corresponding lighting engine
    /// todo make a hash set for this
    /// </summary>
    /// <param name="zone"></param>
    private void RegisterZone(LightingZone zone, int index)
    {
        var service = _colorServiceProvider.GetService(zone);
        service.Init(zone);
        _colorServiceCollection.Add(service);
    }

    public void UnregisterZone(LightingZone zone)
    {
        var service = _colorServiceCollection.GetByZone(zone);
        if (service == null)
            return;
        service.Dispose();
    }

    /// <summary>
    /// render activated child to canvas
    /// </summary>
    private void Render(CancellationToken token)
    {
        try
        {


            // Stopwatch sw = new Stopwatch();
            // Stopwatch reportStopwatch = new Stopwatch();
            // reportStopwatch.Start();
            long totalFrameTime = 0;

            Timer timer = new Timer(1000 / _framerate);
            timer.Elapsed += (sender, e) =>
            {
                if (token.IsCancellationRequested)
                {
                    timer.Stop();
                    timer.Dispose();
                    return;
                }

                // sw.Restart();
                var brightness = _currentPlayingProfile.Brightness / 100d;
                _buffer.BrightnessFactor = brightness;
                foreach (var service in _colorServiceCollection.GetAll())
                {
                    if (!service.IsAvailable)
                        continue;
                    service.Render();
                }
                if (ShouldUpdateFrame)
                    FrameUpdate?.Invoke();
            };

            timer.Start();

            // Wait for the cancellation token to be triggered
            token.WaitHandle.WaitOne();
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
        }
        finally
        {
            foreach (var service in _colorServiceCollection.GetAll())
            {
                service.Dispose();
            }

            GC.Collect();
        }
    }
}
