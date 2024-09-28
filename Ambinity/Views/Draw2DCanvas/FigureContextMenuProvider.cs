using System.Linq;
using System.Threading.Tasks;
using Ambinity.Views.Screens.DeviceLayout;
using AmbinityCore.Converters;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Geography;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Repositories;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core;

namespace Ambinity.Views.Draw2DCanvas;

public class FigureContextMenuProvider
{
    private Draw2DCanvasViewModel _canvasVM;
    private MenuFlyout _contextMenu;
    private MenuItem _pasteMenuItem;

    public FigureContextMenuProvider(Draw2DCanvasViewModel canvas)
    {
        _canvasVM = canvas;
        _contextMenu = new MenuFlyout()
        {
            Placement = PlacementMode.Pointer
        };
        _pasteMenuItem = new MenuItem()
        {
            Header = "Paste", Command = _canvasVM.PasteCommand,
            InputGesture = new KeyGesture(Key.V, KeyModifiers.Control)
        };
        HotKeyManager.SetHotKey(_pasteMenuItem, new KeyGesture(Key.V, KeyModifiers.Control));
    }

    public MenuFlyout GetContextMenu(Figure clickedItem, Point clickPoint)
    {
        if (clickedItem == null)
        {
            CreateCanvasContextMenu(clickPoint);
        }
        else
        {
            CreateFigureContextMenu(clickedItem);
        }

        return _contextMenu;
    }

    private void CreateCanvasContextMenu(Point clickPoint)
    {
        _contextMenu.Items.Clear();

        _contextMenu.Items.Add(_pasteMenuItem);
    }

    private void CreateFigureContextMenu(Figure figure)
    {
        var containerFigure = figure as ContainerFigure;
        _contextMenu.Items.Clear();
        if (containerFigure is LightingZoneFigure)
        {
            if (containerFigure.ChildItem.IsDeleteable)
            {
                _contextMenu.Items.Add(new MenuItem()
                {
                    Header = "Copy", Command = _canvasVM.CopySelectedFigureCommand,
                    InputGesture = new KeyGesture(Key.C, KeyModifiers.Control)
                });
                _contextMenu.Items.Add(new MenuItem()
                {
                    Header = "Delete", Command = _canvasVM.DeleteCommand, InputGesture = new KeyGesture(Key.Delete)
                });
            }
        }
        else if (containerFigure is DeviceContainerFigure)
        {
            _contextMenu.Items.Add(new MenuItem() { Header = "Ping device", Command = new AsyncRelayCommand<AmbinityDevice>(PingDevice),CommandParameter = containerFigure.ChildItem});
            _contextMenu.Items.Add(new MenuItem() { Header = "Order check", Command = new AsyncRelayCommand<AmbinityDevice>(CheckDeviceLedOrder),CommandParameter = containerFigure.ChildItem});
            _contextMenu.Items.Add(new MenuItem() { Header = "Disable device" });
        }


       
    }

    private async Task PingDevice(AmbinityDevice device)
    {
        await device.Ping();
    }

    private async Task CheckDeviceLedOrder(AmbinityDevice device)
    {
        await device.OrderCheck();
    }
}