using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using Newtonsoft.Json;

namespace AmbinityCore.Models.ProfileCategory;

public sealed class LightingProfileCategoryRepository : CollectableItemRepository
{

    private string dbPath =>Path.Combine(Constants.AppDataFolder, "Data");
    private string FolderPath => Path.Combine(dbPath, "Categories");

    public LightingProfileCategoryRepository()
    {
        LocalFolderPath = FolderPath;
        Name = "Lighting Profiles Category";
    }

    public override void CreateDefault()
    {
        CreateDefaultCategories();
    }

    public override void LoadFromDisk()
    {
        Items?.Clear();
        string[] files = Directory.GetFiles(LocalFolderPath);
        foreach (var file in files)
        {

            var category = JsonHelpers.DeserializeJson<LightingProfileCategory>(file);
            category.LocalPath = file;
            if (category == null)
                continue;
            AddItem(category);
        }
    }

    /// <summary>
    /// create default Profile Categories
    /// </summary>
    private void CreateDefaultCategories()
    {
        var AmbinoDefault = new LightingProfileCategory()
        {
            Name = "Ambino Default",
            ID = Guid.NewGuid(),
            IsDefault = true,
        };
        var download = new LightingProfileCategory()
        {
            Name = "Download",
            ID = Guid.NewGuid(),
            IsDefault = false,
        };
        AddItem(AmbinoDefault);
        AddItem(download);
        //write this to disk
    }
}