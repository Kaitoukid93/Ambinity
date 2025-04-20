using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using Ambinity.Views.Screens.ProfileEditor.Library;
using AmbinityCore.DataBase;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Flyout;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Profile;
using AmbinityCore.Models.Toolbar;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core;
using Draw2D.Core.Policies.RouterPolicy;
using DynamicData;
using Canvas = Avalonia.Controls.Canvas;

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
    public event Action OpenFlyoutEvent;
    public event Action CloseFlyoutEvent;

    public ToolsViewModel(GeneralSettingsManager settingsManager, LightingZoneRepository lightingZoneRepository,
        LightingProfileDecoder decoder, LightingZonesLibraryViewModel lightingZonesLibraryViewModel)
    {
        _lightingZonesLibraryViewModel = lightingZonesLibraryViewModel;
        ZoneTools = new ObservableCollection<IToolbarItem>();
        CanvasTools = new ObservableCollection<IToolbarItem>();
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
        OnPropertyChanged(nameof(IsRendering));
        AddAmbilightZoneCommand.NotifyCanExecuteChanged();
        AddAnimationZoneCommand.NotifyCanExecuteChanged();
        AddVideoZoneCommand.NotifyCanExecuteChanged();
        AddColorZoneCommand.NotifyCanExecuteChanged();
        ShowLibraryCommand.NotifyCanExecuteChanged();
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
    public ObservableCollection<IToolbarItem> ZoneTools { get; set; }
    public ObservableCollection<IToolbarItem> CanvasTools { get; set; }
    private GeneralSettingsManager _settingsManager;
    private readonly LightingZoneRepository _lightingZoneRepository;
    private LightingProfileDecoder _decoder;
    private LightingProfile _currentProfile;

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
        AddAnimationZoneCommand = new RelayCommand(AddAnimationZone, () => ZoneToolsCommandCanExecute);
        AddAmbilightZoneCommand = new RelayCommand(AddAmbilightZone, () => ZoneToolsCommandCanExecute);
        AddVideoZoneCommand = new RelayCommand(AddVideoZone,()=>ZoneToolsCommandCanExecute);
        ShowLibraryCommand = new AsyncRelayCommand(ShowLibrary, () => ZoneToolsCommandCanExecute);
        AddColorZoneCommand = new RelayCommand(AddColorZone, () => ZoneToolsCommandCanExecute);
        TogglePlayPauseCommand = new RelayCommand(TogglePlayPause);
        ShowDiagCommand = new RelayCommand(ToggleShowDiag);
    }

    private void ToggleShowDiag()
    {
        ShowDiag = !ShowDiag;
    }

    private void TogglePlayPause()
    {
        if (_currentProfile != null)
            _decoder.Toggle(_currentProfile.ID);
    }

    public LibraryViewModelBase CurrentFlyoutViewModel { get; set; }
    private LightingZonesLibraryViewModel _lightingZonesLibraryViewModel;

    private async Task ShowLibrary()
    {
        CurrentFlyoutViewModel = _lightingZonesLibraryViewModel;
        CurrentFlyoutViewModel.ItemSelected += OnLightingZoneAssetSelected;
        CurrentFlyoutViewModel?.Init();
        OpenFlyoutEvent?.Invoke();
    }

    public void OnFlyoutClosing()
    {
        CurrentFlyoutViewModel.Dispose();
        CurrentFlyoutViewModel = null;
    }

    private void OnLightingZoneAssetSelected(AssetItemViewModelBase item)
    {
        AddAsset(item.Item as LightingZone);
    }

    private void AddColorZone()
    {
        //throw new NotImplementedException();
    }

    private void AddAmbilightZone()
    {
        var zone = _lightingZoneRepository.GetDefaultAmbilightZone("new zone", 100, 100, 100, 100, 0);
        zone.Shape = ZoneShapeEnum.Rectangle;
        var figure = zone.GetContainer();
        figure.SetChild(zone);
        AddFigure?.Invoke(figure);
    }
    private void AddVideoZone()
    {
         var zone = _lightingZoneRepository.GetDefaultAmbilightZone("new zone", 100, 100, 100, 100, 0);
        zone.Shape = ZoneShapeEnum.Rectangle;
        var figure = zone.GetContainer();
        figure.SetChild(zone);
        AddFigure?.Invoke(figure);
    }

    private void AddAnimationZone()
    {
        var zone = _lightingZoneRepository.GetDefaultAnimationZone("new zone", 100, 100, 100, 100);
        zone.Shape = ZoneShapeEnum.Rectangle;
        var figure = zone.GetContainer();
        figure.SetChild(zone);
        AddFigure?.Invoke(figure);
    }

    /// <summary>
    /// init startup tools
    /// </summary>
    public void InitForProfileEditor(LightingProfile profile)
    {
        _currentProfile = profile;
        Brightness = _currentProfile.Brightness;
        _decoder.FrameUpdate += OnFrameUpdated;
        ZoneTools.Clear();
        CanvasTools.Clear();
        var snapToGridTools = new ToggleToolbarItem("SnapToGrid", "Toggle snap to grid", "Snap_to_grid");
        snapToGridTools.IsChecked = _settingsManager.Settings.EnableSnapToGrid;
        snapToGridTools.Command = ToggleSnapToGridCommand;
        var centerCanvasTool = new ButtonToolbarItem("Center", "Reset Canvas", "Center_canvas",
            new SolidColorBrush(Colors.Gray), FitCanvasToViewCommand);
        var showDiagTool =
            new ToggleToolbarItem("Info", "Show stats", "wave_signal__heart_line_beat_square_graph_stats");
        showDiagTool.IsChecked = _showDiag;
        showDiagTool.Command = ShowDiagCommand;
        var separator = new SeparatorToolbarItem();
        ZoneTools.Add(_addColorZoneTools);
        ZoneTools.Add(AddAmbilightZoneTool());
        ZoneTools.Add(AddAnimationZoneTool());
        ZoneTools.Add(AddVideoZoneTool());
        ZoneTools.Add(separator);
        ZoneTools.Add(ShowLibraryTool());

        CanvasTools.Add(snapToGridTools);
        CanvasTools.Add(centerCanvasTool);
        CanvasTools.Add(showDiagTool);
        OnRenderingStatusChanged();
    }

    public void InitForDeviceLayout()
    {
        _currentProfile = _decoder.CurrentPlayingProfile;
        Brightness = _currentProfile.Brightness;
        ZoneTools.Clear();
        CanvasTools.Clear();
        var snapToGridTools = new ToggleToolbarItem("SnapToGrid", "Toggle snap to grid", "Snap_to_grid");
        snapToGridTools.IsChecked = _settingsManager.Settings.EnableSnapToGrid;
        snapToGridTools.Command = ToggleSnapToGridCommand;
        var centerCanvasTool = new ButtonToolbarItem("Center", "Reset Canvas", "Center_canvas",
            new SolidColorBrush(Colors.Gray), FitCanvasToViewCommand);
        CanvasTools.Add(snapToGridTools);
        CanvasTools.Add(centerCanvasTool);
        OnRenderingStatusChanged();
    }

    private void AddPolyline(FlyoutItem obj)
    {
        InstallPolylineTool?.Invoke(new PolylineTool());
    }

    private ButtonToolbarItem AddAmbilightZoneTool()
    {
        return new ButtonToolbarItem("Ambilight", "Add Ambilight Zone",
            "expand__big_bigger_design_expand_larger_resize_size_square", new SolidColorBrush(Color.Parse("#d769ff")),
            AddAmbilightZoneCommand);
    }
    private ButtonToolbarItem AddVideoZoneTool()
    {
        return new ButtonToolbarItem("Video","Add Video Zone","video_zone",new SolidColorBrush(Colors.LimeGreen),AddVideoZoneCommand);
    }

    private ButtonToolbarItem ShowLibraryTool()
    {
        return new ButtonToolbarItem("Show Library", "Show Zone Library", "collection",
            new SolidColorBrush(Colors.Gray), ShowLibraryCommand);
    }

    private ButtonToolbarItem AddAnimationZoneTool()
    {
        return new ButtonToolbarItem("Animation", "Add Animation Zone", "LightingConfiguration_Animation",
            new SolidColorBrush(Color.Parse("#ffb033")), AddAnimationZoneCommand);
    }

    private FlyoutButtonToolbarItem AddColorZoneTool()
    {
        var addZonetools = new FlyoutButtonToolbarItem("Add", "Add new color zone",
            "paint_bucket__bucket_color_colors_design_paint_painting", new SolidColorBrush(Color.Parse("#33bbff")),
            AddColorZoneCommand);
        FlyoutItem addRectangle = new FlyoutItem("Rectangle", "CanvasTool_Rectangle");
        addRectangle.FlyoutItemSelected += AddRectangle;
        FlyoutItem addEllipse = new FlyoutItem("Ellipse", "CanvasTool_Ellipse");
        addEllipse.FlyoutItemSelected += AddEllipse;
        FlyoutItem addPolyline = new FlyoutItem("Poly line", "CanvasTool_PolyLine");
        addPolyline.FlyoutItemSelected += AddPolyline;
        addZonetools.FlyoutItems.Add(addRectangle);
        // addZonetools.FlyoutItems.Add(addEllipse);
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

    private void AddAsset(LightingZone zone)
    {
        var cloneFigure = zone.Clone(100f,
            100f);
        AddFigure?.Invoke(cloneFigure);
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
        _lightingZonesLibraryViewModel.ItemSelected -= OnLightingZoneAssetSelected;
        _decoder.FrameUpdate -= OnFrameUpdated;
        ZoneTools.Clear();
        CanvasTools.Clear();
    }

    private void OnFrameUpdated()
    {
        FrameTime = _decoder.FramesTime.ToList();
    }

    private List<double> _frameTime;

    public List<double> FrameTime
    {
        get => _frameTime;
        set
        {
            _frameTime = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand FitCanvasToViewCommand { get; set; }
    public RelayCommand AddVideoZoneCommand {get;set;}
    public RelayCommand ToggleSnapToGridCommand { get; set; }
    public RelayCommand AddAmbilightZoneCommand { get; set; }
    public RelayCommand AddAnimationZoneCommand { get; set; }
    public RelayCommand AddColorZoneCommand { get; set; }
    public RelayCommand ShowDiagCommand { get; set; }
    public AsyncRelayCommand ShowLibraryCommand { get; set; }
    public bool IsRendering => _decoder.IsRendering && _decoder.CurrentPlayingProfile.ID == _currentProfile.ID;
    public ICommand TogglePlayPauseCommand { get; set; }
    private int _brightness;

    public int Brightness
    {
        get => _brightness;
        set
        {
            _brightness = value;
            _currentProfile.Brightness = value;
            OnPropertyChanged();
        }
    }

    private bool _showDiag;

    public bool ShowDiag
    {
        get => _showDiag;
        set
        {
            _showDiag = value;
            OnPropertyChanged();
        }
    }
}
