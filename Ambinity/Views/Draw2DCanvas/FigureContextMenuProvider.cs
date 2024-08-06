using AmbinityCore.Models.Geography;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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
        if (containerFigure.ChildItem.IsDeleteable)
            _contextMenu.Items.Add(new MenuItem()
            {
                Header = "Copy", Command = _canvasVM.CopySelectedFigureCommand,
                InputGesture = new KeyGesture(Key.C, KeyModifiers.Control)
            });

        _contextMenu.Items.Add(new MenuItem() { Header = "-" });
        _contextMenu.Items.Add(new MenuItem() { Header = "Show/Hide" });
        _contextMenu.Items.Add(new MenuItem() { Header = "Lock/Unlock" });
        _contextMenu.Items.Add(new MenuItem() { Header = "-" });
        if (containerFigure.ChildItem.IsDeleteable)
            _contextMenu.Items.Add(new MenuItem()
                { Header = "Delete", Command = _canvasVM.DeleteCommand, InputGesture = new KeyGesture(Key.Delete) });
    }
}