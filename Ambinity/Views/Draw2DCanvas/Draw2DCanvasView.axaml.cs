using AmbinityCore.Models.Device;
using AmbinityCore.Models.Lighting.Zone;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Draw2D.Core;
using Draw2D.Core.Shapes.Basic;
using Microsoft.Extensions.DependencyInjection;

namespace Ambinity.Views.Draw2DCanvas;

public partial class Draw2DCanvasView : UserControl
{
    private ICanvas _canvas;
    private Draw2DCanvasViewModel _viewModel;

    public Draw2DCanvasView()
    {
        InitializeComponent();
        _viewModel = Ioc.Default.GetRequiredService<Draw2DCanvasViewModel>();
        _canvas = _viewModel.Canvas;
        _canvas.FigureRightClicked += OnFigureRightClicked;
    }

    private void OnFigureRightClicked(object? sender, FigureClickEventArgs e)
    {
        var figure = e.Sender as Figure;
        if(!figure.IsDragable)
            return;
        if (figure is DeviceContainerFigure)
        {
            var deviceContainerFigure = figure as DeviceContainerFigure;
            var flyOut = CreateDeviceFlyoutMenu(deviceContainerFigure);
            flyOut.ShowAt(Draw2DControl);
        }
        else if (figure is PolyLine)
        {
            var polyLine = figure as PolyLine;
            var flyOut = CreatePolyLineFlyoutMenu(polyLine);
            flyOut.ShowAt(Draw2DControl);
        }
        else if (figure is LightingZoneFigure)
        {
            var zoneFigure = figure as LightingZoneFigure;
            var flyOut = CreateZoneFlyoutMenu(zoneFigure);
            flyOut.ShowAt(Draw2DControl);
        }
    }

    private MenuFlyout CreateDeviceFlyoutMenu(DeviceContainerFigure figure)
    {
        var flyout = new MenuFlyout()
        {
            Placement = PlacementMode.Pointer
        };
        var identifyDeviceMenu = new MenuItem() { Header = "Identify" };
        var propertiesMenu = new MenuItem() { Header = "Properties" };
        flyout.Items.Add(identifyDeviceMenu);
        flyout.Items.Add(propertiesMenu);
        return flyout;
    }
    private MenuFlyout CreateZoneFlyoutMenu(LightingZoneFigure figure)
    {
        var flyout = new MenuFlyout()
        {
            Placement = PlacementMode.Pointer
        };
        flyout.Items.Add(new MenuItem() { Header = "Copy"});
        flyout.Items.Add(new MenuItem() { Header = "Paste" });
        flyout.Items.Add(new MenuItem() { Header = "-" });
        flyout.Items.Add(new MenuItem() { Header = "Show/Hide"});
        flyout.Items.Add(new MenuItem() { Header = "Lock/Unlock" });
        flyout.Items.Add(new MenuItem() { Header = "-" });
        flyout.Items.Add(new MenuItem() { Header = "Rotate 90" });
        flyout.Items.Add(new MenuItem() { Header = "Rotate-90" });
        flyout.Items.Add(new MenuItem() { Header = "Reset appearance" });
        flyout.Items.Add(new MenuItem() { Header = "-" });
        flyout.Items.Add(new MenuItem() { Header = "Delete" });
        return flyout;
    }
    private MenuFlyout CreatePolyLineFlyoutMenu(PolyLine figure)
    {
        var flyout = new MenuFlyout()
        {
            Placement = PlacementMode.Pointer
        };
        var identifyDeviceMenu = new MenuItem() { Header = "Delete" };
        var propertiesMenu = new MenuItem() { Header = "Properties" };
        flyout.Items.Add(identifyDeviceMenu);
        flyout.Items.Add(propertiesMenu);
        return flyout;
    }
}