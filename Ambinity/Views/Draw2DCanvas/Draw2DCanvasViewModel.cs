using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using adrilight_shared.Models.Device.SlaveDevice;
using Ambinity.Stores;
using Ambinity.ViewModels;
using Ambinity.Views.Screens.Dashboard;
using AmbinityCore.DataBase;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.LED;
using AmbinityCore.Models.Flyout;
using AmbinityCore.Models.GeneralSetting;
using Avalonia.Controls;
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
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using SixLabors.Primitives;
using Canvas = Draw2D.Core.Canvas;
using Grid = Draw2D.Core.Grid;
using Size = Avalonia.Size;

namespace Ambinity.Views.Draw2DCanvas;

public class Draw2DCanvasViewModel : ViewModelBase
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
            RaisePropertyChanged(nameof(Figures));
        }
    }

    private IGeneralSettings _generalSettings;

    public IGeneralSettings GeneralSettings
    {
        get { return _generalSettings; }
        set
        {
            _generalSettings = value;
            RaisePropertyChanged(nameof(GeneralSettings));
        }
    }

    public Draw2DCanvasViewModel(GeneralSettingsManager settingManager)
    {
        GeneralSettings = settingManager.Settings;
    }

    /// <summary>
    /// initialize canvas with new list of figures and canvas size
    /// </summary>
    /// <param name="figures"></param>
    /// <param name="canvasSize"></param>
    public void Init(List<Figure> figures, Size canvasSize)
    {
        Canvas = new Canvas(0)
        {
            Width = (float)canvasSize.Width,
            Height = (float)canvasSize.Height,
            Grid = new Grid()
            {
                UnitX = 20,
                UnitY = 20
            }
        };

        GridUnitX = 20;
        GridUnitY = 20;
        Canvas.CoordinateSystem = new TopDownCartesianCoordinateSystem(0, 0);
        Canvas.StrokeColor = GeneralSettings.PrimaryColor;
        Canvas.SelectionChanged += (sender, args) =>
        {
            SelectionCount = ((ICanvas)sender).Selection.All.Count();
            DeleteCommand.NotifyCanExecuteChanged();
        };
        var regionPolicy =
            new RegionDragDropEditPolicy(new Draw2D.Core.Geo.Rectangle(0, 0, Canvas.Width, Canvas.Height));
        CreateCommands();
        Canvas.InstallEditPolicy(new BoundingBoxSelectionPolicy());
        // get all device that is in global lighting mode?
       // Canvas?.InstallTool(new PolylineTool(), (tool) => PolylineCommand.NotifyCanExecuteChanged());
        Canvas?.InstallEditPolicy(_snapGridPolicy);
       // Canvas?.InstallEditPolicy(_snapElementPolicy);
        //get snap setting from general settings
        _snapGridPolicy.Enabled = _generalSettings.EnableSnapToGrid;
        _snapElementPolicy.Enabled = false;
        foreach (Figure figure in figures)
        {
            if (figure is DeviceContainerFigure deviceContainer)
            {
                deviceContainer.StrokeColor = Colors.Transparent;
                deviceContainer.OverrideStrokeStyle = false;
                deviceContainer.FillColor = Colors.Transparent;
                deviceContainer.IsResizable = false;
            }
            else
            {
                figure.AddHandlesCornerDirections(Canvas, HandleSizes.Small, HandleShapeType.Round);
                figure.InstallEditPolicy(SelectionFeedbackPolicy);
                figure.InstallEditPolicy(regionPolicy);
            }

            Canvas.AddFigure(figure);
        }

        UpdateFigure();
    }


    public int SelectionCount
    {
        get { return _selectionCount; }
        set
        {
            if (value == _selectionCount) return;
            _selectionCount = value;
            RaisePropertyChanged(nameof(SelectionCount));
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
            RaisePropertyChanged(nameof(GridUnitX));
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
            RaisePropertyChanged(nameof(GridUnitY));
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
            RaisePropertyChanged(nameof(EnablePanModeCommand));
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
        _generalSettings.EnableSnapToGrid = _snapGridPolicy.Enabled;
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
            RaisePropertyChanged(nameof(WorldMousePosX));
        }
    }

    public double WorldMousePosY
    {
        get { return _worldMousePosY; }
        set
        {
            if (value.Equals(_worldMousePosY)) return;
            _worldMousePosY = Math.Round(value, 2);
            RaisePropertyChanged(nameof(WorldMousePosY));
        }
    }

    public int RenderedItemsCount
    {
        get { return _renderedItemsCount; }
        set
        {
            if (value == _renderedItemsCount) return;
            _renderedItemsCount = value;
            RaisePropertyChanged(nameof(RenderedItemsCount));
        }
    }

    private ICanvas _canvas;

    public ICanvas Canvas
    {
        get { return _canvas; }
        set
        {
            _canvas = value;
            RaisePropertyChanged(nameof(Canvas));
        }
    }
}