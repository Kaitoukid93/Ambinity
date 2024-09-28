using Avalonia;
using Draw2D.Core.Geo;

namespace AmbinityCore.Models.Device;

/// <summary>
/// store output position for user interface
/// </summary>
public class LEDControllerOutputMapping
{
    public LEDControllerOutputMapping()
    {
        
    }
    public LEDControllerOutputMapping(int numOutput)
    {
        OutputsMap = new LEDOutputPosition[numOutput];
    }
    public LEDOutputPosition[] OutputsMap { get; set; }


}