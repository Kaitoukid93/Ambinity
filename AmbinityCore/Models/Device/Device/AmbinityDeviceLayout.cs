using System.Collections.ObjectModel;
using adrilight_shared.Models.Device.SlaveDevice;
using AmbinityCore.Enums;
using AmbinityCore.Models.Device.LED;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using Serilog;

namespace AmbinityCore.Models.Device;

/// <summary>
/// Store layout for slave device without saving to actual slave device data
/// </summary>
public class AmbinityDeviceLayout
{
    public AmbinityDeviceLayout(string filePath)
    {
        FilePath = filePath;
        Leds = new List<AmbinityLEDLayout>();
        Image = new Uri(Path.Combine(FilePath, "thumbnail.png"));
        LoadLayout();
    }

    public string FilePath { get; }
    [JsonIgnore] public List<AmbinityLEDLayout> Leds { get; }
    [JsonIgnore] public Uri? Image { get; private set; }
    [JsonIgnore] public Uri? Thumbnail { get; private set; }

    private void LoadLayout()
    {
        if (Directory.Exists(FilePath))
        {
            var json = File.ReadAllText(Path.Combine(FilePath, "config.json"));
            try
            {
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
                Thumbnail = new Uri(Path.Combine(FilePath, "colored_thumbnail.png"), UriKind.Absolute);
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
    public  RenderTargetBitmap RenderLayout(int width, int height, int scale = 2)
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
}