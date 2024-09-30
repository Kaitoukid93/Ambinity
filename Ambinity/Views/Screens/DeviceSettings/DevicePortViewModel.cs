using System;
using Ambinity.ViewModels;
using AmbinityCore.Models.Device;

namespace Ambinity.Views.Screens.DeviceSettings;

public class DevicePortViewModel : ViewModelBase
{
    public DevicePortViewModel(LEDOutput output)
    {
        Content = output.Device.Name;
        Icon = output.Device.Icon;
        Output = output;
        _device = output.Device;
        _device.DeviceUpdate += OnDeviceUpdated;
    }

    private void OnDeviceUpdated()
    {
        Content = _device.Name;
        Icon = _device.Icon;
        OnPropertyChanged(nameof(Icon));
        OnPropertyChanged(nameof(Content));
    }

    private AmbinityDevice _device;
    public LEDOutput Output { get; set; }
    public event Action<DevicePortViewModel, bool> Selected;
    public string Content { get; set; }
    private void OnMouseOverChanged(bool value)
    {
        IsMouseOver = value;
    }

    private bool _isMouseOver;

    public bool IsMouseOver
    {
        get => _isMouseOver;
        set
        {
            _isMouseOver = value;
            OnPropertyChanged(nameof(IsMouseOver));
            OnPropertyChanged(nameof(ShowButtons));
        }
    }

    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowButtons));
        }
    }
    public string Icon { get; set; }
    public bool ShowButtons => IsSelected || IsMouseOver;
    public void Update()
    {
        // Name = _zoneFigure.Zone.Name;
        //IsSelected = _figure.IsSelected;
    }

    public void OnLayerPointerPress(bool isCtrl)
    {
        Selected?.Invoke(this, isCtrl);
    }
}