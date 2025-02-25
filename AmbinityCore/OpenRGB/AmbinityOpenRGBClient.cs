using System.Diagnostics;
using AmbinityCore.DataBase;
using AmbinityCore.Models.GeneralSetting;
using OpenRGB.NET;
using Polly;
using Polly.Retry;
using Serilog;

namespace AmbinityCore.OpenRGB;

/// <summary>
/// wraper for openrgb net client
/// </summary>
public class AmbinityOpenRGBClient
{
    private OpenRgbClient _client;
    public OpenRgbClient OpenRGBClient => _client;
    private readonly AsyncRetryPolicy _retryPolicy;
    private bool _isInitialize;
    public event Action DeviceListUpdated;
    public object Lock { get; } = new object();

    public AmbinityOpenRGBClient(OpenRGBService service)
    {
        _openRGBService = service;
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(10, _ => TimeSpan.FromSeconds(5));
    }

    public bool IsInitialized
    {
        get { return _isInitialize; }
        private set { _isInitialize = value; }
    }

    private bool _isInitializing;
    private readonly OpenRGBService _openRGBService;

    public bool IsInitializing
    {
        get { return _isInitializing; }
        private set { _isInitializing = value; }
    }

    public async Task Init()
    {
        //this client is being hold by another process
        if (IsInitializing) return;
        //turn on this flag to preven multiple thread access
        IsInitializing = true;


        if (!IsInitialized) // Only run OpenRGB Stream if User enable OpenRGB Utilities in General Settings
        {
            //check if OpenRGB process is alive
            if (!isRunning("OpenRGB"))
            {
                //start openRGB process
                _openRGBService.StartOpenRGBProcess();
                //wait for openRGB to start
                await Task.Delay(5000);
            }

            try
            {
                await _retryPolicy.ExecuteAsync(async () => await Connect());
                //OnDeviceListUpdated(this, EventArgs.Empty);
                IsInitialized = true;
                IsInitializing = false;
            }
            catch (TimeoutException)
            {
                // HandyControl.Controls.MessageBox.Show(
                //     "Không tìm thấy Server OpenRGB, Hãy thử thoát ứng dụng và mở lại");
                IsInitialized = false;
                IsInitializing = false;
                //IsAvailable= false;
            }
            catch (System.Net.Sockets.SocketException)
            {
                // HandyControl.Controls.MessageBox.Show(
                //     "Mất kết nối ứng dụng OpenRGB, vui lòng không thoát OpenRGB khi đang sử dụng");
                IsInitialized = false;
                IsInitializing = false;
                //IsAvailable= false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OpenRGB External Exception");
                IsInitialized = false;
                IsInitializing = false;
            }
        }
    }

    private async Task Connect() //init
    {
        if (_client != null)
        {
            _client.Dispose();
            _client.DeviceListUpdated -= OnDeviceListUpdatedFromServer;
        }

        _client = new OpenRgbClient(name: "Ambinity", timeoutMs: 1000, autoConnect: false);
        _client.DeviceListUpdated += OnDeviceListUpdatedFromServer;
        try
        {
            _client.Connect();
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
            throw;
        }
    }

    private int _eventCounter;
    private bool _isUpdatingDeviceList;

    private async void OnDeviceListUpdate()
    {
        if (_isUpdatingDeviceList)
        {
            Log.Information("Race condition: " + (_eventCounter));
            return;
        }

        _isUpdatingDeviceList = true;
        Log.Information("Event count: " + _eventCounter);
        var counter = _eventCounter;
        await Task.Run(() => Task.Delay(2000));
        //could be user change, process event
        Log.Information("Updating device list... ");
        _eventCounter = 0;
        DeviceListUpdated?.Invoke();
        _isUpdatingDeviceList = false;
    }

    private void OnDeviceListUpdatedFromServer(object? sender, EventArgs e)
    {
        _eventCounter++;
        OnDeviceListUpdate();
    }

    private static bool isRunning(string name)
    {
        try
        {
            var processes = Process.GetProcessesByName(name);
            if (processes.Count() == 0 || processes == null)
                return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}