using AmbinityCore.Models.GeneralSetting;
using Newtonsoft.Json;

namespace AmbinityCore.DataBase;

public class GeneralSettingsManager
{
    public GeneralSettingsManager()
    {
        Settings = LoadGeneralSettings();
        if (Settings == null)
        {
            Settings = CreateDefaultSettings();
        }
    }

    public IGeneralSettings Settings { get; set; }

    public IGeneralSettings LoadGeneralSettings()
    {
        if (!File.Exists(Constants.GeneralSettingsFilePath)) return null;

        var json = File.ReadAllText(Constants.GeneralSettingsFilePath);
        try
        {
            var generalSettings = JsonConvert.DeserializeObject<GeneralSettings>(json);
            if (generalSettings == null)
                return null;
            generalSettings.PropertyChanged += (_, __) => SaveGeneralSettings(generalSettings);
            return generalSettings;
        }
        catch (JsonReaderException)
        {
            return null;
        }
    }

    public void SaveGeneralSettings(GeneralSettings generalSettings)
    {
        var json = JsonConvert.SerializeObject(generalSettings, Formatting.Indented);
        Directory.CreateDirectory(Constants.AppDataFolder);
        File.WriteAllText(Constants.GeneralSettingsFilePath, json);
    }

    private IGeneralSettings CreateDefaultSettings()
    {
        var generalSettings = new GeneralSettings();
        generalSettings.PropertyChanged += (_, __) => SaveGeneralSettings(generalSettings);
        if (!Directory.Exists(Constants.AppDataFolder)) return generalSettings;
        
        var legacyFiles = Directory.GetFiles(Constants.AppDataFolder, "user.config", SearchOption.AllDirectories);
        
        var file = legacyFiles
            .Select(f => new FileInfo(f))
            .OrderByDescending(fi => fi.LastWriteTimeUtc)
            .FirstOrDefault();
        return generalSettings;
    }
}