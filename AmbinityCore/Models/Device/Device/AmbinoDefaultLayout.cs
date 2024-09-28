using Avalonia;

namespace AmbinityCore.Models.Device.Device;

/// <summary>
/// provide default position for various devices
/// </summary>
public static class AmbinoDefaultLayout
{
    public static List<Point>  GetDefaultLayout(HardwareTypeEnum hardwareType)
    {
        switch (hardwareType)
        {
            case HardwareTypeEnum.AmbinoBasic:
                return DefaultAmbinoBasicZone();
            case HardwareTypeEnum.AmbinoFanHub:
                return DefaultAmbinoFanHUBZone();
            case HardwareTypeEnum.AmbinoEDGE:
                return DefaultAmbinoBasicZone();
            case HardwareTypeEnum.AmbinoHUBV3:
                return DefaultAmbinoHUBV3Zone();
            case HardwareTypeEnum.AmbinoHUBV2:
                return DefaultAmbinoHUBV3Zone();
            default: return new List<Point>() { new Point(0, 0) };
        }
    }
    public static List<Point> DefaultAmbinoFanHUBZone()
    {
        var points = new List<Point>();
        var output0 = new Point(500, 160);
        var output1 = new Point(500, 195);
        var output2 = new Point(500, 230);
        var output3 = new Point(500, 265);
        var output4 = new Point(500, 300);
        var output5 = new Point(535, 160);
        var output6 = new Point(535, 195);
        var output7 = new Point(535, 230);
        var output8 = new Point(535, 265);
        var output9 = new Point(535, 300);
        points.Add(output0);
        points.Add(output1);
        points.Add(output2);
        points.Add(output3);
        points.Add(output4);
        points.Add(output5);
        points.Add(output6);
        points.Add(output7);
        points.Add(output8);
        points.Add(output9);
        return points;
    }
    public static List<Point> DefaultAmbinoHUBV3Zone()
    {
        var points = new List<Point>();
        var output0 = new Point(34, 406);
        var output1 = new Point(34, 421);
        var output2 = new Point(34, 436);
        var output3 = new Point(34, 451);
        var output4 = new Point(372, 203);
        var output5 = new Point(372, 241);
        var output6 = new Point(34, 466);
        points.Add(output0);
        points.Add(output1);
        points.Add(output2);
        points.Add(output3);
        points.Add(output4);
        points.Add(output5);
        points.Add(output6);
        return points;
    }
    public static List<Point> DefaultAmbinoBasicZone()
    {
        var points = new List<Point>();
        var output0 = new Point(34, 180);
        points.Add(output0);
        return points;
    }
}

