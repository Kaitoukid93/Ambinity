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
        _client.DeviceListUpdated += UpdateDeviceList;
    }

    private async void UpdateDeviceList()
    {
        var devices = _client.OpenRGBClient.GetAllControllerData();
        foreach (var device in devices)
        {
            var controller = new OpenRGBController();
            controller.Name = device.Name.ToValidFileName();
            controller.SerialPort = device.Location.ToValidFileName();
            controller.SerialNumber = device.Serial;
            controller.MaxLEDSupport = device.Leds.Length;
            controller.HardwareType = GetNativeHardwareType(device);
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
        await _client.Init();
        UpdateDeviceList();
    }
/// <summary>
/// Convert OpenRGB Hardware to native Ambinity hardware
/// </summary>
/// <param name="device"></param>
/// <returns></returns>
    private HardwareTypeEnum GetNativeHardwareType(global::OpenRGB.NET.Device device)
    {
        switch (device.Type)
        {
            case DeviceType.Dram:
                return HardwareTypeEnum.Dram;
            case DeviceType.Motherboard:
                return HardwareTypeEnum.Motherboard;
            case DeviceType.Gpu:
                return HardwareTypeEnum.Gpu;
            case DeviceType.Mouse:
                return HardwareTypeEnum.Mouse;
            case DeviceType.Keyboard:
                return HardwareTypeEnum.Keyboard;
            case DeviceType.Speaker:
                return HardwareTypeEnum.Speaker;
            default: return HardwareTypeEnum.Unknown;
        }
    }
}