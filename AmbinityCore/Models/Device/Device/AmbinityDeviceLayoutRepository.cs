using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device.Controller;

namespace AmbinityCore.Models.Device.Device;
/// <summary>
/// store entry point for exsted ambinity devicec
/// </summary>
public class AmbinityDeviceLayoutRepository : CollectableItemRepository
{
    private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Ambinity\\");

    private string dbPath => Path.Combine(JsonPath, "Resources");
    private string FolderPath => Path.Combine(dbPath, "AmbinityDevices");

    public AmbinityDeviceLayoutRepository()
    {
        LocalFolderPath = FolderPath;
        Name = "Layout";
    }
    public override void CreateDefault()
    {
        //  CreateDefaultSolidColors();
        // var hubV3 = DefaultLEDControllers.AmbinoHubV3();
        // Items.Add(hubV3);
    }

    public AmbinityDeviceLayout GetLayoutByName( string name)
    {
        var match = Items.Where(i => i.Name == name).FirstOrDefault().LocalPath;
        var layout = new AmbinityDeviceLayout(match);
        return layout;
    }
    public override void LoadFromDisk()
    {
        Items?.Clear();
        string[] directories = Directory.GetDirectories(FolderPath);
        foreach (var dir in directories)
        {

            var layout = new AmbinityDeviceLayout(dir);
            AddItem(layout);
        }
    }
}