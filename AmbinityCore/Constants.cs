namespace AmbinityCore;

public static class Constants
{
    /// <summary>
    ///     The Ambinity data folder
    /// </summary>
    public static readonly string AppDataFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ambinity\\");

    #region database local folder paths
    public static readonly string GeneralSettingsFilePath = Path.Combine(AppDataFolder, "config.json");
    public static readonly string PalettesCollectionFolderPath = Path.Combine(AppDataFolder, "ColorPalettes");
    public static readonly string ChasingPatternsCollectionFolderPath = Path.Combine(AppDataFolder, "ChasingPatterns");
    public static readonly string ColorsCollectionFolderPath = Path.Combine(AppDataFolder, "Colors");
    public static readonly string GifsCollectionFolderPath = Path.Combine(AppDataFolder, "Gifs");
    public static readonly string VIDCollectionFolderPath = Path.Combine(AppDataFolder, "VID");
    public static readonly string MIDCollectionFolderPath = Path.Combine(AppDataFolder, "MID");

    #endregion
}