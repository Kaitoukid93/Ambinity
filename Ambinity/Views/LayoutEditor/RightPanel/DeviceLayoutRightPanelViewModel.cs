using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Views.CollectableItem.AmbinityDeviceLayout;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using Ambinity.Views.LayoutEditor.Canvas;
using Ambinity.Views.LayoutEditor.LEDLayoutCreator;
using Ambinity.Views.Screens.DeviceLayout;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Profile;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core;

namespace Ambinity.Views.LayoutEditor;

/// <summary>
/// UI logic for device layout editor view
/// </summary>
public class DeviceLayoutRightPanelViewModel : ViewModelBase
{
    public event Action OpenFlyoutEvent;
    public event Action CloseFlyoutEvent;
    private Window _layoutCreatorInitialWindow;
    private Window _layoutCreatorWindow;

    public DeviceLayoutRightPanelViewModel(
        CanvasViewModelFactory canvasViewModelFactory,
        LightingProfileDecoder decoder,
        LEDLayoutCreatorViewModel layoutCreatorViewModel,
        LibraryViewModelFactory libraryViewModelFactory, IWindowService windowService
    )
    {
        _windowService = windowService;
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
        _layoutCreatorViewModel?.Init(width, height, ledCount, image);

        var lifeTime = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;

        _layoutCreatorWindow = await _windowService.ShowDialogWindow(_layoutCreatorViewModel, _windowService.GetCurrentWindow());
        _layoutCreatorWindow.Closed += (sender, args) =>
        {
            _layoutCreatorViewModel?.Dispose();
        };

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
            return;
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
        FlyoutViewModel.Dispose();
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
