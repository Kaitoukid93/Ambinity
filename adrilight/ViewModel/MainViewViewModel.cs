using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using adrilight.Resources;
using adrilight.Spots;
using adrilight.Util;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using OpenRGB.NET.Models;
using Un4seen.BassWasapi;
using BO;
using Application = System.Windows.Forms.Application;
using GalaSoft.MvvmLight;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Windows.Media;
using System.Drawing;
using System.Drawing.Imaging;
using adrilight.Shaders;

namespace adrilight.ViewModel
{
    public class MainViewViewModel : BaseViewModel
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");

        private string JsonDeviceFileNameAndPath => Path.Combine(JsonPath, "adrilight-deviceInfos.json");
        private string JsonGroupFileNameAndPath => Path.Combine(JsonPath, "adrilight-groupInfos.json");

        #region constant string
        public const string ImagePathFormat = "pack://application:,,,/adrilight;component/View/Images/{0}";
        public const string dashboard = "Dashboard";
        public const string deviceSetting = "Device Settings";
        public const string appSetting = "App Settings";
        public const string faq = "FAQ";
        public const string general = "General";
        public const string lighting = "Lighting";
        public const string canvasLighting = "Canvas Lighting";
        public const string groupLighting = "Group Lighting";
        #endregion
        #region property
        private ObservableCollection<VerticalMenuItem> _menuItems;
        public ObservableCollection<VerticalMenuItem> MenuItems {
            get { return _menuItems; }
            set
            {
                if (_menuItems == value) return;
                _menuItems = value;
                RaisePropertyChanged();
            }
        }
        private VerticalMenuItem _selectedVerticalMenuItem;
        public VerticalMenuItem SelectedVerticalMenuItem {
            get { return _selectedVerticalMenuItem; }
            set
            {
                if (_selectedVerticalMenuItem == value) return;
                _selectedVerticalMenuItem = value;
                RaisePropertyChanged();

            }
        }
        private bool _isDashboardType = true;
        public bool IsDashboardType {
            get { return _isDashboardType; }
            set
            {
                if (_isDashboardType == value) return;
                _isDashboardType = value;
                RaisePropertyChanged();
                LoadMenuByType(value);
            }
        }
     

       
        private string _buildVersion = "";
        public string BuildVersion {
            get { return _buildVersion; }
            set
            {
                if (_buildVersion == value) return;
                _buildVersion = value;
                RaisePropertyChanged();

            }
        }
        private DateTime? _lastUpdate;
        public DateTime? LastUpdate {
            get { return _lastUpdate; }
            set
            {
                if (_lastUpdate == value) return;
                _lastUpdate = value;
                RaisePropertyChanged();

            }
        }
        private string _author = "";
        public string Author {
            get { return _author; }
            set
            {
                if (_author == value) return;
                _author = value;
                RaisePropertyChanged();

            }
        }
        private string _git = "";
        public string Git {
            get { return _git; }
            set
            {
                if (_git == value) return;
                _git = value;
                RaisePropertyChanged();

            }
        }
        private string _faq = "";
        public string FAQ {
            get { return _faq; }
            set
            {
                if (_faq == value) return;
                _faq = value;
                RaisePropertyChanged();

            }
        }
        private string _appName = "";
        public string AppName {
            get { return _appName; }
            set
            {
                if (_appName == value) return;
                _appName = value;
                RaisePropertyChanged();

            }
        }
        private Visibility _visibleTabControl;
        public Visibility VisibleTabControl {
            get
            {
                if (CurrentDevice.IsHUB || CurrentDevice.ParrentLocation != 151293)
                {
                    return Visibility.Visible;
                }
                else return Visibility.Collapsed;


            }
            set
            {
                _visibleTabControl = value;

            }
        }

        private Visibility _dFUVisibility;
        public Visibility DFUVisibility {
            get
            {
                if (CurrentDevice.ParrentLocation != 151293)
                {
                    return Visibility.Collapsed;
                }
                else return Visibility.Visible;


            }
            set
            {
                _dFUVisibility = value;

            }
        }
        private ViewModelBase _currentView;

        private ViewModelBase _detailView;
        private ViewModelBase _deviceSettingView;
        private ViewModelBase _appSettingView;
        private ViewModelBase _faqSettingView;
        public ViewModelBase CurrentView {
            get { return _currentView; }
            set
            {
                _currentView = value;
                RaisePropertyChanged("CurrentView");
            }
        }
        private IDeviceSettings _currentDevice;
        public IDeviceSettings CurrentDevice {
            get { return _currentDevice; }
            set
            {
                if (_currentDevice == value) return;
                if (_currentDevice != null) _currentDevice.PropertyChanged -= _currentDevice_PropertyChanged;
                _currentDevice = value;
                if (_currentDevice != null) _currentDevice.PropertyChanged += _currentDevice_PropertyChanged;
                RaisePropertyChanged("CurrentDevice");

            }
        }
        private IGroupSettings _currentGroup;
        public IGroupSettings CurrentGroup {
            get { return _currentGroup; }
            set
            {
                if (_currentGroup == value) return;
                if (_currentGroup != null) _currentGroup.PropertyChanged -= _currentDevice_PropertyChanged;
                _currentGroup = value;
                if (_currentGroup != null) _currentGroup.PropertyChanged += _currentDevice_PropertyChanged;
                RaisePropertyChanged("CurrentGroup");

            }
        }
        private void _currentDevice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!IsDashboardType)
            {
                WriteDeviceInfoJson();
            }
            if (CurrentDevice.DeviceID == 151293)
            {

            }
            else
            {
                foreach (var spotset in SpotSets)
                    if (spotset.ID == CurrentDevice.DeviceID)
                    {
                        PreviewSpots = spotset.Spots;
                    }

            }
        }

        public ICommand SelectMenuItem { get; set; }
        public ICommand BackCommand { get; set; }
        public ICommand DeviceRectDropCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand DeleteGroupCommand { get; set; }
        public ICommand AdjustPositionCommand { get; set; }
        public ICommand SnapshotCommand { get; set; }
        public ICommand DeleteDeviceCommand { get; set; }
        public ICommand SetNextSpotVIDCommand { get; set; }
        #endregion
        private ObservableCollection<IDeviceSettings> _cards;
        public ObservableCollection<IDeviceSettings> Cards {
            get { return _cards; }
            set
            {
                if (_cards == value) return;
                _cards = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<IGroupSettings> _groups;
        public ObservableCollection<IGroupSettings> Groups {
            get { return _groups; }
            set
            {
                if (_groups == value) return;
                _groups = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<IDeviceSettings> _displayCards;
        public ObservableCollection<IDeviceSettings> DisplayCards {
            get { return _displayCards; }
            set
            {
                if (_displayCards == value) return;
                _displayCards = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<IDeviceSpotSet> _spotSets;
        public ObservableCollection<IDeviceSpotSet> SpotSets {
            get { return _spotSets; }
            set
            {
                if (_spotSets == value) return;
                _spotSets = value;
                RaisePropertyChanged();
            }
        }
        // [Inject, Named("0")]
        // public IDeviceSettings Card1 { get; set; }

        public ICommand SelectCardCommand { get; set; }
        public ICommand SelectGroupCommand { get; set; }
        public ICommand LightingModeSelection { get; set; }
        public ICommand SelectShaderCommand { get; set; }
        public ICommand ShowAddNewCommand { get; set; }
        public ICommand RefreshDeviceCommand { get; set; }

       
        private string JsonDeviceNameAndPath => Path.Combine(JsonPath, "adrilight-deviceInfos.json");
        public IList<String> _AvailableComPorts;
        public IList<String> AvailableComPorts {
            get
            {


                _AvailableComPorts = SerialPort.GetPortNames().Concat(new[] { "Không có" }).ToList();
                _AvailableComPorts.Remove("COM1");

                return _AvailableComPorts;
            }
        }
        private ObservableCollection<string> _caseEffects;
        public ObservableCollection<string> CaseEffects {
            get { return _caseEffects; }
            set
            {
                if (_caseEffects == value) return;
                _caseEffects = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<CollectionItem> _collectionItm;
        public ObservableCollection<CollectionItem> CollectionItems {
            get { return _collectionItm; }
            set
            {
                if (_collectionItm == value) return;
                _collectionItm = value;
                RaisePropertyChanged();
            }
        }
     
        private ObservableCollection<string> _availableEffects;
        public ObservableCollection<string> AvailableEffects {
            get { return _availableEffects; }
            set
            {
                if (_availableEffects == value) return;
                _availableEffects = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<ShaderCard> _availableShader;
        public ObservableCollection<ShaderCard> AvailableShader {
            get { return _availableShader; }
            set
            {
                if (_availableShader == value) return;
                _availableShader = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<string> _availableLayout;
        public ObservableCollection<string> AvailableLayout {
            get { return _availableLayout; }
            set
            {
                if (_availableLayout == value) return;
                _availableLayout = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<string> _availableRotation;
        public ObservableCollection<string> AvailableRotation {
            get { return _availableRotation; }
            set
            {
                if (_availableRotation == value) return;
                _availableRotation = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<string> _availableMatrixOrientation;
        public ObservableCollection<string> AvailableMatrixOrientation {
            get { return _availableMatrixOrientation; }
            set
            {
                if (_availableMatrixOrientation == value) return;
                _availableMatrixOrientation = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<string> _availableMatrixStartPoint;
        public ObservableCollection<string> AvailableMatrixStartPoint {
            get { return _availableMatrixStartPoint; }
            set
            {
                if (_availableMatrixStartPoint == value) return;
                _availableMatrixStartPoint = value;
                RaisePropertyChanged();
            }
        }
        public IDeviceSpot[] _previewSpots;
        public IDeviceSpot[] PreviewSpots {
            get => _previewSpots;
            set
            {
                _previewSpots = value;
                RaisePropertyChanged();
            }
        }
       

        public  WriteableBitmap _shaderBitmap;
        public  WriteableBitmap ShaderBitmap {
            get => _shaderBitmap;
            set
            {
                 _shaderBitmap = value;
                RaisePropertyChanged(nameof(ShaderBitmap));
             
                RaisePropertyChanged(() => CanvasWidth);
                RaisePropertyChanged(() => CanvasHeight);
            }
        }
        public int CanvasWidth => ShaderBitmap.PixelWidth;
        public int CanvasHeight => ShaderBitmap.PixelHeight;

        private int _parrentLocation;
        public int ParrentLocation {
            get => _parrentLocation;
            set
            {
                _parrentLocation = value;
                RaisePropertyChanged();
            }
        }
        
        //public int DeviceSpotX {
        //    get 
        //    {
        //          CurrentDevice.SpotsX
        //    }
        //    set
        //    {
        //        if(CurrentDevice.SpotsY)
        //        CurrentDevice.SpotsX = value;
        //    }
            
        //}
        
        //public int DeviceSpotY {
        //    get => _deviceSpotY;
        //    set => _deviceSpotY = value;

        //}

        public IList<string> _AvailableAudioDevice = new List<string>();
        public IList<String> AvailableAudioDevice {
            get
            {
                _AvailableAudioDevice.Clear();
                int devicecount = BassWasapi.BASS_WASAPI_GetDeviceCount();
                string[] devicelist = new string[devicecount];
                for (int i = 0; i < devicecount; i++)
                {

                    var devices = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);

                    if (devices.IsEnabled && devices.IsLoopback)
                    {
                        var device = string.Format("{0} - {1}", i, devices.name);

                        _AvailableAudioDevice.Add(device);
                    }

                }

                return _AvailableAudioDevice;
            }
        }
        public int _audioDeviceID = -1;
        public int AudioDeviceID {
            get
            {
                if (CurrentDevice.SelectedAudioDevice > AvailableAudioDevice.Count)
                {
                    System.Windows.MessageBox.Show("Last Selected Audio Device is not Available");
                    return -1;
                }
                else
                {
                    var currentDevice = AvailableAudioDevice.ElementAt(CurrentDevice.SelectedAudioDevice);

                    var array = currentDevice.Split(' ');
                    _audioDeviceID = Convert.ToInt32(array[0]);
                    return _audioDeviceID;
                }

            }



        }
        private bool _deviceLightingModeCollection;
        public bool DeviceLightingModeCollection {
            get {  return _deviceLightingModeCollection;}
            set { _deviceLightingModeCollection = value;}
        }






        public ObservableCollection<string> AvailablePalette { get; private set; }
        public IContext Context { get; }
        public IList<String> _AvailableDisplays;
        public IList<String> AvailableDisplays {
            get
            {
                var listDisplay = new List<String>();
                foreach (var screen in System.Windows.Forms.Screen.AllScreens)
                {

                    listDisplay.Add(screen.DeviceName);
                }
                _AvailableDisplays = listDisplay;
                return _AvailableDisplays;
            }
        }
      
        public ObservableCollection<string> AvailableFrequency { get; private set; }

        public ObservableCollection<string> AvailableMusicPalette { get; private set; }
        public ObservableCollection<string> AvailableMusicMode { get; private set; }
        public ICommand SelectGif { get; set; }
        public BitmapImage gifimage;
        public Stream gifStreamSource;
        private static int _gifFrameIndex = 0;
        private BitmapSource _contentBitmap;
        public BitmapSource ContentBitmap {
            get { return _contentBitmap; }
            set
            {
                if (value != _contentBitmap)
                {
                    _contentBitmap = value;
                    RaisePropertyChanged(() => ContentBitmap);

                }
            }
        }
        GifBitmapDecoder decoder;
        public IGeneralSettings GeneralSettings { get; }
       
        public ISerialStream[] SerialStreams { get; }
        public IOpenRGBStream OpenRGBStream { get; set; }
        public ISerialDeviceDetection SerialDeviceDetection { get; set; }
         public static IShaderEffect ShaderEffect {get;set;}
        public IDesktopFrame DesktopFrame { get; set; }
        public ISecondDesktopFrame SecondDesktopFrame { get; set; }
        public IThirdDesktopFrame ThirdDesktopFrame { get; set; }
        public int AddedDevice { get; }

        public MainViewViewModel(IContext context,
            IDeviceSettings[] cards,
            IGroupSettings[] groups,
            IDeviceSpotSet[] deviceSpotSets,
            IGeneralSettings generalSettings,
            IOpenRGBStream openRGBStream,
            ISerialDeviceDetection serialDeviceDetection,
            ISerialStream[] serialStreams,
            IShaderEffect shaderEffect,
            IDesktopFrame desktopFrame,
            ISecondDesktopFrame secondDesktopFrame,
            IThirdDesktopFrame thirdDesktopFrame
           )
        {

            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            SerialStreams = serialStreams ?? throw new ArgumentNullException(nameof(serialStreams));
            DesktopFrame = desktopFrame ?? throw new ArgumentNullException(nameof(desktopFrame));
            SecondDesktopFrame = secondDesktopFrame ?? throw new ArgumentNullException(nameof(secondDesktopFrame));
            ThirdDesktopFrame = thirdDesktopFrame ?? throw new ArgumentNullException(nameof(thirdDesktopFrame));
            Cards = new ObservableCollection<IDeviceSettings>();
            Groups = new ObservableCollection<IGroupSettings>();
            DisplayCards = new ObservableCollection<IDeviceSettings>();
            AddedDevice = cards.Length;
            Context=context ?? throw new ArgumentNullException(nameof(context));
            SpotSets = new ObservableCollection<IDeviceSpotSet>();
            OpenRGBStream = openRGBStream ?? throw new ArgumentNullException(nameof(openRGBStream));
            SerialDeviceDetection = serialDeviceDetection ?? throw new ArgumentNullException(nameof(serialDeviceDetection));
            ShaderEffect = shaderEffect ?? throw new ArgumentNullException();
            ShaderEffect.PropertyChanged+= ShaderImageUpdate;
            DesktopFrame.PropertyChanged += ShaderImageUpdate;
            //ShaderSpots = generalSpotSet.ShaderSpot;
            //ShaderBitmap = shaderEffect.MatrixBitmap;
            foreach (IDeviceSettings card in cards)
            {
                Cards.Add(card);
                if (card.IsVissible)
                    DisplayCards.Add(card);
            }
            foreach (IDeviceSpotSet spotSet in deviceSpotSets)
            {
                SpotSets.Add(spotSet);
            }
            foreach(IGroupSettings group in groups)
            {
                Groups.Add(group);
            }



           // WriteJson();
           






            GeneralSettings.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    
                        case nameof(GeneralSettings.Autostart):
                                if (GeneralSettings.Autostart)
                                {
                                    StartUpManager.AddApplicationToCurrentUserStartup();
                                }
                                else
                                {
                                    StartUpManager.RemoveApplicationFromCurrentUserStartup();
                                }
                                break;
                         default:
                                break;
                        
                        


                }

            };
       
        }
      
        public byte CurrentDeviceSelectedEffect {
            get => CurrentDevice.SelectedEffect;
            set
            {
                CurrentDevice.SelectedEffect = value;
                RaisePropertyChanged(() => DeviceRectX);
                RaisePropertyChanged(() => DeviceRectY);
                RaisePropertyChanged(() => DeviceRectWidth);
                RaisePropertyChanged(() => DeviceRectHeight);
            }

        }

        private void ShaderImageUpdate(object sender, PropertyChangedEventArgs e)
        {
            if(IsSplitLightingWindowOpen)
            {
                if (CurrentDevice != null)
                {
                    switch (CurrentDeviceSelectedEffect)
                    {
                        case 5:
                            Context.Invoke(() =>
                            {
                                var MatrixBitmap = new WriteableBitmap(240, 135, 96, 96, PixelFormats.Bgra32, null);
                                MatrixBitmap.Lock();
                                IntPtr pixelAddress = MatrixBitmap.BackBuffer;
                                var CurrentFrame = ShaderEffect.Frame;

                                if (CurrentFrame != null)
                                {
                                    Marshal.Copy(CurrentFrame, 0, pixelAddress, CurrentFrame.Length);

                                    MatrixBitmap.AddDirtyRect(new Int32Rect(0, 0, 240, 135));

                                    MatrixBitmap.Unlock();
                                    ShaderBitmap = MatrixBitmap;
                                    RaisePropertyChanged(() => DeviceRectWidthMax);
                                    RaisePropertyChanged(() => DeviceRectHeightMax);
                                }
                            });
                            break;
                        case 0:
                            switch(CurrentDevice.SelectedDisplay)
                            {
                                case 0:
                                    Context.Invoke(() =>
                                    {
                                        
                                        var CurrentFrame = DesktopFrame.Frame;
                                        if (CurrentFrame != null)
                                        {
                                            var MatrixBitmap = new WriteableBitmap(DesktopFrame.FrameWidth, DesktopFrame.FrameHeight, 96, 96, PixelFormats.Bgra32, null);
                                            MatrixBitmap.Lock();
                                            IntPtr pixelAddress = MatrixBitmap.BackBuffer;
                                            Marshal.Copy(CurrentFrame, 0, pixelAddress, CurrentFrame.Length);

                                            MatrixBitmap.AddDirtyRect(new Int32Rect(0, 0, DesktopFrame.FrameWidth, DesktopFrame.FrameHeight));

                                            MatrixBitmap.Unlock();
                                            ShaderBitmap = MatrixBitmap;
                                            RaisePropertyChanged(() => DeviceRectWidthMax);
                                            RaisePropertyChanged(() => DeviceRectHeightMax);
                                        }
                                        else
                                        {
                                            //notify the UI show error message
                                           
                                        }

                                    });
                                    break;
                                case 1:
                                    Context.Invoke(() =>
                                    {
                                       
                                        var CurrentFrame = SecondDesktopFrame.Frame;
                                        if (CurrentFrame != null)
                                        {
                                            var MatrixBitmap = new WriteableBitmap(SecondDesktopFrame.FrameWidth, SecondDesktopFrame.FrameHeight, 96, 96, PixelFormats.Bgra32, null);
                                            MatrixBitmap.Lock();
                                            IntPtr pixelAddress = MatrixBitmap.BackBuffer;
                                            Marshal.Copy(CurrentFrame, 0, pixelAddress, CurrentFrame.Length);

                                            MatrixBitmap.AddDirtyRect(new Int32Rect(0, 0, 240, 135));

                                            MatrixBitmap.Unlock();
                                            ShaderBitmap = MatrixBitmap;
                                            RaisePropertyChanged(() => DeviceRectWidthMax);
                                            RaisePropertyChanged(() => DeviceRectHeightMax);
                                        }
                                        else
                                        {
                                            //notify the UI show error message
                                            IsSecondDesktopValid = false;
                                            RaisePropertyChanged(() => IsSecondDesktopValid);
                                        }

                                    });
                                    break;
                                case 2:
                                    Context.Invoke(() =>
                                    {
                                       
                                        var CurrentFrame = ThirdDesktopFrame.Frame;
                                        if (CurrentFrame != null)
                                        {
                                            var MatrixBitmap = new WriteableBitmap(ThirdDesktopFrame.FrameWidth, ThirdDesktopFrame.FrameHeight, 96, 96, PixelFormats.Bgra32, null);
                                            MatrixBitmap.Lock();
                                            IntPtr pixelAddress = MatrixBitmap.BackBuffer;
                                            Marshal.Copy(CurrentFrame, 0, pixelAddress, CurrentFrame.Length);

                                            MatrixBitmap.AddDirtyRect(new Int32Rect(0, 0, 240, 135));

                                            MatrixBitmap.Unlock();
                                            ShaderBitmap = MatrixBitmap;
                                            RaisePropertyChanged(() => DeviceRectWidthMax);
                                            RaisePropertyChanged(() => DeviceRectHeightMax);
                                        }
                                        else
                                        {
                                            //notify the UI show error message
                                            IsThirdDesktopValid = false;
                                            RaisePropertyChanged(() => IsThirdDesktopValid);
                                        }

                                    });
                                    break;
                            }
                         
                            break;
                    }
                }
            }
           

            if(IsCanvasLightingWindowOpen)
            {
                Context.Invoke(() =>
                {
                    var MatrixBitmap = new WriteableBitmap(240, 135, 96, 96, PixelFormats.Bgra32, null);
                    MatrixBitmap.Lock();
                    IntPtr pixelAddress = MatrixBitmap.BackBuffer;
                    var CurrentFrame = ShaderEffect.Frame;
                    if(CurrentFrame!=null)
                    {
                        Marshal.Copy(CurrentFrame, 0, pixelAddress, CurrentFrame.Length);

                        MatrixBitmap.AddDirtyRect(new Int32Rect(0, 0, 240, 135));

                        MatrixBitmap.Unlock();
                        ShaderBitmap = MatrixBitmap;
                    }
                   
                });
            }
          

        }

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public void SetPreviewImage(byte[] frame)
        {
            Context.Invoke(() =>
            {
                 PreviewImageSource = new WriteableBitmap(120, 120, 96, 96, PixelFormats.Bgra32, null);
                PreviewImageSource.Lock();
                IntPtr pixelAddress = PreviewImageSource.BackBuffer;
                var CurrentFrame = frame;

                Marshal.Copy(CurrentFrame, 0, pixelAddress, CurrentFrame.Length);

                PreviewImageSource.AddDirtyRect(new Int32Rect(0, 0, 120, 120));
                RaisePropertyChanged(nameof(PreviewImageSource));
                
                PreviewImageSource.Unlock();
               // ShaderBitmap = MatrixBitmap;
            });
        }
        private WriteableBitmap _previewImageSource;
        public WriteableBitmap PreviewImageSource {
            get => _previewImageSource;
            set
            {
               // _log.Info("PreviewImageSource created.");
                Set(ref _previewImageSource, value);
                RaisePropertyChanged();
              
            }
        }
        private bool _isCanvasLightingWindowOpen;
        public bool IsCanvasLightingWindowOpen {
            get => _isCanvasLightingWindowOpen;
            set
            {
                Set(ref _isCanvasLightingWindowOpen, value);
               // _log.Info($"IsSettingsWindowOpen is now {_isSettingsWindowOpen}");
            }
        }
        private bool _isSecondDesktopValid;
        public bool IsSecondDesktopValid {
            get => _isSecondDesktopValid;
            set
            {
                Set(ref _isSecondDesktopValid, value);
                // _log.Info($"IsSettingsWindowOpen is now {_isSettingsWindowOpen}");
            }
        }
        private bool _isThirdDesktopValid;
        public bool IsThirdDesktopValid {
            get => _isThirdDesktopValid;
            set
            {
                Set(ref _isThirdDesktopValid, value);
                // _log.Info($"IsSettingsWindowOpen is now {_isSettingsWindowOpen}");
            }
        }
        private bool _isSplitLightingWindowOpen;
        public bool IsSplitLightingWindowOpen {
            get => _isSplitLightingWindowOpen;
            set
            {
                Set(ref _isSplitLightingWindowOpen, value);
                // _log.Info($"IsSettingsWindowOpen is now {_isSettingsWindowOpen}");
            }
        }

        
        public int DeviceRectX {
            get
            {
                switch (CurrentDeviceSelectedEffect)
                {
                    case 0://ambilight mode
                        return CurrentDevice.DeviceRectLeft1;
                    case 5:
                        return CurrentDevice.DeviceRectLeft;
                    default:
                        return CurrentDevice.DeviceRectLeft;
                }
            }

            set
            {
                switch (CurrentDeviceSelectedEffect)
                {
                    case 0://ambilight mode
                        CurrentDevice.DeviceRectLeft1 = value;
                        RaisePropertyChanged(() => CurrentDevice.DeviceRectLeft1);
                        break;
                    case 5:
                        CurrentDevice.DeviceRectLeft = value;
                        RaisePropertyChanged(() => CurrentDevice.DeviceRectLeft);
                        break;


                }
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<IDeviceSettings> _hUBOutputCollection;
        public ObservableCollection<IDeviceSettings> HUBOutputCollection {
            get { return _hUBOutputCollection; }
            set
            {
                _hUBOutputCollection = value;
            }
        }

       
        public int DeviceRectY {
            get
            {
                switch (CurrentDeviceSelectedEffect)
                {
                    case 0://ambilight mode
                        return CurrentDevice.DeviceRectTop1;
                    case 5:
                        return CurrentDevice.DeviceRectTop;
                    default:
                        return CurrentDevice.DeviceRectTop;
                }
            }

            set
            {
                switch (CurrentDeviceSelectedEffect)
                {
                    case 0://ambilight mode
                        CurrentDevice.DeviceRectTop1 = value;
                        RaisePropertyChanged(() => CurrentDevice.DeviceRectTop1);
                        break;
                    case 5:
                        CurrentDevice.DeviceRectTop = value;
                        RaisePropertyChanged(() => CurrentDevice.DeviceRectTop);
                        break;


                }
                RaisePropertyChanged();
            }
        }
        private bool _hubOutputsNavigationEnable;
        public bool HubOutputNavigationEnable {
            get { return _hubOutputsNavigationEnable;}
            set { _hubOutputsNavigationEnable = value; }
        }
        
      
       
        public int DeviceRectHeightMax {
            get => (int)ShaderBitmap.Height - DeviceRectY;

        }
       
        public int DeviceRectWidthMax {
            get => (int)ShaderBitmap.Width - DeviceRectX;

        }
        public int DeviceRectHeight {
            get
            {
                switch(CurrentDeviceSelectedEffect)
                {
                    case 0://ambilight mode
                        return CurrentDevice.DeviceRectHeight1;
                    case 5:
                        return CurrentDevice.DeviceRectHeight;
                    default:
                        return CurrentDevice.DeviceRectHeight;
                }
            }

            set
            {
                switch (CurrentDeviceSelectedEffect)
                {
                    case 0://ambilight mode
                        CurrentDevice.DeviceRectHeight1 = value;
                        RaisePropertyChanged(() => CurrentDevice.DeviceRectHeight1);
                        break;
                    case 5:
                        CurrentDevice.DeviceRectHeight = value;
                        RaisePropertyChanged(() => CurrentDevice.DeviceRectHeight);
                        break;
                  
                        
                }
                RaisePropertyChanged();
            }
        }
        public int DeviceRectWidth {
            get
            {
                switch (CurrentDevice.SelectedEffect)
                {
                    case 0://ambilight mode
                        return CurrentDevice.DeviceRectWidth1;
                    case 5:
                        return CurrentDevice.DeviceRectWidth;
                    default:
                        return CurrentDevice.DeviceRectWidth;
                }
            }

            set
            {
                switch (CurrentDevice.SelectedEffect)
                {
                    case 0://ambilight mode
                        CurrentDevice.DeviceRectWidth1 = value;
                        RaisePropertyChanged(() => CurrentDevice.DeviceRectWidth1);
                        break;
                    case 5:
                        CurrentDevice.DeviceRectWidth = value;
                        RaisePropertyChanged(() => CurrentDevice.DeviceRectWidth);
                        break;


                }
                RaisePropertyChanged();
            }
        }
     
        public bool SyncOn {
            get => CurrentDevice.SyncOn;
            set
            {
                
                CurrentDevice.SyncOn = value;
                HubOutputNavigationEnable = !value;
                RaisePropertyChanged(() => CurrentDevice.SyncOn);
                RaisePropertyChanged(() => HubOutputNavigationEnable);
                RaisePropertyChanged();

            }
        }
        public bool SyncOff {
            get => !CurrentDevice.SyncOn;
            set
            {

                CurrentDevice.SyncOn = !value;
                HubOutputNavigationEnable = value;
                RaisePropertyChanged(() => HubOutputNavigationEnable);
                RaisePropertyChanged(() => CurrentDevice.SyncOn);
                RaisePropertyChanged();

            }
        }
        public override void ReadData()
        {
            LoadMenu();
            LoadMenuByType(true);
            ReadDataDevice();
            // ReadFAQ();

            //CurrentView = _allDeviceView.CreateViewModel();
            SelectMenuItem = new RelayCommand<VerticalMenuItem>((p) => {
                return true;
            }, (p) =>
            {
                ChangeView(p);
            });
            SelectedVerticalMenuItem = MenuItems.FirstOrDefault();
         
            DeleteCommand = new RelayCommand<string>((p) => {
                return true;
            }, (p) =>
            {
                ShowDeleteDialog();
            });
            AdjustPositionCommand = new RelayCommand<string>((p) => {
                return true;
            }, (p) =>
            {
                ShowAdjustPositon();
            });


            DeleteDeviceCommand = new RelayCommand<IDeviceSettings>((p) => {
                return true;
            }, (p) =>
            {
                ShowDeleteFromDashboard(p);
            });
            DeleteGroupCommand = new RelayCommand<IGroupSettings>((p) => {
                return true;
            }, (p) =>
            {
                ShowDeleteGroupFromDashboard(p);
            });

            SnapshotCommand = new RelayCommand<string>((p) => {
                return true;
            }, (p) =>
            {

                SnapShot();
            });

            DeviceRectDropCommand = new RelayCommand<string>((p) => {
                return true;
            }, (p) =>
            {

                DeviceRectSavePosition();
            });

            RefreshDeviceCommand = new RelayCommand<string>((p) => {
                return true;
            }, (p) =>
            {

                RefreshDevice();
            });
            SelectCardCommand = new RelayCommand<IDeviceSettings>((p) => {
                return p != null;
            }, (p) =>
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) &&! Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    this.GotoChild(p);
                }     
            });
            SelectGroupCommand = new RelayCommand<IGroupSettings>((p) => {
                return p != null;
            }, (p) =>
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    this.GotoGroup(p);
                }
            });
            SetNextSpotVIDCommand = new RelayCommand<IDeviceSpot>((p) => {
                return p != null;
            }, (p) =>
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    this.IncreaseVID(p);
                }
            });

            //LightingModeSelection = new RelayCommand<string>((p) => {
            //    return p != null;
            //}, (p) =>
            //{
            //    switch(p)
            //    {
            //        case "Riêng Lẻ":
            //            //Enbale HUB output Navigation bar
            //            //Notify child devices to restart their own background service
            //            HubOutputNavigationEnable = true;
            //            RaisePropertyChanged(() => HubOutputNavigationEnable);
            //            break;
            //        case "Đồng Bộ":
            //            //disable navigation  
            //            HubOutputNavigationEnable = false;
            //            //Notify child devices to end their own background services and expose their spotset for parrent HUB to take over the Lighting control
            //            RaisePropertyChanged(() => HubOutputNavigationEnable);
            //            break;
            //    }


            //});
            SelectShaderCommand = new RelayCommand<ShaderCard>((p) => {
                return p != null;
            }, (p) =>
            {
                //if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                //{

                //}
                //change shader to selected
                GeneralSettings.SelectedShader = p.Header.ToString();
                RaisePropertyChanged(nameof(GeneralSettings.SelectedShader));

            });

            ShowAddNewCommand = new RelayCommand<IDeviceSettings>((p) => {
                return true;
            }, (p) =>
            {
                ShowAddNewDialog();
            });
            BackCommand = new RelayCommand<string>((p) => {
                return true;
            }, (p) =>
            {
                BackToDashboard();
               

            });
        }

        public void SnapShot()
        {
            int counter = 0;
            byte[] snapshot = new byte[256];
            foreach (IDeviceSpot spot in PreviewSpots)
            {

                snapshot[counter++] = spot.Red;
                snapshot[counter++] = spot.Green;
                snapshot[counter++] = spot.Blue;
                // counter++;
            }
            CurrentDevice.SnapShot = snapshot;
            RaisePropertyChanged(() => CurrentDevice.SnapShot);
        }
        public void CurrentSpotSetVIDChanged()
        {
            
          foreach(var spot in PreviewSpots)
            {
                CurrentDevice.VirtualIndex[spot.id] = spot.VID;
               
            }
                
            WriteDeviceInfoJson();
        }

        public void DeviceRectSavePosition()
        {
            //save current device rect position to json database
            CurrentDevice.DeviceRectLeft = DeviceRectX;
            CurrentDevice.DeviceRectTop = DeviceRectY;
           // CurrentDevice.DeviceRectWidth = DeviceRectWidth;
            //CurrentDevice.DeviceRectHeight = DeviceRectHeight;
           // CurrentDevice.DeviceScale = DeviceScale;
            RaisePropertyChanged(() => CurrentDevice.DeviceRectLeft);
            RaisePropertyChanged(() => CurrentDevice.DeviceRectTop);
           // RaisePropertyChanged(() => CurrentDevice.DeviceRectWidth);
           // RaisePropertyChanged(() => CurrentDevice.DeviceRectHeight);
           // RaisePropertyChanged(() => CurrentDevice.DeviceScale);


        }
        public void DFU()
        {
            foreach (var serialStream in SerialStreams)
            {

                if (CurrentDevice.ParrentLocation != 151293) // device is in hub object
                {
                    if (serialStream.ID == CurrentDevice.ParrentLocation)
                        serialStream.DFU();
                }
                else
                {
                    if (serialStream.ID == CurrentDevice.DeviceID)
                        serialStream.DFU();
                }
            }

        }
        private int _dFUProgress;
        public int DFUProgress {
            get { return _dFUProgress; }
            set
            {
                _dFUProgress = value;
                if (value == 75)
                {
                    DFU();
                }
            }

        }

      

      
        public void RefreshDevice()
        {
            var detectedDevices = SerialDeviceDetection.RefreshDevice();
            var newdevices = new List<string>();
            OpenRGBStream.RefreshTransferState();//refresh device list
            //get a controer data (devices)
            if(OpenRGBStream.AmbinityClient!=null)
            {
                var openRGBdevices = OpenRGBStream.AmbinityClient.GetAllControllerData();
                var detectedOpenRGBDevices = new List<Device>();

                var oldDeviceNum = Cards.Count;
                if (OpenRGBStream.AmbinityClient != null)
                {
                    foreach (var device in openRGBdevices)//add openrgb device to list
                    {
                        detectedOpenRGBDevices.Add(device);
                    }

                    foreach (var device in openRGBdevices)// check if device already exist
                    {
                        foreach (var item in Cards)
                        {
                            if (device.Location == item.DevicePort)
                                detectedOpenRGBDevices.Remove(device);
                        }
                    }
                }

                if (detectedOpenRGBDevices.Count > 0)
                {
                    var result = HandyControl.Controls.MessageBox.Show("Phát hiện " + detectedOpenRGBDevices.Count + " Thiết bị OpenRGB" + " Nhấn [Confirm] để add vào Dashboard", "OpenRGB Device", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (result == MessageBoxResult.OK)//restart app
                    {
                        foreach (var device in openRGBdevices)//convert openRGB device to ambino Device
                        {

                            IDeviceSettings newDevice = new DeviceSettings();
                            newDevice.DeviceName = device.Name.ToString();
                            newDevice.DeviceType = device.Type.ToString();
                            newDevice.DevicePort = device.Location.ToString();
                            newDevice.DeviceID = Cards.Count + 1;
                            newDevice.DeviceSerial = device.Serial;
                            newDevice.NumLED = device.Colors.Length;
                            newDevice.SpotsX = newDevice.NumLED;
                            newDevice.SpotsY = 1;
                            switch (device.Type)
                            {
                                case OpenRGB.NET.Enums.DeviceType.Mouse:
                                    newDevice.DeviceLayout = 1; //strip type
                                    break;
                                case OpenRGB.NET.Enums.DeviceType.Keyboard:
                                    newDevice.DeviceLayout = 1; //strip type
                                    break;
                                case OpenRGB.NET.Enums.DeviceType.Headset:
                                    newDevice.DeviceLayout = 1; //strip type
                                    break;
                                case OpenRGB.NET.Enums.DeviceType.HeadsetStand:
                                    newDevice.DeviceLayout = 1; //strip type
                                    break;
                                case OpenRGB.NET.Enums.DeviceType.Motherboard:
                                    newDevice.DeviceLayout = 1; //strip type
                                    break;
                                case OpenRGB.NET.Enums.DeviceType.Dram:
                                    newDevice.DeviceLayout = 1; //strip type
                                    break;
                                case OpenRGB.NET.Enums.DeviceType.Ledstrip:
                                    newDevice.DeviceLayout = 1; //strip type
                                    break;
                                case OpenRGB.NET.Enums.DeviceType.Gpu:
                                    newDevice.DeviceLayout = 1; //strip type
                                    break;
                                case OpenRGB.NET.Enums.DeviceType.Mousemat:
                                    newDevice.DeviceLayout = 1; //strip type
                                    break;
                            }

                            Cards.Add(newDevice);
                        }
                    }
                }


                if (oldDeviceNum != Cards.Count) //there are changes in device list, we simply restart the application to add process
                {
                    WriteDeviceInfoJson();
                    Application.Restart();
                    Process.GetCurrentProcess().Kill();
                }
            }    
            

        }

        public void ReadDataDevice()
        {
          
            AvailablePalette = new ObservableCollection<string>
       {
           "Rainbow",
           "Cloud",
           "Forest",
           "Sunset",
           "Scarlet",
           "Aurora",
           "France",
           "Lemon",
           "Badtrip",
           "Police",
           "Ice and Fire",
           "Custom"

        };
            AvailableLayout= new ObservableCollection<string>
        {
           "Square, Ring, Rectangle",
           "Strip, Bar",
           "Matrix"
          

        };
            AvailableMatrixOrientation = new ObservableCollection<string>
   {
           "Dọc",
           "Ngang",


        };
            AvailableMatrixStartPoint = new ObservableCollection<string>
{
           "Top Left",
           "Top Right",
           "Bottom Right",
           "Bottom Left"


        };
            AvailableRotation = new ObservableCollection<string>
    {
           "0",
           "90",
           "180",
           "360"


        };
            AvailableEffects = new ObservableCollection<string>
      {
            "Sáng theo màn hình",
           "Sáng theo dải màu",
           "Sáng màu tĩnh",
           "Sáng theo nhạc",
           "Atmosphere",
           "Canvas Lighting",
           "Group Lighting"
        };
            AvailableMusicPalette = new ObservableCollection<string>
{
           "Rainbow",
           "Cafe",
           "Jazz",
           "Party",
           "Custom"


        };
            AvailableFrequency = new ObservableCollection<string>
{
           "1",
           "2",
           "3",
           "4"


        };
            AvailableMusicMode = new ObservableCollection<string>
{
          "Equalizer",
           "VU metter",
           "End to End",
           "Push Pull",
          "Symetric VU",
          "Floating VU",
          "Center VU",
          "Naughty boy"

        };

            AvailableShader = new ObservableCollection<ShaderCard>

            {

                new ShaderCard
                {
                    Header = "Gooey",
                    Content = "Gooey.jpg",
                    Footer = "Super Smooth Gooey color",
                    Thumbnail= "/Shaders/Thumbnail/Gooey.png"

                },
                 new ShaderCard
                {
                    Header = "Plasma",
                    Content = "Plasma.jpg",
                    Footer = "Simple Plasma",
                     Thumbnail= "/Shaders/Thumbnail/Plasma.png"
                },
                
                
                   new ShaderCard
                {
                    Header = "MetaBalls",
                    Content = "MetaBalls.jpg",
                    Footer = "Bouncing Fireflies",
                     Thumbnail= "/Shaders/Thumbnail/Metaballs.png"
                },
                    new ShaderCard
                {
                    Header = "Pixel Rainbow",
                    Content = "PixelRainbow.jpg",
                    Footer = "Diagonal Rainbow Effect",
                     Thumbnail= "/Shaders/Thumbnail/PixelRainbow.png"
                }


            };

        }

        public async void ShowAddNewDialog()
        {
            
            var vm = new ViewModel.AddDeviceViewModel(Cards, Groups,DesktopFrame);
            var view = new View.AddDevice();
            view.DataContext = vm;
            bool addResult = (bool)await DialogHost.Show(view, "mainDialog");
            if (addResult)
            {
                try
                {
                    switch (vm.AddedItemType)
                    {
                        case AddDeviceViewModel.AvailableTypes.Device:
                            if (vm.Device.DeviceType != "ABHV2" && vm.Device.DeviceType != "ABFANHUB")
                            {

                                vm.Device.DeviceID = Cards.Count() + 1;

                                Cards.Add(vm.Device);
                                WriteDeviceInfoJson();

                            }
                            else if (vm.Device.DeviceType == "ABFANHUB")
                            {
                                Cards.Add(vm.Device);//add HUB first
                                foreach (var fan in vm.SelectedOutputs)//add child device
                                {
                                    Cards.Add(fan);
                                }
                                WriteDeviceInfoJson();
                            }
                            else
                            {


                                vm.Device.DeviceID = Cards.Count() + 1;
                                vm.Device.IsHUB = true;
                                vm.Device.HUBID = Cards.Count() + 1;
                                Cards.Add(vm.Device);
                                // WriteJson();

                                if (vm.ARGB1Selected) // ARGB1 output port is in the list
                                {
                                    var argb1 = new DeviceSettings();
                                    argb1.DeviceType = "Strip";                           //add to device settings
                                    argb1.DeviceID = Cards.Count() + 1;
                                    argb1.SpotsX = 5;
                                    argb1.SpotsY = 5;
                                    argb1.NumLED = 16;
                                    argb1.DeviceName = "ARGB1";
                                    argb1.ParrentLocation = vm.Device.HUBID;
                                    argb1.OutputLocation = 0;
                                    argb1.IsVissible = false;
                                    argb1.DeviceLayout = 1;
                                    Cards.Add(argb1);
                                }
                                if (vm.ARGB2Selected)
                                {
                                    var argb2 = new DeviceSettings();
                                    argb2.DeviceType = "Matrix";                           //add to device settings
                                    argb2.DeviceID = Cards.Count() + 1;
                                    argb2.SpotsX = 20;
                                    argb2.SpotsY = 6;
                                    argb2.NumLED = 120;
                                    argb2.DeviceName = "ARGB2";
                                    argb2.ParrentLocation = vm.Device.HUBID;
                                    argb2.OutputLocation = 1;
                                    argb2.IsVissible = false;
                                    argb2.DeviceLayout = 2;
                                    Cards.Add(argb2);
                                }
                                if (vm.PCI1Selected)
                                {
                                    var PCI = new DeviceSettings();
                                    PCI.DeviceType = "Square";                           //add to device settings
                                    PCI.DeviceID = Cards.Count() + 1;
                                    PCI.SpotsX = 12;
                                    PCI.SpotsY = 7;
                                    PCI.NumLED = 34;
                                    PCI.DeviceName = "PCI1";
                                    PCI.ParrentLocation = vm.Device.HUBID;
                                    PCI.OutputLocation = 2;
                                    PCI.DeviceLayout = 0;
                                    PCI.IsVissible = false;
                                    Cards.Add(PCI);
                                }
                                if (vm.PCI2Selected)
                                {
                                    var PCI = new DeviceSettings();
                                    PCI.DeviceType = "Square";                           //add to device settings
                                    PCI.DeviceID = Cards.Count() + 1;
                                    PCI.SpotsX = 12;
                                    PCI.SpotsY = 7;
                                    PCI.NumLED = 34;
                                    PCI.DeviceName = "PCI2";
                                    PCI.ParrentLocation = vm.Device.HUBID;
                                    PCI.OutputLocation = 3;
                                    PCI.DeviceLayout = 0;
                                    PCI.IsVissible = false;
                                    Cards.Add(PCI);
                                }
                                if (vm.PCI3Selected)
                                {
                                    var PCI = new DeviceSettings();
                                    PCI.DeviceType = "Square";                           //add to device settings
                                    PCI.DeviceID = Cards.Count() + 1;
                                    PCI.SpotsX = 12;
                                    PCI.SpotsY = 7;
                                    PCI.NumLED = 34;
                                    PCI.DeviceName = "PCI3";
                                    PCI.ParrentLocation = vm.Device.HUBID;
                                    PCI.OutputLocation = 4;
                                    PCI.DeviceLayout = 0;
                                    PCI.IsVissible = false;
                                    Cards.Add(PCI);
                                }
                                if (vm.PCI4Selected)
                                {
                                    var PCI = new DeviceSettings();
                                    PCI.DeviceType = "Strip";                           //add to device settings
                                    PCI.DeviceID = Cards.Count() + 1;
                                    PCI.SpotsX = 12;
                                    PCI.SpotsY = 7;
                                    PCI.NumLED = 34;
                                    PCI.DeviceName = "PCI4";
                                    PCI.ParrentLocation = vm.Device.HUBID;
                                    PCI.OutputLocation = 5;
                                    PCI.DeviceLayout = 0;
                                    PCI.IsVissible = false;
                                    Cards.Add(PCI);
                                }


                                WriteDeviceInfoJson();
                                // _isAddnew = false;
                            }
                            break;
                        case AddDeviceViewModel.AvailableTypes.Group:
                            Groups.Add(vm.Group);//add HUB first
                            //foreach (var child in vm.SelectedChilds)//add child device
                            //{
                            //    Cards.Add(fan);
                            //}
                            WriteGroupInfoJson();
                            WriteDeviceInfoJson();

                            break;

                    }
                   

                }
                catch (Exception ex)
                {
                    HandyControl.Controls.MessageBox.Show(ex.Message);
                }
                Application.Restart();
                Process.GetCurrentProcess().Kill();
            }


        }

        
    


        public async void ShowDeleteDialog()
        {
            var view = new View.DeleteMessageDialog();
            DeleteMessageDialogViewModel dialogViewModel = new DeleteMessageDialogViewModel(CurrentDevice);
            view.DataContext = dialogViewModel;
            bool addResult = (bool)await DialogHost.Show(view, "mainDialog");
            if (addResult)
            {
                DeleteCard(CurrentDevice);
                int counter = 1;
                foreach (var card in Cards)
                {
                  
                    card.DeviceID = counter;
                    counter++;

                }
                WriteDeviceInfoJson();
                Application.Restart();
                Process.GetCurrentProcess().Kill();
            }


        }
        public async  void ShowAdjustPositon()
        {
            var view = new View.DeviceRectPositon();
            var allDevices = Cards.ToArray();
           AdjustPostionViewModel dialogViewModel = new AdjustPostionViewModel(DeviceRectX,DeviceRectY,DeviceRectWidth,DeviceRectHeight, allDevices,ShaderBitmap, CurrentDevice.SelectedEffect);
            view.DataContext = dialogViewModel;
            bool dialogResult = (bool)await DialogHost.Show(view, "mainDialog");
            if (dialogResult)
            {   //save current device rect position to json database
                DeviceRectX = dialogViewModel.DeviceRectX / 4;
                DeviceRectY = dialogViewModel.DeviceRectY / 4;
                //DeviceRectX = CurrentDevice.DeviceRectLeft;
                //DeviceRectY = CurrentDevice.DeviceRectTop;
                
                




                //RaisePropertyChanged(() => CurrentDevice.DeviceRectLeft);
                //RaisePropertyChanged(() => CurrentDevice.DeviceRectTop);
                RaisePropertyChanged(() => DeviceRectX);
                RaisePropertyChanged(() => DeviceRectY);
                //RaisePropertyChanged(() => DeviceRectWidthMax);
                //RaisePropertyChanged(() => DeviceRectHeightMax);

            }



        }
        public async void ShowDeleteGroupFromDashboard(IGroupSettings group)
        {
        }
            public async void ShowDeleteFromDashboard(IDeviceSettings device)
        {
            var view = new View.DeleteMessageDialog();
            DeleteMessageDialogViewModel dialogViewModel = new DeleteMessageDialogViewModel(device);
            view.DataContext = dialogViewModel;
            bool dialogResult = (bool)await DialogHost.Show(view, "mainDialog");
            if (dialogResult)
            {
                DeleteCard(device);
                int counter = 1;
                foreach (var card in Cards)
                {
                    card.DeviceID = counter;
                    counter++;

                }
                WriteDeviceInfoJson();
                Application.Restart();
                Process.GetCurrentProcess().Kill();
            }


        }



        public void DeleteCard(IDeviceSettings deviceInfo)
        {
            var childcards = new ObservableCollection<IDeviceSettings>();
            if (deviceInfo.IsHUB)
            {
                foreach (var device in Cards)
                {
                    //if (device.ParrentLocation == deviceInfo.DeviceID)
                        if (device.ParrentLocation == deviceInfo.HUBID)
                            childcards.Add(device);
                }
            }
            foreach (var device in childcards)
            {
                Cards.Remove(device);
            }
            Cards.Remove(deviceInfo);
            WriteDeviceInfoJson();
        }

        public void WriteGroupInfoJson()
        {
            
            var groups = new List<IGroupSettings>();

            
            foreach(var group in Groups)
            {
                groups.Add(group);
            }
          
            var groupjson = JsonConvert.SerializeObject(groups, Formatting.Indented);
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonGroupFileNameAndPath, groupjson);
        }
       
        /// <summary>
        /// Change View
        /// </summary>
        /// <param name="menuItem"></param>
        public void ChangeView(VerticalMenuItem menuItem)
        {
            SelectedVerticalMenuItem = menuItem;
            SetMenuItemActiveStatus(menuItem.Text);
        }
        public void WriteDeviceInfoJson()
        {
           
            var devices = new List<IDeviceSettings>();
            foreach (var item in Cards)
            {
                devices.Add(item);
            }
            var json = JsonConvert.SerializeObject(devices, Formatting.Indented);
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonDeviceFileNameAndPath, json);

        }
        public void IncreaseVID(IDeviceSpot spot)
        {
            spot.VID = 5;
        }
        public void GotoGroup(IGroupSettings group)
        {
            SelectedVerticalMenuItem = MenuItems.FirstOrDefault(t => t.Text == groupLighting);
            IsDashboardType = false;
            IsSplitLightingWindowOpen = false;
            IsCanvasLightingWindowOpen = false;
            CurrentGroup = group;
            SetMenuItemActiveStatus(groupLighting);
        }
        public void GotoChild(IDeviceSettings card)
        {
           
            SelectedVerticalMenuItem = MenuItems.FirstOrDefault(t => t.Text == lighting);
            IsDashboardType = false;
            CurrentDevice = card;
            IsSplitLightingWindowOpen = true;
            IsCanvasLightingWindowOpen = false;
            RaisePropertyChanged(() => CurrentDeviceSelectedEffect);
            RaisePropertyChanged(() => DeviceRectX);
            RaisePropertyChanged(() => DeviceRectY);
            RaisePropertyChanged(() => DeviceRectWidth);
            RaisePropertyChanged(() => DeviceRectHeight);
            if (CurrentDevice.IsHUB)
            {
                ParrentLocation = CurrentDevice.HUBID;
                var childList = new List<IDeviceSettings>();
                HUBOutputCollection = new ObservableCollection<IDeviceSettings>();
               // HUBOutputCollection.Add(CurrentDevice);
                DeviceLightingModeCollection = true;
                SyncOn = CurrentDevice.SyncOn;
                RaisePropertyChanged(() => SyncOn);
                //only HUB object has ability to sync it's child device
                RaisePropertyChanged(() => DeviceLightingModeCollection);


                foreach (var device in Cards)
                {
                    if (device.ParrentLocation == CurrentDevice.HUBID)
                    {
                        childList.Add(device);
                        HUBOutputCollection.Add(device);
                    }
                 
                }
                CurrentDevice = childList[0];// navigate to first output
                CurrentDevice.IsNavigationSelected = true;
                
                //CurrentDevice = childList[0];
                
            }
            else
            {
                if (CurrentDevice.ParrentLocation != 151293)
                { 
                        DeviceLightingModeCollection = false;
                    RaisePropertyChanged(() => DeviceLightingModeCollection);
                    //keep current HUBOutputCollection
                }
                else
                {
                    if(HUBOutputCollection!=null)
                       HUBOutputCollection.Clear();

                    DeviceLightingModeCollection = false;
                    RaisePropertyChanged(() => DeviceLightingModeCollection);
                    //Clear current HUBOutputCollection cuz this is a single output device
                }

            }
         
          
            
                foreach (var spotset in SpotSets)
                    if (spotset.ID == CurrentDevice.DeviceID)
                    {
                        PreviewSpots = spotset.Spots;
                    }

            

           // DeviceRectX = CurrentDevice.DeviceRectLeft;
           // DeviceRectY = CurrentDevice.DeviceRectTop;
          //  DeviceRectWidth = CurrentDevice.DeviceRectWidth;
          //  DeviceRectHeight = CurrentDevice.DeviceRectHeight;
            //DeviceRectHeightMax = (int)ShaderBitmap.Height - DeviceRectY;
            //DeviceRectWidthMax = (int)ShaderBitmap.Width - DeviceRectX;
            RaisePropertyChanged(() => DeviceRectHeightMax);
            RaisePropertyChanged(() => DeviceRectWidthMax);

            SetMenuItemActiveStatus(lighting);
        }
        public void BackToDashboard()
        {

          
            IsDashboardType = true;
            SelectedVerticalMenuItem = MenuItems.FirstOrDefault();
            SetMenuItemActiveStatus(dashboard);
        }
        public void BackToDashboardAndDelete(IDeviceSettings device)
        {
            Cards.Remove(device);
            IsDashboardType = true;
            SelectedVerticalMenuItem = MenuItems.FirstOrDefault();
            SetMenuItemActiveStatus(dashboard);
        }
        /// <summary>
        /// Load vertical menu
        /// </summary>
        public void LoadMenu()
        {
            MenuItems = new ObservableCollection<VerticalMenuItem>();
            MenuItems.Add(new VerticalMenuItem() { Text = dashboard, IsActive = true, Type = MenuButtonType.Dashboard });
            MenuItems.Add(new VerticalMenuItem() { Text = deviceSetting, IsActive = false, Type = MenuButtonType.Dashboard });
            MenuItems.Add(new VerticalMenuItem() { Text = appSetting, IsActive = false, Type = MenuButtonType.Dashboard });
            MenuItems.Add(new VerticalMenuItem() { Text = canvasLighting, IsActive = false, Type = MenuButtonType.Dashboard });
            MenuItems.Add(new VerticalMenuItem() { Text = general, IsActive = true, Type = MenuButtonType.DeviceSettings });
            MenuItems.Add(new VerticalMenuItem() { Text = lighting, IsActive = false, Type = MenuButtonType.DeviceSettings });
            MenuItems.Add(new VerticalMenuItem() { Text = groupLighting, IsActive = false, Type = MenuButtonType.GroupLighting });

        }
        /// <summary>
        /// set active state
        /// </summary>
        /// <param name="key"></param>
        public void SetMenuItemActiveStatus(string key)
        {
            foreach (var item in MenuItems)
            {
                item.IsActive = item.Text == key;
            }
        }
        /// <summary>
        /// hide show vertical menu item
        /// </summary>
        /// <param name="isDashboard"></param>
        private void LoadMenuByType(bool isDashboard)
        {
            if (MenuItems == null) return;
            foreach (var item in MenuItems)
            {
                item.IsVisible = item.Type == MenuButtonType.Dashboard ? isDashboard : !isDashboard;
            }
            RaisePropertyChanged(nameof(MenuItems));
        }

    }
}
