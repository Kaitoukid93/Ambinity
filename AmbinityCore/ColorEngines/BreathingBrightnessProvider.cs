using AmbinityCore.Models.Lighting.Zone.Configuration;
using Serilog;

namespace AmbinityCore.LightingEngines;

public class BreathingBrightnessProvider : IBrightnessProvider
{
    /// <summary>
    /// breathing constant value
    /// </summary>
    float gamma = 0.14f; // affects the width of peak (more or less darkness)

    float beta = 0.5f; // shifts the gaussian to be symmetric
    float ii = 0f;
    private float _brightness;

    public BreathingBrightnessProvider(IMotionConfiguration config)
    {
        _config = config as BreathingMotionConfiguration;
        _config.SpeedUpdate += OnSpeedUpdate;
        _config.SystemSyncUpdate += OnSystemSyncUpdate;
        OnSpeedUpdate();
        OnSystemSyncUpdate();
        Activate();
    }

    private void OnSystemSyncUpdate()
    {
        _isSystemSync = _config.IsSystemSync;
    }

    private void OnSpeedUpdate()
    {
        _breathingSpeed = 2002 - _config.BreathingSpeed;
    }

    private float _breathingSpeed = 1800;
    private BreathingMotionConfiguration _config;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _isSystemSync;

    public void GetBrightness(float[] reusableArray)
    {
        for (int i = 0; i < reusableArray.Length; i++)
        {
            reusableArray[i] = _brightness * 255f;
        }
    }

    public void Activate()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        Thread thread = new Thread(() => { Breath(_cancellationTokenSource.Token); })
        {
            IsBackground = true,
            Priority = ThreadPriority.BelowNormal,
            Name = "Breathing"
        };
        thread.Start();
    }

    public void Deactivate()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = null;
    }

    private void Breath(CancellationToken token)
    {
        Log.Information("Breathing brightness provider activated");
        while (!token.IsCancellationRequested)
        {
            if (_isSystemSync)
            {
                ///get global brightness here
            }

            float smoothness_pts = _breathingSpeed;
            float pwm_val = 255.0f * (1.0f - Math.Abs((2.0f * (ii++ / smoothness_pts)) - 1.0f));
            if (ii > smoothness_pts)
                ii = 0f;
            _brightness = pwm_val / 255f;
            Thread.Sleep(10);
        }
    }
}