
using AmbinityCore.CapturingService.AudioCapturing;
using AmbinityCore.DataBase;
using ManagedBass;
//
using Serilog;

namespace AmbinityCore.CapturingService;

public class AudioCapturingService : ICapturingService
{
    public event Action DefaultDeviceChanged;
    public bool IsEnabled { get; private set; }
    private bool _bassInitialized;
    private int _defaultDeviceID;
   // private WasapiProcedure? _process;
   // private WasapiNotifyProcedure? _notifyProc;
    private List<AudioCaptureBasic> _availableAudioCapture;
    public int DefaultDeviceID => _defaultDeviceID;
    public AudioBuffer Buffer => _buffer;
    private AudioBuffer _buffer;
    private int _eventCounter;
    private int _activeVisualizerCount;
    private int _activeBrightnessProviderCount;
    public event Action VisualizerUpdate;
    public List<AudioCaptureBasic> AvalableAudioCaptures => _availableAudioCapture;

    public AudioCapturingService(BassAudioDeviceEnumerationService enumerationService,
        GeneralSettingsManager settingsManager)
    {
        IsEnabled = settingsManager.Settings.EnableAudioCapture&&!OperatingSystem.IsMacOS();
        _enumerationService = enumerationService;
        _availableAudioCapture = new List<AudioCaptureBasic>();
       // _process = new WasapiProcedure(Process);
       // _notifyProc = new WasapiNotifyProcedure(WasapiNotifyProc);
        Init();
    }

    private int Process(IntPtr buffer, int length, IntPtr user)
    {
        return length;
    }

   /* private void WasapiNotifyProc(WasapiNotificationType notify, int device, IntPtr user)
    {
        switch (notify)
        {
            case WasapiNotificationType.Enabled:
                Log.Information("Wasapi device notification - Device enabled: " + device);
                _eventCounter++;
                OnAudioDeviceChanged();
                break;
            case WasapiNotificationType.Disabled:
                Log.Information("Wasapi device notification - Device disabled: " + device);
                _eventCounter++;
                OnAudioDeviceChanged();
                break;
            case WasapiNotificationType.DefaultInput:
                Log.Information("Wasapi device notification - Default input device: " + device);
                _eventCounter++;
                OnAudioDeviceChanged();
                break;
            case WasapiNotificationType.DefaultOutput:
                Log.Information("Wasapi device notification - Default output device: " + device);
                _eventCounter++;
                OnAudioDeviceChanged();
                break;
            case WasapiNotificationType.Fail:
                Log.Information("Wasapi device notification - Device failed: " + device);
                _eventCounter++;
                OnAudioDeviceChanged();
                break;
            default:
                Log.Information("Wasapi device - Unknown notification: " + notify);
                break;
        }
    }*/

    private bool _handlingDeviceChanged;

    private async void OnAudioDeviceChanged()
    {
        Log.Information("Event count: " + _eventCounter);
        var counter = _eventCounter;
        await Task.Run(() => Task.Delay(2000));
        if (counter == _eventCounter)
        {
            //could be user change, process event
            Log.Information("Event count: " + _eventCounter);
            Log.Information("Reseting audio capture... ");
            _eventCounter = 0;
            OnDeviceChanged();
        }
        else
        {
            // we got race condition
            Log.Information("Race condition: " + (_eventCounter - counter));
        }
    }

    private void OnDeviceChanged()
    {
        // basswasapi does not like handling in deletage callback
        if (_handlingDeviceChanged)
            return;
        _handlingDeviceChanged = true;
        Task.Run(() =>
        {
            Dispose();
            Init();
            DefaultDeviceChanged?.Invoke();
            _handlingDeviceChanged = false;
           // Log.Information("Done!" + " New default device ID is: " + _defaultDeviceID + " " +
                           // BassWasapi.GetDeviceInfo(_defaultDeviceID).Name);
        });
    }

    private BassAudioDeviceEnumerationService _enumerationService;
    private CancellationTokenSource _cancellationTokenSource;

    public void Init()
    {
        if (!IsEnabled)
            return;
        BASSInit();
        var devices = _enumerationService.GetAvailableAudioDevices();
        UpdateDefaultEndpoint();

       // _buffer = new AudioBuffer(BassWasapi.DeviceCount);
        _availableAudioCapture.Clear();
        foreach (var device in devices)
        {
            _availableAudioCapture.Add(new AudioCaptureBasic(device, _buffer));
        }

        _cancellationTokenSource = new CancellationTokenSource();

        Thread thread = new Thread(() => { Capture(_cancellationTokenSource.Token); })
        {
            IsBackground = true,
            Priority = ThreadPriority.BelowNormal,
            Name = "Wasapi " + "Capture"
        };
        thread.Start();
    }

    public void RegisterVisualizer()
    {
        _activeVisualizerCount++;
    }

    public void UnregisterVisualizer()
    {
        if (_activeVisualizerCount > 0)
            _activeVisualizerCount--;
    }

    public void RegisterBrightnessProvider()
    {
        _activeBrightnessProviderCount++;
    }

    public void UnRegisterBrightnessProvider()
    {
        if (_activeBrightnessProviderCount > 0)
            _activeBrightnessProviderCount--;
    }

    private void Capture(CancellationToken token)
    {
        //init each device
        //todo lock until all device is init
        foreach (var device in _availableAudioCapture)
        {
            //device.Init(_process);
        }

        while (!token.IsCancellationRequested)
        {
            if (_activeBrightnessProviderCount > 0 || _activeVisualizerCount > 0)
            {
                foreach (var device in _availableAudioCapture)
                {
                    device.StartBassWasapi();
                    device.Capture();
                }

                if (_activeVisualizerCount > 0)
                    VisualizerUpdate?.Invoke();
                Thread.Sleep(1000 / 40);
            }
            else
            {
                Thread.Sleep(1000);
            }
        }
    }

    public void UpdateDefaultEndpoint()
    {
        var defaultDevice = _enumerationService.GetDefaultAudioDevice();
        if (defaultDevice != null)
        {
            _defaultDeviceID = defaultDevice.ID;
        }
    }

    private void BASSInit()
    {
        //BassWasapi.BASS_WASAPI_Free();
        //Bass.BASS_Free();
        if (_bassInitialized)
            return;
         Bass.Configure(Configuration.UpdateThreads, false);
        Bass.Configure(Configuration.IncludeDefaultDevice, true);
        //BassWasapi.SetNotify(_notifyProc, IntPtr.Zero);
        var result = Bass.Init(0, 44100, DeviceInitFlags.Default, IntPtr.Zero);
        if (result)
        {
            _bassInitialized = true;
        }

        if (!result) throw new Exception("Init Error");
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = null;
        foreach (var capture in _availableAudioCapture)
        {
            capture.Dispose();
        }
    }
}