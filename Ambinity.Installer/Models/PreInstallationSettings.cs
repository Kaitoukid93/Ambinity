
namespace Ambinity.Installer.Models;

public class PreInstallationSettings
{
    /// <summary>
    /// this class only care about exisitence of config file and language settings, other settings will be handled in post installation settings
    /// </summary>
    public PreInstallationSettings()
    {
      
    }
    public string SelectedLanguage {get; set;} = "en";
}
