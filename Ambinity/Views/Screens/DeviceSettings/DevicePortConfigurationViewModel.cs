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
    public DevicePortConfigurationViewModel(PortDetailViewModel portDetailViewModel)
    {
        PortDetailViewModel = portDetailViewModel;
    }

    private IController _controller;
    public PortDetailViewModel PortDetailViewModel { get; set; }

    public void Init(IController controller)
    {
        _controller = controller;
        Ports = new List<DevicePortViewModel>();
        foreach (var output in controller.LedController.Outputs)
        {
            var port = new DevicePortViewModel(output);
            RegisterPort(port);
            Ports.Add(port);
        }

        OnPortSelected(Ports[0], false);
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
            foreach (var output in Ports)
            {
                output.IsSelected = false;
            }

            port.IsSelected = true;
        }
        else
        {
            port.IsSelected = !port.IsSelected;
        }

        if (SelectedPorts.Count == 1)
        {
            PortDetailViewModel.Init(port);
        }

        else if (SelectedPorts.Count > 1)
        {
            PortDetailViewModel.Init(SelectedPorts);
        }

        Dispatcher.UIThread.Invoke(GetCurrentElementsGeometry);
    }


    public List<DevicePortViewModel> Ports { get; set; }
    private List<DevicePortViewModel> SelectedPorts => Ports.Where(o => o.IsSelected).ToList();
    private DevicePortViewModel _multiplePorts;
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
        foreach (var output in SelectedPorts)
        {
            if (output.Output.OutputPosition == null)
                continue;
            var geometry = new RectangleGeometry(output.Output.OutputPosition.ToRect())
            {
                RadiusX = 2,
                RadiusY = 2
            };
            newGroup.Children.Add(geometry);
        }

        var controllerRect =
            new RectangleGeometry(new Rect(0, 0, _controller.PhysicalWidth, _controller.PhysicalHeight));
        var geo = new CombinedGeometry(GeometryCombineMode.Exclude, controllerRect, newGroup);
        CurrentGeometry = geo;
    }

    public override void Dispose()
    {
        base.Dispose();
        if (Ports != null)
        {
            foreach (var port in Ports)
            {
                UnRegiseterPort(port);
            }
        }
        PortDetailViewModel?.Dispose();
    }
}