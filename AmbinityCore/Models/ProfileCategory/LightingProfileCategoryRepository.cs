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
        Init();
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
            Items.Add(category);
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
            IsDefault = true
        };
        var staticColor = new LightingProfileCategory()
        {
            Name = "Static Color",
            ID = Guid.NewGuid(),
            IsDefault = true
        };
        Items.Add(colorPalette);
        Items.Add(staticColor);
        //write this to disk
    }
}