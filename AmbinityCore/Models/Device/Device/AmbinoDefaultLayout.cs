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
            case HardwareTypeEnum.Motherboard:
                return DefaultMotherboardZone();
            case HardwareTypeEnum.Dram:
                return DefaultDramZone();
            case HardwareTypeEnum.Mouse:
                return DefaultMouseZone();
                
            default: return new List<Point>() { new Point(0, 0) };
        }
    }
    public static List<Point> DefaultAmbinoFanHUBZone()
    {
        var points = new List<Point>();
        var output0 = new Point(535, 275);
        var output1 = new Point(535, 240);
        var output2 = new Point(535, 205);
        var output3 = new Point(485, 155);
        var output4 = new Point(450, 155);
        var output5 = new Point(415, 155);
        var output6 = new Point(365, 205);
        var output7 = new Point(415, 320);
        var output8 = new Point(450, 320);
        var output9 = new Point(485, 320);
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
        var output4 = new Point(408, 229);
        var output5 = new Point(408, 257);
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
    public static List<Point> DefaultMotherboardZone()
    {
        var points = new List<Point>();
        var output0 = new Point(34, 406);
        points.Add(output0);
        return points;
    }
    public static List<Point> DefaultMouseZone()
    {
        var points = new List<Point>();
        var output0 = new Point(282, 316);
        points.Add(output0);
        return points;
    }
    public static List<Point> DefaultKeyboardZone()
    {
        var points = new List<Point>();
        var output0 = new Point(34, 406);
        points.Add(output0);
        return points;
    }
    public static List<Point> DefaultDramZone()
    {
        var points = new List<Point>();
        var output0 = new Point(358, 233);
        points.Add(output0);
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

