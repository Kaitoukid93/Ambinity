using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.Service;
using AmbinityCore.Repositories;
using OpenRGB.NET;

namespace AmbinityCore.Models.Device.Provider;

public class OpenRGBControllerProvider
{
    private readonly OpenRGBControllerDiscoveryService _discoveryService;
    private readonly AmbinityDeviceLayoutRepository _layoutRepository;
    public event Action<OpenRGBController> NewDeviceFound;

    public OpenRGBControllerProvider(OpenRGBControllerDiscoveryService discoveryService,
        AmbinityDeviceLayoutRepository layoutRepository)
    {
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
    private void  BuildController(OpenRGBController controller)
    {
        var ledController = new LEDController();
        controller.DashboardHeight = 270;
        controller.DashboardWidth = 230;
        ledController.Outputs.Add(new LEDOutput(controller.MaxLEDSupport, 0,
            new AmbinityDevice(_layoutRepository.GetLayout(controller.Name, controller.MaxLEDSupport), 0.4f)));
        ledController.PopulateDefaultLayout();
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
}