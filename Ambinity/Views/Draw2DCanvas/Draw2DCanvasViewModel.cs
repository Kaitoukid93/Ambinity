using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Windows;
using AmbinityCore.DataBase;
using AmbinityCore.Models.GeneralSetting;
using AmbinityCore.Models.Geography;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Profile;
using AmbinityCore.Utils;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core;
using Draw2D.Core.Constants;
using Draw2D.Core.Graphic;
using Draw2D.Core.Policies.CanvasPolicy;
using Draw2D.Core.Policies.FigurePolicy;
using Draw2D.Core.Policies.RouterPolicy;
using Draw2D.Core.Shapes.Basic;
using Draw2D.Core.Shapes.FigureExtensions;
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
    private List<Figure> _clipboardFigures;
    private int _gridUnitX;
    private int _gridUnitY;
    private ICommand _enablePanModeCommand;

    public SelectionFeedbackPolicy SelectionFeedbackPolicy { get; set; } = new SelectionFeedbackPolicy();

    public RelayCommand LineCommand { get; set; }

    public RelayCommand PolylineCommand { get; set; }

    public RelayCommand ToggleGridSnapCommand { get; set; }
    public RelayCommand ToggleElementSnapCommand { get; set; }

    public ICommand DeleteCommand { get; set; }
    public RelayCommand UpdateFigureData { get; set; }
    private ObservableCollection<Figure> _figures;

    public event Action SelectionChanged;
    public event Action<Figure> FigureAdded;
    public event Action<Figure> FigureRemoved;

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
    private IDialogService _dialogService;

    public Draw2DCanvasViewModel(GeneralSettingsManager _settingsManager)
    {
        _generalSettings = _settingsManager.Settings;
        CreateCommands();
    }

    public Draw2DCanvasViewModel(GeneralSettingsManager settingManager, IDialogService dialogService,
        LightingZoneRepository zoneRepository,
        FrameBuffer buffer, LightingProfileDecoder decoder)
    {
        _zoneRepository = zoneRepository;
        _generalSettings = settingManager.Settings;
        _dialogService = dialogService;
        _buffer = buffer;
        _decoder = decoder;
        _decoder.FrameUpdate += OnFrameUpdate;
        _decoder.RenderingStatusChanged += OnRenderingStatusChanged;
        CreateCommands();
    }

    private void OnRenderingStatusChanged()
    {
        //incase this canvas is use for another purpose 
        if (_decoder == null)
            return;
        if (_decoder.IsRendering)
        {
            LockCanvas();
            return;
        }

        UnlockCanvas();
    }

    private void OnFrameUpdate()
    {
        (Canvas as Canvas)?.NeedsRepaint(null);
    }


    private FrameBuffer _buffer;
    private LightingProfileDecoder _decoder;

    /// <summary>
    /// Init a new canvas with <param name="canvasSize"></param>
    /// </summary>
    /// <param name="canvasSize"></param>
    public void Init(Size canvasSize, bool enableSelection = true)
    {
        if (Canvas == null)
        {
            Canvas = new Canvas(0)
            {
                Width = (float)canvasSize.Width,
                Height = (float)canvasSize.Height,
                Grid = new Grid()
                {
                    UnitX = 5,
                    UnitY = 5
                }
            };
            Canvas.BackgroundImageBuffer = _buffer;
            GridUnitX = 5;
            GridUnitY = 5;
            Canvas.CoordinateSystem = new TopDownCartesianCoordinateSystem(0, 0);
            Canvas.StrokeColor = _generalSettings.PrimaryColor;
            Canvas.SelectionChanged += (sender, args) =>
            {
                SelectionChanged?.Invoke();
                SelectionCount = ((ICanvas)sender).Selection.All.Count();
            };
            if(enableSelection)
            Canvas.InstallEditPolicy(new BoundingBoxSelectionPolicy());
            // get all device that is in global lighting mode?

            Canvas?.InstallEditPolicy(_snapGridPolicy);
            // Canvas?.InstallEditPolicy(_snapElementPolicy);
            //get snap setting from general settings
            _snapGridPolicy.Enabled = _generalSettings.EnableSnapToGrid;
            _snapElementPolicy.Enabled = false;
        }

        CurrentZoom = 1d;
        Canvas.Clear();
        UpdateFigure();
        //update lock status
        OnRenderingStatusChanged();
        SelectionChanged?.Invoke();
    }

    /// <summary>
    /// disable all action on the canvas 
    /// </summary>
    public bool IsLocked { get; set; }

    public void LockCanvas()
    {
        if (Figures == null)
            return;
        foreach (var figure in Figures)
        {
            figure.IsResizable = false;
            figure.IsDragable = false;
            figure.Unselect();
        }

        IsLocked = true;
    }


    public void InstallPolylineTool()
    {
        Canvas?.InstallTool(new PolylineTool(), (tool) => OnPolylineFinishDrawing());
    }

    private void OnPolylineFinishDrawing()
    {
        var polyLine = Canvas.Figures.Where(f => f is PolyLine).First() as PolyLine;
        var points = new List<Point>();

        for (int i = 0; i < polyLine.Points.Count - 1; i++)
        {
            points.Add(new Point(polyLine.Points[i].X, polyLine.Points[i].Y));
        }

        var bound = polyLine.BoundingBox;

        var newZone =
            _zoneRepository.GetDefaultSolidColorZone("Polyline", (int)bound.X, (int)bound.Y, (int)bound.Width,
                (int)bound.Height,
                Colors.Red);
        if (newZone.Width < 2)
        {
            newZone.Width = 2;
            // newZone.X += 1;
        }

        if (newZone.Height < 2)
        {
            newZone.Height = 2;
            // newZone.Y += 1;
        }

        newZone.Points = points;
        newZone.Shape = ZoneShapeEnum.Polyline;
        newZone.IsResizeable = false;
        var container = newZone.GetContainer();
        container.SetChild(newZone);
        Canvas.RemoveSelected();
        UpdateFigure();
        AddFigure(container, true);
    }

    /// <summary>
    /// Enable actions that should be enabled
    /// </summary>
    public void UnlockCanvas()
    {
        if (Figures == null)
            return;
        foreach (var figure in Figures)
        {
            figure.IsResizable = true;
            figure.IsDragable = true;
            figure.Unselect();
        }

        IsLocked = false;
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

    public ICommand CopySelectedFigureCommand { get; set; }
    public ICommand PasteCommand { get; set; }
    public ICommand GroupSelectedFigureCommand { get; set; }

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

        DeleteCommand = new AsyncRelayCommand(RemoveFigure, () => Canvas.Selection.All.Any());

        EnablePanModeCommand = new RelayCommand(EnablePanModeCommandExecute);
        UpdateFigureData = new RelayCommand(UpdateFigure);
        CopySelectedFigureCommand = new RelayCommand(Copy, CanCopy);
        PasteCommand = new RelayCommand(Paste, CanPaste);
        GroupSelectedFigureCommand = new RelayCommand(Group, CanGroup);
    }

    private async Task RemoveFigure()
    {
        //show dialogvar vm = new InputDialogContentViewModel();
        var vm = new InputDialogContentViewModel();
        await _dialogService.ShowInputDialog(vm, "Rename", "Ok", "Cancel");
        var result = vm.UserInput;
        if (result == "OK")
        {
            foreach (var figure in Canvas.Selection.All)
            {
                FigureRemoved?.Invoke(figure);
            }

            Canvas.RemoveSelected();
            UpdateFigure();
        }
    }

    /// <summary>
    /// copy selected figure to clipboard
    /// </summary>
    /// <param name="figure"></param>
    private void Copy()
    {
        var selectedFigure = Canvas.Figures.Where(f => f.IsSelected && f.IsSelectable).ToList();
        _clipboardFigures = selectedFigure;
    }

    private bool CanCopy()
    {
        var selectedFigure = Canvas.Figures.Where(f => f.IsSelected && f.IsSelectable).ToList();
        if (selectedFigure.Count > 0)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// paste clipboard item at certain point, at the moment this method only apply to container figure
    /// </summary>
    /// <param name="point"></param>
    private void Paste()
    {
        var bound = Getbound(_clipboardFigures);
        foreach (var figure in _clipboardFigures)
        {
            //todo implementing paste abstract
            var clipboardChilItem = (figure as ContainerFigure).ChildItem;
            var offSetX = clipboardChilItem.X - bound.X;
            var offSetY = clipboardChilItem.Y - bound.Y;
            var cloneFigure = clipboardChilItem.Clone((float)WorldMousePosX + (float)offSetX,
                (float)WorldMousePosY + (float)offSetY);
            AddFigure(cloneFigure, true);
            cloneFigure.Select();
        }
    }

    private bool CanPaste()
    {
        if (_clipboardFigures == null || _clipboardFigures.Count == 0)
            return false;
        // var bound = Getbound(_clipboardFigures);
        // if (WorldMousePosX + bound.Width > Canvas.Width || WorldMousePosY + bound.Height > Canvas.Height)
        //     return false;
        return true;
    }

    private void Group()
    {
        var newGroup = new LightingZoneGroup();
        foreach (ContainerFigure figure in Canvas.Selection.All)
        {
            newGroup.AddChild(figure.ChildItem);
        }

        newGroup.UpdateSizeByChild(true);
        var newGroupContainer =
            new LightingZoneGroupContainerFigure(newGroup.X, newGroup.Y, newGroup.Width, newGroup.Height);
        AddFigure(newGroupContainer, false);
        foreach (ContainerFigure figure in Canvas.Selection.All)
        {
            figure.IsDragable = false;
            figure.IsResizable = false;
            newGroupContainer.InstallEditPolicy(new MasterSlaveDragDropPolicy(figure));
        }
    }

    private bool CanGroup()
    {
        if (Canvas.Selection.All.Count > 1)
            return true;
        return false;
    }

    private Rect Getbound(List<Figure> figuers)
    {
        var rects = new List<Rect>();
        foreach (var fig in _clipboardFigures)
        {
            var rect = new Rect(fig.X, fig.Y, fig.Width, fig.Height);
            rects.Add(rect);
        }

        var bound = RectCalculation.GetBound(rects.ToArray());
        return bound;
    }

    public void AddFigure(Figure figure, bool notify)
    {
        // var regionPolicy =
        //     new RegionDragDropEditPolicy(new Draw2D.Core.Geo.Rectangle(0, 0, Canvas.Width, Canvas.Height));
        if (figure.IsResizable)
            figure.AddHandlesAllDirections(Canvas, HandleSizes.Tiny, HandleShapeType.Square);
        if (figure.IsSelectable)
            figure.InstallEditPolicy(SelectionFeedbackPolicy);

        if (IsLocked)
        {
            figure.IsDragable = false;
            figure.IsResizable = false;
        }

        // figure.InstallEditPolicy(regionPolicy);
        Canvas.AddFigure(figure);
        UpdateFigure();
        if (notify)
            FigureAdded?.Invoke(figure);
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
    private readonly LightingZoneRepository _zoneRepository;

    public ICanvas Canvas
    {
        get { return _canvas; }
        set
        {
            _canvas = value;
            OnPropertyChanged();
        }
    }

    public bool IsDisposed { get; set; }
    public double CurrentZoom { get; set; }
    public object MinimumZoom { get; set; } = 0.0001;
    public object MaximumZoom { get; set; } = 10;

    public override void Dispose()
    {
        // Canvas = null;
        Canvas?.Clear();
        Figures = null;
    }
}