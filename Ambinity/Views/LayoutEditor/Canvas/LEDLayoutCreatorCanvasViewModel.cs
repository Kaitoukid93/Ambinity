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
    public class LEDLayoutCreatorCanvasViewModel : CanvasViewModelBase
    {


        public ToolsViewModel ToolsViewModel { get; }
        private ImageFigure _deviceImage;
        private string _imagePath;
        public Draw2DCanvasInfoBarViewModel InfoBarViewModel { get; }
        public LEDLayoutCreatorCanvasViewModel(GeneralSettingsManager settingsManager,
         IDialogService dialogService, ToolsViewModel toolsViewModel)
            : base(settingsManager, dialogService)
        {

            ToolsViewModel = toolsViewModel;
            ToolsViewModel.InstallPolylineTool += InstallTool;
            toolsViewModel.AddFigure += OnLEDAdded;

        }

        private void OnLEDAdded(Figure figure)
        {
            figure.IsResizable = true;
            figure.IsDragable = true;
            figure.IsSelectable = true;
            Canvas.AddFigure(figure);
        }

        private void InstallTool(PolylineTool tool)
        {
            InstallPolylineTool();
        }

        private void OnFigureAddedFromTool(Figure figure)
        {
            AddFigure(figure, true);
            figure.Select();
        }

        /// <summary>
        /// Each device can only have one single image
        /// New image added will be stretch to device size
        /// </summary>
        public void SetDeviceImage(string imagePath, int width, int height)
        {
            if (Canvas == null)
                return;
            Canvas?.Clear();
            _imagePath = imagePath;
            float x = (Canvas.Width - width) / 2;
            float y = (Canvas.Height - height) / 2;
            _deviceImage = new ImageFigure(x, y, width, height);
            _deviceImage.IsDragable = false;
            _deviceImage.IsSelectable = false;
            _deviceImage.IsResizable = false;
            Canvas?.AddFigure(_deviceImage);
            _deviceImage?.SetImage(_imagePath);

        }
        /// <summary>
        /// Actually changing canvas size, all items will be removed
        /// </summary>
        /// <param name="size"></param>
        public void SetDeviceSize(int width, int height)
        {
            // Canvas.Clear();
            // _deviceImage = new ImageFigure(0, 0, width, height);
            // if (_imagePath == null)
            //     return;
            // _deviceImage?.SetImage(_imagePath);
            // _deviceImage.IsDragable = false;
            // _deviceImage.IsSelectable = false;
            // _deviceImage.IsResizable = false;
            // Canvas.AddFigure(_deviceImage);

        }

        public bool ShoudDrawBackground { get; set; }

        private void OnFigureRemoved(Figure figure)
        {

        }

        private void OnFigureAdded(Figure figure)
        {

        }

        private void ToggleSnapToGrid()
        {
            ToggleGridSnapCommand.Execute(null);
        }

        private void FitCanvasToView()
        {
            FitCommand.Execute(null);
        }

        public void Init(int width = 500, int height = 500)
        {
            //Register Tools
            ToolsViewModel.FitCanvasToViewEvent += FitCanvasToView;
            ToolsViewModel.ToggleSnapToGridEvent += ToggleSnapToGrid;
            ToolsViewModel.InstallPolylineTool += InstallTool;
            ToolsViewModel.AddFigure += OnFigureAddedFromTool;
            ToolsViewModel.ImageVisibilityChanged += ToggleImageVisibility;

            //Create Canvas
            var canvasSize = new Size(width, height);
            base.Init(canvasSize);
            FigureAdded += OnFigureAdded;
            FigureRemoved += OnFigureRemoved;
            Canvas.ShouldDrawBackgroundImage = false;

            //resolve list figures
            int zOrder = 0;
            //Register InforBar
            ToolsViewModel.InitForLEDLayoutCreator();

            FitCommand?.Execute(null);
            UnlockCanvas();
        }

        private void ToggleImageVisibility()
        {
            _deviceImage.IsVisible = !_deviceImage.IsVisible;
        }

        public override void UnlockCanvas()
        {
            IsLocked = false;
        }
        public override void LockCanvas()
        {
            if (Figures == null)
                return;

            // Lock all figures
            foreach (var figure in Figures)
            {
                if (figure is LightingZoneFigure)
                {
                    figure.IsResizable = false;
                    figure.IsDragable = false;
                    figure.Unselect();
                }
            }

            IsLocked = true;
        }
        public override void OnPolylineFinishDrawing()
        {

        }

        public override void Paste()
        {
            var bound = Getbound(ClipboardFigures);
            foreach (var figure in ClipboardFigures)
            {
                //todo implementing paste abstract
                var clipboardChilItem = (figure as ContainerFigure).ChildItem;
                var offSetX = clipboardChilItem.X - bound.X;
                var offSetY = clipboardChilItem.Y - bound.Y;
                var cloneFigure = clipboardChilItem.Clone((float)WorldMousePosX + (float)offSetX,
                    (float)WorldMousePosY + (float)offSetY);
                AddFigure(cloneFigure, true);
                cloneFigure.Select();
            }
        }

        public override void Dispose()
        {
            ToolsViewModel.FitCanvasToViewEvent -= FitCanvasToView;
            ToolsViewModel.ToggleSnapToGridEvent -= ToggleSnapToGrid;
            FigureAdded -= OnFigureAdded;
            FigureRemoved -= OnFigureRemoved;
            ToolsViewModel.InstallPolylineTool -= InstallTool;
            ToolsViewModel.AddFigure -= OnFigureAddedFromTool;
            ToolsViewModel?.Dispose();
        }

    }
}
