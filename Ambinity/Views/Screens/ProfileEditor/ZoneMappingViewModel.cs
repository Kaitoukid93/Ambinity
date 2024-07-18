using System;
using System.Collections.Generic;
using System.IO;
using Ambinity.ViewModels;
using Ambinity.Views.Draw2DCanvas;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Lighting.Zone;
using Avalonia;
using Draw2D.Core;
using Draw2D.Core.Shapes.Basic;

namespace Ambinity.Views.Screens.ProfileEditor;

public class ZoneMappingViewModel : ViewModelBase
{
    private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "adrilight\\");

    private string SupportedDeviceCollectionFolderPath => Path.Combine(JsonPath, "SupportedDevices");

    public ZoneMappingViewModel(Draw2DCanvasViewModel canvasViewModel, ZoneMappingToolsViewModel toolsViewModel)
    {
        CanvasViewModel = canvasViewModel;
        ToolsViewModel = toolsViewModel;
        ToolsViewModel.FitCanvasToViewEvent += FitCanvasToView;
        ToolsViewModel.ToggleSnapToGridEvent += ToggleSnapToGrid;
    }

    private void ToggleSnapToGrid()
    {
        CanvasViewModel.ToggleGridSnapCommand.Execute(null);
    }

    private void FitCanvasToView()
    {
        CanvasViewModel.FitCommand.Execute(null);
    }


    public Draw2DCanvasViewModel CanvasViewModel { get; }
    public ZoneMappingToolsViewModel ToolsViewModel { get; }

    public void Init(List<LightingZone> zones)
    {
        //add device
        var figures = new List<Figure>();
        var device = new AmbinityDevice();
        var folderPath = Path.Combine(SupportedDeviceCollectionFolderPath, "Ambino Dualring Fan");
        var deviceLayout = new AmbinityDeviceLayout(folderPath);
        deviceLayout.ApplyToDevice(device);
        device.Layout = deviceLayout;
        device.X = 100;
        device.Y = 100;
        device.SetRotation(0);
        device.SetScale(1f);

        var slaveDeviceContainerFigure =
            new DeviceContainerFigure(device.X, device.Y, device.Width, device.Height);
        slaveDeviceContainerFigure.SetDevice(device);
        slaveDeviceContainerFigure.IsDragable = false;
        figures.Add(slaveDeviceContainerFigure);
        // add lighting zone on top
        foreach (var zone in zones)
        {
            var zoneFigure = new LightingZoneFigure(zone.X, zone.Y, zone.Width, zone.Height);
            zoneFigure.SetZone(zone);
            figures.Add(zoneFigure);
        }

        //init the canvas
        CanvasViewModel.Init(figures, new Size(2000, 1000));
        ToolsViewModel.Init();
    }
}