using System.Collections.ObjectModel;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Device.Zone.Spot;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.LED;
using Avalonia;
using Avalonia.Media;
using Serilog;

namespace AmbinityCore.Repositories;

/// <summary>
/// store entry point for exsted ambinity devicec
/// </summary>
public class AmbinityDeviceLayoutRepository : CollectableItemRepository
{
    private string FolderPath => Path.Combine(Constants.AppDataFolder, "AmbinityDevices");
    private AmbinityDeviceLayout _defaultLayout;

    public AmbinityDeviceLayoutRepository()
    {
        LocalFolderPath = FolderPath;
        Name = "Layout";
    }

    public override void CreateDefault()
    {
    }

    public AmbinityDeviceLayout GetLayout(string name, int numLED)
    {
        var match = Items.Where(i => i.Name == name).FirstOrDefault();
        if (match != null)
        {
            var layout = new AmbinityDeviceLayout(match.LocalPath);
            return layout;
        }
        else
        {
            //construct new layout match the led number
            return CreateLayout(name, numLED);
        }
    }

    public override void LoadFromDisk()
    {
        Items?.Clear();
        string[] directories = Directory.GetDirectories(LocalFolderPath);
        foreach (var dir in directories)
        {
            var layout = new AmbinityDeviceLayout(dir);
            AddItem(layout);
        }
    }

    public override void ImportItem(string path)
    {
        //simply copy folder to repository folder path
        LocalFileHelpers.CopyDirectory(path, LocalFolderPath, true);
        LoadFromDisk();
        //update the collection
    }

    public AmbinityDeviceLayout CreateLayout(string name, int numLED)
    {
        var existed = Items.Where(i => i.Name == name).FirstOrDefault();
        if (existed != null)
            return existed as AmbinityDeviceLayout;
        var device = new ARGBLEDSlaveDevice();
        device.Name = name;
        device.ControlableZones = new ObservableCollection<LEDSetup>() { new LEDSetup() };
        var maxWidth = 750;
        var ledWidth = 10;
        var ledHeight = 10;
        var maxLEDPerRow = numLED > 15 ? 15 : numLED;
        var gap = 2;
        var columnsCount = Math.Ceiling((double)numLED / maxLEDPerRow);
        var ledRect = new Rect(0, 0, ledWidth, ledHeight);
        for (int i = 0; i < columnsCount; i++)
        {
            for (int j = 0; j < maxLEDPerRow; j++)
            {
                var led = new DeviceSpot();
                led.Left = j * (ledWidth + gap);
                led.Top = i * (ledHeight + gap);
                led.Width = ledWidth;
                led.Height = ledHeight;
                led.Index = i * (maxLEDPerRow) + j;
                device.ControlableZones[0].Spots.Add(led);
            }
        }

        Directory.CreateDirectory(Path.Combine(LocalFolderPath, name));
        var configPath = Path.Combine(LocalFolderPath, name, "config.json");
        JsonHelpers.WriteSimpleJson(device, configPath);
        var layout = new AmbinityDeviceLayout(Path.Combine(LocalFolderPath, name));
        layout.Name = name;
        AddItem(layout);
        return layout;
    }
}