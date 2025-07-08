using System;
using System.Linq;
using System.Threading.Tasks;
using Ambinity.Views.LayoutEditor.Canvas;
using Ambinity.Views.Screens.DeviceLayout;
using AmbinityCore.Converters;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.LED;
using AmbinityCore.Models.Geography;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Repositories;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core;
using Draw2D.Core.Shapes.Basic;
using System.Collections.Generic;
using static System.FormattableString;
namespace Ambinity.Views.Draw2DCanvas;

public class FigureContextMenuProvider
{
    public CanvasViewModelBase CanvasVM;
    private MenuFlyout _contextMenu;
    private MenuItem _pasteMenuItem;

    public FigureContextMenuProvider(CanvasViewModelFactory canvasViewModelFactory)
    {
        // HotKeyManager.SetHotKey(_pasteMenuItem, new KeyGesture(Key.V, KeyModifiers.Control));
    }


    public void Init()
    {
        if (CanvasVM == null)
            return;
        _pasteMenuItem = new MenuItem()
        {
            Header = "Paste",
            Command = CanvasVM?.PasteCommand,
            // InputGesture = new KeyGesture(Key.V, KeyModifiers.Control)
        };
        _contextMenu = new MenuFlyout()
        {
            Placement = PlacementMode.Pointer
        };
    }
    public MenuFlyout GetContextMenu(Figure clickedItem, Point clickPoint)
    {
        if (clickedItem == null || !clickedItem.IsSelectable)
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
                    Command = CanvasVM.CopySelectedFigureCommand,
                    // InputGesture = new KeyGesture(Key.C, KeyModifiers.Control)
                });
                _contextMenu.Items.Add(new MenuItem()
                {
                    Header = "Delete",
                    Command = CanvasVM.DeleteCommand,
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

            if (CanvasVM.Canvas.Selection.AllActive.Count > 1 && containerFigure.ChildItem.GroupID == Guid.Empty)
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
        if (CanvasVM is LEDLayoutCreatorCanvasViewModel ledCanvasVM)
        {
            if (figure is PolyLine)
            {
                _contextMenu.Items.Add(new MenuItem()
                {
                    Header = "Create LED",
                    Command = new RelayCommand(CreateLED),
                    CommandParameter = null
                });
            }
            if (figure is LEDContainerFigure)
            {
                _contextMenu.Items.Add(new MenuItem()
                {
                    Header = "Copy LED",
                    Command = CanvasVM.CopySelectedFigureCommand,
                    CommandParameter = null
                });
            }
            _contextMenu.Items.Add(new MenuItem()
            {
                Header = "Delete",
                Command = CanvasVM.DeleteCommand,
                //   InputGesture = new KeyGesture(Key.Delete)
            });

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
    private void CreateLED()
    {
        var selectedFigures = CanvasVM.Canvas.Selection.All
            .OfType<PolyLine>()
            .ToList();

        if (selectedFigures.Count == 0)
            return;
        var minX = selectedFigures.Min(f => f.X);
        var minY = selectedFigures.Min(f => f.Y);
        var maxX = selectedFigures.Max(f => f.X + f.Width);
        var maxY = selectedFigures.Max(f => f.Y + f.Height);
        var boundingBox = new Rect(minX, minY, maxX - minX, maxY - minY);
        var geometries = new List<Geometry>();

        foreach (var polyline in selectedFigures)
        {
            var points = polyline.Points.ToList();
            if (points.Count < 2)
                continue;

            // Auto-close if not closed
            if (points.First() != points.Last())
                points.Add(points.First());

            var geom = new StreamGeometry();
            using (StreamGeometryContext ctx = geom.Open())
            {
                var startVertex = CanvasVM.Canvas.CoordinateSystem.ToScreenSpace(polyline.StartPoint);
                ctx.BeginFigure(new Avalonia.Point(startVertex[0], startVertex[1]), false);
                int count = 0;
                foreach (var point in polyline.Points)
                {
                    if (count == 0)
                    {
                        count++;
                        continue;
                    }

                    var vertex = CanvasVM.Canvas.CoordinateSystem.ToScreenSpace(point);
                    var v = new Avalonia.Point(vertex[0], vertex[1]);
                    ctx.LineTo(v);
                    count++;
                }
                // ctx.PolyLineTo(Points.Skip(1).Select(p =>
                //     {
                //         var vertex = Canvas.CoordinateSystem.ToScreenSpace(p);
                //         return new Avalonia.Point(vertex[0], vertex[1]);
                //     }).ToList(),
                //     true /* is stroked */, true /* is smooth join */);
            }
            geometries.Add(geom);
        }

        if (geometries.Count == 0)
            return;

        // Combine all geometries into one
        Geometry combined = geometries[0];
        for (int i = 1; i < geometries.Count; i++)
        {
            combined = new CombinedGeometry(GeometryCombineMode.Union, combined, geometries[i]);
        }
        string geometryString = Invariant(combined.ToString()
                                .Replace("F1", "")
                                .Replace(";", ",")
                                .Replace("L", " L")
                                .Replace("C", " C"));

        (CanvasVM as LEDLayoutCreatorCanvasViewModel).CreateLEDFromGeometry(geometryString, boundingBox);
    }


    private async Task LockUnlockMultipleItems(bool lockItems)
    {
        var selectedFigures = CanvasVM.Canvas.Selection.AllActive
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
        foreach (var figure in CanvasVM.Canvas.Selection.AllActive)
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

        foreach (var figure in CanvasVM.Canvas.Selection.All)
        {
            if (figure is ContainerFigure containerFigure)
            {
                if (containerFigure.ChildItem.GroupID == groupID)
                    containerFigure.ChildItem.GroupID = Guid.Empty;
            }
        }
    }
}
