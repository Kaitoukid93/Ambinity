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
using AmbinityCore.Models.Device;
using AmbinityCore.Repositories;
using Ambinity.Views.Notification;

namespace Ambinity.Views.LayoutEditor.Canvas
{
    public class DeviceLayoutCanvasViewModel : CanvasViewModelBase
    {

        private FrameBuffer _buffer;

        public Draw2DCanvasInfoBarViewModel InfoBarViewModel { get; }
        public ToolsViewModel ToolsViewModel { get; }

        private AmbinityDeviceRepository _deviceRepository;
        private GeneralSettingsManager _settingsManager;
        private LightingProfileDecoder _decoder;
        private readonly LightingZoneRepository _zoneRepository;

        public DeviceLayoutCanvasViewModel(GeneralSettingsManager settingsManager,
         IDialogService dialogService,
         AmbinityDeviceRepository deviceRepository,
         ToolsViewModel toolsViewModel,
         Draw2DCanvasInfoBarViewModel infoBarViewModel,
         LightingProfileDecoder decoder)
            : base(settingsManager, dialogService)
        {
            InfoBarViewModel = infoBarViewModel;
            ToolsViewModel = toolsViewModel;
            _deviceRepository = deviceRepository;
            _settingsManager = settingsManager;
            _decoder = decoder;
            _decoder.RenderingStatusChanged += OnRenderingStatusChanged;
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
        public void Init()
        {
            ToolsViewModel.FitCanvasToViewEvent += FitCanvasToView;
            ToolsViewModel.ToggleSnapToGridEvent += ToggleSnapToGrid;
            var canvasSize = new Size(
                _settingsManager.Settings.CanvasWidth,
                _settingsManager.Settings.CanvasHeight);
            base.Init(canvasSize);
            OnRenderingStatusChanged();
            var devices = ResolveDevices();
            int zOrder = 0;
            foreach (var device in devices)
            {
                var containerFigure = device.GetContainer();
                containerFigure.SetChild(device);
                containerFigure.ZOrder = zOrder++;
                AddFigure(containerFigure, false);
            }

            //init layout canvas
            Canvas.ShouldDrawBackgroundImage = false;
            Canvas.ShouldDrawEntityColors = false; ;

            InfoBarViewModel.Init();
            ToolsViewModel.InitForDeviceLayout();

            // Add any ProfileEditor-specific initialization here
        }
        private void ToggleSnapToGrid()
        {
            ToggleGridSnapCommand.Execute(null);
        }

        private void FitCanvasToView()
        {
            FitCommand.Execute(null);
        }

        private List<AmbinityDevice>? ResolveDevices()
        {

            var devices = new List<AmbinityDevice>();
            foreach (var device in _deviceRepository.Devices)
            {

                device.IsSelectable = true;
                //device.IsDraggable = false;
                device.IsResizeable = false;
                device.IsRotatable = true;
                device.IsScalable = true;
                device.IsDeleteable = false;
                devices.Add(device);
            }
            return devices;
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
                figure.IsResizable = false;
                figure.IsDragable = false;
                figure.Unselect();
            }

            IsLocked = true;
        }

    }
}
