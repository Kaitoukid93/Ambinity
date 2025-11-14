using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.LayoutEditor;
using Ambinity.Views.LayoutEditor.Canvas;
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

public class DeviceLayoutEditorViewModel : ScreenViewModelBase
{
    public DeviceLayoutEditorViewModel(CanvasViewModelFactory canvasViewModelFactory,
        DeviceLayoutRightPanelViewModel rightPanelViewModel,
        IMainWindowService mainWindowService, DevicePropertiesViewModel propertiesViewModel,
        ToolsViewModel toolsViewModel,
        AmbinityDeviceRepository deviceRepository)
    {
        _canvasViewModelFactory = canvasViewModelFactory;
        _deviceRepository = deviceRepository;
        RightPanelViewModel = rightPanelViewModel;
        _propertiesViewModel = propertiesViewModel;
        mainWindowService.MainWindowClosed += OnMainWindowClosed;
        _toolsViewModel = toolsViewModel;
        _toolsViewModel.ResetLayout += OnLayoutReset;
    }
    private void OnLayoutReset()
    {
        _deviceRepository.ResetDefaultLayout();
    }

    private void OnMainWindowClosed(object? sender, EventArgs e)
    {
        Dispose();
    }

    public DeviceLayoutRightPanelViewModel RightPanelViewModel { get; set; }
    private DeviceLayoutCanvasViewModel _canvasViewModel;
    public DeviceLayoutCanvasViewModel CanvasViewModel
    {
        get => _canvasViewModel;
        set
        {
            _canvasViewModel = value;
            OnPropertyChanged(nameof(CanvasViewModel));
        }
    }
    private DevicePropertiesViewModel _propertiesViewModel;
    private readonly ToolsViewModel _toolsViewModel;
    private readonly CanvasViewModelFactory _canvasViewModelFactory;
    private readonly AmbinityDeviceRepository _deviceRepository;

    public override async Task Init()
    {
        CanvasViewModel = _canvasViewModelFactory.Get<DeviceLayoutCanvasViewModel>();
        _canvasViewModelFactory.SetCurrent(CanvasViewModel);
        CanvasViewModel.Init();
        RightPanelViewModel.PropertiesViewModel = _propertiesViewModel;
        await RightPanelViewModel.Init();
    }

    public override void Dispose()
    {
        CanvasViewModel?.Dispose();
        RightPanelViewModel?.Dispose();
    }

}
