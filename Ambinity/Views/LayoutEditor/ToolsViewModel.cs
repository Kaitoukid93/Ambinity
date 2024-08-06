using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Ambinity.ViewModels;
using AmbinityCore.DataBase;
using AmbinityCore.Models.Flyout;
using AmbinityCore.Models.Toolbar;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core;

namespace Ambinity.Views.LayoutEditor;

/// <summary>
/// UI logic for toolbar
/// </summary>
public class ToolsViewModel : ViewModelBase
{
    public event Action FitCanvasToViewEvent;
    public event Action ToggleSnapToGridEvent;

    public ToolsViewModel(GeneralSettingsManager settingsManager)
    {
        ToolbarItems = new ObservableCollection<IToolbarItem>();
        _settingsManager = settingsManager;
        CommandSetup();
    }

    public ObservableCollection<IToolbarItem> ToolbarItems { get; set; }
    private GeneralSettingsManager _settingsManager;

    /// <summary>
    /// update tools based on selected item and state
    /// </summary>
    public void UpdateTools()
    {
    }

    private void CommandSetup()
    {
        FitCanvasToViewCommand = new RelayCommand(FitCanvasToView);
        ToggleSnapToGridCommand = new RelayCommand(ToggleSnapToGrid);
    }

    /// <summary>
    /// init startup tools
    /// </summary>
    public void Init()
    {
        ToolbarItems.Clear();
        var addZonetools = new FlyoutButtonToolbarItem("Add", "Add new Zone", "Add_new_zone");
        FlyoutItem addAmbilightZone = new FlyoutItem("Ambilight", "ambilight");
        FlyoutItem addColorPaletteZone = new FlyoutItem("Color Palette", "colorpalette");
        addZonetools.FlyoutItems.Add(addAmbilightZone);
        addZonetools.FlyoutItems.Add(addColorPaletteZone);
        var snapToGridTools = new ToggleToolbarItem("SnapToGrid", "Toggle snap to grid", "Snap_to_grid");
        snapToGridTools.IsChecked = _settingsManager.Settings.EnableSnapToGrid;
        snapToGridTools.Command = ToggleSnapToGridCommand;
        var centerCanvasTool = new ButtonToolbarItem("Center", "Reset Canvas", "Center_canvas");
        centerCanvasTool.Command = FitCanvasToViewCommand;
        var separator = new SeparatorToolbarItem();
        ToolbarItems.Add(addZonetools);
        ToolbarItems.Add(separator);
        ToolbarItems.Add(snapToGridTools);
        ToolbarItems.Add(centerCanvasTool);
    }

    private void FitCanvasToView()
    {
        FitCanvasToViewEvent?.Invoke();
    }

    private void ToggleSnapToGrid()
    {
        ToggleSnapToGridEvent?.Invoke();
    }

    public override void Dispose()
    {
        ToolbarItems.Clear();
    }
    public ICommand FitCanvasToViewCommand { get; set; }
    public ICommand ToggleSnapToGridCommand { get; set; }
}