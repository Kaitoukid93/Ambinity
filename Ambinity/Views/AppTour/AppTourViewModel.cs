using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Views.SideMenu;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.AppTour;

public class AppTourViewModel : ViewModelBase
{
    public event Action<ViewModelBase> NextStepActivated;
    private readonly IMainWindowService _mainWindowService;
    public AppTourViewModel(IMainWindowService mainWindowService)
    {
        ApptourElements = new ObservableCollection<AppTourElementViewModel>();
        _mainWindowService = mainWindowService;
        _mainWindowService.MainWindowOpened += OnMainWindowOpened;
        _mainWindowService.MainWindowClosed += OnMainWindowClosed;
    }

    private void OnMainWindowClosed(object? sender, EventArgs e)
    {
        IsRunning = false;
        NextStepActivated = null;
        Dispose();
    }

    private void OnMainWindowOpened(object? sender, EventArgs e)
    {
        //todo apptour condition
        _lifeTime = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
        _mainWindow = _lifeTime.MainWindow;
        Start();
    }

    private void Skip()
    {
        IsRunning = false;
        NextStepActivated = null;
        Dispose();
    }

    public void Start()
    {
        NextStepActivated?.Invoke(Ioc.Default.GetRequiredService<SideMenuViewModel>());
    }

    private bool _isRunning;
    private IClassicDesktopStyleApplicationLifetime _lifeTime;
    private Window _mainWindow;

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            _isRunning = value;
            OnPropertyChanged();
        }
    }

    //show apptour on main window
    public void Show(AppTourElementViewModel appTourElement, bool overwrite = true)
    {
        if (_mainWindow == null)
            return;
        SkipCommand = new RelayCommand(Skip);
        appTourElement.SkipCommand = SkipCommand;
        IsRunning = true;
        if (overwrite)
        {
            ApptourElements =
            [
                appTourElement
            ];
        }
        else
        {
            ApptourElements.Add(appTourElement);
        }

        GetCurrentElementsGeometry();
    }

    public void NextStep(ViewModelBase vm)
    {
        NextStepActivated?.Invoke(vm);
    }

    private ObservableCollection<AppTourElementViewModel> _appTourElements;

    public ObservableCollection<AppTourElementViewModel> ApptourElements
    {
        get => _appTourElements;
        set
        {
            _appTourElements = value;
            OnPropertyChanged();
        }
    }


    /// <summary>
    /// Combine current elements geometry to a single geometry
    /// </summary>
    private void GetCurrentElementsGeometry()
    {
        GeometryGroup newGroup = new GeometryGroup();
        foreach (var element in ApptourElements)
        {
            var geometry = new RectangleGeometry(element.Rect)
            {
                RadiusX = 7,
                RadiusY = 7
            };
            newGroup.Children.Add(geometry);
        }

        var windowRect = new RectangleGeometry(new Rect(0, 0, _mainWindow.Width, _mainWindow.Height));
        var geo = new CombinedGeometry(GeometryCombineMode.Exclude, windowRect, newGroup);
        CurrentGeometry = geo;
    }


    private Geometry _currentGeometry;

    public Geometry CurrentGeometry
    {
        get => _currentGeometry;
        set
        {
            _currentGeometry = value;
            OnPropertyChanged();
        }
    }

    public ICommand SkipCommand { get; set; }

    public override void Dispose()
    {
        ApptourElements.Clear();
    }
}