

using Avalonia.Media;

namespace Ambinity.Installer.Models;

public class PostInstallationSettings
{
    public PostInstallationSettings()
    {
        
    }

    public bool OpenAfterFinish { get; set; } = true;
    public Color PrimaryColor = Avalonia.Media.Color.Parse("#FF1DB954");
    public bool AutoStart { get; set; } = true;
    public bool CreateDesktopShortcut { get; set; } = true;
}