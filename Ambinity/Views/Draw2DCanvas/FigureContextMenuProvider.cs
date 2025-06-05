using System;
using System.Linq;
using System.Threading.Tasks;
using Ambinity.Views.LayoutEditor.Canvas;
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
    private CanvasViewModelBase _canvasVM;
    private MenuFlyout _contextMenu;
    private MenuItem _pasteMenuItem;

    public FigureContextMenuProvider(CanvasViewModelFactory canvasViewModelFactory)
    {
        canvasViewModelFactory.CurrentChanged += OnCurrentCanvasViewModelChanged;

        // HotKeyManager.SetHotKey(_pasteMenuItem, new KeyGesture(Key.V, KeyModifiers.Control));
    }

    private void OnCurrentCanvasViewModelChanged(CanvasViewModelBase vm)
    {
        _canvasVM = vm;
        _contextMenu = new MenuFlyout()
        {
            Placement = PlacementMode.Pointer
        };
        _pasteMenuItem = new MenuItem()
        {
            Header = "Paste",
            Command = _canvasVM.PasteCommand,
            // InputGesture = new KeyGesture(Key.V, KeyModifiers.Control)
        };
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
                    Header = "Copy",
                    Command = _canvasVM.CopySelectedFigureCommand,
                    // InputGesture = new KeyGesture(Key.C, KeyModifiers.Control)
                });
                _contextMenu.Items.Add(new MenuItem()
                {
                    Header = "Delete",
                    Command = _canvasVM.DeleteCommand,
                    //   InputGesture = new KeyGesture(Key.Delete)
                });
            }

        }
        else if (containerFigure is DeviceContainerFigure)
        {
            _contextMenu.Items.Add(new MenuItem()
            {
                Header = "Ping device",
                Command = new AsyncRelayCommand<AmbinityDevice>(PingDevice),
                CommandParameter = containerFigure.ChildItem
            });
            _contextMenu.Items.Add(new MenuItem()
            {
                Header = "Order check",
                Command = new AsyncRelayCommand<AmbinityDevice>(CheckDeviceLedOrder),
                CommandParameter = containerFigure.ChildItem
            });
            if (containerFigure.IsDragable)
            {
                _contextMenu.Items.Add(new MenuItem()
                {
                    Header = "Lock",
                    Command = new AsyncRelayCommand(() => LockUnlockMultipleItems(true)),
                    CommandParameter = null
                });
            }
            else
            {
                _contextMenu.Items.Add(new MenuItem()
                {
                    Header = "Unlock",
                    Command = new AsyncRelayCommand(() => LockUnlockMultipleItems(false)),
                    CommandParameter = null
                });
            }

            if (_canvasVM.Canvas.Selection.AllActive.Count > 1 && containerFigure.ChildItem.GroupID == Guid.Empty)
            {
                _contextMenu.Items.Add(new MenuItem()
                {
                    Header = "Link",
                    Command = new AsyncRelayCommand(LinkItem),
                    CommandParameter = containerFigure.ChildItem
                });
            }
            if (containerFigure.ChildItem.GroupID != Guid.Empty)
            {
                _contextMenu.Items.Add(new MenuItem()
                {
                    Header = "Unlink",
                    Command = new AsyncRelayCommand<Guid>(UnlinkItem),
                    CommandParameter = containerFigure.ChildItem.GroupID
                });
            }
        }

        // if (_canvasVM.Canvas.Selection.AllActive.Count > 1 && containerFigure.ChildItem.GroupID == Guid.Empty)
        // {
        //     _contextMenu.Items.Add(new MenuItem() { Header = "Link", Command = new AsyncRelayCommand(LinkItem), CommandParameter = containerFigure.ChildItem });
        // }
        // if (containerFigure.ChildItem.GroupID != Guid.Empty)
        // {
        //     _contextMenu.Items.Add(new MenuItem() { Header = "Unlink", Command = new AsyncRelayCommand<Guid>(UnlinkItem), CommandParameter = containerFigure.ChildItem.GroupID });
        // }

    }
    private async Task LockUnlockMultipleItems(bool lockItems)
    {
        var selectedFigures = _canvasVM.Canvas.Selection.AllActive
            .OfType<ContainerFigure>()
            .ToList();

        foreach (var containerFigure in selectedFigures)
        {
            containerFigure.IsDragable = !lockItems;
            if (containerFigure.ChildItem != null)
                containerFigure.ChildItem.IsDraggable = !lockItems;
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
    private async Task LinkItem()
    {
        var groupID = Guid.NewGuid();
        foreach (var figure in _canvasVM.Canvas.Selection.AllActive)
        {
            if (figure is ContainerFigure containerFigure)
            {
                containerFigure.ChildItem.GroupID = groupID;
            }
        }
    }

    private async Task LockUnlockItem(ContainerFigure containerFigure)
    {
        if (containerFigure.IsDragable)
        {
            containerFigure.IsDragable = false;
            containerFigure.ChildItem.IsDraggable = false;
        }
        else
        {
            containerFigure.IsDragable = true;
            containerFigure.ChildItem.IsDraggable = true;
        }
    }
    private async Task UnlinkItem(Guid groupID)
    {

        foreach (var figure in _canvasVM.Canvas.Selection.All)
        {
            if (figure is ContainerFigure containerFigure)
            {
                if (containerFigure.ChildItem.GroupID == groupID)
                    containerFigure.ChildItem.GroupID = Guid.Empty;
            }
        }
    }
}
