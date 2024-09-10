using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Ambinity.ViewModels;
using AmbinityCore.DataBase;
using AmbinityCore.Models.Flyout;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Models.Profile;
using AmbinityCore.Models.Toolbar;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core;
using Draw2D.Core.Policies.RouterPolicy;
using Draw2D.Core.Shapes.Basic;

namespace Ambinity.Views.LayoutEditor;

/// <summary>
/// UI logic for toolbar
/// </summary>
public class ToolsViewModel : ViewModelBase
{
    public event Action FitCanvasToViewEvent;
    public event Action ToggleSnapToGridEvent;
    public event Action<Figure> AddFigure;
    public event Action<PolylineTool> InstallPolylineTool;

    public ToolsViewModel(GeneralSettingsManager settingsManager,LightingZoneRepository lightingZoneRepository, LightingProfileDecoder decoder)
    {
        ToolbarItems = new ObservableCollection<IToolbarItem>();
        _settingsManager = settingsManager;
        _lightingZoneRepository = lightingZoneRepository;
        _decoder = decoder;
        _decoder.RenderingStatusChanged += OnRenderingStatusChanged;
        CommandSetup();
        _addColorZoneTools = AddColorZoneTool();
    }

    private FlyoutButtonToolbarItem _addColorZoneTools;
    private FlyoutButtonToolbarItem _addAmbilightZoneTools;
    private FlyoutButtonToolbarItem _addAnimationZoneTools;
    private void OnRenderingStatusChanged()
    {
        ZoneToolsCommandCanExecute = !_decoder.IsRendering;
        ShowLockSymbol = _decoder.IsRendering;
        AddAmbilightZoneCommand.NotifyCanExecuteChanged();
        AddAnimationZoneCommand.NotifyCanExecuteChanged();
        AddColorZoneCommand.NotifyCanExecuteChanged();

    }

    private bool _showLockSymbol;

    public bool ShowLockSymbol
    {
        get => _showLockSymbol;
        set
        {
            _showLockSymbol = value;
            OnPropertyChanged();
        }
    }
    public bool ZoneToolsCommandCanExecute { get; set; }
    public ObservableCollection<IToolbarItem> ToolbarItems { get; set; }
    private GeneralSettingsManager _settingsManager;
    private readonly LightingZoneRepository _lightingZoneRepository;
    private LightingProfileDecoder _decoder;

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
        AddAnimationZoneCommand = new RelayCommand(AddAnimationZone,()=>ZoneToolsCommandCanExecute);
        AddAmbilightZoneCommand = new RelayCommand(AddAmbilightZone,()=>ZoneToolsCommandCanExecute);
        AddColorZoneCommand = new RelayCommand(AddColorZone,()=>ZoneToolsCommandCanExecute);
    }

    private void AddColorZone()
    {
        //throw new NotImplementedException();
    }

    private void AddAmbilightZone()
    {
        var zone = _lightingZoneRepository.GetDefaultAmbilightZone("new zone", 100, 100, 100, 100,0);
        zone.Shape = ZoneShapeEnum.Rectangle;
        var figure = zone.GetContainer();
        figure.SetChild(zone);
        AddFigure?.Invoke(figure);
    }

    private void AddAnimationZone()
    {
        var zone = _lightingZoneRepository.GetDefaultAnimationZone("new zone", 100, 100, 100, 100,null);
        zone.Shape = ZoneShapeEnum.Rectangle;
        var figure = zone.GetContainer();
        figure.SetChild(zone);
        AddFigure?.Invoke(figure);
    }

    /// <summary>
    /// init startup tools
    /// </summary>
    public void InitForProfileEditor()
    {
        ToolbarItems.Clear();
        var snapToGridTools = new ToggleToolbarItem("SnapToGrid", "Toggle snap to grid", "Snap_to_grid");
        snapToGridTools.IsChecked = _settingsManager.Settings.EnableSnapToGrid;
        snapToGridTools.Command = ToggleSnapToGridCommand;
        var centerCanvasTool = new ButtonToolbarItem("Center", "Reset Canvas", "Center_canvas", FitCanvasToViewCommand);
        var separator = new SeparatorToolbarItem();
        ToolbarItems.Add(_addColorZoneTools);
        ToolbarItems.Add(AddAmbilightZoneTool());
        ToolbarItems.Add(AddAnimationZoneTool());
        ToolbarItems.Add(separator);
        ToolbarItems.Add(snapToGridTools);
        ToolbarItems.Add(centerCanvasTool);
        OnRenderingStatusChanged();
    }

    public void InitForDeviceLayout()
    {
        ToolbarItems.Clear();
        var snapToGridTools = new ToggleToolbarItem("SnapToGrid", "Toggle snap to grid", "Snap_to_grid");
        snapToGridTools.IsChecked = _settingsManager.Settings.EnableSnapToGrid;
        snapToGridTools.Command = ToggleSnapToGridCommand;
        var centerCanvasTool = new ButtonToolbarItem("Center", "Reset Canvas", "Center_canvas", FitCanvasToViewCommand);
        ToolbarItems.Add(snapToGridTools);
        ToolbarItems.Add(centerCanvasTool);
        OnRenderingStatusChanged();
    }

    private void AddPolyline(FlyoutItem obj)
    {
       InstallPolylineTool?.Invoke(new PolylineTool());
    }

    private ButtonToolbarItem AddAmbilightZoneTool()
    {
        return new ButtonToolbarItem("Ambilight", "Add Ambilight Zone", "ambilight", AddAmbilightZoneCommand);
    }
    private ButtonToolbarItem AddAnimationZoneTool()
    {
        return new ButtonToolbarItem("Animation", "Add Animation Zone", "LightingConfiguration_Animation", AddAnimationZoneCommand);
    }

    private FlyoutButtonToolbarItem AddColorZoneTool()
    {
        var addZonetools = new FlyoutButtonToolbarItem("Add", "Add new color zone", "SolidColor_Button", AddColorZoneCommand);
        FlyoutItem addRectangle = new FlyoutItem("Rectangle", "CanvasTool_Rectangle");
        addRectangle.FlyoutItemSelected += AddRectangle;
        FlyoutItem addEllipse = new FlyoutItem("Ellipse", "CanvasTool_Ellipse");
        addEllipse.FlyoutItemSelected += AddEllipse;
        FlyoutItem addPolyline = new FlyoutItem("Poly line", "CanvasTool_PolyLine");
        addPolyline.FlyoutItemSelected += AddPolyline;
        addZonetools.FlyoutItems.Add(addRectangle);
        addZonetools.FlyoutItems.Add(addEllipse);
        addZonetools.FlyoutItems.Add(addPolyline);
        return addZonetools;
    }
    private void AddEllipse(FlyoutItem obj)
    {
        var zone = _lightingZoneRepository.GetDefaultSolidColorZone("new zone", 100, 100, 100, 100, Colors.Aqua);
        zone.Shape = ZoneShapeEnum.Ellipse;
        var figure = zone.GetContainer();
        figure.SetChild(zone);
        AddFigure?.Invoke(figure);
    }

    private void AddRectangle(FlyoutItem obj)
    {
        var zone = _lightingZoneRepository.GetDefaultSolidColorZone("new zone", 100, 100, 100, 100, Colors.Aqua);
        zone.Shape = ZoneShapeEnum.Rectangle;
        var figure = zone.GetContainer();
        figure.SetChild(zone);
        AddFigure?.Invoke(figure);
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
    public RelayCommand FitCanvasToViewCommand { get; set; }
    public RelayCommand ToggleSnapToGridCommand { get; set; }
    public RelayCommand AddAmbilightZoneCommand { get; set; }
    public RelayCommand AddAnimationZoneCommand { get; set; }
    public RelayCommand AddColorZoneCommand { get; set; }
}