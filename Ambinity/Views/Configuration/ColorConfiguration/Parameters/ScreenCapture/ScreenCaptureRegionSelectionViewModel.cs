using System;
using System.Windows.Input;
using Ambinity.Views.Draw2DCanvas;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core.Policies.CanvasPolicy;
using Draw2D.Core.Policies.FigurePolicy;
using Draw2D.Core.Shapes.Basic;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class ScreenCaptureRegionSelectionViewModel
{
    public event Action CloseMe;
    public ScreenCaptureRegionSelectionViewModel(Draw2DCanvasViewModel canvasViewModel)
    {
        CanvasViewModel = canvasViewModel;
        CloseCommand = new RelayCommand(Close);
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var size = desktop.MainWindow.Screens.Primary.Bounds.Size;
            CanvasViewModel.Init(size.ToSize(1d),false);
            CanvasViewModel.MinimumZoom = 1;
            CanvasViewModel.MaximumZoom = 1;
            var rect = new Rectangle(100f, 100f, 500f, 500f);
            rect.MinWidth = 100;
            rect.MinHeight = 100;
            rect.InstallEditPolicy(new RegionDragDropEditPolicy(new Draw2D.Core.Geo.Rectangle(10, 10,
                CanvasViewModel.Canvas.Width-20, CanvasViewModel.Canvas.Height-20)));
            CanvasViewModel.AddFigure(rect, false);
            CanvasViewModel.Canvas.ShouldDrawBackgroundImage = false;
            CanvasViewModel.Canvas.InstallEditPolicy(new RegionSelectionPolicy());
            rect.Select();
        }
    }

    private void Close()
    { 
        CloseMe?.Invoke();
    }

    public Draw2DCanvasViewModel CanvasViewModel { get; }
    public ICommand CloseCommand { get; set; }
}