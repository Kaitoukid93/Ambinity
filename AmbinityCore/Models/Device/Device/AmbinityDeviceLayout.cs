using adrilight_shared.Enums;
using adrilight_shared.Models.Device.SlaveDevice;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device.Device;
using AmbinityCore.Models.Device.LED;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

namespace AmbinityCore.Models.Device;

/// <summary>
/// Store layout for slave device without saving to actual slave device data
/// </summary>
public class AmbinityDeviceLayout : ObservableObject, ICollectableItem
{
    /// <summary>
    /// construct new layout from file path
    /// </summary>
    /// <param name="filePath"></param>
    public AmbinityDeviceLayout(string filePath)
    {
        FilePath = filePath;
        Leds = new List<AmbinityLEDLayout>();
        Image = new Uri(Path.Combine(FilePath, "thumbnail.png"));
        LoadLayout();
    }

    //if this is null, index will be applied from layout
    public int[] CustomIndex { get; set; }
    public string Description { get; set; }
    public string FilePath { get; }
    [JsonIgnore] public List<AmbinityLEDLayout> Leds { get; }
    [JsonIgnore] public Uri? Image { get; private set; }
    [JsonIgnore] public string Thumbnail { get; private set; }
    [JsonIgnore] public float ImageWidth { get; private set; }
    [JsonIgnore] public float ImageHeight { get; private set; }
    [JsonIgnore] public CollectableItemRepository LocalRepository { get; set; }

    private void LoadLayout()
    {
        if (Directory.Exists(FilePath))
        {
            LocalPath = FilePath;
            try
            {
                var json = File.ReadAllText(Path.Combine(FilePath, "config.json"));
                var legacyDevice = JsonConvert.DeserializeObject<ARGBLEDSlaveDevice>(json);
                var image = legacyDevice?.Image;
                if (legacyDevice == null)
                    return;
                foreach (var zone in legacyDevice.ControlableZones)
                {
                    foreach (var spot in zone.Spots)
                    {
                        var led = new AmbinityLEDLayout((float)spot.Left + (float)zone.Left,
                            (float)spot.Top + (float)zone.Top, (float)spot.Width,
                            (float)spot.Height, spot.Geometry, spot.Index);

                        Leds.Add(led);
                    }
                }

                if (Image != null)
                {
                    ImageWidth = (float)image.Width;
                    ImageHeight = (float)image.Height;
                }

                Image = new Uri(Path.Combine(FilePath, "thumbnail.png"), UriKind.Absolute);
                Thumbnail = Path.Combine(FilePath, "colored_thumbnail.png");
                Name = legacyDevice.Name;
                LayoutType = LayoutTypeConverter(legacyDevice.DeviceType);
            }
            catch (Exception ex)
            {
                Log.Error("Layout parse failed");
            }
        }
        else
        {
            //handle file offline deleted- could be disk malfunctioning or user delete
            Log.Error("Could not find " + "[" + FilePath + "]" + "  default layout used");
        }
    }

    /// <summary>
    /// apply this layout to specific device
    /// </summary>
    /// <param name="device"></param>
    public void ApplyToDevice(AmbinityDevice device)
    {
        if (device == null)
            return;
        if (Leds.Count <= 0)
            return;
        var usableLeds = new List<AmbinityLED>();
        int ledCount = 0;
        foreach (var ledLayout in Leds)
        {
            var led = device.Leds.Where(l => l.Index == ledLayout.Index).FirstOrDefault();
            //add led if missing
            if (led == null)
            {
                int index = 0;
                if (CustomIndex != null)
                {
                    if (CustomIndex.Length > ledCount)
                    {
                        index = CustomIndex[ledCount];
                    }
                    else
                    {
                        //set this led to first led since customIndex is not set
                        index = 0;
                    }
                }
                else
                {
                    index = ledLayout.Index;
                }

                var missingLED = new AmbinityLED(new ArgbLed(), device,
                    ledLayout.X,
                    ledLayout.Y,
                    ledLayout.Width,
                    ledLayout.Height,
                    index,
                    true,
                    ledLayout.Geometry);
                usableLeds.Add(missingLED);
                ledCount++;
                continue;
            }

            led.RelativeX = ledLayout.X;
            led.RelativeY = ledLayout.Y;
            led.Width = ledLayout.Width;
            led.Height = ledLayout.Height;


            led.Geometry = ledLayout.Geometry;

            usableLeds.Add(led);
            
        }

        device.Leds.Clear();
        foreach (var led in usableLeds)
        {
            device.Leds.Add(led);
        }

        device.DeviceName = this.Name;
        device.Layout = this;
        device.UpdateSizeByChild(false);
        device.TransformLeds();
    }

    /// <summary>
    /// render device image from path with specific size and scale
    /// Thanks to artemis
    /// </summary>
    /// <param name="layout"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    public RenderTargetBitmap RenderLayout(int width, int height, int scale = 1)
    {
        string? path = Image?.LocalPath;

        // Create a bitmap that'll be used to render the device and LED images just once
        // Render 4 times the actual size of the device to make sure things look sharp when zoomed in
        RenderTargetBitmap renderTargetBitmap =
            new(new PixelSize(width * scale, height * scale));

        using DrawingContext context = renderTargetBitmap.CreateDrawingContext();

        // Draw device background
        if (path != null && File.Exists(path))
        {
            using Bitmap bitmap = new(path);
            using Bitmap scaledBitmap = bitmap.CreateScaledBitmap(renderTargetBitmap.PixelSize);
            context.DrawImage(scaledBitmap, new Rect(scaledBitmap.Size));
        }

        return renderTargetBitmap;
    }


    public OnlineItemTypeEnum GetType()
    {
        return OnlineItemTypeEnum.DeviceLayout;
    }

    /// <summary>
    /// save data to local path
    /// </summary>
    public void Save()
    {
        //todo implement profile save with icon 
        if (LocalPath == null || !Directory.Exists(LocalPath))
        {
            //create local path
            var dbPath = GetLocalRepository().LocalFolderPath;
            LocalPath = Path.Combine(dbPath, Name);
            Directory.CreateDirectory(LocalPath);
        }

        JsonHelpers.WriteSimpleJson(this, Path.Combine(LocalPath, "layout.json"));
    }

    public CollectableItemRepository GetLocalRepository()
    {
        return Ioc.Default.GetRequiredService<AmbinityDeviceLayoutRepository>();
    }

    public OnlineItemRepository GetOnlineRerpository()
    {
        return Ioc.Default.GetRequiredService<AmbinityDeviceOnlineRepository>();
    }

    private DeviceLayoutType LayoutTypeConverter(SlaveDeviceTypeEnum legacyType)
    {
        switch (legacyType)
        {
            case SlaveDeviceTypeEnum.FanLED:
                return DeviceLayoutType.FanLED;
            case SlaveDeviceTypeEnum.LEDFrame:
                return DeviceLayoutType.ScreenBackLight;
            case SlaveDeviceTypeEnum.LEDStrip:
                return DeviceLayoutType.LEDStrip;
            case SlaveDeviceTypeEnum.Matrix:
                return DeviceLayoutType.Matrix;
            default: return DeviceLayoutType.Unknown;
        }
    }

    public event Action<ICollectableItem>? ItemNameChanged;
    public event Action<ICollectableItem>? ItemPinStatusChanged;
    public event Action<ICollectableItem>? ItemCheckStatusChanged;
    public string Name { get; set; }
    public DeviceLayoutType LayoutType { get; set; }
    [JsonIgnore] public bool IsSelected { get; set; }
    [JsonIgnore] public bool IsEditing { get; set; }
    [JsonIgnore] public bool IsChecked { get; set; }
    [JsonIgnore] public bool IsPinned { get; set; }
    [JsonIgnore] public string LocalPath { get; set; }
}