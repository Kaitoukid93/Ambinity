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
using SkiaSharp;
using Ambinity.Services;
using Avalonia.Controls.ApplicationLifetimes;
using Ambinity.Views.LayoutEditor.LEDLayoutCreator;
using AmbinityCore.Utils;
namespace Ambinity.Views.Draw2DCanvas;

public class FigureContextMenuProvider
{
    public CanvasViewModelBase CanvasVM;
    private MenuFlyout _contextMenu;
    private MenuItem _pasteMenuItem;
    private IWindowService _windowService;

    public FigureContextMenuProvider(CanvasViewModelFactory canvasViewModelFactory, IWindowService windowService)
    {
        _windowService = windowService;
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

            if (CanvasVM.SelectionCount > 1)
            {
                _contextMenu.Items.Add(new MenuItem()
                {
                    Header = "Combine LEDs",
                    Command = new RelayCommand(CombineLED),
                    CommandParameter = null
                });
                _contextMenu.Items.Add(new MenuItem()
                {
                    Header = "Arrange Horizontal",
                    Command = new RelayCommand(ArrangeHorizontal),
                    CommandParameter = null
                });
                _contextMenu.Items.Add(new MenuItem()
                {
                    Header = "Arrange Vertical",
                    Command = new RelayCommand(ArrangeVertical),
                    CommandParameter = null
                });
            }
            if (CanvasVM.SelectionCount == 1 && figure is LEDContainerFigure)
            {
                _contextMenu.Items.Add(new MenuItem()
                {
                    Header = "Split Into Matrix",
                    Command = new RelayCommand(OpenSplitConfiguration),
                    CommandParameter = null
                });
            }

        }

    }
    private void CombineLED()
    {
        var selectedFigures = CanvasVM.Canvas.Selection.All
            .OfType<LEDContainerFigure>()
            .ToList();

        var combinedLED = GeometryUltilities.CombineLED(selectedFigures);
        if (combinedLED != null)
        {
            CanvasVM.Canvas.RemoveSelected();
            CanvasVM.AddFigure(combinedLED, false);
            combinedLED.Select();
        }

    }
    /// <summary>
    /// Arrange LED horizontally
    /// </summary>
    private void ArrangeHorizontal()
    {
        //bring LEDs to same Top
        //make LEDs distance equal
        var selectedFigures = CanvasVM.Canvas.Selection.AllActive
            .OfType<LEDContainerFigure>()
            .ToList();

        if (selectedFigures.Count < 2)
            return;

        var minY = selectedFigures.Min(f => f.Y);
        // Sort by X
        selectedFigures.Sort((a, b) => a.X.CompareTo(b.X));

        float minX = selectedFigures.Min(f => f.X);
        float maxX = selectedFigures.Max(f => f.X + f.Width);

        float totalWidth = selectedFigures.Sum(f => f.Width);
        int count = selectedFigures.Count;
        float availableSpace = (maxX - minX) - totalWidth;
        float gap = count > 1 ? availableSpace / (count - 1) : 0;

        float currentX = minX;
        foreach (var figure in selectedFigures)
        {
            figure.ForceTranslate(currentX - figure.X, minY - figure.Y);
            currentX += figure.Width + gap;
        }
    }

    /// <summary>
    /// Arrange LED vertically with equal spacing
    /// </summary>
    private void ArrangeVertical()
    {
        var selectedFigures = CanvasVM.Canvas.Selection.AllActive
            .OfType<LEDContainerFigure>()
            .ToList();

        if (selectedFigures.Count < 2)
            return;

        var minX = selectedFigures.Min(f => f.X);
        // Sort by Y
        selectedFigures.Sort((a, b) => a.Y.CompareTo(b.Y));

        float minY = selectedFigures.Min(f => f.Y);
        float maxY = selectedFigures.Max(f => f.Y + f.Height);

        float totalHeight = selectedFigures.Sum(f => f.Height);
        int count = selectedFigures.Count;
        float availableSpace = (maxY - minY) - totalHeight;
        float gap = count > 1 ? availableSpace / (count - 1) : 0;

        float currentY = minY;
        foreach (var figure in selectedFigures)
        {
            figure.ForceTranslate(minX - figure.X, currentY - figure.Y);
            currentY += figure.Height + gap;
        }
    }
    private void CreateLED()
    {
        var selectedFigures = CanvasVM.Canvas.Selection.All
            .OfType<PolyLine>()
            .ToList();
        var led = GeometryUltilities.CreateLEDFromSelectedPolyLines(selectedFigures);
        if (led != null)
        {
            CanvasVM.Canvas.RemoveSelected();
            CanvasVM.AddFigure(led, false);
        }
    }


    private async void OpenSplitConfiguration()
    {
        var lifeTime = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
        var selectedFigure = CanvasVM.Canvas.Selection.Primary as LEDContainerFigure;
        if (selectedFigure == null)
            return;
        if (selectedFigure.ChildItem is AmbinityLED led && !string.IsNullOrEmpty(led.Geometry))
        {
            var vm = new LEDSplitToolConfigurationViewModel(selectedFigure);
            vm.Accept+= () =>
            {
               SplitFigureToMatrix(vm.RowNumber, vm.ColumnNumber, vm.ColumnGutter, vm.RowGutter);
                vm?.Dispose();
            };
            var splitConfigurationWindow = await _windowService.ShowDialogWindow(vm, _windowService.GetCurrentWindow());

        }

    }

    /// <summary>
    /// Split the geometry of a single selected figure into an n x m matrix of new LEDs using CreateLEDFromGeometry, with gaps between columns and rows.
    /// </summary>
    private void SplitFigureToMatrix(int nRows, int nCols, float colGap = 2, float rowGap = 2)
    {
        var selectedFigure = CanvasVM.Canvas.Selection.All
            .OfType<LEDContainerFigure>()
            .FirstOrDefault();

        var leds = GeometryUltilities.SplitLEDInToMatrix(selectedFigure, nRows, nCols, colGap, rowGap);
        CanvasVM.Canvas.RemoveSelected();
        foreach (var led in leds)
        {
            CanvasVM.AddFigure(led, false);
        }
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
