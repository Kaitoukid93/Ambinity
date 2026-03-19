namespace AmbinityDB.Storage.Infrastructure;
using System;
using System.Diagnostics;
using System.IO;

public static class StoragePaths
{
    // Root
    public static readonly string BaseFolder =
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public static readonly string Root =
        Path.Combine(BaseFolder, "Ambinity");

    // Core folders
    public static readonly string Data = Path.Combine(Root, "Data");
    public static readonly string Cache = Path.Combine(Root, "Cache");
    public static readonly string Tools = Path.Combine(Root, "Tools");
    public static readonly string FirmwareTools = Path.Combine(Tools, "FirmwareTools");

    // Resource folders
    public static readonly string Images = Path.Combine(Root, "Images");
    public static readonly string Locales = Path.Combine(Root, "Locales");

    // Asset folders
    public static readonly string AnimationsFolderPath = Path.Combine(Data, "Animations");
    public static readonly string ProfileCategoriesFolderPath = Path.Combine(Data, "ProfileCategories");
    public static readonly string PredefinedColorsFolderPath = Path.Combine(Data, "PredefinedColors");
    public static readonly string ProfilesFolderPath = Path.Combine(Data, "Profiles");
    public static readonly string ColorPalettesFolderPath = Path.Combine(Data, "ColorPalettes");
    public static readonly string ShortcutsFolderPath = Path.Combine(Data, "Shortcuts");
    public static readonly string LightingZonesFolderPath = Path.Combine(Data, "LightingZones");
    public static readonly string DeviceLayoutsFolderPath = Path.Combine(Data, "DeviceLayouts");
    public static readonly string HardwaresFolderPath = Path.Combine(Data, "Hardwares");
    //App Config
    public static readonly string ConfigFile = Path.Combine(Root, "config.json");
    public static readonly string InitFlag = Path.Combine(Root, ".init");

    // App info (not storage, but still useful)
    public static readonly string ApplicationFolder =
        Path.GetDirectoryName(typeof(StoragePaths).Assembly.Location)!;

    public static readonly string ExecutablePath = GetCurrentLocation();

    public const string StartupServiceName = "Ambinity Startup Task";

    private static string GetCurrentLocation()
    {
        return Process.GetCurrentProcess().MainModule!.FileName!;
    }

    public static IEnumerable<string> AllDirectories => new[]
{
    //Core folders
    Root,
    Data,
    Cache,
    Tools,
    //Asset Folders
    AnimationsFolderPath,
    ProfileCategoriesFolderPath,
    PredefinedColorsFolderPath,
    ProfilesFolderPath,
    ColorPalettesFolderPath,
    ShortcutsFolderPath,
    LightingZonesFolderPath,
    DeviceLayoutsFolderPath,
    HardwaresFolderPath,

    //Util Folders
    FirmwareTools,
    Images,
    Locales
};
}
