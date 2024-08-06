namespace AmbinityCore.Models.Device.Device;

public class DefaultAmbinityDevice
{
    private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "adrilight\\");
    private string SupportedDeviceCollectionFolderPath =>  Path.Combine(JsonPath, "SupportedDevices");
    private string folderPath => Path.Combine(SupportedDeviceCollectionFolderPath, "Ambino Dualring Fan");
   
    public AmbinityDevice DefaultARGBLEDStrip()
    {
        var device = new AmbinityDevice();
        device = new AmbinityDevice()
        {
            DeviceName = "ARGB LED Strip",
            DeviceDescription = "Default LED Setup for ARGB LED Strip",
            X = 100,
            Y = 100,
            Rotation = 0,
            Scale = 0.25f,
        };
        var layout =
            new AmbinityDeviceLayout(Path.Combine(SupportedDeviceCollectionFolderPath, "Default ARGB LED Strip"));
        layout.ApplyToDevice(device);
        return device;
    }
    public AmbinityDevice DefaultAmbinoDualring()
    {
        var device = new AmbinityDevice();
        device = new AmbinityDevice()
        {
            DeviceName = "Ambino Dualring Fan",
            DeviceDescription = "Default Device for Ambino Dualring Fan",
            X = 100,
            Y = 100,
            Rotation = 0,
            Scale = 0.25f,
        };
        var layout =
            new AmbinityDeviceLayout(Path.Combine(SupportedDeviceCollectionFolderPath, "Ambino Dualring Fan"));
        layout.ApplyToDevice(device);
        return device;
    }
}