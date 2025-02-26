using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.Device;
using AmbinityCore.Models.Device.Service;
using AmbinityCore.Repositories;
using Avalonia.Controls;
using RGBLEDOrderEnum = AmbinityCore.Enums.RGBLEDOrderEnum;

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
                    new AmbinityDevice(_layoutRepository.GetLayout("Ambino Basic 24inch","Default ARGB LED Strip", 30), 0.4f)
                    {
                        RGBOrder = RGBLEDOrderEnum.GRB
                    })
                {
                });
                break;
            case HardwareTypeEnum.AmbinoEDGE:
                controller.DashboardHeight = 270;
                controller.DashboardWidth = 230;
                ledController.Outputs.Add(new LEDOutput(80, 0,
                    new AmbinityDevice(_layoutRepository.GetLayout("Ambino Edge 1m2", "Default ARGB LED Strip",20))
                    {
                        RGBOrder = RGBLEDOrderEnum.GRB
                    }));
                break;
            case HardwareTypeEnum.AmbinoFanHub:
                controller.DashboardHeight = 270;
                controller.DashboardWidth = 400;
                for (var i = 0; i < 10; i++)
                {
                    ledController.Outputs.Add(new LEDOutput(80, i,
                        new AmbinityDevice(_layoutRepository.GetLayout("Ambino Dualring Fan"," ", 30))
                        {
                            RGBOrder = RGBLEDOrderEnum.GRB
                        }));
                }

                controller.FanController = new FanController()
                {
                    Name = "Generic PWM Fan Controller",
                    DeviceDescription = "Ambino high performance dual mode pwm controller",
                    Outputs = [new FanOutput("Fan Controller #1", "Ambino High Performance Fan Controller")]
                };
                break;
            case HardwareTypeEnum.AmbinoHUBV3:
                controller.DashboardHeight = 270;
                controller.DashboardWidth = 290;
              
                for (int i = 0; i < 4; i++)
                {
                    ledController.Outputs.Add(new LEDOutput(80, i,
                        new AmbinityDevice(_layoutRepository.GetLayout("Default ARGB LED Strip","", 64))
                        {
                            RGBOrder = RGBLEDOrderEnum.GRB
                        }));
                }

                ledController.Outputs.Add(new LEDOutput(80, 4,
                    new AmbinityDevice(_layoutRepository.GetLayout("Ambino Neon 24P","", 200), 0.5f)
                    {
                        RGBOrder = RGBLEDOrderEnum.GRB
                    }));
                ledController.Outputs.Add(new LEDOutput(80, 5,
                    new AmbinityDevice(_layoutRepository.GetLayout("Ambino Neon 24P","", 200), 0.5f)
                    {
                        RGBOrder = RGBLEDOrderEnum.GRB
                    }));
                ledController.Outputs.Add(new LEDOutput(80, 6,
                    new AmbinityDevice(_layoutRepository.GetLayout("Default ARGB LED Strip","", 64))
                    {
                        RGBOrder = RGBLEDOrderEnum.GRB
                    }));
                break;
            // non ambino device, for testing
            case HardwareTypeEnum.Dram:
                controller.DashboardHeight = 270;
                controller.DashboardWidth = 230;
                ledController.Outputs.Add(new LEDOutput(8, 0,
                    new AmbinityDevice(_layoutRepository.GetLayout("Generic Dram","Default ARGB LED Strip", 8), 0.4f)
                    {
                        RGBOrder = RGBLEDOrderEnum.GRB
                    })
                {
                });
                break;
            case HardwareTypeEnum.Motherboard:
                controller.DashboardHeight = 270;
                controller.DashboardWidth = 230;
                ledController.Outputs.Add(new LEDOutput(8, 0,
                    new AmbinityDevice(_layoutRepository.GetLayout("Generic Motherboard","Default ARGB LED Strip", 8), 0.4f)
                    {
                        RGBOrder = RGBLEDOrderEnum.GRB
                    })
                {
                });
                break;
            case HardwareTypeEnum.Speaker:
                controller.DashboardHeight = 270;
                controller.DashboardWidth = 230;
                ledController.Outputs.Add(new LEDOutput(8, 0,
                    new AmbinityDevice(_layoutRepository.GetLayout("Generic Speaker","Default ARGB LED Strip", 8), 0.4f)
                    {
                        RGBOrder = RGBLEDOrderEnum.GRB
                    })
                {
                });
                break;
            case HardwareTypeEnum.Gpu:
                controller.DashboardHeight = 270;
                controller.DashboardWidth = 230;
                ledController.Outputs.Add(new LEDOutput(8, 0,
                    new AmbinityDevice(_layoutRepository.GetLayout("Generic Gpu","Default ARGB LED Strip", 8), 0.4f)
                    {
                        RGBOrder = RGBLEDOrderEnum.GRB
                    })
                {
                });
                break;
            case HardwareTypeEnum.Keyboard:
                controller.DashboardHeight = 270;
                controller.DashboardWidth = 400;
                ledController.Outputs.Add(new LEDOutput(8, 0,
                    new AmbinityDevice(_layoutRepository.GetLayout("Generic Keyboard","Default ARGB LED Strip", 8), 0.4f)
                    {
                        RGBOrder = RGBLEDOrderEnum.GRB
                    })
                {
                });
                break;
        }
        

        ledController.HardwareSettings.HardwareType = controller.HardwareType;
        ledController.PopulateDefaultLayout();
        controller.LedController = ledController;
        controller.RegisterLEDController();
        controller.RegisterFanController();
        NewDeviceFound?.Invoke(controller);
        _discoveryService.PortInUse.Add(controller.SerialPort);
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