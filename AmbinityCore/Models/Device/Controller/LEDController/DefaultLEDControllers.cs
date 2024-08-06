using AmbinityCore.Enums;
using AmbinityCore.Models.Device.Device;

namespace AmbinityCore.Models.Device;

public static class DefaultLEDControllers
{
    private static DefaultAmbinityDevice _defaultDevices = new DefaultAmbinityDevice();
    public static LEDController AmbinoHubV3()
    {
        var controller = new LEDController();
        controller.Outputs = new List<LEDOutput>()
        {
            new(200, 0, _defaultDevices.DefaultAmbinoDualring()),
            new (200, 1, _defaultDevices.DefaultAmbinoDualring()),
            new (200, 2, _defaultDevices.DefaultAmbinoDualring()),
            new (200, 3, _defaultDevices.DefaultAmbinoDualring()),
            new (200, 4, _defaultDevices.DefaultAmbinoDualring()),
            new (200, 5, _defaultDevices.DefaultAmbinoDualring()),
            new (200, 6, _defaultDevices.DefaultAmbinoDualring()),
        };
        return controller;
    }

    public static LEDController AmbinoBasic()
    {
        var controller = new LEDController();
        controller.Outputs = new List<LEDOutput>()
        {
            new(85, 0, _defaultDevices.DefaultAmbinoDualring()),
        };
        return controller;
    }
    
}