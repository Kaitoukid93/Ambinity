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

namespace Ambinity.Views.LayoutEditor.Canvas
{
    public class ProfileEditorCanvasViewModel : CanvasViewModelBase
    {

        private FrameBuffer _buffer;
        private LightingProfileDecoder _decoder;
        private readonly LightingZoneRepository _zoneRepository;
        private AmbinityDeviceRepository _deviceRepository;
        private LightingProfile _profile;

        public ToolsViewModel ToolsViewModel { get; }
        public Draw2DCanvasInfoBarViewModel InfoBarViewModel { get; }
        public ProfileEditorCanvasViewModel(GeneralSettingsManager settingsManager,
         IDialogService dialogService,
        LightingZoneRepository zoneRepository,
        FrameBuffer buffer,
        AmbinityDeviceRepository deviceRepository,
        Draw2DCanvasInfoBarViewModel infoBarViewModel,
         LightingProfileDecoder decoder, ToolsViewModel toolsViewModel)
            : base(settingsManager, dialogService)
        {
            _deviceRepository = deviceRepository;
            ToolsViewModel = toolsViewModel;
            InfoBarViewModel = infoBarViewModel;
            _zoneRepository = zoneRepository;
            _buffer = buffer;
            _decoder = decoder;
            _decoder.FrameUpdate += OnFrameUpdate;
            _decoder.RenderingStatusChanged += OnRenderingStatusChanged;
        }


        private void OnFrameUpdate()
        {
            (this.Canvas as Draw2D.Core.Canvas)?.NeedsRepaint(null);
        }

        private void OnRenderingStatusChanged()
        {
            //incase this canvas is use for another purpose
            if (_decoder == null)
                return;
            if (_decoder.IsRendering)
            {
                LockCanvas();
                return;
            }

            UnlockCanvas();
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

        public bool ShoudDrawBackground { get; set; }

        private void OnFigureRemoved(Figure figure)
        {
            var zoneFigure = figure as LightingZoneFigure;
            _profile.RemoveLightingZone(zoneFigure.ChildItem as LightingZone);
        }

        private void OnFigureAdded(Figure figure)
        {
            var zoneFigure = figure as LightingZoneFigure;
            _profile.AddLightingZone(zoneFigure.ChildItem as LightingZone);
        }

        private void ToggleSnapToGrid()
        {
            ToggleGridSnapCommand.Execute(null);
        }

        private void FitCanvasToView()
        {
            FitCommand.Execute(null);
        }

        public void Init(LightingProfile profile)
        {
            //Register Tools
            _profile = profile;
            ToolsViewModel.FitCanvasToViewEvent += FitCanvasToView;
            ToolsViewModel.ToggleSnapToGridEvent += ToggleSnapToGrid;
            ToolsViewModel.InstallPolylineTool += InstallTool;
            ToolsViewModel.AddFigure += OnFigureAddedFromTool;

            //Create Canvas
            var canvasSize = new Size(_buffer.FrameWidth, _buffer.FrameHeight);
            base.Init(canvasSize);
            Canvas.BackgroundImageBuffer = _buffer;
            OnRenderingStatusChanged();
            FigureAdded += OnFigureAdded;
            FigureRemoved += OnFigureRemoved;
            Canvas.ShouldDrawBackgroundImage = false;

            //resolve list figures
            int zOrder = 0;
            var devices = ResolveDevices();
            var zones = ResolveProfile() ?? new List<LightingZone>();
            var items = new List<IPositionAware>();
            items.AddRange(devices);
            items.AddRange(zones);
            foreach (var item in items)
            {
                var containerFigure = item.GetContainer();
                containerFigure.SetChild(item);
                containerFigure.ZOrder = zOrder++;
                AddFigure(containerFigure, false);
            }
            //Register InforBar
            InfoBarViewModel.Init();
            ToolsViewModel.InitForProfileEditor(_profile);
        }

        private List<LightingZone>? ResolveProfile()
        {
            if (_profile == null)
                return null;
            var zones = new List<LightingZone>();
            foreach (var zone in _profile.Zones)
            {
                zone.IsSelectable = true;
                zone.IsDraggable = true;
                zone.IsResizeable = true;
                zone.IsRotatable = false; // zone cant be rotate
                zone.IsScalable = false; // zone cant be scale, use resize instead
                zone.IsDeleteable = true;
                zones.Add(zone);
            }
            return zones;

        }
        private List<AmbinityDevice>? ResolveDevices()
        {

            var devices = new List<AmbinityDevice>();
            foreach (var device in _deviceRepository.Devices)
            {
                //simply lock the device in profile editor canvas, todo implement lock method
                device.IsSelectable = false;
                device.IsDraggable = false;
                device.IsResizeable = false;
                device.IsRotatable = false;
                device.IsScalable = false;
                device.IsDeleteable = false;
                devices.Add(device);
            }
            return devices;
        }

        public override void UnlockCanvas()
        {
            if (Figures == null)
                return;

            // Restore last state only for DeviceContainerFigure
            foreach (var figure in Figures)
            {
                if (figure is LightingZoneFigure)
                {
                    // Default if not found
                    figure.IsResizable = true;
                    figure.IsDragable = true;
                    figure.Unselect();
                }
            }

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
            var polyLine = Canvas.Figures.Where(f => f is PolyLine).First() as PolyLine;
            var points = new List<Point>();

            for (int i = 0; i < polyLine.Points.Count; i++)
            {
                points.Add(new Point(polyLine.Points[i].X, polyLine.Points[i].Y));
            }

            var bound = polyLine.BoundingBox;

            var newZone =
                _zoneRepository.GetDefaultSolidColorZone("Polyline", (int)bound.X, (int)bound.Y, (int)bound.Width,
                    (int)bound.Height,
                    Colors.Red, ZoneShapeEnum.Polyline);
            if (newZone.Width < 2)
            {
                newZone.Width = 2;
                // newZone.X += 1;
            }

            if (newZone.Height < 2)
            {
                newZone.Height = 2;
                // newZone.Y += 1;
            }

            newZone.Points = points;
            newZone.Shape = ZoneShapeEnum.Polyline;
            newZone.IsResizeable = true;
            var container = newZone.GetContainer();
            container.SetChild(newZone);
            Canvas.RemoveSelected();
            UpdateFigure();
            AddFigure(container, true);
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
