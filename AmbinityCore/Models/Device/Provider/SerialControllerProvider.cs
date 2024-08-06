using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.Device;
using AmbinityCore.Models.Device.Service;

namespace AmbinityCore.Models.Device.Provider;

/// <summary>
/// Scan for serial device and create new device from serial ports
/// </summary>
/// todo load device from disk and provide to repository instead of repository loading itself,
/// but this violate the pattern of ICollectableItemRepository
public class SerialControllerProvider
{
    public event Action<SerialController> NewDeviceFound;

    public SerialControllerProvider(SerialControllerDiscoveryService disconveryService,
        AmbinityDeviceLayoutRepository layoutRepository)
    {
        _discoveryService = disconveryService;
        _discoveryService.NewDevicesFound += OnNewDevicesFound;
        _layoutRepository = layoutRepository;
    }

    private void OnNewDevicesFound(SerialController controller)
    {
        //try construct new controller from received infomationdevice.OutputPort = comPort;
        BuildController(controller);
    }

    /// <summary>
    /// Build fully functional controller from core
    /// </summary>
    /// <returns></returns>
    private void BuildController(SerialController controller)
    {
        if (_isBusy)
            return;
        _discoveryService.Hold();
        var ledController = new LEDController();
        switch (controller.HardwareType)
        {
            case HardwareTypeEnum.AmbinoBasic:
                controller.DashboardHeight = 270;
                controller.DashboardWidth = 230;
                ledController.Outputs.Add(new LEDOutput(80, 0,
                    new AmbinityDevice(_layoutRepository.GetLayoutByName("Ambino Basic 24inch"))));
                break;
            case HardwareTypeEnum.AmbinoEDGE:
                controller.DashboardHeight = 270;
                controller.DashboardWidth = 230;
                ledController.Outputs.Add(new LEDOutput(80, 0,
                    new AmbinityDevice(_layoutRepository.GetLayoutByName("Ambino Basic 24inch"))));
                break;
            case HardwareTypeEnum.AmbinoFanHub:
                controller.DashboardHeight = 270;
                controller.DashboardWidth = 400;
                ledController.Outputs.Add(new LEDOutput(80, 0,
                    new AmbinityDevice(_layoutRepository.GetLayoutByName("Ambino Dualring Fan"))));
                break;
            case HardwareTypeEnum.AmbinoHUBV3:
                controller.DashboardHeight = 270;
                controller.DashboardWidth = 290;
                ledController.Outputs.Add(new LEDOutput(80, 0,
                    new AmbinityDevice(_layoutRepository.GetLayoutByName("Default ARGB LED Strip"))));
                break;
        }

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

    private SerialControllerDiscoveryService _discoveryService;
    private AmbinityDeviceLayoutRepository _layoutRepository;
    private bool _isBusy;
    private List<SerialController> _newDevicesQ;
}