using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.Device;
using AmbinityCore.Models.Device.Service;
using AmbinityCore.Repositories;
using Avalonia.Controls;

namespace AmbinityCore.Models.Device.Provider;

/// <summary>
/// Scan for serial device and create new device from serial ports
/// </summary>
/// todo load device from disk and provide to repository instead of repository loading itself,
/// but this violate the pattern of ICollectableItemRepository
public class SerialControllerProvider
{
    public event Action<IController> NewDeviceFound;

    public SerialControllerProvider(SerialControllerDiscoveryService disconveryService,
        AmbinityDeviceLayoutRepository layoutRepository)
    {
        _discoveryService = disconveryService;
        _discoveryService.NewDevicesFound += OnNewDevicesFound;
        _layoutRepository = layoutRepository;
        _layoutRepository.Init();
    }

    public void Init()
    {
        _discoveryService.Start();
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
                    new AmbinityDevice(_layoutRepository.GetLayout("Ambino Basic 24inch", 30), 0.4f)));
                break;
            case HardwareTypeEnum.AmbinoEDGE:
                controller.DashboardHeight = 270;
                controller.DashboardWidth = 230;
                ledController.Outputs.Add(new LEDOutput(80, 0,
                    new AmbinityDevice(_layoutRepository.GetLayout("Ambino Basic 24inch", 48))));
                break;
            case HardwareTypeEnum.AmbinoFanHub:
                controller.DashboardHeight = 270;
                controller.DashboardWidth = 400;
                for (int i = 0; i < 10; i++)
                {
                    ledController.Outputs.Add(new LEDOutput(80, i,
                        new AmbinityDevice(_layoutRepository.GetLayout("Ambino Dualring Fan", 30))));
                }

                controller.FanController = new FanController()
                {
                    Name = "Generic PWM Fan Controller",
                    DeviceDescription = "Ambino high performance dual mode pwm controller"
                    
                };
                break;
            case HardwareTypeEnum.AmbinoHUBV3:
                controller.DashboardHeight = 270;
                controller.DashboardWidth = 290;

                for (int i = 0; i < 4; i++)
                {
                    ledController.Outputs.Add(new LEDOutput(80, i,
                        new AmbinityDevice(_layoutRepository.GetLayout("Default ARGB LED Strip", 64))));
                }

                ledController.Outputs.Add(new LEDOutput(80, 4,
                    new AmbinityDevice(_layoutRepository.GetLayout("Ambino Neon 24P", 200), 0.5f)));
                ledController.Outputs.Add(new LEDOutput(80, 5,
                    new AmbinityDevice(_layoutRepository.GetLayout("Ambino Neon 24P", 200), 0.5f)));
                ledController.Outputs.Add(new LEDOutput(80, 6,
                    new AmbinityDevice(_layoutRepository.GetLayout("Default ARGB LED Strip", 64))));
                break;
        }

        ledController.HardwareSettings.HardwareType = controller.HardwareType;
        ledController.ApplyOutputMapping(OutputMappingProvider.GetOutputMapping(controller.HardwareType));
        ledController.PopulateDefaultLayout();
        controller.PhysicalHeight = 317;
        controller.PhysicalWidth = 500;
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
}