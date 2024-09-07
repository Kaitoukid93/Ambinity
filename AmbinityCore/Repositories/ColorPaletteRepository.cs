using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;

namespace AmbinityCore.Repositories;

public class ColorPaletteRepository : CollectableItemRepository
{

    private string dbPath => Path.Combine(Constants.AppDataFolder, "Data", "Colors");
    private string FolderPath => Path.Combine(dbPath, "ColorPalette");

    public ColorPaletteRepository()
    {
        Name = "Color Palette";
        LocalFolderPath = FolderPath;
    }
    public override void CreateDefault()
    {
        CreateDefaultPalette();
    }

   
    public override void LoadFromDisk()
    {
        Items?.Clear();
        string[] directories = Directory.GetDirectories(FolderPath);
        var hexConverter = new HexColorConverter();
        foreach (var dir in directories)
        {

            var palette = JsonHelpers.DeserializeJson<ColorPalette>(Path.Combine(dir,"config.json"),hexConverter);
            if (palette == null)
                continue;
            palette.LocalPath = dir;
            AddItem(palette);
        }
    }
    /// <summary>
    /// create default Solid Colors
    /// </summary>
    private void CreateDefaultPalette()
    {
        AddItem(DefaultColorPalettes.RetroPalette());
    }
    public override void ImportItem(string path)
    {
        //simply copy folder to repository folder path
        LocalFileHelpers.CopyDirectory(path,LocalFolderPath,true);
        LoadFromDisk();
        //update the collection
        
    }
}