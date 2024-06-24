using System;
using Ambinity.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Media.Imaging;
using ScreenCapture.NET;

namespace Ambinity.Views.Screens.CaptureEngine;

public class ScreenPreviewViewModel : ViewModelBase
{
    public ScreenPreviewViewModel(Image previewImageControl,ICaptureZone zone)
    {
        _captureZone = zone;
        PreviewImageControl = previewImageControl;
        PreviewImage = new WriteableBitmap(
            new PixelSize(zone.Width,
                zone.Height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Opaque);
    }

    private Image? _previewImageControl;

    public Image? PreviewImageControl
    {
        get { return _previewImageControl; }
        set
        {
            _previewImageControl = value;
            OnPropertyChanged();
        }
    }
    

    public WriteableBitmap? PreviewImage { get; }

    private ICaptureZone _captureZone;
    

    public void Update()
    {
        if (_captureZone == null) return;
        using (_captureZone.Lock())
        {
            WritePixels(PreviewImage, _captureZone.GetRefImage<ColorBGRA>());
            PreviewImageControl?.InvalidateVisual();
        }
    }

    private static unsafe void WritePixels(WriteableBitmap preview, RefImage<ColorBGRA> image)
    {
        using ILockedFramebuffer framebuffer = preview.Lock();
        image.CopyTo(new Span<ColorBGRA>((void*)framebuffer.Address, framebuffer.Size.Width * framebuffer.Size.Height));
        
    }
}