using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using AmbinityCore.CaptureEngines;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ScreenCapture.NET;

namespace Ambinity.Views.Screens.CaptureEngine;

public class ScreenCapturingViewModel
{
    public ScreenCapturingViewModel(DesktopCapturingEngine capturingEngine)
    {
        _capturingEngine = capturingEngine;
    }

    private DesktopCapturingEngine _capturingEngine;
    public ObservableCollection<WriteableBitmap> AvailableBitmaps { get; set; }

    public void Init()
    {
        AvailableBitmaps = new ObservableCollection<WriteableBitmap>();
    }

    public void DesktopsPreviewUpdate(Bitmap image, int index)
    {
       
    }
}