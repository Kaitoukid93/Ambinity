using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Ambinity.Stores;
using Ambinity.ViewModels;
using Ambinity.Views.Screens.Dashboard;
using AmbinityCore.DataBase;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.GeneralSetting;
using Avalonia.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core;
using Draw2D.Core.Constants;
using Draw2D.Core.Policies.CanvasPolicy;
using Draw2D.Core.Policies.FigurePolicy;
using Draw2D.Core.Policies.RouterPolicy;
using Draw2D.Core.Shapes.Basic;
using Draw2D.Core.Shapes.FigureExtensions;

namespace Ambinity.Views.Screens.DeviceControl.DeviceCanvas;

public class DeviceCanvasViewModel : ViewModelBase
{
    private double _worldMousePosX;
    private double _worldMousePosY;
    private int _renderedItemsCount;
    private ICommand _fitCommand;
    private ICommand _fillCommand;
    private ICommand _oneHundredPercentCommand;
    private ICommand _zoomInCommand;
    private ICommand _zoomOutCommand;
    private ICommand _undoZoomCommand;
    private ICommand _redoZoomCommand;
    private int _selectionCount;
    private readonly SnapGridPolicy _snapGridPolicy = new SnapGridPolicy();
    private readonly SnapElementPolicy _snapElementPolicy = new SnapElementPolicy();

    private int _gridUnitX;
    private int _gridUnitY;
    private ICommand _enablePanModeCommand;

    public SelectionFeedbackPolicy SelectionFeedbackPolicy { get; set; } = new SelectionFeedbackPolicy();

    public RelayCommand LineCommand { get; set; }

    public RelayCommand PolylineCommand { get; set; }

    public RelayCommand ToggleGridSnapCommand { get; set; }
    public RelayCommand ToggleElementSnapCommand { get; set; }

    public RelayCommand DeleteCommand { get; set; }
    public RelayCommand UpdateFigureData { get; set; }
    private ObservableCollection<Figure> _figures;

    public ObservableCollection<Figure> Figures
    {
        get { return _figures; }
        set
        {
            _figures = value;
            OnPropertyChanged();
        }
    }

    private IGeneralSettings _generalSettings;

    public IGeneralSettings GeneralSettings
    {
        get { return _generalSettings; }
        set
        {
            _generalSettings = value;
            OnPropertyChanged();
        }
    }

    public DeviceCanvasViewModel(GeneralSettingsManager settingManager, RootNavigationStores rootNavigationStores)
    {
        _rootNavigationStore = rootNavigationStores;
        GeneralSettings = settingManager.Settings;
        //AddAxes();
        Canvas = new Canvas(0)
        {
            Width = 800,
            Height = 600,
            Grid = new Grid()
            {
                UnitX = 20,
                UnitY = 20
            }
        };

        GridUnitX = 20;
        GridUnitY = 20;

        Canvas.SelectionChanged += (sender, args) =>
        {
            SelectionCount = ((ICanvas)sender).Selection.All.Count();
            DeleteCommand.NotifyCanExecuteChanged();
        };

        CreateCommands();

        Canvas.InstallEditPolicy(new BoundingBoxSelectionPolicy());
        // get all device that is in global lighting mode?
        Canvas?.InstallTool(new PolylineTool(), (tool) => PolylineCommand.NotifyCanExecuteChanged());
        Canvas?.InstallEditPolicy(_snapGridPolicy);
        Canvas?.InstallEditPolicy(_snapElementPolicy);
        _snapGridPolicy.Enabled = true;
        _snapElementPolicy.Enabled = false;
        // get current playing 
    }

    public void Init()
    {
        //background size bind to zone size
        //grid can be modified???
        Canvas.CoordinateSystem = new TopDownCartesianCoordinateSystem(0, 0);
        Canvas.StrokeColor = GeneralSettings.PrimaryColor;
        var line1 = new Line(100, 100, 300, 300);
        line1.StrokeThickness = 1;
        line1.InstallEditPolicy(new SelectionFeedbackPolicy());
        line1.InstallLineHandle(0);
        line1.InstallLineHandle(1);
        line1.InstallEditPolicy(
            new RegionDragDropEditPolicy(new Draw2D.Core.Geo.Rectangle(0, 0, Canvas.Width, Canvas.Height)));

        var _polyline = new Line(0, 0, 100, 200);
        _polyline.InstallLineHandle(0);
        _polyline.InstallLineHandle(1);
        var device = new Device();
        device.Rectangle = new DeviceFigure(100, 100, 200, 200);
        device.Rectangle.AddHandlesCornerDirections(Canvas, HandleSizes.Small, HandleShapeType.Round);
        device.Rectangle.InstallEditPolicy(SelectionFeedbackPolicy);
        device.Rectangle.StrokeColor = GeneralSettings.PrimaryColor;
        device.Rectangle.FillColor = Colors.Transparent;
        device.Rectangle.AddHandlesCornerDirections(Canvas, HandleSizes.Small, HandleShapeType.Round);
        Canvas.AddFigure(device.Rectangle);
    }

    public int SelectionCount
    {
        get { return _selectionCount; }
        set
        {
            if (value == _selectionCount) return;
            _selectionCount = value;
            OnPropertyChanged();
        }
    }

    public int GridUnitX
    {
        get { return _gridUnitX; }
        set
        {
            if (value.Equals(_gridUnitX)) return;
            _gridUnitX = value;
            Canvas.Grid = new Grid() { UnitX = _gridUnitX, UnitY = GridUnitY };
            OnPropertyChanged();
        }
    }

    public int GridUnitY
    {
        get { return _gridUnitY; }
        set
        {
            if (value.Equals(_gridUnitY)) return;
            _gridUnitY = value;
            Canvas.Grid = new Grid() { UnitX = GridUnitX, UnitY = _gridUnitY };
            OnPropertyChanged();
        }
    }

    private void AddAxes()
    {
        var xLine = new Line(-Canvas.Width / 2, 0, Canvas.Width / 2, 0)
        {
            IsDragable = false,
            IsSelectable = false,
            CanBeSnapTarget = false
        };
        Canvas.AddFigure(xLine);
        xLine.SendToBack();

        var yLine = new Line(0, -Canvas.Height / 2, 0, Canvas.Height / 2)
        {
            IsDragable = false,
            IsSelectable = false,
            CanBeSnapTarget = false
        };
        Canvas.AddFigure(yLine);
        yLine.SendToBack();
    }

    public ICommand FitCommand
    {
        get { return _fitCommand; }
        set
        {
            if (value == null)
                return;
            _fitCommand = value;
            //_eventAggregator.PublishOnUIThread(this);
        }
    }

    public ICommand FillCommand
    {
        get { return _fillCommand; }
        set
        {
            if (value == null)
                return;
            _fillCommand = value;
            //_eventAggregator.PublishOnUIThread(this);
        }
    }

    public ICommand OneHundredPercentCommand
    {
        get { return _oneHundredPercentCommand; }
        set
        {
            if (value == null)
                return;
            _oneHundredPercentCommand = value;
            //_eventAggregator.PublishOnUIThread(this);
        }
    }

    public ICommand ZoomInCommand
    {
        get { return _zoomInCommand; }
        set
        {
            if (value == null)
                return;
            _zoomInCommand = value;
            // _eventAggregator.PublishOnUIThread(this);
        }
    }

    public ICommand ZoomOutCommand
    {
        get { return _zoomOutCommand; }
        set
        {
            if (value == null)
                return;
            _zoomOutCommand = value;
            //_eventAggregator.PublishOnUIThread(this);
        }
    }

    public ICommand UndoZoomCommand
    {
        get { return _undoZoomCommand; }
        set
        {
            if (value == null)
                return;
            _undoZoomCommand = value;
            // _eventAggregator.PublishOnUIThread(this);
        }
    }

    public ICommand RedoZoomCommand
    {
        get { return _redoZoomCommand; }
        set
        {
            if (value == null)
                return;
            _redoZoomCommand = value;
            //  _eventAggregator.PublishOnUIThread(this);
        }
    }

    public ICommand EnablePanModeCommand
    {
        get { return _enablePanModeCommand; }
        set
        {
            if (Equals(value, _enablePanModeCommand)) return;
            _enablePanModeCommand = value;
            OnPropertyChanged();
        }
    }


    private void CreateCommands()
    {
        LineCommand =
            new RelayCommand(
                () => { Canvas.InstallTool(new LineTool(), (tool) => LineCommand.NotifyCanExecuteChanged()); },
                () => Canvas.ActiveTool == null);


        PolylineCommand = new RelayCommand(
            () => { Canvas.InstallTool(new PolylineTool(), (tool) => PolylineCommand.NotifyCanExecuteChanged()); },
            () => Canvas.ActiveTool == null);

        ToggleGridSnapCommand = new RelayCommand(EnableGridSnapCommandExecute);
        ToggleElementSnapCommand = new RelayCommand(EnableElementSnapCommandExecute);

        DeleteCommand = new RelayCommand(() => { Canvas.RemoveSelected(); }, () => Canvas.Selection.All.Any());

        EnablePanModeCommand = new RelayCommand(EnablePanModeCommandExecute);
        UpdateFigureData = new RelayCommand(UpdateFigure);
        BackToDashBoardCommand = new RelayCommand(BackToDashBoard);
    }

    private void UpdateFigure()
    {
        if (Figures == null)
            Figures = new ObservableCollection<Figure>();
        Figures.Clear();
        foreach (var figure in Canvas.Figures)
        {
            Figures.Add(figure);
        }
    }

    private void EnablePanModeCommandExecute()
    {
        //if (Canvas.GetInstalledSnapPolicies().OfType<SnapGridPolicy>().Any())
        //{
        //    Canvas.UninstallEditPolicy(_snapGridPolicy);
        //}
        //else
        //{
        //    Canvas.InstallEditPolicy(_snapGridPolicy);
        //}
    }


    private void EnableGridSnapCommandExecute()
    {
        _snapGridPolicy.Enabled = !_snapGridPolicy.Enabled;
    }

    private void EnableElementSnapCommandExecute()
    {
        _snapElementPolicy.Enabled = !_snapElementPolicy.Enabled;
    }

    public double WorldMousePosX
    {
        get { return _worldMousePosX; }
        set
        {
            if (value.Equals(_worldMousePosX)) return;
            _worldMousePosX = Math.Round(value, 2);
            OnPropertyChanged();
        }
    }

    public double WorldMousePosY
    {
        get { return _worldMousePosY; }
        set
        {
            if (value.Equals(_worldMousePosY)) return;
            _worldMousePosY = Math.Round(value, 2);
            OnPropertyChanged();
        }
    }

    public int RenderedItemsCount
    {
        get { return _renderedItemsCount; }
        set
        {
            if (value == _renderedItemsCount) return;
            _renderedItemsCount = value;
            OnPropertyChanged();
        }
    }

    private ICanvas _canvas;

    public ICanvas Canvas
    {
        get { return _canvas; }
        set
        {
            _canvas = value;
            OnPropertyChanged();
        }
    }

    public ICommand BackToDashBoardCommand { get; set; }
    private RootNavigationStores _rootNavigationStore;

    private void BackToDashBoard()
    {
        var vm = Ioc.Default.GetRequiredService<DashboardViewModel>();
        vm.Init();
        _rootNavigationStore.CurrentViewModel = vm;
    }
}