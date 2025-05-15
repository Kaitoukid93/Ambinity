using System.IO.Compression;
using adrilight_shared.Models.Store;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.ProfileCategory;
using AmbinityCore.Repositories;
using Newtonsoft.Json;
using Serilog;

namespace AmbinityCore.Models.Profile;

public sealed class LightingProfileRepository : CollectableItemRepository
{
    //we will not use base class ItemAdded event because it will trigger circle dependency
    //this event simply tell side menu to update
    public event Action<LightingProfile> ItemDownloaded;
    private string dbPath => Path.Combine(Constants.AppDataFolder, "Data");
    private string FolderPath => Path.Combine(dbPath, "Profiles");
    private LightingZoneRepository _zoneRepository;
    public LightingProfileRepository(LightingZoneRepository zoneRepository)
    {
        LocalFolderPath = FolderPath;
        _zoneRepository = zoneRepository;
        Name = "Lighting Profiles";
    }
    public override bool Contains(object itemProperty)
    {
        foreach (LightingProfile profile in Items)
        {
            if (profile.ID == (Guid)itemProperty)
                return true;
        }

        return false;
    }

    //todo import thumbnail also
    /// <summary>
    /// Import from download cache, this should be call from download action only
    /// if not, please dont raise isDownloaded
    /// </summary>
    /// <param name="path"></param>
    public override void ImportItem(string path)
    {
        var itemFolder = Directory.GetDirectories(path).First();
        if (itemFolder == null)
            return;
        ImportProfile(itemFolder, true);
    }

    public override void LoadFromDisk()
    {
        //this step is for first time downloading profile is in zip format
        LoadZipProfileIfExist();
        Items?.Clear();
        string[] files = Directory.GetDirectories(FolderPath);
        foreach (var file in files)
        {
            var profilePath = Path.Combine(file, "config.json");
            var profile = JsonHelpers.DeserializeJson<LightingProfile>(profilePath);
            if (profile == null)
                continue;
            profile.LocalPath = profilePath;
            AddItem(profile);
        }
    }

    private void LoadZipProfileIfExist()
    {
        //import zip if exist
        string[] files = Directory.GetFiles(FolderPath);
        foreach (var file in files)
        {
            if (file.EndsWith(".zip"))
            {
                try
                {
                    ImportZipProfile(file, null, true);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to load ZIP profile");
                    continue;
                }
                // we need to remove zip file or next step will throw, im too lazy to implement a switch
                File.Delete(file);
            }
        }
    }

    /// <summary>
    /// import profile from directory, contains conig, thumb, or assets
    /// </summary>
    private void ImportProfile(string path, bool isDownloaded = false, LightingProfileCategory category = null,
        bool isDefault = false)
    {
        //find config
        var configPath = Path.Combine(path, "config.json");
        if (!File.Exists(configPath))
        {
            //search for profile.json
            configPath = Path.Combine(path, "profile.json");
            if (!File.Exists(configPath))
                return;
        }

        //deserialize this config to localize it
        var profile = JsonHelpers.DeserializeJson<LightingProfile>(configPath);
        if (profile == null)
            return;
        var matchedItems = Items.Where(x => x.Name != null && x.Name.Contains(profile.Name));
        //rename if match
        if (matchedItems != null && matchedItems.Count() > 0)
        {
            profile.Name = profile.Name + "(" + matchedItems.Count() + ")";
            Log.Information("Profile existed, rename new profile to " + profile.Name);
        }

        profile.ID = Guid.NewGuid();
        profile.IsDefault = isDefault; // import profile can not be default
        if (category != null)
            profile.CategoryID = category.ID;
        AddItem(profile);
        //copy assets and icon if exist
        var iconPath = Path.Combine(path, "icon.png");
        if (File.Exists(iconPath))
            File.Copy(iconPath, Path.Combine(profile.LocalPath, "icon.png"));
        var assetsPath = Path.Combine(path, "assets");
        if (Directory.Exists(assetsPath))
            LocalFileHelpers.CopyDirectory(assetsPath, profile.AssetPath, true);

        //notify side menu
        if (isDownloaded)
            ItemDownloaded?.Invoke(profile);
    }

    /// <summary>
    /// A task for importing downloaded profile from zip file
    /// </summary>
    /// <param name="importFilePath"></param>
    public void ImportZipProfile(string importFilePath, LightingProfileCategory category = null, bool isDefault = false)
    {
        if (!Directory.Exists(Constants.CacheFolderPath))
            Directory.CreateDirectory(Constants.CacheFolderPath);
        ZipFile.ExtractToDirectory(importFilePath, Constants.CacheFolderPath, true);
        ImportProfile(Constants.CacheFolderPath, false, category, isDefault);
        ClearCache();
    }

    private void ClearCache()
    {
        if (Directory.Exists(Constants.CacheFolderPath))
            Directory.Delete(Constants.CacheFolderPath, true);
    }

}
