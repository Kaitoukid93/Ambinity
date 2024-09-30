using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.Screens.DeviceLayout;
using Ambinity.Views.Screens.DeviceLayout.Library;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Profile;
using AmbinityCore.Repositories;
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

    public DeviceLayoutRightPanelViewModel(
        Draw2DCanvasViewModel canvasViewModel,
        LightingProfileDecoder decoder,
        LibraryViewModelFactory libraryViewModelFactory
    )
    {
        _libraryViewModelFactory = libraryViewModelFactory;
        _decoder = decoder;
        _canvasViewModel = canvasViewModel;
        OpenLibraryCommand = new AsyncRelayCommand(OpenLibrary);
    }

    private async Task OpenLibrary()
    {
        _libraryViewModel = _libraryViewModelFactory.GetLibraryViewModel("DeviceLayout");
        _libraryViewModel?.Init();
        _libraryViewModel.ItemSelected += OnLibraryItemSelected;
        // _libraryViewModel.ItemSelected += OnPaletteSelected;
        OpenFlyout(_libraryViewModel);
    }

    private void OnLibraryItemSelected(ICollectableItem item)
    {
        ApplyLayout(item as AmbinityDeviceLayout);
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
    private Draw2DCanvasViewModel _canvasViewModel;
    private readonly LightingProfileDecoder _decoder;
    private readonly LibraryViewModelFactory _libraryViewModelFactory;
    private LibraryViewModelBase _libraryViewModel;

    public async Task Init()
    {
        _canvasViewModel.SelectionChanged += OnCanvasSelectionChanged;
        _canvasViewModel.FigureRemoved += OnFigureRemoved;
        _decoder.RenderingStatusChanged += OnRenderingStatusChanged;
        PropertiesViewModel.Init();
    }

    public override void Dispose()
    {
        //todo
        _canvasViewModel.SelectionChanged -= OnCanvasSelectionChanged;
    }

    public bool IsLocked => _decoder.IsRendering;
    public ICommand OpenLibraryCommand { get; }
}