using System.Diagnostics;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.OpenRGB;
using OpenRGB.NET;
using Polly;
using Polly.Retry;
using Serilog;

namespace AmbinityCore.Models.Device.Service;

public class OpenRGBControllerDiscoveryService
{
    /// <summary>
    /// Continuously searching for new device in background
    /// </summary>
    public event Action<OpenRGBController> NewDevicesFound;

    public event Action<string> NewComportDetected;

    private AmbinityOpenRGBClient _client;

    public OpenRGBControllerDiscoveryService(AmbinityOpenRGBClient client)
    {
        _client = client;
    }

    private async Task UpdateDeviceList()
    {
        //update device list here
        await _client.Init();
        var devices = _client.OpenRGBClient.GetAllControllerData();
        foreach (var device in devices)
        {
            var controller = new OpenRGBController();
            controller.Name = device.Name.ToValidFileName();
            controller.SerialPort = device.Location.ToValidFileName();
            controller.SerialNumber = device.Serial;
            controller.MaxLEDSupport = device.Leds.Length;
            controller.HardwareType = device.Type;
            while (_onHold)
            {
                await Task.Delay(100);
            }

            NewDevicesFound?.Invoke(controller);
        }
    }

    public bool IsRunning { get; set; }
    private bool _openRGBIsInit = false;
    private Thread _workerThread;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _onHold;

    public void Hold()
    {
        _onHold = true;
    }

    public void Resume()
    {
        _onHold = false;
    }


    public async Task Start()
    {
        await UpdateDeviceList();
    }

  
}