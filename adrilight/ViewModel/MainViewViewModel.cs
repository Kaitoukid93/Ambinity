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

using Newtonsoft.Json;
using OpenRGB.NET.Models;
using Un4seen.BassWasapi;
using BO;

using GalaSoft.MvvmLight;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Windows.Media;
using System.Drawing;

using adrilight.Shaders;
using HandyControl.Controls;
using adrilight.View;
using HandyControl.Data;
using System.Windows.Data;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace adrilight.ViewModel
{
    public class MainViewViewModel : BaseViewModel
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");

        private string JsonDeviceFileNameAndPath => Path.Combine(JsonPath, "adrilight-deviceInfos.json");
        private string JsonGroupFileNameAndPath => Path.Combine(JsonPath, "adrilight-groupInfos.json");
        private string JsonPaletteFileNameAndPath => Path.Combine(JsonPath, "adrilight-PaletteCollection.json");

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
        //private Visibility _visibleTabControl;
        //public Visibility VisibleTabControl {
        //    get
        //    {
        //        if (CurrentDevice.IsHUB || CurrentDevice.ParrentLocation != 151293)
        //        {
        //            return Visibility.Visible;
        //        }
        //        else return Visibility.Collapsed;


        //    }
        //    set
        //    {
        //        _visibleTabControl = value;

        //    }
        //}

        //private Visibility _dFUVisibility;
        //public Visibility DFUVisibility {
        //    get
        //    {
        //        if (CurrentDevice.ParrentLocation != 151293)
        //        {
        //            return Visibility.Collapsed;
        //        }
        //        else return Visibility.Visible;


        //    }
        //    set
        //    {
        //        _dFUVisibility = value;

        //    }
        //}
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
                RaisePropertyChanged(nameof(CurrentDevice));

            }
        }
        private IOutputSettings _currentOutput;
        public IOutputSettings CurrentOutput {
            get { return _currentOutput; }
            set
            {
                if (_currentOutput == value) return;
                if (_currentOutput != null) _currentOutput.PropertyChanged -= _currentDevice_PropertyChanged;
                _currentOutput = value;
                if (_currentOutput != null) _currentOutput.PropertyChanged += _currentDevice_PropertyChanged;
                RaisePropertyChanged("_currentOutput");

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
            //if (CurrentDevice.DeviceID == 151293)
            //{

            //}
            //else
            //{
            //    foreach (var spotset in SpotSets)
            //        if (spotset.ID == CurrentDevice.DeviceID)
            //        {
            //            PreviewSpots = spotset.LEDSetup.Spots;
            //            CurrentLEDSetup = spotset.LEDSetup;
            //        }

            //}
        }
        //VIDs commands//
        public ICommand ZerolAllCommand { get; set; }
        public ICommand CancelEditWizardCommand { get; set; }
        public ICommand SetBorderSpotActiveCommand { get; set; }
        public ICommand SaveNewUserEditLEDSetup { get; set; }
        public ICommand ResetCountCommand { get; set; }
        public ICommand SetSpotPIDCommand { get; set; }
        public ICommand ResetMaxCountCommand { get; set; }
        public ICommand SetPIDNeutral { get; set; }
        public ICommand SetPIDReverseNeutral { get; set; }
        public ICommand JumpToNextWizardStateCommand { get; set; }
        public ICommand BackToPreviousWizardStateCommand { get; set; }
        public ICommand LaunchPositionEditWindowCommand { get; set; }
        public ICommand LaunchPIDEditWindowCommand { get; set; }
        public ICommand EditSelectedPaletteCommand { get; set; }
        public ICommand AddNewSolidColorCommand { get; set; }
        public ICommand ImportPaletteCardFromFileCommand { get; set; }
        public ICommand ExportCurrentSelectedPaletteToFileCommand { get; set; }
        public ICommand EditSelectedPaletteSaveConfirmCommand { get; set; }
        public ICommand DeleteSelectedPaletteCommand { get; set; }
        public ICommand CreateNewPaletteCommand { get; set; }
        public ICommand SetIncreamentCommand { get; set; }
        public ICommand SetIncreamentCommandfromZero { get; set; }
        public ICommand UserInputIncreamentCommand { get; set; }
        public ICommand ExportSpotDataCommand { get; set; }
        public ICommand ImportSpotDataCommand { get; set; }
        public ICommand ChangeCurrentDeviceSelectedPalette { get; set; }
        public ICommand SelectMenuItem { get; set; }
        public ICommand BackCommand { get; set; }
        public ICommand DeviceRectDropCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand SetCustomColorCommand { get; set; }
        public ICommand DeleteGroupCommand { get; set; }
        public ICommand AdjustPositionCommand { get; set; }
        public ICommand SnapshotCommand { get; set; }
        public ICommand DeleteDeviceCommand { get; set; }
        public ICommand SetSpotActiveCommand { get; set; }
        public ICommand SetAllSpotActiveCommand { get; set; }
        public ICommand SetZoneColorCommand { get; set; }
        public ICommand GetZoneColorCommand { get; set; }
        #endregion
        private ObservableCollection<IDeviceSettings> _availableDevices;
        public ObservableCollection<IDeviceSettings> AvailableDevices {
            get { return _availableDevices; }
            set
            {
                if (_availableDevices == value) return;
                _availableDevices = value;
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
        private ObservableCollection<IColorPaletteCard> _availablePallete;
        public ObservableCollection<IColorPaletteCard> AvailablePallete {
            get { return _availablePallete; }
            set
            {
                if (_availablePallete == value) return;
                _availablePallete = value;
                RaisePropertyChanged();

            }
        }
        private ObservableCollection<IGradientColorCard> _availableGradient;
        public ObservableCollection<IGradientColorCard> AvailableGradient {
            get { return _availableGradient; }
            set
            {
                if (_availableGradient == value) return;
                _availableGradient = value;
                RaisePropertyChanged();

            }
        }
        private ObservableCollection<Color> _availableSolidColors;
        public ObservableCollection<Color> AvailableSolidColors {
            get { return _availableSolidColors; }
            set
            {
                if (_availableSolidColors == value) return;
                _availableSolidColors = value;
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
        public ILEDSetup _currentLEDSetup;
        public ILEDSetup CurrentLEDSetup {
            get => _currentLEDSetup;
            set
            {
                _currentLEDSetup = value;
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

                RaisePropertyChanged(() => CanvasWidth);
                RaisePropertyChanged(() => CanvasHeight);
            }
        }
        private byte[] _visualizerFFT;
        public byte[] VisualizerFFT {
            get { return _visualizerFFT; }
            set
            {
                _visualizerFFT = value;
                RaisePropertyChanged(nameof(VisualizerFFT));
            }
        }
        private int _visualizerAvailableSpace;
        public int VisualizerAvailableSpace {
            get { return _visualizerAvailableSpace; }
            set
            {
                _visualizerAvailableSpace = value;
                RaisePropertyChanged(nameof(_visualizerAvailableSpace));
            }
        }

        public int MusicProgressBarWidth {
            get { return (VisualizerAvailableSpace - (VisualizerFFT.Length - 1) * 5) / VisualizerFFT.Length; ; }

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


        public int CurrentSelectedCustomColorIndex {


            set
            {
                if (value >= 0)
                {

                    SetCustomColor(value);
                }



            }

        }


        private ObservableCollection<System.Windows.Media.Color> _currentCustomZoneColor;
        public ObservableCollection<System.Windows.Media.Color> CurrentCustomZoneColor {
            get
            {


                return _currentCustomZoneColor;
            }
            set
            {
                _currentCustomZoneColor = value;
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
                if (CurrentOutput.OutputSelectedAudioDevice > AvailableAudioDevice.Count)
                {
                    System.Windows.MessageBox.Show("Last Selected Audio Device is not Available");
                    return -1;
                }
                else
                {
                    var currentDevice = AvailableAudioDevice.ElementAt(CurrentOutput.OutputSelectedAudioDevice);

                    var array = currentDevice.Split(' ');
                    _audioDeviceID = Convert.ToInt32(array[0]);
                    return _audioDeviceID;
                }

            }



        }
        private bool _deviceLightingModeCollection;
        public bool DeviceLightingModeCollection {
            get { return _deviceLightingModeCollection; }
            set { _deviceLightingModeCollection = value; }
        }






        public ObservableCollection<string> AvailablePalette { get; private set; }
        public IContext Context { get; }
        private List<string> _availableDisplays;
        public List<string> AvailableDisplays {
            get
            {
                var listDisplay = new List<string>();
                foreach (var screen in System.Windows.Forms.Screen.AllScreens)
                {

                    listDisplay.Add(screen.DeviceName);
                }
                _availableDisplays = listDisplay;
                return _availableDisplays;
            }
        }

        public ObservableCollection<string> AvailableFrequency { get; private set; }

        public ObservableCollection<string> AvailableMusicPalette { get; private set; }
        public ObservableCollection<string> AvailableMusicMode { get; private set; }
        public ObservableCollection<string> AvailableMenu { get; private set; }
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
        public static IShaderEffect ShaderEffect { get; set; }
        public IDesktopFrame DesktopFrame { get; set; }
        public ISecondDesktopFrame SecondDesktopFrame { get; set; }
        public IThirdDesktopFrame ThirdDesktopFrame { get; set; }
        public int AddedDevice { get; }

        public MainViewViewModel(IContext context,
            IDeviceSettings[] devices,
            //IGroupSettings[] groups,
            //IDeviceSpotSet[] deviceSpotSets,
            IGeneralSettings generalSettings,
            //IOpenRGBStream openRGBStream,
            ISerialDeviceDetection serialDeviceDetection,
            ISerialStream[] serialStreams,
            IShaderEffect shaderEffect,

            ISecondDesktopFrame secondDesktopFrame,
            IThirdDesktopFrame thirdDesktopFrame
           )
        {

            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            SerialStreams = serialStreams ?? throw new ArgumentNullException(nameof(serialStreams));
            //DesktopFrame = desktopFrame ?? throw new ArgumentNullException(nameof(desktopFrame));
            SecondDesktopFrame = secondDesktopFrame ?? throw new ArgumentNullException(nameof(secondDesktopFrame));
            ThirdDesktopFrame = thirdDesktopFrame ?? throw new ArgumentNullException(nameof(thirdDesktopFrame));
            AvailableDevices = new ObservableCollection<IDeviceSettings>();
            Groups = new ObservableCollection<IGroupSettings>();
            DisplayCards = new ObservableCollection<IDeviceSettings>();
            //AddedDevice = cards.Length;
            Context = context ?? throw new ArgumentNullException(nameof(context));
            SpotSets = new ObservableCollection<IDeviceSpotSet>();
            //OpenRGBStream = openRGBStream ?? throw new ArgumentNullException(nameof(openRGBStream));
            SerialDeviceDetection = serialDeviceDetection ?? throw new ArgumentNullException(nameof(serialDeviceDetection));
            ShaderEffect = shaderEffect ?? throw new ArgumentNullException();
            //ShaderEffect.PropertyChanged += ShaderImageUpdate;
            //DesktopFrame.PropertyChanged += ShaderImageUpdate;
            //ShaderSpots = generalSpotSet.ShaderSpot;
            //ShaderBitmap = shaderEffect.MatrixBitmap;
            foreach (IDeviceSettings device in devices)
            {
                AvailableDevices.Add(device);

                //if (card.IsVissible)
                //    DisplayCards.Add(card);
            }
            var addNewButton = new DeviceSettings {
                IsDummy = true
            };
            AvailableDevices.Add(addNewButton);
            //foreach (IDeviceSpotSet spotSet in deviceSpotSets)
            //{
            //    SpotSets.Add(spotSet);
            //}
            //foreach (IGroupSettings group in groups)
            //{
            //    Groups.Add(group);
            //}




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

        //public int CurrentDeviceSelectedEffect {
        //    get => CurrentDevice.SelectedEffect;
        //    set
        //    {
        //        CurrentDevice.SelectedEffect = value;
        //        RaisePropertyChanged(() => DeviceRectX);
        //        RaisePropertyChanged(() => DeviceRectY);
        //        RaisePropertyChanged(() => DeviceRectWidth);
        //        RaisePropertyChanged(() => DeviceRectHeight);
        //    }

        //}

        public void ShaderImageUpdate(byte[] frame)
        {
            //if (IsSplitLightingWindowOpen)
            //{
            //    if (CurrentDevice != null)
            //    {
            //        switch (CurrentDeviceSelectedEffect)
            //        {
            //            case 5:
            //                Context.Invoke(() =>
            //                {
            //                    var MatrixBitmap = new WriteableBitmap(240, 135, 96, 96, PixelFormats.Bgra32, null);
            //                    MatrixBitmap.Lock();
            //                    IntPtr pixelAddress = MatrixBitmap.BackBuffer;
            //                    var CurrentFrame = ShaderEffect.Frame;

            //                    if (CurrentFrame != null)
            //                    {
            //                        Marshal.Copy(CurrentFrame, 0, pixelAddress, CurrentFrame.Length);

            //                        MatrixBitmap.AddDirtyRect(new Int32Rect(0, 0, 240, 135));

            //                        MatrixBitmap.Unlock();
            //                        ShaderBitmap = MatrixBitmap;
            //                        RaisePropertyChanged(() => DeviceRectWidthMax);
            //                        RaisePropertyChanged(() => DeviceRectHeightMax);
            //                    }
            //                });
            //                break;
            //            case 0:
            //                switch (CurrentDevice.SelectedDisplay)
            //                {
            //                    case 0:
            Context.Invoke(() =>
            {

                var CurrentFrame = frame;
                if (CurrentFrame != null)
                {
                    var MatrixBitmap = new WriteableBitmap(240, 135, 96, 96, PixelFormats.Bgra32, null);
                    MatrixBitmap.Lock();
                    IntPtr pixelAddress = MatrixBitmap.BackBuffer;
                    Marshal.Copy(CurrentFrame, 0, pixelAddress, CurrentFrame.Length);

                    MatrixBitmap.AddDirtyRect(new Int32Rect(0, 0, 240, 135));

                    MatrixBitmap.Unlock();
                    ShaderBitmap = MatrixBitmap;
                    //RaisePropertyChanged(() => DeviceRectWidthMax);
                    //RaisePropertyChanged(() => DeviceRectHeightMax);
                }
                else
                {
                    //notify the UI show error message

                }

            });
            //                    break;
            //                case 1:
            //                    Context.Invoke(() =>
            //                    {

            //                        var CurrentFrame = SecondDesktopFrame.Frame;
            //                        if (CurrentFrame != null)
            //                        {
            //                            var MatrixBitmap = new WriteableBitmap(SecondDesktopFrame.FrameWidth, SecondDesktopFrame.FrameHeight, 96, 96, PixelFormats.Bgra32, null);
            //                            MatrixBitmap.Lock();
            //                            IntPtr pixelAddress = MatrixBitmap.BackBuffer;
            //                            Marshal.Copy(CurrentFrame, 0, pixelAddress, CurrentFrame.Length);

            //                            MatrixBitmap.AddDirtyRect(new Int32Rect(0, 0, 240, 135));

            //                            MatrixBitmap.Unlock();
            //                            ShaderBitmap = MatrixBitmap;
            //                            RaisePropertyChanged(() => DeviceRectWidthMax);
            //                            RaisePropertyChanged(() => DeviceRectHeightMax);
            //                        }
            //                        else
            //                        {
            //                            //notify the UI show error message
            //                            IsSecondDesktopValid = false;
            //                            RaisePropertyChanged(() => IsSecondDesktopValid);
            //                        }

            //                    });
            //                    break;
            //                case 2:
            //                    Context.Invoke(() =>
            //                    {

            //                        var CurrentFrame = ThirdDesktopFrame.Frame;
            //                        if (CurrentFrame != null)
            //                        {
            //                            var MatrixBitmap = new WriteableBitmap(ThirdDesktopFrame.FrameWidth, ThirdDesktopFrame.FrameHeight, 96, 96, PixelFormats.Bgra32, null);
            //                            MatrixBitmap.Lock();
            //                            IntPtr pixelAddress = MatrixBitmap.BackBuffer;
            //                            Marshal.Copy(CurrentFrame, 0, pixelAddress, CurrentFrame.Length);

            //                            MatrixBitmap.AddDirtyRect(new Int32Rect(0, 0, 240, 135));

            //                            MatrixBitmap.Unlock();
            //                            ShaderBitmap = MatrixBitmap;
            //                            RaisePropertyChanged(() => DeviceRectWidthMax);
            //                            RaisePropertyChanged(() => DeviceRectHeightMax);
            //                        }
            //                        else
            //                        {
            //                            //notify the UI show error message
            //                            IsThirdDesktopValid = false;
            //                            RaisePropertyChanged(() => IsThirdDesktopValid);
            //                        }

            //                    });
            //                    break;
            //            }

            //            break;
            //    }
            //}
        }


        //    if (IsCanvasLightingWindowOpen)
        //    {
        //        Context.Invoke(() =>
        //        {
        //            var MatrixBitmap = new WriteableBitmap(240, 135, 96, 96, PixelFormats.Bgra32, null);
        //            MatrixBitmap.Lock();
        //            IntPtr pixelAddress = MatrixBitmap.BackBuffer;
        //            var CurrentFrame = ShaderEffect.Frame;
        //            if (CurrentFrame != null)
        //            {
        //                Marshal.Copy(CurrentFrame, 0, pixelAddress, CurrentFrame.Length);

        //                MatrixBitmap.AddDirtyRect(new Int32Rect(0, 0, 240, 135));

        //                MatrixBitmap.Unlock();
        //                ShaderBitmap = MatrixBitmap;
        //            }

        //        });
        //    }


        //}

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
        private System.Windows.Media.Color _currentPickedColor;
        public System.Windows.Media.Color CurrentPickedColor {
            get => _currentPickedColor;
            set
            {
                // _log.Info("PreviewImageSource created.");
                Set(ref _currentPickedColor, value);
                RaisePropertyChanged();

            }
        }
        private int _currentLEDEditWizardState = 0;
        public int CurrentLEDEditWizardState {
            get => _currentLEDEditWizardState;
            set
            {
                // _log.Info("PreviewImageSource created.");
                Set(ref _currentLEDEditWizardState, value);
                RaisePropertyChanged();

            }
        }
        private IDeviceSpot[] _bufferSpots;
        public IDeviceSpot[] BufferSpots {
            get => _bufferSpots;
            set
            {
                // _log.Info("PreviewImageSource created.");
                Set(ref _bufferSpots, value);
                RaisePropertyChanged();

            }
        }
        /// <summary>
        /// This group define visibility binding property and mode selecting for device
        /// </summary>
        /// 1.StaticColor
        #region StaticColor Dependency
       
        public bool IsStaticColorGradientChecked {
            get => CurrentOutput.OutputStaticColorMode=="gradient";
            set
            {
                
                CurrentOutput.OutputStaticColorMode = "gradient";
                RaisePropertyChanged(nameof(CurrentOutput.OutputStaticColorMode));

            }
        }
        
        public bool IsStaticColorBreathingChecked {
            get => CurrentOutput.OutputStaticColorMode == "breathing";
            set
            {
                
                CurrentOutput.OutputStaticColorMode = "breathing";
                RaisePropertyChanged(nameof(CurrentOutput.OutputStaticColorMode));



            }
        }


       
        public bool IsStaticColorSpectrumCyclingChecked {
            get => CurrentOutput.OutputStaticColorMode == "spectrumcycling";
            set
            {
                CurrentOutput.OutputStaticColorMode = "spectrumcycling";
                RaisePropertyChanged(nameof(CurrentOutput.OutputStaticColorMode));

            }
        }
     
        public bool IsStaticColorSolidChecked {
            get => CurrentOutput.OutputStaticColorMode == "solid";
            set
            {
                CurrentOutput.OutputStaticColorMode = "solid";
                RaisePropertyChanged(nameof(CurrentOutput.OutputStaticColorMode));



            }
        }
       
   
        public bool IsStaticColorGradientModeFullCycleChecked {
            get => CurrentOutput.OutputStaticColorGradientMode == "full";
            set
            {
                CurrentOutput.OutputStaticColorGradientMode = "full";
                RaisePropertyChanged(nameof(CurrentOutput.OutputStaticColorGradientMode));

            }
        }
       
        public bool IsStaticColorGradientModeFirstChecked {
            get => CurrentOutput.OutputStaticColorGradientMode == "first";
            set
            {
                CurrentOutput.OutputStaticColorGradientMode = "first";
                RaisePropertyChanged(nameof(CurrentOutput.OutputStaticColorGradientMode));

            }
        }
      
        public bool IsStaticColorGradientModeLastChecked {
            get => CurrentOutput.OutputStaticColorGradientMode == "last";
            set
            {
                CurrentOutput.OutputStaticColorGradientMode = "last";
                RaisePropertyChanged(nameof(CurrentOutput.OutputStaticColorGradientMode));

            }
        }
   
        public bool IsStaticColorGradientModeCustomChecked {
            get => CurrentOutput.OutputStaticColorGradientMode == "custom";
            set
            {

                CurrentOutput.OutputStaticColorGradientMode = "custom";
                RaisePropertyChanged(nameof(CurrentOutput.OutputStaticColorGradientMode));

            }
        }
        #endregion
        #region Screen Capture Dependency
      
        public bool IsRightScreenRegionChecked {
            get => CurrentOutput.OutputScreenCapturePosition == "right";
            set
            {
                CurrentOutput.OutputScreenCapturePosition = "right";
                RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCapturePosition));
                RaisePropertyChanged(() => IsRightScreenRegionChecked);
                RaisePropertyChanged(() => IsLeftScreenRegionChecked);
                RaisePropertyChanged(() => IsTopScreenRegionChecked);
                RaisePropertyChanged(() => IsBottomScreenRegionChecked);
                RaisePropertyChanged(() => IsCustomScreenRegionChecked);
                RaisePropertyChanged(() => IsFullScreenRegionChecked);
            }
        }
        
        public bool IsLeftScreenRegionChecked {
            get => CurrentOutput.OutputScreenCapturePosition == "left";
            set
            {
                CurrentOutput.OutputScreenCapturePosition = "left";
                RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCapturePosition));
                RaisePropertyChanged(() => IsRightScreenRegionChecked);
                RaisePropertyChanged(() => IsLeftScreenRegionChecked);
                RaisePropertyChanged(() => IsTopScreenRegionChecked);
                RaisePropertyChanged(() => IsBottomScreenRegionChecked);
                RaisePropertyChanged(() => IsCustomScreenRegionChecked);
                RaisePropertyChanged(() => IsFullScreenRegionChecked);
            }
        }
       
        public bool IsTopScreenRegionChecked {
            get => CurrentOutput.OutputScreenCapturePosition == "top";
            set
            {
                CurrentOutput.OutputScreenCapturePosition = "top";
                RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCapturePosition));
                RaisePropertyChanged(() => IsRightScreenRegionChecked);
                RaisePropertyChanged(() => IsLeftScreenRegionChecked);
                RaisePropertyChanged(() => IsTopScreenRegionChecked);
                RaisePropertyChanged(() => IsBottomScreenRegionChecked);
                RaisePropertyChanged(() => IsCustomScreenRegionChecked);
                RaisePropertyChanged(() => IsFullScreenRegionChecked);
            }
        }
       
        public bool IsBottomScreenRegionChecked {
            get => CurrentOutput.OutputScreenCapturePosition == "bot";
            set
            {
                CurrentOutput.OutputScreenCapturePosition = "bot";
                RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCapturePosition));
                RaisePropertyChanged(() => IsRightScreenRegionChecked);
                RaisePropertyChanged(() => IsLeftScreenRegionChecked);
                RaisePropertyChanged(() => IsTopScreenRegionChecked);
                RaisePropertyChanged(() => IsBottomScreenRegionChecked);
                RaisePropertyChanged(() => IsCustomScreenRegionChecked);
                RaisePropertyChanged(() => IsFullScreenRegionChecked);
            }
        }
     
        public bool IsCustomScreenRegionChecked {
            get => CurrentOutput.OutputScreenCapturePosition == "custom";
            set
            {
                CurrentOutput.OutputScreenCapturePosition = "custom";
                RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCapturePosition));
                RaisePropertyChanged(() => IsRightScreenRegionChecked);
                RaisePropertyChanged(() => IsLeftScreenRegionChecked);
                RaisePropertyChanged(() => IsTopScreenRegionChecked);
                RaisePropertyChanged(() => IsBottomScreenRegionChecked);
                RaisePropertyChanged(() => IsCustomScreenRegionChecked);
                RaisePropertyChanged(() => IsFullScreenRegionChecked);
            }
        }
        public bool IsFullScreenRegionChecked {
            get => CurrentOutput.OutputScreenCapturePosition == "full";
            set
            {
                CurrentOutput.OutputScreenCapturePosition = "full";
                RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCapturePosition));
                RaisePropertyChanged(() => IsRightScreenRegionChecked);
                RaisePropertyChanged(() => IsLeftScreenRegionChecked);
                RaisePropertyChanged(() => IsTopScreenRegionChecked);
                RaisePropertyChanged(() => IsBottomScreenRegionChecked);
                RaisePropertyChanged(() => IsCustomScreenRegionChecked);
                RaisePropertyChanged(() => IsFullScreenRegionChecked);
            }
        }
       
        public bool IsWarmWBSelected {
            get => CurrentOutput.OutputScreenCaptureWB == "warm";
            set
            {

                CurrentOutput.OutputScreenCapturePosition = "warm";
                RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCaptureWB));
            }
        }

       
        public bool IsNaturalWBSelected {
            get => CurrentOutput.OutputScreenCaptureWB == "natural";
            set
            {

                CurrentOutput.OutputScreenCapturePosition = "natural";
                RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCaptureWB));
            }
        }
        
        public bool IsColdWBSelected {
            get => CurrentOutput.OutputScreenCaptureWB == "cold";
            set
            {

                CurrentOutput.OutputScreenCapturePosition = "cold";
                RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCaptureWB));
            }
        }
        
        public bool IsCustomWBSelected {
            get => CurrentOutput.OutputScreenCaptureWB == "custom";
            set
            {

                CurrentOutput.OutputScreenCapturePosition = "custom";
                RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCaptureWB));
            }
        }



        #endregion



        private int _count = 0;
        public int Count {

            get => _count;
            set
            {

                Set(ref _count, value);
                RaisePropertyChanged(nameof(IsNextable));
            }
        }
        private int _maxLEDCount = 0;
        public int MaxLEDCount {

            get => _maxLEDCount;
            set
            {
                Set(ref _maxLEDCount, value);
                RaisePropertyChanged(nameof(IsNextable));
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

        private IColorPaletteCard _currentActivePaletteCard;
        public IColorPaletteCard CurrentActivePaletteCard {
            get { return _currentActivePaletteCard; }
            set
            {
                _currentActivePaletteCard = value;

                SetCurrentDeviceSelectedPalette(value);
            }
        }




        public int DeviceRectX {
            get
            {
                return CurrentOutput.OutputLocationX;
            }

            set
            {
                CurrentOutput.OutputLocationX = value;

                RaisePropertyChanged();
            }
        }
        public int DeviceRectY {
            get
            {
                return CurrentOutput.OutputLocationY;
            }

            set
            {
                CurrentOutput.OutputLocationX = value;

                RaisePropertyChanged();
            }
        }
        private List<IDeviceSpot> _activatedSpots;
        public List<IDeviceSpot> ActivatedSpots {
            get
            {
                return _activatedSpots;
            }

            set
            {
                _activatedSpots = value;

                RaisePropertyChanged();
            }
        }
        private List<IDeviceSpot> _backupSpots;
        public List<IDeviceSpot> BackupSpots {
            get
            {
                return _backupSpots;
            }

            set
            {
                _backupSpots = value;

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
        private string _newPaletteName;
        public string NewPaletteName {
            get { return _newPaletteName; }
            set
            {
                _newPaletteName = value;
            }
        }
        private string _newPaletteOwner;
        public string NewPaletteOwner {
            get { return _newPaletteOwner; }
            set
            {
                _newPaletteOwner = value;
            }
        }
        private string _newPaletteDescription;
        public string NewPaletteDescription {
            get { return _newPaletteDescription; }
            set
            {
                _newPaletteDescription = value;
            }
        }





        //public List<PaletteCardContextMenu> PaletteCardOptions { get; set; }
        private bool _hubOutputsNavigationEnable;
        public bool HubOutputNavigationEnable {
            get { return _hubOutputsNavigationEnable; }
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
                return CurrentOutput.OutputPixelHeight;
            }

            set
            {
                CurrentOutput.OutputPixelHeight = value;
                RaisePropertyChanged();
            }
        }
        public bool IsNextable {
            get
            {
                if (CurrentLEDEditWizardState == 0)
                    return Count != 0;
                else if (CurrentLEDEditWizardState == 1)
                    return MaxLEDCount == 0;
                else
                    return true;
            }
        }
        public int DeviceRectWidth {
            get
            {
                return CurrentOutput.OutputPixelWidth;
            }

            set
            {
                CurrentOutput.OutputPixelHeight = value;
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
            //VIDs command//


            ZerolAllCommand = new RelayCommand<string>((p) =>
    {
        return true;
    }, (p) =>
    {
        ShowZeroingDialog();
    });
            ChangeCurrentDeviceSelectedPalette = new RelayCommand<IColorPaletteCard>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetCurrentDeviceSelectedPalette(CurrentActivePaletteCard);
            });

            EditSelectedPaletteCommand = new RelayCommand<IColorPaletteCard>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenEditPaletteDialog();
            });
            LaunchPositionEditWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenPositionEditWindow();
            });


            AddNewSolidColorCommand = new RelayCommand<string>((p) =>
                 {
                     return true;
                 }, (p) =>
                 {
                     OpenColorPickerWindow();
                 });
            EditSelectedPaletteSaveConfirmCommand = new RelayCommand<string>((p) =>
                 {
                     return true;
                 }, (p) =>
                 {
                     OpenSaveConfirmMessage();
                 });

            CreateNewPaletteCommand = new RelayCommand<string>((p) =>
                   {
                       return true;
                   }, (p) =>
                   {
                       CreateNewPaletteFromCurrentEditPalette();
                   });
            DeleteSelectedPaletteCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeleteSelectedPalette(CurrentActivePaletteCard);
            });
            SetIncreamentCommandfromZero = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetIncreament(0, 1, 0, PreviewSpots.Length - 1);
            });
            SetIncreamentCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SetIncreament(10, 1, 0, PreviewSpots.Length - 1);
            });

            UserInputIncreamentCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ShowIncreamentDataDialog();
            });
            ExportSpotDataCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ExportCurrentSpotData();
            });
            ImportSpotDataCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ImportCurrentSpotData();
            });

            SelectMenuItem = new RelayCommand<VerticalMenuItem>((p) =>
            {
                return true;
            }, (p) =>
            {
                ChangeView(p);
            });
            SelectedVerticalMenuItem = MenuItems.FirstOrDefault();

            DeleteCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ShowDeleteDialog();
            });
            AdjustPositionCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ShowAdjustPositon();
            });
            ImportPaletteCardFromFileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ImportPaletteCardFromFile();
            });
            ExportCurrentSelectedPaletteToFileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ExportCurrentSelectedPaletteToFile();
            });


            DeleteDeviceCommand = new RelayCommand<IDeviceSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                ShowDeleteFromDashboard(p);
            });
            DeleteGroupCommand = new RelayCommand<IGroupSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                ShowDeleteGroupFromDashboard(p);
            });

            SnapshotCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                //SnapShot();
            });
            LaunchPIDEditWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                LaunchPIDEditWindow();
            });


            DeviceRectDropCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                DeviceRectSavePosition();
            });

            RefreshDeviceCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                RefreshDevice();
            });
            SelectCardCommand = new RelayCommand<IDeviceSettings>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    this.GotoChild(p);
                }
            });
            SelectGroupCommand = new RelayCommand<IGroupSettings>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    this.GotoGroup(p);
                }
            });



            SaveNewUserEditLEDSetup = new RelayCommand<string>((p) =>
                  {
                      return p != null;
                  }, (p) =>
                  {
                      WriteDeviceInfoJson();
                      RaisePropertyChanged(nameof(CurrentOutput));
                  });
            JumpToNextWizardStateCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                CurrentLEDEditWizardState++;

                if (CurrentLEDEditWizardState == 1)
                {


                    GrabActivatedSpot();

                }
                else if (CurrentLEDEditWizardState == 2)
                {
                    ReorderActivatedSpot();
                    CurrentOutput.IsInSpotEditWizard = false;
                }

            });

            CancelEditWizardCommand = new RelayCommand<string>((p) =>
                 {
                     return p != null;
                 }, (p) =>
                 {

                 });
            BackToPreviousWizardStateCommand = new RelayCommand<string>((p) =>
                 {
                     return p != null;
                 }, (p) =>
                 {
                     CurrentOutput.IsInSpotEditWizard = true;
                     CurrentLEDEditWizardState--;

                     //if (CurrentLEDEditWizardState == 1)
                     //{

                     //    BufferSpots = new IDeviceSpot[MaxLEDCount];
                     //    GrabActivatedSpot();

                     //}
                     //else if (CurrentLEDEditWizardState == 2)
                     //{
                     //    ReorderActivatedSpot();
                     //}
                     //else if (CurrentLEDEditWizardState == 3)
                     //{
                     //    RunTestSequence();
                     //}
                 });

            SetSpotActiveCommand = new RelayCommand<IDeviceSpot>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                    {
                        spot.SetStroke(0.5);
                        spot.IsActivated = true;
                        Count++;
                    }
                }
                else
                {
                    if (p.BorderThickness != 0)
                    {
                        p.SetStroke(0);
                        Count--;
                        p.IsActivated = false;
                    }

                    else
                    {
                        p.SetStroke(0.5);
                        Count++;
                        p.IsActivated = true;
                    }
                }
             


            });
            SetAllSpotActiveCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                Count=0;
                foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                    {
                        spot.SetStroke(0.5);
                        spot.IsActivated = true;
                        Count++;
                    }
               



            });
            SetBorderSpotActiveCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                Count=0;
                foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                {
                    spot.SetStroke(0);
                    spot.IsActivated = false;
                    if (spot.XIndex == 0 || spot.YIndex == 0|| spot.XIndex == CurrentOutput.OutputNumLEDX-1 || spot.YIndex == CurrentOutput.OutputNumLEDY-1)
                    {
                        spot.SetStroke(0.5);
                        spot.IsActivated = true;
                        Count++;
                    }

                }


            });
            SetSpotPIDCommand = new RelayCommand<IDeviceSpot>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                
                    if (p.OnDemandColor == Color.FromRgb(0, 0, 0))
                    {
                        p.SetColor(100, 27, 0, true);
                        p.SetID(ActivatedSpots.Count() - MaxLEDCount--);
                        p.SetIDVissible(true);




                    }

                    else
                    {
                        p.SetColor(0, 0, 0, true);
                        p.SetID(0);
                        p.SetIDVissible(false);
                        MaxLEDCount++;
                    }
                
              


            });

            ResetCountCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {

                Count = 0;
                foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                {
                    spot.SetStroke(0);
                }

            });
            SetPIDNeutral = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                MaxLEDCount = ActivatedSpots.Count;
                foreach (var spot in ActivatedSpots)
                {
                    spot.SetColor(0, 0, 0, true);
                    spot.SetIDVissible(false);
                }
                foreach (var spot in ActivatedSpots)
                {
                    spot.SetColor(100, 27, 0, true);
                    spot.SetID(ActivatedSpots.Count() - MaxLEDCount--);
                    spot.SetIDVissible(true);
                   
                }

            });
            SetPIDReverseNeutral = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                MaxLEDCount = ActivatedSpots.Count;
                foreach (var spot in ActivatedSpots)
                {
                    spot.SetColor(0, 0, 0, true);
                    spot.SetIDVissible(false);
                }
                foreach (var spot in ActivatedSpots)
                {
                    spot.SetColor(100, 27, 0, true);
                    spot.SetID(MaxLEDCount--);
                    spot.SetIDVissible(true);

                }

            });
            ResetMaxCountCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {

                MaxLEDCount = ActivatedSpots.Count;
                foreach (var spot in ActivatedSpots)
                {
                    spot.SetColor(0, 0, 0, true);
                    spot.SetIDVissible(false);
                }

            });


            SetZoneColorCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                //if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))

                //    CurrentPickedColor = CurrentDevice.CustomZone[CurrentSelectedCustomColorIndex];

                //else
                //    SetCustomColor(CurrentSelectedCustomColorIndex);
            });
            GetZoneColorCommand = new RelayCommand<System.Windows.Media.Color>((p) =>
            {
                return true;
            }, (p) =>
            {



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
            SelectShaderCommand = new RelayCommand<ShaderCard>((p) =>
            {
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

            ShowAddNewCommand = new RelayCommand<IDeviceSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                ShowAddNewDialog();
            });
            BackCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                BackToDashboard();


            });
        }

        private void RunTestSequence()
        {

        }
        private void ReorderActivatedSpot()
        {
            BackupSpots.Clear();
            BackupSpots = ActivatedSpots.OrderBy(o => o.id).ToList();


        }
        private void GrabActivatedSpot()
        {
            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
            {
                if (spot.IsActivated)
                {
                    ActivatedSpots.Add(spot);
                    spot.SetStroke(0);

                }

            }


        }

        private void LaunchPIDEditWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"PIDEditWindow"}") is System.Windows.Window window)
            {
                BackupSpots = new List<IDeviceSpot>();
                foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                {
                    BackupSpots.Add(spot);
                }
                CurrentOutput.IsInSpotEditWizard = true;
                ActivatedSpots = new List<IDeviceSpot>();
                RaisePropertyChanged(nameof(CurrentOutput.IsInSpotEditWizard));

                foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                {
                    spot.SetColor(0, 0, 0, true);
                }
                CurrentLEDEditWizardState = 0;
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();

            }
        }

        private void OpenColorPickerWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"ColorPickerWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();

            }
        }

        private static void OpenPositionEditWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"PositionEditWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();

            }
        }

        private void ExportCurrentSelectedPaletteToFile()
        {
            SaveFileDialog Export = new SaveFileDialog();
            Export.CreatePrompt = true;
            Export.OverwritePrompt = true;

            Export.Title = "Xuất dữ liệu";
            Export.FileName = CurrentActivePaletteCard.Name + " Color Palette";
            Export.CheckFileExists = false;
            Export.CheckPathExists = true;
            Export.DefaultExt = "col";
            Export.Filter = "All files (*.*)|*.*";
            Export.InitialDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Export.RestoreDirectory = true;


            string[] paletteData = new string[19];
            paletteData[0] = CurrentActivePaletteCard.Name;
            paletteData[1] = CurrentActivePaletteCard.Owner;
            paletteData[2] = CurrentActivePaletteCard.Description;
            for (int i = 0; i < CurrentActivePaletteCard.Thumbnail.Length; i++)
            {
                paletteData[i + 3] = CurrentActivePaletteCard.Thumbnail[i].ToString();
            }
            if (Export.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllLines(Export.FileName, paletteData);

            }
        }
        public void SetPreviewVisualizerFFT(byte[] fft)
        {

            VisualizerFFT = fft;
        }
        private void CreateNewPaletteFromCurrentEditPalette()
        {
            var name = NewPaletteName;
            var owner = NewPaletteOwner;
            var description = NewPaletteDescription;
            var newPaletteThumbnail = new System.Windows.Media.Color[16];
            int counter = 0;
            foreach (var color in CurrentCustomZoneColor)
            {
                newPaletteThumbnail[counter++] = color;
            }
            IColorPaletteCard newpalette = new ColorPaletteCard(name, owner, "RGBPalette16", description, newPaletteThumbnail);
            AvailablePallete.Add(newpalette);
            CurrentActivePaletteCard = newpalette;
            RaisePropertyChanged(nameof(CurrentActivePaletteCard));
            SetCurrentDeviceSelectedPalette(CurrentActivePaletteCard);
            RaisePropertyChanged(nameof(AvailablePallete));
            WritePaletteCollectionJson();
        }
        private void DeleteSelectedPalette(IColorPaletteCard palette)
        {
            if (AvailablePallete.ElementAt(CurrentOutput.OutputSelectedChasingPalette) == palette && AvailablePallete.ElementAt(CurrentOutput.OutputSelectedMusicPalette) == palette)
            {
                var result = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo {
                    Message = " Có một chế độ khác đang sử dụng dải màu này, bạn có muốn xóa không ?",
                    Caption = "Xóa dải màu",
                    Button = MessageBoxButton.YesNo,
                    IconBrushKey = ResourceToken.AccentBrush,
                    IconKey = ResourceToken.AskGeometry,
                    StyleKey = "MessageBoxCustom"
                });
                if (result == MessageBoxResult.Yes) // overwrite current palette
                {

                    CurrentOutput.OutputSelectedChasingPalette = 0;
                    CurrentOutput.OutputSelectedMusicPalette = 0;
                    RaisePropertyChanged(nameof(CurrentOutput.OutputSelectedChasingPalette));
                    RaisePropertyChanged(nameof(CurrentOutput.OutputSelectedMusicPalette));
                    CurrentActivePaletteCard = AvailablePallete.First();
                    SetCurrentDeviceSelectedPalette(CurrentActivePaletteCard);
                    RaisePropertyChanged(nameof(CurrentActivePaletteCard));
                    AvailablePallete.Remove(palette);
                    RaisePropertyChanged(nameof(AvailablePallete));
                    WritePaletteCollectionJson();
                }
                else
                {

                }

            }
            else
            {
                switch (CurrentOutput.OutputSelectedMode)
                {
                    case 1:
                        CurrentOutput.OutputSelectedChasingPalette = 0;
                        RaisePropertyChanged(nameof(CurrentOutput.OutputSelectedChasingPalette));
                        break;
                    case 3:
                        CurrentOutput.OutputSelectedMusicPalette = 0;
                        RaisePropertyChanged(nameof(CurrentOutput.OutputSelectedMusicPalette));
                        break;
                }
                CurrentActivePaletteCard = AvailablePallete.First();
                SetCurrentDeviceSelectedPalette(CurrentActivePaletteCard);
                RaisePropertyChanged(nameof(CurrentActivePaletteCard));
                AvailablePallete.Remove(palette);
                RaisePropertyChanged(nameof(AvailablePallete));
                WritePaletteCollectionJson();

            }





            WritePaletteCollectionJson();
        }


        private void OpenSaveConfirmMessage()
        {
            var result = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo {
                Message = "Bạn có muốn ghi đè lên dải màu hiện tại?",
                Caption = "Lưu dải màu",
                Button = MessageBoxButton.YesNo,
                IconBrushKey = ResourceToken.AccentBrush,
                IconKey = ResourceToken.AskGeometry,
                StyleKey = "MessageBoxCustom"
            });
            if (result == MessageBoxResult.Yes) // overwrite current palette
            {
                var activePalette = CurrentOutput.OutputCurrentActivePalette;
                CurrentActivePaletteCard.Thumbnail = activePalette;
                SetCurrentDeviceSelectedPalette(CurrentActivePaletteCard);
                WritePaletteCollectionJson();
                //reload all available palette;
                AvailablePallete.Clear();
                foreach (var palette in LoadPaletteIfExists())
                {
                    AvailablePallete.Add(palette);
                }

                CurrentCustomZoneColor.Clear();
                foreach (var color in CurrentOutput.OutputCurrentActivePalette)
                {
                    CurrentCustomZoneColor.Add(color);
                }
                RaisePropertyChanged(nameof(CurrentCustomZoneColor));
            }

            else // open create new dialog
            {
                OpenCreateNewDialog();
            }
        }

        public void OpenEditPaletteDialog()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"PaletteEditWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                CurrentCustomZoneColor.Clear();
                foreach (var color in CurrentOutput.OutputCurrentActivePalette)
                {
                    CurrentCustomZoneColor.Add(color);
                }
                RaisePropertyChanged(nameof(CurrentCustomZoneColor));
                window.ShowDialog();

            }
        }
        public void OpenCreateNewDialog()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AddNewPaletteWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();

            }
        }
        public void SetCurrentDeviceSelectedPalette(IColorPaletteCard palette)
        {
            if (palette != null)
            {
                for (var i = 0; i < CurrentOutput.OutputCurrentActivePalette.Length; i++)
                {
                    CurrentOutput.OutputCurrentActivePalette[i] = palette.Thumbnail[i];

                }

                RaisePropertyChanged(nameof(AvailablePallete));
                RaisePropertyChanged(nameof(CurrentOutput.OutputCurrentActivePalette));
                WriteDeviceInfoJson();
            }


        }

        //public void SnapShot()
        //{
        //    int counter = 0;
        //    byte[] snapshot = new byte[256];
        //    foreach (IDeviceSpot spot in PreviewSpots)
        //    {

        //        snapshot[counter++] = spot.Red;
        //        snapshot[counter++] = spot.Green;
        //        snapshot[counter++] = spot.Blue;
        //        // counter++;
        //    }
        //    CurrentOutput.SnapShot = snapshot;
        //    RaisePropertyChanged(() => CurrentDevice.SnapShot);
        //}
        public void SetCustomColor(int index)
        {
            CurrentOutput.OutputCurrentActivePalette[index] = CurrentPickedColor;
            RaisePropertyChanged(nameof(CurrentOutput.OutputCurrentActivePalette));
            CurrentCustomZoneColor.Clear();
            foreach (var color in CurrentOutput.OutputCurrentActivePalette)
            {
                CurrentCustomZoneColor.Add(color);
            }
            RaisePropertyChanged(nameof(CurrentCustomZoneColor));
        }
        public void CurrentSpotSetVIDChanged()
        {

            //foreach (var spot in PreviewSpots)
            //{
            //    CurrentDevice.VirtualIndex[spot.id] = spot.VID;

            //}

            WriteDeviceInfoJson();
        }

        public void DeviceRectSavePosition()
        {
            //save current device rect position to json database
            //CurrentDevice.DeviceRectLeft = DeviceRectX;
            //CurrentDevice.DeviceRectTop = DeviceRectY;
            // CurrentDevice.DeviceRectWidth = DeviceRectWidth;
            //CurrentDevice.DeviceRectHeight = DeviceRectHeight;
            // CurrentDevice.DeviceScale = DeviceScale;
            //RaisePropertyChanged(() => CurrentDevice.DeviceRectLeft);
            //RaisePropertyChanged(() => CurrentDevice.DeviceRectTop);
            // RaisePropertyChanged(() => CurrentDevice.DeviceRectWidth);
            // RaisePropertyChanged(() => CurrentDevice.DeviceRectHeight);
            // RaisePropertyChanged(() => CurrentDevice.DeviceScale);


        }
        public void DFU()
        {
            foreach (var serialStream in SerialStreams)
            {

                //if (CurrentDevice.ParrentLocation != 151293) // device is in hub object
                //{
                //    if (serialStream.ID == CurrentDevice.ParrentLocation)
                //        serialStream.DFU();
                //}
                //else
                //{
                //    if (serialStream.ID == CurrentDevice.DeviceID)
                //        serialStream.DFU();
                //}
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
            //var detectedDevices = SerialDeviceDetection.RefreshDevice();
            //var newdevices = new List<string>();
            //OpenRGBStream.RefreshTransferState();//refresh device list
            ////get a controer data (devices)
            //if (OpenRGBStream.AmbinityClient != null)
            //{
            //    var openRGBdevices = OpenRGBStream.AmbinityClient.GetAllControllerData();
            //    var detectedOpenRGBDevices = new List<Device>();

            //    var oldDeviceNum = Cards.Count;
            //    if (OpenRGBStream.AmbinityClient != null)
            //    {
            //        foreach (var device in openRGBdevices)//add openrgb device to list
            //        {
            //            detectedOpenRGBDevices.Add(device);
            //        }

            //        foreach (var device in openRGBdevices)// check if device already exist
            //        {
            //            foreach (var item in Cards)
            //            {
            //                if (device.Location == item.DevicePort)
            //                    detectedOpenRGBDevices.Remove(device);
            //            }
            //        }
            //    }

            //    if (detectedOpenRGBDevices.Count > 0)
            //    {
            //        var result = HandyControl.Controls.MessageBox.Show("Phát hiện " + detectedOpenRGBDevices.Count + " Thiết bị OpenRGB" + " Nhấn [Confirm] để add vào Dashboard", "OpenRGB Device", MessageBoxButton.OK, MessageBoxImage.Information);
            //        if (result == MessageBoxResult.OK)//restart app
            //        {
            //            foreach (var device in openRGBdevices)//convert openRGB device to ambino Device
            //            {

            //                IDeviceSettings newDevice = new DeviceSettings();
            //                newDevice.DeviceName = device.Name.ToString();
            //                newDevice.DeviceType = device.Type.ToString();
            //                newDevice.DevicePort = device.Location.ToString();
            //                newDevice.DeviceID = Cards.Count + 1;
            //                newDevice.DeviceSerial = device.Serial;
            //                newDevice.NumLED = device.Colors.Length;
            //                newDevice.SpotsX = newDevice.NumLED;
            //                newDevice.SpotsY = 1;
            //                switch (device.Type)
            //                {
            //                    case OpenRGB.NET.Enums.DeviceType.Mouse:
            //                        newDevice.DeviceLayout = 1; //strip type
            //                        break;
            //                    case OpenRGB.NET.Enums.DeviceType.Keyboard:
            //                        newDevice.DeviceLayout = 1; //strip type
            //                        break;
            //                    case OpenRGB.NET.Enums.DeviceType.Headset:
            //                        newDevice.DeviceLayout = 1; //strip type
            //                        break;
            //                    case OpenRGB.NET.Enums.DeviceType.HeadsetStand:
            //                        newDevice.DeviceLayout = 1; //strip type
            //                        break;
            //                    case OpenRGB.NET.Enums.DeviceType.Motherboard:
            //                        newDevice.DeviceLayout = 1; //strip type
            //                        break;
            //                    case OpenRGB.NET.Enums.DeviceType.Dram:
            //                        newDevice.DeviceLayout = 1; //strip type
            //                        break;
            //                    case OpenRGB.NET.Enums.DeviceType.Ledstrip:
            //                        newDevice.DeviceLayout = 1; //strip type
            //                        break;
            //                    case OpenRGB.NET.Enums.DeviceType.Gpu:
            //                        newDevice.DeviceLayout = 1; //strip type
            //                        break;
            //                    case OpenRGB.NET.Enums.DeviceType.Mousemat:
            //                        newDevice.DeviceLayout = 1; //strip type
            //                        break;
            //                }

            //                Cards.Add(newDevice);
            //            }
            //        }
            //    }


            //    if (oldDeviceNum != Cards.Count) //there are changes in device list, we simply restart the application to add process
            //    {
            //        WriteDeviceInfoJson();
            //        Application.Restart();
            //        Process.GetCurrentProcess().Kill();
            //    }
            //}


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
            AvailableLayout = new ObservableCollection<string>
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
            AvailableMenu = new ObservableCollection<string>
{
          "Dashboard",
           "Settings",


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
            AvailablePallete = new ObservableCollection<IColorPaletteCard>();
            foreach (var loadedPalette in LoadPaletteIfExists())
            {
                AvailablePallete.Add(loadedPalette);
            }
            WritePaletteCollectionJson();

            AvailableGradient = new ObservableCollection<IGradientColorCard>();
            foreach (var gradient in LoadGradientIfExists())
            {
                AvailableGradient.Add(gradient);
            }
            AvailableSolidColors = new ObservableCollection<Color>();
            foreach (var color in LoadSolidColorIfExists())
            {
                AvailableSolidColors.Add(color);
            }
            CurrentCustomZoneColor = new ObservableCollection<System.Windows.Media.Color>();
            //var shareMenu = new PaletteCardContextMenu("Share");
            //var shareMenuOptions = new List<PaletteCardContextMenu>();
            //shareMenuOptions.Add(new PaletteCardContextMenu("File Export"));
            //shareMenuOptions.Add(new PaletteCardContextMenu("Ambinity Post"));
            //shareMenu.MenuItem = shareMenuOptions;

            //PaletteCardOptions = new List<PaletteCardContextMenu>();
            //PaletteCardOptions.Add(shareMenu);
            //PaletteCardOptions.Add(new PaletteCardContextMenu("Create new"));
            //PaletteCardOptions.Add(new PaletteCardContextMenu("Import"));

        }
        public List<IColorPaletteCard> LoadPaletteIfExists()
        {
            if (!File.Exists(JsonPaletteFileNameAndPath))
            {
                //create default palette
                var paletteCards = new List<IColorPaletteCard>();
                IColorPaletteCard rainbow = new ColorPaletteCard("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow);
                IColorPaletteCard police = new ColorPaletteCard("Police", "Zooey", "RGBPalette16", "Police Car Light mimic", DefaultColorCollection.police);
                IColorPaletteCard forest = new ColorPaletteCard("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.forest);
                IColorPaletteCard aurora = new ColorPaletteCard("Police", "Zooey", "RGBPalette16", "Police Car Light mimic", DefaultColorCollection.aurora);
                IColorPaletteCard iceandfire = new ColorPaletteCard("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.iceandfire);
                IColorPaletteCard scarlet = new ColorPaletteCard("Police", "Zooey", "RGBPalette16", "Police Car Light mimic", DefaultColorCollection.scarlet);

                paletteCards.Add(rainbow);
                paletteCards.Add(police);
                paletteCards.Add(forest);
                paletteCards.Add(aurora);
                paletteCards.Add(iceandfire);
                paletteCards.Add(scarlet);







                return paletteCards;


            }

            var json = File.ReadAllText(JsonPaletteFileNameAndPath);
            var loadedPaletteCard = new List<IColorPaletteCard>();
            var existPaletteCard = JsonConvert.DeserializeObject<List<ColorPaletteCard>>(json);
            foreach (var paletteCard in existPaletteCard)
            {
                loadedPaletteCard.Add(paletteCard);
            }


            return loadedPaletteCard;
        }
        public Color[] LoadSolidColorIfExists()
        {

            Color[] colors =
            {
        (Color)ColorConverter.ConvertFromString("Red"),
        Color.FromArgb(255, 255, 192, 192),
        Color.FromArgb(255, 255, 224, 192),
        Color.FromArgb(255, 255, 255, 192),
        Color.FromArgb(255, 192, 255, 192),
        Color.FromArgb(255, 192, 255, 255),
        Color.FromArgb(255, 192, 192, 255),
        Color.FromArgb(255, 255, 192, 255),
        Color.FromArgb(255, 224, 224, 224),
        Color.FromArgb(255, 255, 128, 128),
        Color.FromArgb(255, 255, 192, 128),
        Color.FromArgb(255, 255, 255, 128),
        Color.FromArgb(255, 128, 255, 128),
        Color.FromArgb(255, 128, 255, 255),
        Color.FromArgb(255, 128, 128, 255),
        Color.FromArgb(255, 255, 128, 255),
        (Color)ColorConverter.ConvertFromString("Silver"),
        (Color)ColorConverter.ConvertFromString("Red"),
        Color.FromArgb(255, 255, 128, 0),
        (Color)ColorConverter.ConvertFromString("Yellow"),
        (Color)ColorConverter.ConvertFromString("Lime"),
        (Color)ColorConverter.ConvertFromString("Cyan"),
        (Color)ColorConverter.ConvertFromString("Blue"),
        (Color)ColorConverter.ConvertFromString("Fuchsia"),
        (Color)ColorConverter.ConvertFromString("Gray"),
        Color.FromArgb(255, 192, 0, 0),
        Color.FromArgb(255, 192, 64, 0),
        Color.FromArgb(255, 192, 192, 0),
        Color.FromArgb(255, 0, 192, 0),
        Color.FromArgb(255, 0, 192, 192),
        Color.FromArgb(255, 0, 0, 192),
        Color.FromArgb(255, 192, 0, 192),
        Color.FromArgb(255, 64, 64, 64),
        (Color)ColorConverter.ConvertFromString("Maroon"),
        Color.FromArgb(255, 128, 64, 0),
        (Color)ColorConverter.ConvertFromString("Olive"),
        (Color)ColorConverter.ConvertFromString("Green"),
        (Color)ColorConverter.ConvertFromString("Teal"),
        (Color)ColorConverter.ConvertFromString("Navy"),
        (Color)ColorConverter.ConvertFromString("Purple"),
        (Color)ColorConverter.ConvertFromString("Black"),
        Color.FromArgb(255, 64, 0, 0),
        Color.FromArgb(255, 128, 64, 64),
        Color.FromArgb(255, 64, 64, 0),
        Color.FromArgb(255, 0, 64, 0),
        Color.FromArgb(255, 0, 64, 64),
        Color.FromArgb(255, 0, 0, 64),
        Color.FromArgb(255, 64, 0, 64),
    };

            return colors;
        }
        public List<IGradientColorCard> LoadGradientIfExists()
        {
            //if (!File.Exists(JsonPaletteFileNameAndPath))
            //{
            //create default palette
            var gradientCards = new List<IGradientColorCard>();
            IGradientColorCard a = new GradientColorCard("Default", "Zooey", "RGBGradient", "Full Color Spectrum", System.Windows.Media.Color.FromRgb(254, 141, 198), System.Windows.Media.Color.FromRgb(254, 209, 199));
            IGradientColorCard b = new GradientColorCard("Default", "Zooey", "RGBGradient", "Police Car Light mimic", System.Windows.Media.Color.FromRgb(127, 0, 255), System.Windows.Media.Color.FromRgb(255, 0, 255));
            IGradientColorCard c = new GradientColorCard("Default", "Zooey", "RGBGradient", "Full Color Spectrum", System.Windows.Media.Color.FromRgb(251, 176, 64), System.Windows.Media.Color.FromRgb(249, 237, 50));
            IGradientColorCard d = new GradientColorCard("Default", "Zooey", "RGBGradient", "Police Car Light mimic", System.Windows.Media.Color.FromRgb(0, 161, 255), System.Windows.Media.Color.FromRgb(0, 255, 143));
            IGradientColorCard e = new GradientColorCard("Default", "Zooey", "RGBGradient", "Full Color Spectrum", System.Windows.Media.Color.FromRgb(238, 42, 123), System.Windows.Media.Color.FromRgb(255, 125, 184));
            IGradientColorCard f = new GradientColorCard("Default", "Zooey", "RGBGradient", "Police Car Light mimic", System.Windows.Media.Color.FromRgb(255, 0, 212), System.Windows.Media.Color.FromRgb(0, 221, 255));

            gradientCards.Add(a);
            gradientCards.Add(b);
            gradientCards.Add(c);
            gradientCards.Add(d);
            gradientCards.Add(e);
            gradientCards.Add(f);







            return gradientCards;


            //}

            //var json = File.ReadAllText(JsonPaletteFileNameAndPath);
            //var loadedPaletteCard = new List<IColorPaletteCard>();
            //var existPaletteCard = JsonConvert.DeserializeObject<List<ColorPaletteCard>>(json);
            //foreach (var paletteCard in existPaletteCard)
            //{
            //    loadedPaletteCard.Add(paletteCard);
            //}


            return gradientCards;
        }
        public async void ShowAddNewDialog()
        {

            var vm = new ViewModel.AddDeviceViewModel(AvailableDevices, Groups, DesktopFrame);
            var view = new View.AddDevice();
            view.DataContext = vm;
            bool addResult=false;
            if (addResult)
            {
                try
                {
                    switch (vm.AddedItemType)
                    {
                        case AddDeviceViewModel.AvailableTypes.Device:
                            //if (vm.Device.DeviceType != "ABHV2" && vm.Device.DeviceType != "ABFANHUB")
                            //{

                            //vm.Device.DeviceID = AvailableDevices.Count() + 1);

                            AvailableDevices.Add(vm.Device);
                            WriteDeviceInfoJson();

                            //}
                            //else if (vm.Device.DeviceType == "ABFANHUB")
                            //{
                            //    AvailableDevices.Add(vm.Device);//add HUB first
                            //    foreach (var fan in vm.SelectedOutputs)//add child device
                            //    {
                            //        AvailableDevices.Add(fan);
                            //    }
                            //    WriteDeviceInfoJson();
                            //}
                            //else
                            //{


                            //    vm.Device.DeviceID = Cards.Count() + 1;
                            //    vm.Device.IsHUB = true;
                            //    vm.Device.HUBID = Cards.Count() + 1;
                            //    Cards.Add(vm.Device);
                            //    // WriteJson();

                            //    if (vm.ARGB1Selected) // ARGB1 output port is in the list
                            //    {
                            //        var argb1 = new DeviceSettings();
                            //        argb1.DeviceType = "Strip";                           //add to device settings
                            //        argb1.DeviceID = Cards.Count() + 1;
                            //        argb1.SpotsX = 5;
                            //        argb1.SpotsY = 5;
                            //        argb1.NumLED = 16;
                            //        argb1.DeviceName = "ARGB1";
                            //        argb1.ParrentLocation = vm.Device.HUBID;
                            //        argb1.OutputLocation = 0;
                            //        argb1.IsVissible = false;
                            //        argb1.DeviceLayout = 1;
                            //        Cards.Add(argb1);
                            //    }
                            //    if (vm.ARGB2Selected)
                            //    {
                            //        var argb2 = new DeviceSettings();
                            //        argb2.DeviceType = "Matrix";                           //add to device settings
                            //        argb2.DeviceID = Cards.Count() + 1;
                            //        argb2.SpotsX = 20;
                            //        argb2.SpotsY = 6;
                            //        argb2.NumLED = 120;
                            //        argb2.DeviceName = "ARGB2";
                            //        argb2.ParrentLocation = vm.Device.HUBID;
                            //        argb2.OutputLocation = 1;
                            //        argb2.IsVissible = false;
                            //        argb2.DeviceLayout = 2;
                            //        Cards.Add(argb2);
                            //    }
                            //    if (vm.PCI1Selected)
                            //    {
                            //        var PCI = new DeviceSettings();
                            //        PCI.DeviceType = "Square";                           //add to device settings
                            //        PCI.DeviceID = Cards.Count() + 1;
                            //        PCI.SpotsX = 12;
                            //        PCI.SpotsY = 7;
                            //        PCI.NumLED = 34;
                            //        PCI.DeviceName = "PCI1";
                            //        PCI.ParrentLocation = vm.Device.HUBID;
                            //        PCI.OutputLocation = 2;
                            //        PCI.DeviceLayout = 0;
                            //        PCI.IsVissible = false;
                            //        Cards.Add(PCI);
                            //    }
                            //    if (vm.PCI2Selected)
                            //    {
                            //        var PCI = new DeviceSettings();
                            //        PCI.DeviceType = "Square";                           //add to device settings
                            //        PCI.DeviceID = Cards.Count() + 1;
                            //        PCI.SpotsX = 12;
                            //        PCI.SpotsY = 7;
                            //        PCI.NumLED = 34;
                            //        PCI.DeviceName = "PCI2";
                            //        PCI.ParrentLocation = vm.Device.HUBID;
                            //        PCI.OutputLocation = 3;
                            //        PCI.DeviceLayout = 0;
                            //        PCI.IsVissible = false;
                            //        Cards.Add(PCI);
                            //    }
                            //    if (vm.PCI3Selected)
                            //    {
                            //        var PCI = new DeviceSettings();
                            //        PCI.DeviceType = "Square";                           //add to device settings
                            //        PCI.DeviceID = Cards.Count() + 1;
                            //        PCI.SpotsX = 12;
                            //        PCI.SpotsY = 7;
                            //        PCI.NumLED = 34;
                            //        PCI.DeviceName = "PCI3";
                            //        PCI.ParrentLocation = vm.Device.HUBID;
                            //        PCI.OutputLocation = 4;
                            //        PCI.DeviceLayout = 0;
                            //        PCI.IsVissible = false;
                            //        Cards.Add(PCI);
                            //    }
                            //    if (vm.PCI4Selected)
                            //    {
                            //        var PCI = new DeviceSettings();
                            //        PCI.DeviceType = "Strip";                           //add to device settings
                            //        PCI.DeviceID = Cards.Count() + 1;
                            //        PCI.SpotsX = 12;
                            //        PCI.SpotsY = 7;
                            //        PCI.NumLED = 34;
                            //        PCI.DeviceName = "PCI4";
                            //        PCI.ParrentLocation = vm.Device.HUBID;
                            //        PCI.OutputLocation = 5;
                            //        PCI.DeviceLayout = 0;
                            //        PCI.IsVissible = false;
                            //        Cards.Add(PCI);
                            //    }


                            //    WriteDeviceInfoJson();
                            //    // _isAddnew = false;
                            //}
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
                System.Windows.Forms.Application.Restart();
                Process.GetCurrentProcess().Kill();
            }


        }





        public async void ShowDeleteDialog()
        {
            var view = new View.DeleteMessageDialog();
            DeleteMessageDialogViewModel dialogViewModel = new DeleteMessageDialogViewModel(CurrentDevice);
            view.DataContext = dialogViewModel;
            bool addResult = false;
            if (addResult)
            {
                DeleteCard(CurrentDevice);
                int counter = 1;
                foreach (var card in AvailableDevices)
                {

                    card.DeviceID = counter;
                    counter++;

                }
                WriteDeviceInfoJson();
                System.Windows.Forms.Application.Restart();
                Process.GetCurrentProcess().Kill();
            }


        }
        public async void ShowIncreamentDataDialog()
        {
            //UserIncreamentInputViewModel dialogViewModel = new UserIncreamentInputViewModel(PreviewSpots);
            //var view = new View.UserIncreamentInput();
            //view.DataContext = dialogViewModel;
            //bool UserResult = (bool)await DialogHost.Show(view, "mainDialog");
            //if (UserResult)
            //{
            //    var startIndex = dialogViewModel.StartIndex;
            //    var spacing = dialogViewModel.Spacing;
            //    var startPoint = dialogViewModel.StartPoint;
            //    var endPoint = dialogViewModel.EndPoint;
            //    SetIncreament(startIndex, spacing, startPoint, endPoint);
            //    //apply increament function
            //}


        }
        public async void ShowZeroingDialog()
        {
            //UserIncreamentInputViewModel dialogViewModel = new UserIncreamentInputViewModel(PreviewSpots);
            //var view = new View.UserNumberInput();
            //view.DataContext = dialogViewModel;
            //bool UserResult = (bool)await DialogHost.Show(view, "mainDialog");
            //if (UserResult)
            //{
            //    var spreadNumber = dialogViewModel.SpreadNumber;

            //    SetZerotoAll(spreadNumber);
            //    //apply increament function
            //}


        }
        public async void ShowAdjustPositon()
        {
            //var view = new View.DeviceRectPositon();
            //var allDevices = AvailableDevices.ToArray();
            ////AdjustPostionViewModel dialogViewModel = new AdjustPostionViewModel(DeviceRectX, DeviceRectY, DeviceRectWidth, DeviceRectHeight, allDevices, ShaderBitmap, CurrentOutput.OutputSelectedMode);
            //view.DataContext = dialogViewModel;
            //bool dialogResult = (bool)await DialogHost.Show(view, "mainDialog");
            //if (dialogResult)
            //{   //save current device rect position to json database
            //    DeviceRectX = dialogViewModel.DeviceRectX / 4;
            //    DeviceRectY = dialogViewModel.DeviceRectY / 4;
            //    //DeviceRectX = CurrentDevice.DeviceRectLeft;
            //    //DeviceRectY = CurrentDevice.DeviceRectTop;






            //    //RaisePropertyChanged(() => CurrentDevice.DeviceRectLeft);
            //    //RaisePropertyChanged(() => CurrentDevice.DeviceRectTop);
            //    RaisePropertyChanged(() => DeviceRectX);
            //    RaisePropertyChanged(() => DeviceRectY);
            //    //RaisePropertyChanged(() => DeviceRectWidthMax);
            //    //RaisePropertyChanged(() => DeviceRectHeightMax);

            //}



        }
        public async void ShowDeleteGroupFromDashboard(IGroupSettings group)
        {
        }
        public async void ShowDeleteFromDashboard(IDeviceSettings device)
        {
            //var view = new View.DeleteMessageDialog();
            //DeleteMessageDialogViewModel dialogViewModel = new DeleteMessageDialogViewModel(device);
            //view.DataContext = dialogViewModel;
            //bool dialogResult = (bool)await DialogHost.Show(view, "mainDialog");
            //if (dialogResult)
            //{
            //    //DeleteCard(device);
            //    //int counter = 1;
            //    //foreach (var card in AvailableDevices)
            //    //{
            //    //    card.DeviceID = counter;
            //    //    counter++;

            //    //}
            //    //WriteDeviceInfoJson();
            //    //Application.Restart();
            //    //Process.GetCurrentProcess().Kill();
            //}


        }



        public void DeleteCard(IDeviceSettings device)
        {

            AvailableDevices.Remove(device);
            WriteDeviceInfoJson();
        }

        public void WriteGroupInfoJson()
        {

            var groups = new List<IGroupSettings>();


            foreach (var group in Groups)
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
            foreach (var item in AvailableDevices)
            {
                if (!item.IsDummy)
                    devices.Add(item);
            }

            var json = JsonConvert.SerializeObject(devices, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonDeviceFileNameAndPath, json);

        }
        public void WritePaletteCollectionJson()
        {

            var palettes = new List<IColorPaletteCard>();
            foreach (var palette in AvailablePallete)
            {
                palettes.Add(palette);
            }
            var json = JsonConvert.SerializeObject(palettes, Formatting.Indented);
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonPaletteFileNameAndPath, json);

        }
        public void ExportCurrentSpotData()
        {
            SaveFileDialog Export = new SaveFileDialog();
            Export.CreatePrompt = true;
            Export.OverwritePrompt = true;

            Export.Title = "Xuất dữ liệu";
            Export.FileName = CurrentDevice.DeviceName + " Spot Data";
            Export.CheckFileExists = false;
            Export.CheckPathExists = true;
            Export.DefaultExt = "vid";
            Export.Filter = "All files (*.*)|*.*";
            Export.InitialDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Export.RestoreDirectory = true;


            string[] spotData = new string[256];
            for (int i = 0; i < PreviewSpots.Length; i++)
            {
                //spotData[i] = CurrentDevice.VirtualIndex[i].ToString();
            }
            if (Export.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllLines(Export.FileName, spotData);

            }
        }
        public void ImportCurrentSpotData()
        {
            OpenFileDialog Import = new OpenFileDialog();
            Import.Title = "Chọn vid file";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "vid";
            Import.Filter = "Text files (*.vid)|*.vid";
            Import.FilterIndex = 2;


            Import.ShowDialog();


            if (!string.IsNullOrEmpty(Import.FileName) && File.Exists(Import.FileName))
            {


                var lines = File.ReadAllLines(Import.FileName);


                try
                {
                    for (int i = 0; i < PreviewSpots.Length; i++)
                    {
                        PreviewSpots[i].SetVID(int.Parse(lines[i]));
                    }
                    CurrentSpotSetVIDChanged();
                }
                catch (FormatException)
                {
                    HandyControl.Controls.MessageBox.Show("Corrupted vid data File!!!");
                }

            }
        }
        public void ImportPaletteCardFromFile()
        {
            OpenFileDialog Import = new OpenFileDialog();
            Import.Title = "Chọn col file";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "col";
            Import.Filter = "Text files (*.col)|*.col";
            Import.FilterIndex = 2;


            Import.ShowDialog();


            if (!string.IsNullOrEmpty(Import.FileName) && File.Exists(Import.FileName))
            {


                var lines = File.ReadAllLines(Import.FileName);
                if (lines.Length < 19)
                {
                    HandyControl.Controls.MessageBox.Show("Corrupted Color Palette data File!!!");
                    return;
                }

                var name = lines[0];
                var owner = lines[1];
                var description = lines[2];
                var color = new System.Windows.Media.Color[16];
                try
                {
                    for (int i = 0; i < color.Length; i++)
                    {
                        color[i] = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(lines[i + 3]);
                    }

                    IColorPaletteCard importedPaletteCard = new ColorPaletteCard(name, owner, "Imported from local file", description, color);
                    AvailablePallete.Add(importedPaletteCard);
                    RaisePropertyChanged(nameof(AvailablePallete));
                    WritePaletteCollectionJson();

                }
                catch (FormatException)
                {
                    HandyControl.Controls.MessageBox.Show("Corrupted Color Palette data File!!!");
                }



            }
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
        //VIDs function
        public void SetZerotoAll(int number)
        {
            foreach (var spot in PreviewSpots)
            {
                spot.SetVID(number);

            }
            CurrentSpotSetVIDChanged();
        }
        public void SetIncreament(int startIndex, int spacing, int startPoint, int endPoint)
        {

            int counter = startPoint;
            for (var i = startIndex; i <= startIndex + (endPoint - startPoint) * spacing; i += spacing)
            {
                PreviewSpots[counter++].SetVID(i);

            }



            CurrentSpotSetVIDChanged();
        }
        public void GotoChild(IDeviceSettings selectedDevice)
        {

            SelectedVerticalMenuItem = MenuItems.FirstOrDefault(t => t.Text == lighting);
            IsDashboardType = false;
            CurrentDevice = selectedDevice;
            if (CurrentDevice.SelectedOutput >= 0)
                CurrentOutput = CurrentDevice.AvailableOutputs[CurrentDevice.SelectedOutput];
            else
            {
                CurrentDevice.SelectedOutput = 0;
                CurrentOutput = CurrentDevice.AvailableOutputs[CurrentDevice.SelectedOutput];
            }
            CurrentLEDSetup = CurrentOutput.OutputLEDSetup;
            RaisePropertyChanged(nameof(CurrentDevice.SelectedOutput));
            IsSplitLightingWindowOpen = true;
            IsCanvasLightingWindowOpen = false;
            //RaisePropertyChanged(() => CurrentDeviceSelectedEffect);
            RaisePropertyChanged(() => DeviceRectX);
            RaisePropertyChanged(() => DeviceRectY);
            RaisePropertyChanged(() => DeviceRectWidth);
            RaisePropertyChanged(() => DeviceRectHeight);
            WriteDeviceInfoJson();
            //if (CurrentDevice.IsHUB)
            //{
            //    ParrentLocation = CurrentDevice.HUBID;
            //    var childList = new List<IDeviceSettings>();
            //    HUBOutputCollection = new ObservableCollection<IDeviceSettings>();
            //    // HUBOutputCollection.Add(CurrentDevice);
            //    DeviceLightingModeCollection = true;

            //    //only HUB object has ability to sync it's child device
            //    RaisePropertyChanged(() => DeviceLightingModeCollection);


            //    foreach (var device in Cards)
            //    {
            //        if (device.ParrentLocation == CurrentDevice.HUBID)
            //        {
            //            childList.Add(device);
            //            HUBOutputCollection.Add(device);
            //        }

            //    }

            //    int counter = 0;
            //    foreach (var child in childList)
            //    {
            //        if (child.IsNavigationSelected)
            //        {
            //            counter++;
            //            CurrentDevice = child;
            //        }

            //    }
            //    if (counter == 0)
            //    {
            //        CurrentDevice = childList[0];
            //        CurrentDevice.IsNavigationSelected = true;
            //    }




            //}
            //else
            //{
            //    if (CurrentDevice.ParrentLocation != 151293)
            //    {
            //        DeviceLightingModeCollection = false;
            //        RaisePropertyChanged(() => DeviceLightingModeCollection);
            //        //keep current HUBOutputCollection
            //    }
            //    else
            //    {
            //        if (HUBOutputCollection != null)
            //            HUBOutputCollection.Clear();

            //        DeviceLightingModeCollection = false;
            //        RaisePropertyChanged(() => DeviceLightingModeCollection);
            //        //Clear current HUBOutputCollection cuz this is a single output device
            //    }

            //}
            CurrentDevice.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(CurrentDevice.SelectedOutput):
                        if (CurrentDevice.SelectedOutput >= 0)
                        {
                            CurrentOutput = CurrentDevice.AvailableOutputs[CurrentDevice.SelectedOutput];
                            RaisePropertyChanged(nameof(CurrentOutput));
                        }
                        else
                        {
                            CurrentOutput = CurrentDevice.AvailableOutputs[0];
                        }

                        break;
                    case nameof(CurrentDevice):

                        CurrentOutput = CurrentDevice.AvailableOutputs[0];
                        CurrentDevice.SelectedOutput = 0;
                        RaisePropertyChanged(nameof(CurrentOutput));


                        break;

                  






                }
            };
            CurrentOutput.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(CurrentOutput.OutputNumLEDX):
                    case nameof(CurrentOutput.OutputNumLEDY):
                        if (CurrentOutput.OutputNumLEDX> CurrentOutput.OutputNumLEDY)
                        {
                            CurrentOutput.OutputPixelHeight = (int)ShaderBitmap.Width * CurrentOutput.OutputNumLEDY / CurrentOutput.OutputNumLEDX;
                            CurrentOutput.OutputPixelWidth = (int)ShaderBitmap.Width;
                        }
                       
                        else
                        {
                            CurrentOutput.OutputPixelWidth = (int)ShaderBitmap.Height * CurrentOutput.OutputNumLEDX / CurrentOutput.OutputNumLEDY;
                            CurrentOutput.OutputPixelHeight = (int)ShaderBitmap.Height;
                        }
                        
                        RaisePropertyChanged(nameof(CurrentOutput.OutputPixelWidth));
                        RaisePropertyChanged(nameof(CurrentOutput.OutputPixelHeight));
                        break;
                    
                        



                }
            };


            //switch (CurrentDevice.SelectedEffect)
            //{
            //    case 1:
            //        //CurrentActivePaletteCard = AvailablePallete.ElementAt(CurrentDevice.SelectedPalette);
            //        RaisePropertyChanged(() => CurrentActivePaletteCard);
            //        break;
            //    case 3:
            //        //CurrentActivePaletteCard = AvailablePallete.ElementAt(CurrentDevice.SelectedMusicPalette);
            //        RaisePropertyChanged(() => CurrentActivePaletteCard);
            //        break;
            //}



            RaisePropertyChanged(() => DeviceRectHeightMax);
            RaisePropertyChanged(() => DeviceRectWidthMax);
            //CurrentDevice.PropertyChanged += (s, e) =>
            //{
            //    switch (e.PropertyName)
            //    {
            //        case nameof(CurrentDevice.SelectedEffect):
            //            switch (CurrentDevice.SelectedEffect)
            //            {
            //                case 1:
            //                    CurrentActivePaletteCard = AvailablePallete.ElementAt(CurrentDevice.SelectedPalette);
            //                    RaisePropertyChanged(() => CurrentActivePaletteCard);
            //                    break;
            //                case 3:
            //                    CurrentActivePaletteCard = AvailablePallete.ElementAt(CurrentDevice.SelectedMusicPalette);
            //                    RaisePropertyChanged(() => CurrentActivePaletteCard);
            //                    break;
            //            }
            //            RaisePropertyChanged(() => CurrentActivePaletteCard);

            //            break;
            //        case nameof(CurrentDevice.SelectedPalette):
            //            CurrentActivePaletteCard = AvailablePallete.ElementAt(CurrentDevice.SelectedPalette);
            //            RaisePropertyChanged(() => CurrentActivePaletteCard);

            //            break;
            //        case nameof(CurrentDevice.SelectedMusicPalette):
            //            CurrentActivePaletteCard = AvailablePallete.ElementAt(CurrentDevice.SelectedMusicPalette);
            //            RaisePropertyChanged(() => CurrentActivePaletteCard);

            //            break;
            //    }
            //};
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
            //Cards.Remove(device);
            //IsDashboardType = true;
            //SelectedVerticalMenuItem = MenuItems.FirstOrDefault();
            //SetMenuItemActiveStatus(dashboard);
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
