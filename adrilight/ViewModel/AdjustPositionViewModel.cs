using BO;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace adrilight.ViewModel
{
 public  class AdjustPostionViewModel: ViewModelBase
    {
        private IDeviceSettings _currentDevice;
        public IDeviceSettings CurrentDevice {
            get { return _currentDevice; }
            set
            {
                if (_currentDevice == value) return;
                _currentDevice = value;
                RaisePropertyChanged();
            }
        }
        private IDeviceSettings[] _allDevices;
        public IDeviceSettings[] AllDevices {
            get { return _allDevices; }
            set
            {
                if (_allDevices == value) return;
                _allDevices = value;
                RaisePropertyChanged();
            }
        }
      //  private List<Rectangle> _blockedRectangle;
        public List<Rectangle> BlockedRectangle {
            get
            {
                var blockedRectangle = new List<Rectangle>() ;//_blockedRectangle = new Rectangle[AllDevices.Length - 1];
                foreach (var device in AllDevices)
                {
                    if (!device.IsHUB && device != CurrentDevice)
                    {
                        var top = device.DeviceRectTop * 4;
                        var left = device.DeviceRectLeft * 4;
                        var width = device.DeviceRectWidth * 4;
                        var height = device.DeviceRectHeight * 4;
                        var rect = new Rectangle(left, top, width, height);
                        blockedRectangle.Add(rect);
                    }
                }
                return blockedRectangle;

            }

        }
        public int SourceWidth => ShaderBitmap?.PixelWidth ?? 200;
        public int SourceHeight => ShaderBitmap?.PixelHeight ?? 200;
        private int _deviceRectX;
        public int DeviceRectX {
            get => _deviceRectX;
            set
            {
                _deviceRectX = value;
                RaisePropertyChanged();
            }
        }
        private int _deviceRectY;
        public int DeviceRectY {
            get => _deviceRectY;
            set
            {
                _deviceRectY = value;
                RaisePropertyChanged();
            }
        }

        private int _deviceRectHeight;
        public int DeviceRectHeight {
            get => _deviceRectHeight;
            set
            {
                _deviceRectHeight = value;
                RaisePropertyChanged();
            }
        }
        private int _deviceRectWidth;
        public int DeviceRectWidth {
            get => _deviceRectWidth;
            set
            {
                _deviceRectWidth = value;
                RaisePropertyChanged();
            }
        }
        private string _deviceName;
        public string DeviceName {
            get => _deviceName;
            set
            {
                _deviceName = value;
                RaisePropertyChanged();
            }
        }
        public WriteableBitmap _shaderBitmap;
        public WriteableBitmap ShaderBitmap {
            get => _shaderBitmap;
            set
            {
                _shaderBitmap = value;
                RaisePropertyChanged(nameof(ShaderBitmap));
                RaisePropertyChanged(() => SourceWidth);
                RaisePropertyChanged(() => SourceHeight);
                RaisePropertyChanged(() => CanvasWidth);
                RaisePropertyChanged(() => CanvasHeight);
                RaisePropertyChanged(() => BlockedRectangle);
            }
        }
        // public int CanvasPadding => 300 / DesktopDuplicator.ScalingFactor;

        public int CanvasWidth => SourceWidth * 4;
        public int CanvasHeight => SourceHeight * 4;
        public ICommand DeleteCommand { get; set; }
        public ViewModelBase _parentVm;
        public AdjustPostionViewModel(IDeviceSettings device, IDeviceSettings[] allDevices, WriteableBitmap bitmap)
        {
            ShaderBitmap= bitmap;
            CurrentDevice = device;
            DeviceRectWidth = CurrentDevice.DeviceRectWidth*4;
            DeviceRectHeight = CurrentDevice.DeviceRectHeight*4;
            DeviceRectX = CurrentDevice.DeviceRectLeft*4;
            DeviceRectY = CurrentDevice.DeviceRectTop*4;
            DeviceName = CurrentDevice.DeviceName;
            AllDevices = allDevices;

            
        }
    }
}
