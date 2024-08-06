using System.ComponentModel;

namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public enum ColorApperanceEnum
{
    [Description("Fill entire zone")]
    Fill,
    [Description("Apply stroke only")]
    Stroke
}