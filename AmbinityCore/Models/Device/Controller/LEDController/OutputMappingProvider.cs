using AmbinityCore.Enums;
using Avalonia;
using Draw2D.Core.Geo;

namespace AmbinityCore.Models.Device;

public static class OutputMappingProvider
{
    public static LEDControllerOutputMapping GetOutputMapping(HardwareTypeEnum type)
    {
        LEDControllerOutputMapping outputMap = null;
        switch (type)
        {
            case HardwareTypeEnum.AmbinoHUBV3:
                outputMap = new LEDControllerOutputMapping(7);
                outputMap.OutputsMap[0] = new LEDOutputPosition(418, 0, 76, 41);
                outputMap.OutputsMap[1] = new LEDOutputPosition(342, 0, 76, 41);
                outputMap.OutputsMap[2] = new LEDOutputPosition(266, 0, 76, 41);
                outputMap.OutputsMap[3] = new LEDOutputPosition(191, 0, 76, 41);
                outputMap.OutputsMap[4] = new LEDOutputPosition(115, 0, 76, 41);
                outputMap.OutputsMap[5] = new LEDOutputPosition(39, 0, 76, 41);
                outputMap.OutputsMap[6] = new LEDOutputPosition(336, 258, 93, 58);
                break;
        }

        return outputMap;
    }
}