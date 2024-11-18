using System.ComponentModel;
using Avalonia;
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
    
    bool EnableAudioCapture { get; set; }
    bool EnableScreenCapture { get; set; }
    bool EnableHWMonitor { get; set; }
    bool EnableOpenRGB { get; set; }
    int CanvasWidth { get; set; }
    int CanvasHeight { get; set; }
    int TargetFramerate { get; set; }
}