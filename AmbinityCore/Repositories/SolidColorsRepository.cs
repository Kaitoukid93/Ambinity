using System.Drawing;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.ProfileCategory;
using AmbinityCore.Repositories;
using Newtonsoft.Json;

namespace AmbinityCore.Colors;

public class SolidColorsRepository : CollectableItemRepository
{
    private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Ambinity\\");
    private string dbPath =>Path.Combine(JsonPath, "Data","Colors");
    private string FolderPath => Path.Combine(dbPath, "SolidColor");

    public SolidColorsRepository()
    {
        LocalFolderPath = FolderPath;
        Init();
    }
    public override void CreateDefault()
    {
        CreateDefaultSolidColors();
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
            Items.Add(color);
        }
    }

    /// <summary>
    /// create default Solid Colors
    /// </summary>
    private void CreateDefaultSolidColors()
    {
        foreach (var color in DefaultSolidColors.Colors)
        {
            Items.Add(new SolidColor(color.ToString(),color));
        }
    }
}