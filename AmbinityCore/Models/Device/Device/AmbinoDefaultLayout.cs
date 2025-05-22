using Avalonia;

namespace AmbinityCore.Models.Device.Device;

/// <summary>
/// provide default position for various devices, apply only for 1000-500 canvas
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
                return DefaultAmbinoEdgeZone();
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
            case HardwareTypeEnum.Keyboard:
                return DefaultKeyboardZone();

            default: return new List<Point>() { new Point(0, 0) };
        }
    }
    public static List<Point> DefaultAmbinoFanHUBZone()
    {
        var points = new List<Point>();
        var output0 = new Point(940, 170);
        var output1 = new Point(940, 135);
        var output2 = new Point(940, 100);
        var output3 = new Point(895, 50);
        var output4 = new Point(860, 50);
        var output5 = new Point(825, 50);
        var output6 = new Point(790, 50);
        var output7 = new Point(825, 230);
        var output8 = new Point(860, 230);
        var output9 = new Point(895, 230);
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
        var output4 = new Point(668, 162);
        var output5 = new Point(668, 190);
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
        var output0 = new Point(827, 95);
        points.Add(output0);
        return points;
    }
    public static List<Point> DefaultMouseZone()
    {
        var points = new List<Point>();
        var output0 = new Point(430, 240);
        points.Add(output0);
        return points;
    }
    public static List<Point> DefaultKeyboardZone()
    {
        var points = new List<Point>();
        var output0 = new Point(210, 234);
        points.Add(output0);
        return points;
    }
    public static List<Point> DefaultDramZone()
    {
        var points = new List<Point>();
        var output0 = new Point(667, 111);
        points.Add(output0);
        return points;
    }
    public static List<Point> DefaultAmbinoBasicZone()
    {
        var points = new List<Point>();
        var output0 = new Point(185, 50);
        points.Add(output0);
        return points;
    }
    public static List<Point> DefaultAmbinoEdgeZone()
    {
        var points = new List<Point>();
        var output0 = new Point(85, 194);
        points.Add(output0);
        return points;
    }
}

