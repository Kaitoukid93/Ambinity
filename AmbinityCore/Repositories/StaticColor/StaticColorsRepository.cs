using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Repositories;
using AmbinityDB;
using AmbinityDB.Storage.Infrastructure;
namespace AmbinityCore.Colors;

public class StaticColorsRepository : CollectableItemRepository
{
    private string dbPath => StoragePaths.PredefinedColorsFolderPath;
    private string FolderPath => Path.Combine(dbPath, "SolidColor");

    public StaticColorsRepository()
    {
        LocalFolderPath = FolderPath;
        Name = "Solid Colors";

    }
    public override void CreateDefault()
    {
        CreateDefaultColors();
    }

    public override void LoadFromDisk()
    {
        Items?.Clear();
        string[] files = Directory.GetFiles(FolderPath);
        foreach (var file in files)
        {
            var color = JsonHelpers.DeserializeJson<SolidColor>(file);
            if (color == null)
                continue;
            color.LocalPath = file;
            AddItem(color);
        }
    }

    /// <summary>
    /// create default Solid Colors
    /// </summary>
    private void CreateDefaultColors()
    {
        foreach (var color in DefaultSolidColors.Colors)
        {
           AddItem(new SolidColor(color.ToString(),color));
        }
    }
}
