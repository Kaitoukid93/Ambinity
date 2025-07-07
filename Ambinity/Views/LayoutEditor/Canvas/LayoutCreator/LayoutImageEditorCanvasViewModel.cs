using Avalonia;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Services;
using AmbinityCore.DataBase;
using Ambinity.Windows;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Profile;
using Draw2D.Core.Graphic;
using Canvas = Draw2D.Core.Canvas;
using Avalonia.Media;
using Draw2D.Core.Shapes.Basic;
using System.Collections.Generic;
using System.Linq;
using AmbinityCore.Models.Geography;
using Draw2D.Core.Policies.RouterPolicy;
using Draw2D.Core;
using System;
using AmbinityCore.Repositories;
using AmbinityCore.Models.Device;
using System.Reflection.Metadata;
using AmbinityCore;
using Avalonia.Controls;

namespace Ambinity.Views.LayoutEditor.Canvas
{
    public class LayoutImageEditorCanvasViewModel : CanvasViewModelBase
    {


        public ToolsViewModel ToolsViewModel { get; }
        private ImageFigure _deviceImage;
        private string _imagePath;
        public Draw2DCanvasInfoBarViewModel InfoBarViewModel { get; }
        public LayoutImageEditorCanvasViewModel(GeneralSettingsManager settingsManager,
         IDialogService dialogService, ToolsViewModel toolsViewModel)
            : base(settingsManager, dialogService)
        {

            ToolsViewModel = toolsViewModel;
            ToolsViewModel.FitCanvasToViewEvent += FitCanvasToView;
            toolsViewModel.ToggleSnapToGridEvent += ToggleSnapToGrid;
        }

        private void FitCanvasToView()
        {
            FitCommand?.Execute(null);
        }
        private void ToggleSnapToGrid()
        {
            ToggleGridSnapCommand.Execute(null);
        }
        /// <summary>
        /// Each device can only have one single image
        /// New image added will be stretch to device size
        /// </summary>
        public void SetDeviceImage(string imagePath)
        {
            if (_deviceImage == null)
                return;
            _imagePath = imagePath;
            _deviceImage?.SetImage(_imagePath);

        }
        public void Init(int width = 500, int height = 500, float scale = 1.0f)
        {

            //Create Canvas
            ToolsViewModel.InitForImageEditor();
            var canvasSize = new Size(width * scale, height * scale);
            base.Init(canvasSize);
            Canvas.ShouldDrawBackgroundImage = false;
            Canvas.ShouldDrawBorder = false;
            //resolve list figures
            int zOrder = 0;
            //Register InforBar
            _deviceImage = new ImageFigure(0, 0, width * scale, height * scale);
            _deviceImage.IsDragable = false;
            _deviceImage.IsSelectable = false;
            _deviceImage.IsResizable = false;
            Canvas.AddFigure(_deviceImage);
        }

        public override void Dispose()
        {
            ToolsViewModel.FitCanvasToViewEvent -= FitCanvasToView;
            ToolsViewModel.ToggleSnapToGridEvent -= ToggleSnapToGrid;
            ToolsViewModel?.Dispose();
        }

    }
}
