using System.Collections.Generic;
using System.Linq;
using Ambinity.ViewModels;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;

namespace Ambinity.Views.Screens.DeviceSettings;

public class FanPortViewModel : FanControlItemViewModelBase
{
    private FanOutput _fanOutput;
    public bool HWMonitorAvailable { get; set; }
    public FanPortViewModel(FanOutput fanOutput,bool hwMonitorAvailable)
    {
        _fanOutput =fanOutput;
        Name = _fanOutput.Name;
        HWMonitorAvailable = hwMonitorAvailable;
        Description = _fanOutput.Description;
        AvailableControlMode = new List<string>()
        {
            "Fixed",
            "Adaptive"
        };
        _selectedControlMode = AvailableControlMode.Where(m => m == fanOutput.ControlMode.ToString()).FirstOrDefault();
        _fixedSpeed = _fanOutput.FixedSpeed;
    }

    public List<string> AvailableControlMode { get; set; }
    private string _selectedControlMode;

    public string SelectedControlMode
    {
        get=> _selectedControlMode;
        set
        {
            switch (value)
            {
                case "Fixed":
                    _fanOutput.ControlMode = FanControlModeEnum.Fixed;
                    break;
                case "Adaptive":
                    _fanOutput.ControlMode = FanControlModeEnum.Adaptive;
                    break;
            }
            _selectedControlMode = value;
            OnPropertyChanged();
        }
    }
    public string Name { get; set; }
    public string Description { get; set; }
    private int _currentSpeed;
    public int CurrentSpeed
    {
        get => _currentSpeed;
        set
        {
            _currentSpeed = value;
            OnPropertyChanged();
        }
        
    }
    private int _fixedSpeed;
    public int FixedSpeed
    {
        get => _fixedSpeed;
        set
        {
            _fixedSpeed = value;
            _fanOutput.FixedSpeed = _fixedSpeed;
            OnPropertyChanged();
        }
        
    }
}