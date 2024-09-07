using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.Views.Draw2DCanvas;
using AmbinityCore.CapturingService;
using AmbinityCore.DataBase;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core.Policies.FigurePolicy;
using Draw2D.Core.Shapes.Basic;
using ScreenCapture.NET;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class ScreenRegionSelectionParameterViewModel : ParameterViewModelBase
{
    private readonly IWindowService _windowService;
    private ScreenCaptureRegionSelectionViewModel _regionSelectionViewModel;
    private readonly GeneralSettingsManager _settingsManager;
    private readonly ScreenCaptureConfiguration _config;
    public ScreenRegionSelectionParameterViewModel(ScreenCaptureConfiguration config, IWindowService windowService,
        GeneralSettingsManager settingsManager,ScreenCapturingService capturingService)
    {
        _config = config;
        _windowService = windowService;
        _settingsManager = settingsManager;
        OpenRegionSelectionCommand = new RelayCommand(OpenScreenRegionSelection);
        _capturingService = capturingService;
        AvailableScreen = new List<ScreenDataDisplay>();
        foreach (var screen in _capturingService.AvailableScreens)
        {
            var dataDisplay = new ScreenDataDisplay("Display " + (screen.Index + 1) + "-" + screen.GraphicsCard.Name,
                screen.Index);
            AvailableScreen.Add(dataDisplay);
        }

        _selectedScreen = AvailableScreen.Where(s => s.Index == config.DisplayIndex).FirstOrDefault();
    }

    private void OpenScreenRegionSelection()
    {
        //get sregion property from config
        _canvasViewModel = new Draw2DCanvasViewModel(_settingsManager);
        _regionSelectionViewModel = new ScreenCaptureRegionSelectionViewModel(_canvasViewModel);
        _regionSelectionViewModel.CloseMe += CloseRegionSelectionWindow;
        _screenSelectionWindow = _windowService.ShowWindow(_regionSelectionViewModel,SelectedScreen.Index);
    }

    private void CloseRegionSelectionWindow()
    {
        _screenSelectionWindow?.Close();
    }

    private Draw2DCanvasViewModel _canvasViewModel;
    private Window _screenSelectionWindow;
    private ScreenCapturingService _capturingService;
    public List<ScreenDataDisplay> AvailableScreen { get; set; }
    
    public ICommand OpenRegionSelectionCommand { get; set; }

    public class ScreenDataDisplay
    {
        public ScreenDataDisplay(string displayName,int index)
        {
            Name = displayName;
            Index = index;
        }
        public string Name { get; set; }
        public int Index { get; set; }
    }

    private ScreenDataDisplay _selectedScreen;

    public ScreenDataDisplay SelectedScreen
    {
        get => _selectedScreen;
        set
        {
            _selectedScreen = value;
            _config.DisplayIndex = value.Index;
            OnPropertyChanged();
        }
    }
}