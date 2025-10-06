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
using AmbinityCore.Models.Device.LED;
using Ambinity.Views.LayoutEditor.LEDLayoutCreator.Tools;
using Draw2D.Core.Constants;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using AmbinityCore.Utils;
using Avalonia.Controls.Shapes;
using Serilog;
using SharpGen.Runtime.Win32;
using Ambinity.Views.LayoutEditor.LEDLayoutCreator;

namespace Ambinity.Views.LayoutEditor.Canvas
{
    public class LEDLayoutCreatorCanvasViewModel : CanvasViewModelBase
    {
        private IWindowService _windowService;

        public ToolsViewModel ToolsViewModel { get; }
        private ImageFigure _deviceImage;
        private string _imagePath;
        public Draw2DCanvasInfoBarViewModel InfoBarViewModel { get; }
        public event Action<List<Figure>, ImageFigure,string,string> OnLayoutSaveRequest;
        public LEDLayoutCreatorCanvasViewModel(GeneralSettingsManager settingsManager,
         IDialogService dialogService, ToolsViewModel toolsViewModel,IWindowService windowService)
            : base(settingsManager, dialogService)
        {

            _windowService = windowService;
            ToolsViewModel = toolsViewModel;
            IncreaseCommand = new RelayCommand(Increase);
            DecreaseCommand = new RelayCommand(Decrease);

        }

        private void Increase()
        {
            if (Canvas.ActiveTool != null && Canvas.ActiveTool is LEDTool tool)
            {
                tool.IncreaseToolSize();
            }
        }
        private void Decrease()
        {
            if (Canvas.ActiveTool != null && Canvas.ActiveTool is LEDTool tool)
            {
                tool.DecreaseToolSize();
            }
        }

        private void OnLEDAdded(Figure figure)
        {
            figure.IsResizable = true;
            figure.IsDragable = true;
            figure.IsSelectable = true;
            Canvas.AddFigure(figure);
        }

        private void InstallPolyLineTool(PolylineTool tool)
        {
            InstallPolylineTool();
        }
        private void InstallLEDTool(LEDTool tool)
        {
            tool.CanvasViewModel = this;
            Canvas?.InstallTool(tool, (tool) => OnLEDToolFinish());
        }
        private void OnLEDToolFinish()
        {


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
        public void SetDeviceImage(string imagePath, float x, float y, float width, float height)
        {
            if (Canvas == null)
                return;
            Canvas?.Clear();
            _imagePath = imagePath;
            //alaways bring image to 500 px width for better visualization
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
            ToolsViewModel.InstallPolylineTool += InstallPolyLineTool;
            ToolsViewModel.InstallLEDTool += InstallLEDTool;
            ToolsViewModel.AddFigure += OnFigureAddedFromTool;
            ToolsViewModel.ImageVisibilityChanged += ToggleImageVisibility;
            ToolsViewModel.SaveLayoutEvent += SaveLayout;

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
        private async void  SaveLayout()
        {

            var vm = new SaveLayoutDialogViewModel(Canvas.Figures.Where(f => f is LEDContainerFigure).ToList());
            vm.Accept += () =>
            {
                //save current layout to library
                var boundingBox = GeometryUltilities.GetBoundingBox(Canvas.Figures.Where(f => f is LEDContainerFigure));
                if (!_deviceImage.BoundingBox.Contains(boundingBox))
                {
                    Log.Error("Can not export layout because the leds is out of image's bound");
                    return;
                }
                //get the material for new layout

                var leds = Canvas.Figures.Where(f => f is LEDContainerFigure).ToList();
                OnLayoutSaveRequest?.Invoke(leds, _deviceImage, vm.LayoutName, vm.LayoutDescription);
            };
            var saveDialog = await _windowService.ShowDialogWindow(vm, _windowService.GetCurrentWindow());

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
                cloneFigure.IsResizable = true;
                cloneFigure.MinHeight = 5;
                cloneFigure.MinWidth = 5;
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
            ToolsViewModel.InstallPolylineTool -= InstallPolyLineTool;
            ToolsViewModel.InstallLEDTool -= InstallLEDTool;
            ToolsViewModel.AddFigure -= OnFigureAddedFromTool;
            ToolsViewModel.ImageVisibilityChanged -= ToggleImageVisibility;
             ToolsViewModel.SaveLayoutEvent -= SaveLayout;
            ToolsViewModel?.Dispose();
        }

        public ICommand IncreaseCommand { get; set; }
        public ICommand DecreaseCommand { get; set; }

    }
}
