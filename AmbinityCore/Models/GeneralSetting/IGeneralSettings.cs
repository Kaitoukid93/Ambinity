using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Models.GeneralSetting;

public interface IGeneralSettings
{
    bool AutoStart { get; set; }
    bool StartMinimized { get; set; }
    Color PrimaryColor { get; set; }
    bool EnableSnapToGrid { get; set; }
}