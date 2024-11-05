using AmbinityCore.Enums;
using AmbinityCore.Models.Device.Controller;
using Avalonia;
using Draw2D.Core.Geo;

namespace AmbinityCore.Models.Device;

public static class OutputMappingProvider
{
    public static LEDControllerOutputMapping GetOutputMapping(IController controller)
    {
        LEDControllerOutputMapping outputMap = null;
        switch (controller.HardwareType)
        {
            case HardwareTypeEnum.AmbinoHUBV3:
                controller.PhysicalHeight = 317;
                controller.PhysicalWidth = 500;
                outputMap = new LEDControllerOutputMapping(7)
                {
                    OutputsMap =
                    {
                        [0] = new LEDOutputPosition(418, 0, 76, 41),
                        [1] = new LEDOutputPosition(342, 0, 76, 41),
                        [2] = new LEDOutputPosition(266, 0, 76, 41),
                        [3] = new LEDOutputPosition(191, 0, 76, 41),
                        [4] = new LEDOutputPosition(115, 0, 76, 41),
                        [5] = new LEDOutputPosition(39, 0, 76, 41),
                        [6] = new LEDOutputPosition(336, 258, 93, 58)
                    }
                };
                break;
            case HardwareTypeEnum.AmbinoFanHub:
                controller.PhysicalHeight = 222;
                controller.PhysicalWidth = 500;
                outputMap = new LEDControllerOutputMapping(10)
                {
                    OutputsMap =
                    {
                        [0] = new LEDOutputPosition(35, 111, 75, 28),
                        [1] = new LEDOutputPosition(35, 146, 75, 28),
                        [2] = new LEDOutputPosition(122, 111, 75, 28),
                        [3] = new LEDOutputPosition(122, 146, 75, 28),
                        [4] = new LEDOutputPosition(211, 111, 75, 28),
                        [5] = new LEDOutputPosition(211, 146, 75, 28),
                        [6] = new LEDOutputPosition(298, 111, 75, 28),
                        [7] = new LEDOutputPosition(298, 146, 75, 28),
                        [8] = new LEDOutputPosition(386, 111, 75, 28),
                        [9] = new LEDOutputPosition(386, 146, 75, 28)
                    }
                };
                break;
        }

        return outputMap;
    }
}