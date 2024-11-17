using System.ComponentModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Models.GeneralSetting;

public interface IGeneralSettings : INotifyPropertyChanged
{
    bool AutoStart { get; set; }
    int AutoStartDelay { get; set; }
    bool StartMinimized { get; set; }
    Color PrimaryColor { get; set; }
    bool EnableSnapToGrid { get; set; }
    bool ShowCanvasLockedInfo { get; set; }
    bool ShowAppTour { get; set; }
    Guid LastPlayedProfileID { get; set; }
    bool EnableMica { get; set; }
    string SelectedTheme { get; set; }
}