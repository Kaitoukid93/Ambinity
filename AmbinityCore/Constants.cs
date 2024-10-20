using System.Diagnostics;
using AmbinityCore.Utils;

namespace AmbinityCore;

public static class Constants
{
    /// <summary>
    ///     The Ambinity data folder
    /// </summary>
    private static readonly string BaseFolder = Environment.GetFolderPath(
        Environment.SpecialFolder.LocalApplicationData);
    public static readonly string AppDataFolder =
        Path.Combine(BaseFolder, "Ambinity\\");
    public static readonly string ModelDataFolder =
        Path.Combine(BaseFolder, "Ambinity\\Data\\");

    #region database local folder paths

    public static readonly string GeneralSettingsFilePath = Path.Combine(AppDataFolder, "config.json");
    public static readonly string CacheFolderPath = Path.Combine(AppDataFolder, "Cache");
    public static readonly string ToolsFolderPath = Path.Combine(AppDataFolder, "Tools");
    public const string StartupServiceName = "Ambinity Startup Task";
    public static readonly string ApplicationFolder = Path.GetDirectoryName(typeof(Constants).Assembly.Location)!;
    public static readonly string ExecutablePath = Utilities.GetCurrentLocation();
    public static readonly string UpdatingFolder =  Path.Combine(AppDataFolder, "updating");
    #endregion
}