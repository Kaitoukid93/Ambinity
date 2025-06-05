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
    public class CaptureRegionSelectionCanvasViewModel : CanvasViewModelBase
    {

        private AmbinityDeviceRepository _deviceRepository;
        private GeneralSettingsManager _settingsManager;

        public CaptureRegionSelectionCanvasViewModel(GeneralSettingsManager settingsManager,
         IDialogService dialogService)
            : base(settingsManager, dialogService)
        {
            _settingsManager = settingsManager;
        }


        public void Init(Size size)
        {
            base.Init(size);
            // Add any ProfileEditor-specific initialization here
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
