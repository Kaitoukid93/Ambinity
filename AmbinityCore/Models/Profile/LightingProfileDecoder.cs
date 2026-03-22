using AmbinityCore.LightingEngines;
using AmbinityCore.Models.GeneralSetting;
using AmbinityCore.Models.Lighting.Zone;
using Draw2D.Core.Graphic;
using Serilog;

namespace AmbinityCore.Models.Profile;

public class LightingProfileDecoder
{
    public event Action? RenderingStatusChanged;
    public event Action<string>? CurrentPlayingProfileChanged;
    public event Action? FrameUpdate;

    private readonly ColorServiceProvider _colorServiceProvider;
    private readonly FrameBuffer _buffer;
    private readonly IGeneralSettings _settings;

    private readonly ColorServiceManager _colorServiceManager = new();

    private CancellationTokenSource? _cts;
    private bool _isRendering;

    public bool IsRendering => _isRendering;

    private LightingProfile? _currentData;
    private string _currentProfileId;

    public LightingProfileDecoder(
        ColorServiceProvider colorServiceProvider,
        FrameBuffer buffer,
        IGeneralSettings settings)
    {
        _colorServiceProvider = colorServiceProvider;
        _buffer = buffer;
        _settings = settings;
    }

    // 🔥 MAIN ENTRY: play from item
    public async Task PlayAsync(LightingProfileItem item)
    {
        await StopAsync();

        var data = await item.GetDataAsync();

        _currentData = data;
        _currentProfileId = item.Id;

        await StartRenderingAsync();
    }

    public async Task StopAsync()
    {
        if (!_isRendering)
            return;

        _cts?.Cancel();
        _cts = null;

        _colorServiceManager.Clear();

        lock (_buffer.FrameLock)
        {
            Array.Clear(_buffer.PixelData, 0, _buffer.PixelData.Length);
            FrameUpdate?.Invoke();
        }

        _isRendering = false;
        RenderingStatusChanged?.Invoke();

        await Task.CompletedTask;
    }

    public async Task ToggleAsync(LightingProfileItem item)
    {
        if (_isRendering && item.Id == _currentProfileId)
        {
            await StopAsync();
        }
        else
        {
            await PlayAsync(item);
            CurrentPlayingProfileChanged?.Invoke(item.Id);
        }
    }

    // 🔥 INTERNAL

    private async Task StartRenderingAsync()
    {
        if (_currentData == null)
            return;

        Log.Information($"Start rendering profile: {_currentData.Name}");

        _cts = new CancellationTokenSource();

        InitBuffer();
        InitZones(_currentData);

        _isRendering = true;
        _settings.LastPlayedProfileID = _currentProfileId;

        RenderingStatusChanged?.Invoke();

        await Task.Run(() => RenderLoop(_cts.Token));
    }

    private void InitBuffer()
    {
        lock (_buffer.FrameLock)
        {
            _buffer.PixelData = new byte[_buffer.FrameWidth * _buffer.FrameHeight * 4];
        }
    }

    private void InitZones(LightingProfile data)
    {
        _colorServiceManager.Clear();

        foreach (var zone in data.Zones)
        {
            var service = _colorServiceProvider.GetService(zone);
            service.Init(zone);
            _colorServiceManager.Add(service);
        }
    }

    private void RenderLoop(CancellationToken token)
    {
        try
        {
            var fps = Math.Max(_settings.TargetFramerate, 24);
            var interval = TimeSpan.FromMilliseconds(1000.0 / fps);

            while (!token.IsCancellationRequested)
            {
                var start = DateTime.UtcNow;

                RenderFrame();

                var elapsed = DateTime.UtcNow - start;
                var delay = interval - elapsed;

                if (delay > TimeSpan.Zero)
                    Thread.Sleep(delay);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
        }
        finally
        {
            Cleanup();
        }
    }

    private void RenderFrame()
    {
        if (_currentData == null)
            return;

        var brightness = _currentData.Brightness / 100d;
        _buffer.BrightnessFactor = brightness;

        foreach (var service in _colorServiceManager.GetAll())
        {
            if (!service.IsAvailable)
                continue;

            service.Render();
        }

        FrameUpdate?.Invoke();
    }

    private void Cleanup()
    {
        foreach (var service in _colorServiceManager.GetAll())
        {
            service.Dispose();
        }

        _colorServiceManager.Clear();
        GC.Collect();
    }
}
