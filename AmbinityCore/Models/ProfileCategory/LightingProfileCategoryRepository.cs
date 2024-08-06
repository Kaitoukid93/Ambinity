using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using Newtonsoft.Json;

namespace AmbinityCore.Models.ProfileCategory;

public sealed class LightingProfileCategoryRepository : CollectableItemRepository
{
    private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Ambinity\\");
    private string dbPath =>Path.Combine(JsonPath, "Data");
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
        string[] files = Directory.GetFiles(FolderPath);
        foreach (var file in files)
        {

            var category = JsonHelpers.DeserializeJson<LightingProfileCategory>(file);
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
        var colorPalette = new LightingProfileCategory()
        {
            Name = "Color Palette",
            ID = Guid.NewGuid(),
            IsDefault = true,
            LocalPath = Path.Combine(this.FolderPath,"Color Palette")
        };
        var download = new LightingProfileCategory()
        {
            Name = "Download",
            ID = Guid.NewGuid(),
            IsDefault = false,
            LocalPath = Path.Combine(this.FolderPath,"Download")
        };
        AddItem(colorPalette);
        AddItem(download);
        //write this to disk
    }
}