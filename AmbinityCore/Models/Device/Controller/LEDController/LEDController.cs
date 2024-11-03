using System.ComponentModel;
using AmbinityCore.Enums;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device.Device;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace AmbinityCore.Models.Device;

public class LEDController : ObservableObject
{
    public LEDController()
    {
        Outputs = new List<LEDOutput>();
        HardwareSettings = new SerialLEDControllerHardwareSettings();
    }

    private string _deviceName = "New Device";

    /// <summary>
    /// Display name of the device
    /// </summary>
    public string DeviceName
    {
        get => _deviceName;
        set => SetProperty(ref _deviceName, value);
    }

    private string _deviceDescription = "";

    /// <summary>
    /// Describe the function and look of the device
    /// </summary>
    public string DeviceDescription
    {
        get => _deviceDescription;
        set => SetProperty(ref _deviceDescription, value);
    }

    private ILEDControllerHardwareSettings _hardwareSettings;

    [JsonIgnore]
    public ILEDControllerHardwareSettings HardwareSettings
    {
        get => _hardwareSettings;
        set => SetProperty(ref _hardwareSettings, value);
    }
    private List<LEDOutput> _outputs;

    public List<LEDOutput> Outputs
    {
        get => _outputs;
        set => SetProperty(ref _outputs, value);
    }

    public void PopulateDefaultLayout()
    {
        var layout = AmbinoDefaultLayout.GetDefaultLayout(HardwareSettings.HardwareType);
        foreach (var output in Outputs)
        {
            //gonna clarify or devices will collapse
            if (output.Devices.Count > 1)
            {
                foreach (var device in output.Devices)
                {
                    //todo spread device horizontal if there is more than one device
                    device.ForceTranslate(layout[output.Index].X, layout[output.Index].Y);
                }
            }
            else
            {
                output.Devices[0].ForceTranslate(layout[output.Index].X, layout[output.Index].Y);
            }
            
        }
    }

    public void ApplyOutputMapping(LEDControllerOutputMapping outputMapping)
    {
        if (outputMapping == null)
            return;
        for (int i = 0; i < Outputs.Count; i++)
        {
            Outputs[i].OutputPosition = outputMapping.OutputsMap[Outputs[i].Index];
        }
    }
}