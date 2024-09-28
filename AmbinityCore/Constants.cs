namespace AmbinityCore;

public static class Constants
{
    /// <summary>
    ///     The Ambinity data folder
    /// </summary>
    public static readonly string BaseFolder = Environment.GetFolderPath(
        Environment.SpecialFolder.LocalApplicationData);
    public static readonly string AppDataFolder =
        Path.Combine(BaseFolder, "Ambinity\\");

    #region database local folder paths

    public static readonly string GeneralSettingsFilePath = Path.Combine(AppDataFolder, "config.json");
    public static readonly string CacheFolderPath = Path.Combine(AppDataFolder, "Cache");
    public static readonly string ToolsFolderPath = Path.Combine(AppDataFolder, "Tools");

    #endregion
}