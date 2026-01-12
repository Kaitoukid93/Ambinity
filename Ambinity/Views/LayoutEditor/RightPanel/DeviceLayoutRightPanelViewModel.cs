using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Localization;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Views.CollectableItem.AmbinityDeviceLayout;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using Ambinity.Views.LayoutEditor.Canvas;
using Ambinity.Views.LayoutEditor.LEDLayoutCreator;
using Ambinity.Views.Screens.DeviceLayout;
using Ambinity.Windows;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.LED;
using AmbinityCore.Models.Profile;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core;
using Serilog;

namespace Ambinity.Views.LayoutEditor;

/// <summary>
/// UI logic for device layout editor view
/// </summary>
public class DeviceLayoutRightPanelViewModel : ViewModelBase
{
    public event Action OpenFlyoutEvent;
    public event Action CloseFlyoutEvent;
    public event Action CreatorWindowClosed;
    private Window _layoutCreatorInitialWindow;
    private Window _layoutCreatorWindow;

    public DeviceLayoutRightPanelViewModel(
        CanvasViewModelFactory canvasViewModelFactory,
        LightingProfileDecoder decoder,
        LEDLayoutCreatorViewModel layoutCreatorViewModel,
        LibraryViewModelFactory libraryViewModelFactory, IWindowService windowService,IDialogService dialogService
    )
    {
        _windowService = windowService;
         _dialogService = dialogService;
        _libraryViewModelFactory = libraryViewModelFactory;
        _decoder = decoder;
        _canvasViewModel = canvasViewModelFactory.Get<DeviceLayoutCanvasViewModel>();
        _layoutCreatorViewModel = layoutCreatorViewModel;
        OpenLibraryCommand = new AsyncRelayCommand(OpenLibrary);
        OpenLayoutCreatorCommand = new RelayCommand(OpenNewLayoutInitialSetup);
    }

    private void OpenNewLayoutInitialSetup()
    {
        if (_layoutCreatorInitialWindow != null && _layoutCreatorInitialWindow.IsVisible)
        {
            _layoutCreatorInitialWindow.Activate();
            return;
        }
        if (_layoutCreatorWindow != null && _layoutCreatorWindow.IsVisible)
        {
            _layoutCreatorWindow.Activate();
            return;
        }
        var vm = new LEDLayoutCreatorInitialViewModel();
        _layoutCreatorInitialWindow = _windowService.ShowWindow(vm);
        vm.LayoutPropertiesViewModel.HostWindow = _layoutCreatorInitialWindow;
        vm.Accept += OpenLayoutCreator;
    }
    private async void OpenLayoutCreator(LayoutPropertiesViewModel vm)
    {
        var width = (int)vm.Width;
        var height = (int)vm.Height;
        var image = vm.ImagePath;
        var ledCount = vm.LEDCount;
        var ledShape = vm.SelectedLEDShape;
        var name = vm.LayoutName;
        var leds = GetLEDs(ledShape, ledCount, vm.SelectedExistingLayout);
        bool result = _layoutCreatorViewModel.Init(width, height, leds, image);
        if (!result)
        {
            Log.Error("Error creating or parsing layout project");
            return;
        }
        var lifeTime = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;

        _layoutCreatorWindow = await _windowService.ShowDialogWindow(_layoutCreatorViewModel, _windowService.GetCurrentWindow());
        _layoutCreatorWindow.Closed += (sender, args) =>
        {
            _layoutCreatorViewModel?.Dispose();
            CreatorWindowClosed?.Invoke();
        };

    }
    private void ShowNoDeviceSelectedErrorDialog()
    {
        var vm = new ErrorDialogViewModel();
        vm.ErrorMessage = Loc.Get("DeviceLayout.NoDeviceSelected.Error.Message");
        Dispatcher.UIThread.Invoke(() => _dialogService.ShowErrorDialog(vm, Loc.Get("DeviceLayout.NoDeviceSelected.Error.Header"), "Return"));
    }

    private List<AmbinityLEDLayout> GetLEDs(string ledShape, int ledCount = 20, AmbinityDeviceLayout? existingLayout = null)
    {
        if (existingLayout != null)
        {
            return existingLayout.Leds.ToList();
        }
        var newLEDs = new List<AmbinityLEDLayout>();
        const double canvasWidth = 500;
        const double canvasHeight = 500;
        const double maxWidth = 20;
        const double maxHeight = 20;

        // Calculate how many columns and rows can fit
        int columns = (int)(canvasWidth / maxWidth);
        int rows = (int)(canvasHeight / maxHeight);

        // Calculate actual rectangle size to fit all LEDs if possible
        double rectWidth = Math.Min(canvasWidth / columns, maxWidth);
        double rectHeight = Math.Min(canvasHeight / rows, maxHeight);
        int count = 0;

        for (int row = 0; row < rows && count < ledCount; row++)
        {
            for (int col = 0; col < columns && count < ledCount; col++)
            {
                double x = col * rectWidth;
                double y = row * rectHeight;
                var geometryString = "M0,0 H20 V20 H0 Z";
                if (ledShape == "Circle")
                {
                    geometryString = "M10,0 A10,10 0 1,1 10,-20 A10,10 0 1,1 10,0 Z";
                }

                var ledLayout = new AmbinityLEDLayout((float)x, (float)y, (float)rectWidth, (float)rectHeight, geometryString, count);
                newLEDs.Add(ledLayout);
                count++;
            }
        }
        return newLEDs;
    }

    private async Task OpenLibrary()
    {
        _libraryViewModel = _libraryViewModelFactory.GetLibraryViewModel("DeviceLayout");
        _libraryViewModel?.Init();
        _libraryViewModel.ItemSelected += OnLibraryItemSelected;
        // _libraryViewModel.ItemSelected += OnPaletteSelected;
        OpenFlyout(_libraryViewModel);
    }

    private void OnLibraryItemSelected(AssetItemViewModelBase item)
    {
        if (item is AmbinityDeviceLayoutAssetViewModel)
        {
            var deviceLayoutAsset = item as AmbinityDeviceLayoutAssetViewModel;
            ApplyLayout(deviceLayoutAsset.Item as AmbinityDeviceLayout);
        }

    }

    private void ApplyLayout(AmbinityDeviceLayout layout)
    {
        //get all selected device and apply this layout
        var figs = _canvasViewModel.Canvas.Selection.All;
        if (figs == null || figs.Count == 0)
        {
            ShowNoDeviceSelectedErrorDialog();
            CloseFlyoutEvent?.Invoke();
            return;
        }

        var selectedDevices = new List<AmbinityDevice>();
        foreach (var fig in figs)
        {
            var deviceContainerFigure = fig as DeviceContainerFigure;

            if (deviceContainerFigure != null)
            {
                if (!deviceContainerFigure.IsSelectionActive)
                    continue;
                selectedDevices.Add(deviceContainerFigure.ChildItem as AmbinityDevice);
            }
        }

        foreach (var device in selectedDevices)
        {
            device.LoadLayout(layout);
        }
    }

    private void OnRenderingStatusChanged()
    {
        OnPropertyChanged(nameof(IsLocked));
    }

    public void OpenFlyout(FlyoutContentViewModelBase flyoutViewModel)
    {
        FlyoutViewModel = flyoutViewModel;
        OpenFlyoutEvent?.Invoke();
    }

    public void OnFlyoutClosing()
    {
        FlyoutViewModel?.Dispose();
        FlyoutViewModel = null;
    }

    public FlyoutContentViewModelBase FlyoutViewModel { get; set; }

    /// <summary>
    /// This is when user close by pressing button
    /// </summary>
    private void OnFigureRemoved(Figure obj)
    {
        PropertiesViewModel.DisableEdit();
    }

    private void OnCanvasSelectionChanged()
    {
        PropertiesViewModel.UpdateObjectProperties();
    }

    public DevicePropertiesViewModel PropertiesViewModel { get; set; }
    private DeviceLayoutCanvasViewModel _canvasViewModel;
    private LEDLayoutCreatorViewModel _layoutCreatorViewModel;
    private readonly LightingProfileDecoder _decoder;
    private readonly IWindowService _windowService;
    private readonly LibraryViewModelFactory _libraryViewModelFactory;
    private IDialogService _dialogService;
    private LibraryViewModelBase _libraryViewModel;

    public async Task Init()
    {
        _canvasViewModel.SelectionChanged += OnCanvasSelectionChanged;
        _canvasViewModel.FigureRemoved += OnFigureRemoved;
        _decoder.RenderingStatusChanged += OnRenderingStatusChanged;
        PropertiesViewModel.UpdateObjectProperties();
    }

    public override void Dispose()
    {
        //todo
        if (_libraryViewModel != null)
            _libraryViewModel.ItemSelected -= OnLibraryItemSelected;
        _canvasViewModel.SelectionChanged -= OnCanvasSelectionChanged;
    }

    public bool IsLocked => _decoder.IsRendering;
    public ICommand OpenLibraryCommand { get; }
    public ICommand OpenLayoutCreatorCommand { get; }
}
