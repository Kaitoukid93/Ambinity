using System.IO.Compression;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.Service;
using AmbinityCore.Repositories;
using AmbinityServer;
using OpenRGB.NET;
using Serilog;

namespace AmbinityCore.Models.Device.Provider;

public class OpenRGBControllerProvider
{
    private readonly OpenRGBControllerDiscoveryService _discoveryService;
    private readonly AmbinityDeviceLayoutRepository _layoutRepository;
    public event Action<OpenRGBController> NewDeviceFound;

    public OpenRGBControllerProvider(OpenRGBControllerDiscoveryService discoveryService,
        AmbinityDeviceLayoutRepository layoutRepository, AmbinityClient client)
    {
        _client = client;
        _discoveryService = discoveryService;
        _discoveryService.NewDevicesFound += OnNewDevicesFound;
        _layoutRepository = layoutRepository;
    }

    public void Init()
    {
        _discoveryService.Start();
    }

    private void OnNewDevicesFound(OpenRGBController controller)
    {
        BuildController(controller);
    }

    /// <summary>
    /// Build fully functional controller from core
    /// </summary>
    /// <returns></returns>
    private async Task BuildController(OpenRGBController controller)
    {
        var ledController = new LEDController();
        controller.DashboardHeight = 270;
        controller.DashboardWidth = 230;
        //create a blank device first to make sure it's working, device setup and thumbnail will be later download from server
        ledController.Outputs.Add(new LEDOutput(controller.MaxLEDSupport, 0,
            new AmbinityDevice(_layoutRepository.GetLayout(controller.Name,"Generic "+controller.HardwareType, controller.MaxLEDSupport), 0.4f)));
        ledController.HardwareSettings.HardwareType = controller.HardwareType;
        controller.LedController = ledController;
        controller.RegisterLEDController();
        controller.RegisterFanController();
        NewDeviceFound?.Invoke(controller);
    }

    public void Hold()
    {
        _isBusy = true;
        _discoveryService.Hold();
    }

    public void Resume()
    {
        _isBusy = false;
        _discoveryService.Resume();
    }

    private bool _isBusy;
    private readonly AmbinityClient _client;
}