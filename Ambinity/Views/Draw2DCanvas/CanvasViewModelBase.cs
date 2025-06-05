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
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Utils;
using Avalonia;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core;
using Draw2D.Core.Constants;
using Draw2D.Core.Policies.CanvasPolicy;
using Draw2D.Core.Policies.FigurePolicy;
using Draw2D.Core.Policies.RouterPolicy;
using Draw2D.Core.Shapes.FigureExtensions;
using FluentAvalonia.UI.Controls;
using Canvas = Draw2D.Core.Canvas;
using Grid = Draw2D.Core.Grid;
using Size = Avalonia.Size;

namespace Ambinity.Views.Draw2DCanvas;

public class CanvasViewModelBase : ViewModelBase
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
    public List<Figure> ClipboardFigures
    {
        get { return _clipboardFigures; }
        set
        {
            _clipboardFigures = value;
            OnPropertyChanged();
        }
    }
    private List<Figure> _clipboardFigures;
    private IGeneralSettings _generalSettings;
    private IDialogService _dialogService;


    public CanvasViewModelBase(GeneralSettingsManager settingManager, IDialogService dialogService)
    {

        _generalSettings = settingManager.Settings;
        _dialogService = dialogService;
        CreateCommands();
    }

    /// <summary>
    /// Init a new canvas with <param name="canvasSize"></param>
    /// </summary>
    /// <param name="canvasSize"></param>
    public void Init(Size canvasSize, bool enableSelection = true, bool enableRegionSelection = false)
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
            GridUnitX = 5;
            GridUnitY = 5;
            Canvas.CoordinateSystem = new TopDownCartesianCoordinateSystem(0, 0);
            Canvas.StrokeColor = _generalSettings.PrimaryColor;
            Canvas.SelectionChanged += (sender, args) =>
            {
                SelectionChanged?.Invoke();
                SelectionCount = ((ICanvas)sender).Selection.All.Count();
            };
            if (enableSelection)
                Canvas.InstallEditPolicy(new BoundingBoxSelectionPolicy());
            if (enableRegionSelection)
                Canvas.InstallEditPolicy(new RegionSelectionPolicy());
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
        SelectionChanged?.Invoke();
    }
    /// <summary>
    /// disable all action on the canvas
    /// </summary>
    public bool IsLocked { get; set; }

    public virtual void LockCanvas()
    {
        if (Figures == null)
            return;

        // Store last state
        foreach (var figure in Figures)
        {
            figure.IsResizable = false;
            figure.IsDragable = false;
            figure.Unselect();
        }

        IsLocked = true;
    }

    public virtual void UnlockCanvas()
    {
        if (Figures == null)
            return;
        foreach (var figure in Figures)
        {
            // Default if not found
            figure.IsResizable = true;
            figure.IsDragable = true;
            figure.Unselect();
        }

        IsLocked = false;
    }


    public void InstallPolylineTool()
    {
        Canvas?.InstallTool(new PolylineTool(), (tool) => OnPolylineFinishDrawing());
    }

    public virtual void OnPolylineFinishDrawing()
    {

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


    public ICommand FitCommand
    {
        get { return _fitCommand; }
        set
        {
            if (value == null)
                return;
            _fitCommand = value;
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
    }

    private async Task RemoveFigure()
    {
        var vm = new ConfirmationDialogContentViewModel();
        vm.DialogClosed += OnDeleteDialogClosed;
        vm.Content = "This action cannot be undone";
        await _dialogService.ShowConfirmationDialog(vm, "Delete selected zones?", "Remove", "Cancel");

    }

    private void OnDeleteDialogClosed(object? sender, EventArgs e)
    {
        var vm = sender as DeleteDialogContentViewModel;
        var result = (e as ContentDialogClosedEventArgs).Result;
        if (result == ContentDialogResult.Secondary || result == ContentDialogResult.None)
            return;
        if (result == ContentDialogResult.Primary)
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
    public virtual void Copy()
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
    /// paste clipboard item
    /// </summary>
    /// <param name="point"></param>
    public virtual void Paste()
    {

    }

    private bool CanPaste()
    {
        if (_clipboardFigures == null || _clipboardFigures.Count == 0)
            return false;
        return true;
    }

    public Rect Getbound(List<Figure> figuers)
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

    public virtual void AddFigure(Figure figure, bool notify)
    {
        if (figure.IsResizable)
            figure.AddHandlesAllDirections(Canvas, HandleSizes.Tiny, HandleShapeType.Square);
        if (figure.IsSelectable)
            figure.InstallEditPolicy(SelectionFeedbackPolicy);

        if (IsLocked)
        {
            figure.IsDragable = false;
            figure.IsResizable = false;
        }
        // Add to canvas
        Canvas.AddFigure(figure);
        UpdateFigure();
        if (notify)
            FigureAdded?.Invoke(figure);
    }

    public void UpdateFigure()
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
