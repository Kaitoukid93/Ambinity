using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace Ambinity.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Minimum allowed size
            // MinWidth = 1400;
            // MinHeight = 850;
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            var screens = Screens;
            var screen = screens.ScreenFromWindow(this);
            if (screen is null)
                return;

            var area = screen.WorkingArea;
            double scale = DesktopScaling; // e.g. 1.5 for 150% scaling
            var workingAreaDip = screen.WorkingArea.ToRect(scale);
            double targetWidthRatio = 1600 / 1920d;
            double targetHeightRatio = 900 / 1080d;
            double targetMinWidthRatio = 1400 / 1920d;
            double targetMinHeightRatio = 600 / 1080d;
            // Clamp the window size so it does not exceed screen working area
            double finalWidth = targetWidthRatio * workingAreaDip.Width;
            double finalHeight = targetHeightRatio * workingAreaDip.Height;

            // Apply the final size
            Width = finalWidth;
            Height = finalHeight;
            MinWidth = targetMinWidthRatio * workingAreaDip.Width;
            MinHeight = targetMinHeightRatio * workingAreaDip.Height;

            // Center window
            Position = new PixelPoint(
                (int)(area.X + (workingAreaDip.Width - finalWidth) / 2),
                (int)(area.Y + (workingAreaDip.Height - finalHeight) / 2)
            );
        }
    }
}
