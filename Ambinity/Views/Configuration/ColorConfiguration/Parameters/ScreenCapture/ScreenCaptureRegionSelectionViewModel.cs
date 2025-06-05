using System;
using System.Collections.Generic;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.LayoutEditor.Canvas;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core.Policies.CanvasPolicy;
using Draw2D.Core.Policies.FigurePolicy;
using Draw2D.Core.Shapes.Basic;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class ScreenCaptureRegionSelectionViewModel : ViewModelBase
{
    private readonly CanvasViewModelFactory _canvasViewModelFactory;
    private readonly ScreenCaptureConfiguration _config;

    public event Action CloseMe;
    private Draw2D.Core.Shapes.Basic.Rectangle _rect;
    private IClassicDesktopStyleApplicationLifetime desktop;
    private int _currentScreenIndex;
    private readonly IWindowService _windowService;

    private Window _currentWindow;

//todo poptrait mode
    public ScreenCaptureRegionSelectionViewModel(CanvasViewModelFactory canvasViewModelFactory,
        ScreenCaptureConfiguration config, List<ScreenRegionSelectionParameterViewModel.ScreenDataDisplay> screens,
        IWindowService windowService)
    {
        _canvasViewModelFactory = canvasViewModelFactory;
        _config = config;
        _windowService = windowService;
        CloseCommand = new RelayCommand(Close);
        SaveCurrentLayoutCommand = new RelayCommand(SaveCurrentLayout);
        LeftCommand = new RelayCommand(LeftRegion);
        RightCommand = new RelayCommand(RightRegion);
        TopCommand = new RelayCommand(TopRegion);
        BottomCommand = new RelayCommand(BottomRegion);
        FullScreenCommand = new RelayCommand(FullScreen);
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;
        this.desktop = desktop;
        AvailableScreen = new List<ScreenRegionSelectionParameterViewModel.ScreenDataDisplay>();
        foreach (var screen in screens)
        {
            AvailableScreen.Add(screen);
        }

        _selectedScreen = AvailableScreen[0];
        Init(_selectedScreen.Index);
    }

    private void LeftRegion()
    {
        CanvasViewModel.Canvas.Clear();
        AddRegion(new CaptureArea(0, 0, 0.5, 1.0));
    }

    private void RightRegion()
    {
        CanvasViewModel.Canvas.Clear();
        AddRegion(new CaptureArea(0.5, 0, 0.5, 1.0));
    }

    private void TopRegion()
    {
        CanvasViewModel.Canvas.Clear();
        AddRegion(new CaptureArea(0, 0, 1.0, 0.5));
    }

    private void BottomRegion()
    {
        CanvasViewModel.Canvas.Clear();
        AddRegion(new CaptureArea(0, 0.8, 1.0, 0.2));
    }

    private void FullScreen()
    {
        CanvasViewModel.Canvas.Clear();
        AddRegion(new CaptureArea(0.0, 0.0, 1.0, 1.0));
    }

    public List<ScreenRegionSelectionParameterViewModel.ScreenDataDisplay> AvailableScreen { get; set; }

    private void Init(int index)
    {
        CanvasViewModel = _canvasViewModelFactory.Get<CaptureRegionSelectionCanvasViewModel>();
        if (index >= desktop.MainWindow.Screens.ScreenCount)
            return;
        //init canvas
        _currentScreenIndex = index;
        var selectedScreen = desktop.MainWindow.Screens.All[_currentScreenIndex];
        var size = selectedScreen.Bounds.Size;
        CanvasViewModel.Init(size.ToSize(1d));
        CanvasViewModel.MinimumZoom = 1;
        CanvasViewModel.MaximumZoom = 1;
        var area = _config.ScreenCaptureArea;
        AddRegion(area);
        //show
        _currentWindow = _windowService.ShowWindow(this, index);
    }

    private void AddRegion(CaptureArea area)
    {
        var selectedScreen = desktop.MainWindow.Screens.All[_currentScreenIndex];
        var size = selectedScreen.Bounds.Size;
        _rect = new Rectangle((float)(size.Width * area.RatioX), (float)(size.Height * area.RatioY),
            (float)(size.Width * area.RatioWidth), (float)(size.Height * area.RatioHeight))
        {
            MinWidth = 100,
            MinHeight = 100
        };
        _rect.Padding(10, 10, 10, 10);
        _rect.InstallEditPolicy(new RegionDragDropEditPolicy(new Draw2D.Core.Geo.Rectangle(10, 10,
            CanvasViewModel.Canvas.Width - 20, CanvasViewModel.Canvas.Height - 20)));
        CanvasViewModel.AddFigure(_rect, false);
        CanvasViewModel.Canvas.ShouldDrawBackgroundImage = false;
        _rect.Select();
    }

    private void Close()
    {
        _currentWindow?.Close();
        CloseMe?.Invoke();
    }

    private void SaveCurrentLayout()
    {
        var selectedScreen = desktop.MainWindow.Screens.All[_currentScreenIndex];
        var size = selectedScreen.Bounds.Size;
        var captureArea = new CaptureArea(_rect.X / size.Width, _rect.Y / size.Height, _rect.Width / size.Width,
            _rect.Height / size.Height);
        _config.UpdateCaptureArea(captureArea, _currentScreenIndex);
        _currentWindow?.Close();
        CloseMe?.Invoke();
    }

    private void ChangeScreen(int index)
    {
        _currentWindow?.Close();
        Init(index);
    }

    private ScreenRegionSelectionParameterViewModel.ScreenDataDisplay _selectedScreen;

    public ScreenRegionSelectionParameterViewModel.ScreenDataDisplay SelectedScreen
    {
        get => _selectedScreen;
        set
        {
            _selectedScreen = value;
            ChangeScreen(_selectedScreen.Index);
            OnPropertyChanged();
        }
    }

    public CaptureRegionSelectionCanvasViewModel CanvasViewModel { get; set; }

    public ICommand SaveCurrentLayoutCommand { get; set; }
    public ICommand CloseCommand { get; set; }
    public ICommand LeftCommand { get; set; }
    public ICommand RightCommand { get; set; }
    public ICommand TopCommand { get; set; }
    public ICommand BottomCommand { get; set; }
    public ICommand FullScreenCommand { get; set; }
}
