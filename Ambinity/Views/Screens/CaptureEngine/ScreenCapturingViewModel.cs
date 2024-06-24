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

    private DesktopCapturingEngine _capturingEngine;
    private ObservableCollection<ScreenPreviewViewModel> _availableScreen;

    public ObservableCollection<ScreenPreviewViewModel> AvailableScreen
    {
        get { return _availableScreen; }
        set
        {
            _availableScreen = value;
            OnPropertyChanged();
        }
    }

    public void Init()
    {
        AvailableScreen = new ObservableCollection<ScreenPreviewViewModel>();
        for (int i = 0; i < _capturingEngine.AvailableDesktop.Count; i++)
        {
            var view = new ScreenPreview();
            var vm = new ScreenPreviewViewModel(view.DisplayPreviewImage, _capturingEngine.AvailableDesktop[i]);
            view.DataContext = vm;
            AvailableScreen.Add(vm);
        }

        _capturingEngine.ScreenUpdated += OnScreenUpdate;
    }

    private void OnScreenUpdate(ICaptureZone zone, int index)
    {
        AvailableScreen[index].Update();
    }
    
}