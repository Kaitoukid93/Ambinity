using System.Collections.Generic;
using System.Linq;
using Ambinity.ViewModels;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using Draw2D.Core;

namespace Ambinity.Views.Screens.DeviceSettings;

public class DevicePortConfigurationViewModel : ViewModelBase
{
    public DevicePortConfigurationViewModel( PortDetailViewModel portDetailViewModel)
    {
        PortDetailViewModel = portDetailViewModel;
    }
    private IController _controller;
    public PortDetailViewModel PortDetailViewModel { get; set; }
    public void Init(IController controller)
    {
        _controller = controller;
        Outputs = new List<DevicePortViewModel>();
        foreach (var output in controller.LedController.Outputs)
        {
            var port = new DevicePortViewModel(output);
            RegisterPort(port);
            Outputs.Add(port);
        }

        OnPortSelected(Outputs[0], false);

    }

    private void RegisterPort(DevicePortViewModel port)
    {
        port.Selected += OnPortSelected;
    }

    private void UnRegiseterPort(DevicePortViewModel port)
    {
        port.Selected -= OnPortSelected;
    }
    private void OnPortSelected(DevicePortViewModel port, bool isCtrl)
    {
        if (!isCtrl)
        {
            foreach (var output in Outputs)
            {
                output.IsSelected = false;
            }
        }

        port.IsSelected = true;
        SelectedPort = port;
        Dispatcher.UIThread.Invoke(GetCurrentElementsGeometry);

    }

    private DevicePortViewModel _selectedPort;

    public DevicePortViewModel SelectedPort
    {
        get => _selectedPort;
        set
        {
            _selectedPort = value;
            PortDetailViewModel.Init(_selectedPort);
            OnPropertyChanged();
        }
    }

    public List<DevicePortViewModel> Outputs { get; set; }
    private List<DevicePortViewModel> SelectedOutputs => Outputs.Where(o => o.IsSelected).ToList();
    public IController Controller => _controller;
    private Geometry _currentGeometry;

    public Geometry CurrentGeometry
    {
        get => _currentGeometry;
        set
        {
            _currentGeometry = value;
            OnPropertyChanged();
        }
    }

    private void GetCurrentElementsGeometry()
    {
        GeometryGroup newGroup = new GeometryGroup();
        foreach (var output in SelectedOutputs)
        {
            if(output.Output.OutputPosition ==null)
                continue;
            var geometry = new RectangleGeometry(output.Output.OutputPosition.ToRect())
            {
                RadiusX = 2,
                RadiusY = 2
            };
            newGroup.Children.Add(geometry);
        }

        var controllerRect = new RectangleGeometry(new Rect(0, 0, _controller.PhysicalWidth, _controller.PhysicalHeight));
        var geo = new CombinedGeometry(GeometryCombineMode.Exclude, controllerRect, newGroup);
        CurrentGeometry = geo;
    }
    public override void Dispose()
    {
        base.Dispose();
        if (Outputs != null)
        {
            foreach (var port in Outputs)
            {
                UnRegiseterPort(port);
            }
        }
        
    }
}