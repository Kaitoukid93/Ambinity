using System.Drawing;
using Color = System.Windows.Media.Color;

namespace adrilight.Spots
{
    public interface IDeviceSpot
    {
        byte Red { get; }
        byte Green { get; }
        byte Blue { get; }

        Color OnDemandColor { get; }
        Rectangle Rectangle { get; set; }
        bool IsFirst { get; set; }
        int RadiusX { get; set; }
        int RadiusY { get; set; }
        string ID { get; set; }
        int id { get; set; }
        int VID { get; set; }
        int MID { get; set; }
        int YIndex { get; set; }
        int XIndex { get; set; }
        bool IsActivated { get; set; }
        double BorderThickness { get; set; }


        void IndicateMissingValue();
        void SetColor(byte red, byte green, byte blue, bool raiseEvents);
        void SetVID(int vid);
        void SetStroke(double strokeThickness);


    }
}
