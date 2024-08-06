using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ambinity.ViewModels;
using AmbinityCore.CaptureEngines;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ScreenCapture.NET;
using SkiaSharp;

namespace Ambinity.Views.Screens.CaptureEngine;

public class ScreenCapturingViewModel : ViewModelBase
{
    public ScreenCapturingViewModel(DesktopCapturingEngine capturingEngine)
    {
        _capturingEngine = capturingEngine;
        Init();
    }

    private readonly DesktopCapturingEngine _capturingEngine;
    private ObservableCollection<ScreenPreviewViewModel> _availableScreen;

    public ObservableCollection<ScreenPreviewViewModel> AvailableScreen
    {
        get => _availableScreen;
        set
        {
            _availableScreen = value;
            OnPropertyChanged();
        }
    }

    private void Init()
    {
        _capturingEngine.ServiceRequired++;
        _capturingEngine.RefreshCapturingState();
        AvailableScreen = [];
        foreach (var t in _capturingEngine.AvailableDesktop)
        {
            var view = new ScreenPreview();
            var vm = new ScreenPreviewViewModel(t);
            view.DataContext = vm;
            AvailableScreen.Add(vm);
        }
        _capturingEngine.ScreenUpdated += OnScreenUpdate;
    }

    private void OnScreenUpdate(ICaptureZone zone, int index)
    {
        AvailableScreen[index].Update();
    }

    public override void Dispose()
    {
        _capturingEngine.ServiceRequired--;
        _capturingEngine.RefreshCapturingState();
    }
}