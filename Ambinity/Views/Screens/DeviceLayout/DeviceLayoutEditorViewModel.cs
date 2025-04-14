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
using AmbinityCore.Models.Device.LED;
using AmbinityCore.Models.Geography;
using AmbinityCore.Repositories;
using CommunityToolkit.Mvvm.DependencyInjection;
using Draw2D.Core;

namespace Ambinity.Views.Screens.DeviceLayout;

public class DeviceLayoutEditorViewModel : ViewModelBase
{
    public DeviceLayoutEditorViewModel(LayoutCanvasViewModel layoutViewModel,
        DeviceLayoutRightPanelViewModel rightPanelViewModel,
        IMainWindowService mainWindowService, DevicePropertiesViewModel propertiesViewModel,
        ToolsViewModel toolsViewModel,
        AmbinityDeviceRepository deviceRepository)
    {
        LayoutViewModel = layoutViewModel;
        LayoutViewModel.ItemAdded += OnItemAdded;
        LayoutViewModel.ItemRemoved += OnItemRemoved;
        _deviceRepository = deviceRepository;
        RightPanelViewModel = rightPanelViewModel;
        _propertiesViewModel = propertiesViewModel;
        mainWindowService.MainWindowClosed += OnMainWindowClosed;
        _toolsViewModel = toolsViewModel;
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
        Dispose();
    }

    public LayoutCanvasViewModel LayoutViewModel { get; set; }
    public DeviceLayoutRightPanelViewModel RightPanelViewModel { get; set; }
    private DevicePropertiesViewModel _propertiesViewModel;
    private readonly ToolsViewModel _toolsViewModel;
    private readonly AmbinityDeviceRepository _deviceRepository;

    public void Init()
    {
        var devices = new List<AmbinityDevice>();
        foreach (var device in _deviceRepository.Devices)
        {
            device.IsSelectable = true;
            device.IsDraggable = true;
            device.IsResizeable = false;
            device.IsRotatable = true;
            device.IsScalable = true;
            device.IsDeleteable = false;
            devices.Add(device);

        }
        //init layout canvas
        LayoutViewModel.ShoudDrawBackground = false;
        LayoutViewModel.Init(devices);
        _toolsViewModel.InitForDeviceLayout();
        RightPanelViewModel.PropertiesViewModel = _propertiesViewModel;
        RightPanelViewModel.Init();
    }

    public override void Dispose()
    {
        LayoutViewModel?.Dispose();
        RightPanelViewModel?.Dispose();
    }
}
