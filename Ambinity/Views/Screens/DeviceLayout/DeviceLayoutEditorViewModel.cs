using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.Device;
using AmbinityCore.Models.Geography;
using CommunityToolkit.Mvvm.DependencyInjection;
using Draw2D.Core;

namespace Ambinity.Views.Screens.DeviceLayout;

public class DeviceLayoutEditorViewModel : ViewModelBase
{
    public DeviceLayoutEditorViewModel(LayoutCanvasViewModel layoutViewModel, RightPanelViewModel rightPanelViewModel,
        IMainWindowService mainWindowService, DevicePropertiesViewModel propertiesViewModel,
        SerialControllerRepository serialControllerRepository )
    {
        LayoutViewModel = layoutViewModel;
        LayoutViewModel.ItemAdded += OnItemAdded;
        LayoutViewModel.ItemRemoved += OnItemRemoved;
        _serialControllerRepository = serialControllerRepository;
        RightPanelViewModel = rightPanelViewModel;
        _propertiesViewModel = propertiesViewModel;
        mainWindowService.MainWindowClosed += OnMainWindowClosed;
    }

    private void OnItemRemoved(Figure item)
    {
        //throw new NotImplementedException();
    }

//todo how to properly notify profile to update zone
    private void OnItemAdded(Figure item)
    {
        //throw new NotImplementedException();
    }

    private void OnMainWindowClosed(object? sender, EventArgs e)
    {
        // throw new NotImplementedException();
    }

    public LayoutCanvasViewModel LayoutViewModel { get; set; }
    public RightPanelViewModel RightPanelViewModel { get; set; }
    private SerialControllerRepository _serialControllerRepository;
    private DevicePropertiesViewModel _propertiesViewModel;
    public void Init()
    {
        var devices = new List<AmbinityDevice>();
        foreach (SerialController controller in _serialControllerRepository.Items)
        {
            foreach (var output in controller.LedController.Outputs)
            {
                var device = output.Device;
                device.IsSelectable = true;
                device.IsDraggable = true;
                device.IsResizeable = false;
                device.IsRotatable = true;
                device.IsScalable = true;
                device.IsDeleteable = false;
                devices.Add(device);
            }
        }

        var localRepo = Ioc.Default.GetRequiredService<AmbinityDeviceLayoutRepository>();
        var onlineRepo = Ioc.Default.GetRequiredService<AmbinityDeviceOnlineRepository>();
        //init layout canvas
        LayoutViewModel.ShoudDrawBackground = false;
        LayoutViewModel.Init(devices);
        RightPanelViewModel.PropertiesViewModel = _propertiesViewModel;
         RightPanelViewModel.Init(localRepo,onlineRepo);
    }
}