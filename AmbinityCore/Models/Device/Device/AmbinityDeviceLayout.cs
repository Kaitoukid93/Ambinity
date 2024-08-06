using adrilight_shared.Models.Device.SlaveDevice;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device.Device;
using AmbinityCore.Models.Device.LED;
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

    public string Description { get; set; }
    public string FilePath { get; }
    [JsonIgnore] public List<AmbinityLEDLayout> Leds { get; }
    [JsonIgnore] public Uri? Image { get; private set; }
    [JsonIgnore] public string Thumbnail { get; private set; }

    private void LoadLayout()
    {
        if (Directory.Exists(FilePath))
        {
            LocalPath = FilePath;
            try
            {
                var json = File.ReadAllText(Path.Combine(FilePath, "config.json"));
                var legacyDevice = JsonConvert.DeserializeObject<ARGBLEDSlaveDevice>(json);
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

                Image = new Uri(Path.Combine(FilePath, "thumbnail.png"), UriKind.Absolute);
                Thumbnail = Path.Combine(FilePath, "colored_thumbnail.png");
                Name = legacyDevice.Name;
            }
            catch (Exception ex)
            {
                Log.Error("Layout parse failed");
            }
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
        foreach (var ledLayout in Leds)
        {
            var led = device.Leds.Where(l => l.Index == ledLayout.Index).FirstOrDefault();
            //add led if missing
            if (led == null)
            {
                var missingLED = new AmbinityLED(new ArgbLed(), device,
                    ledLayout.X,
                    ledLayout.Y,
                    ledLayout.Width,
                    ledLayout.Height,
                    ledLayout.Index,
                    true,
                    ledLayout.Geometry);
                device.Leds.Add(missingLED);
                continue;
            }

            led.RelativeX = ledLayout.X;
            led.RelativeY = ledLayout.Y;
            led.Width = ledLayout.Width;
            led.Height = ledLayout.Height;
            led.Index = ledLayout.Index;
            led.Geometry = ledLayout.Geometry;
        }

        device.DeviceName = this.Name;
        device.Layout = this;
        device.UpdateSizeByChild(false);
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
    public RenderTargetBitmap RenderLayout(int width, int height, int scale = 2)
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

    /// <summary>
    /// get default layout for device stored in resource
    /// </summary>
    /// <param name="device"></param>
    public void GetDefaultLayout(AmbinityDevice device)
    {
    }

    /// <summary>
    /// save data to local path
    /// </summary>
    public void Save()
    {
        JsonHelpers.WriteSimpleJson(this, LocalPath);
    }

    public CollectableItemRepository GetLocalRepository()
    {
        return Ioc.Default.GetRequiredService<AmbinityDeviceLayoutRepository>();
    }

    public OnlineItemRepository GetOnlineRerpository()
    {
        return Ioc.Default.GetRequiredService<AmbinityDeviceOnlineRepository>();
    }

    public event Action<ICollectableItem>? ItemNameChanged;
    public event Action<ICollectableItem>? ItemPinStatusChanged;
    public event Action<ICollectableItem>? ItemCheckStatusChanged;
    public string Name { get; set; }
    [JsonIgnore] public bool IsSelected { get; set; }
    [JsonIgnore] public bool IsEditing { get; set; }
    [JsonIgnore] public bool IsChecked { get; set; }
    [JsonIgnore] public bool IsPinned { get; set; }
    [JsonIgnore] public string LocalPath { get; set; }
}