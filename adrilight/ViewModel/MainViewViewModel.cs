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
using adrilight.Settings;
using System.Reflection;
using NonInvasiveKeyboardHookLibrary;
using static adrilight.View.AddNewDeviceWindow;
using System.Management;
using Microsoft.Win32;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using System.Threading.Tasks;
using System.Threading;

namespace adrilight.ViewModel
{
    public class MainViewViewModel : BaseViewModel
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");

        private string JsonDeviceFileNameAndPath => Path.Combine(JsonPath, "adrilight-deviceInfos.json");
        private string JsonSolidColorFileNameAndPath => Path.Combine(JsonPath, "adrilight-solidColor.json");
        private string JsonDeviceProfileFileNameAndPath => Path.Combine(JsonPath, "adrilight-deviceProfiles.json");
        private string JsonAutomationFileNameAndPath => Path.Combine(JsonPath, "adrilight-automations.json");
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
        private ObservableCollection<int> _availableBaudrates;
        public ObservableCollection<int> AvailableBaudrates {
            get
            {
                return _availableBaudrates;
            }

            set
            {
                _availableBaudrates = value;
                RaisePropertyChanged(nameof(AvailableBaudrates));
            }
        }
        private List<IActionParameter> _availableParametersforCurrentAction;
        public List<IActionParameter> AvailableParametersforCurrentAction {
            get
            {
                return _availableParametersforCurrentAction;
            }

            set
            {
                _availableParametersforCurrentAction = value;
                RaisePropertyChanged();
            }
        }
        private List<IActionParameter> _allAvailableParametersforCurrentAction;
        public List<IActionParameter> AllAvailableParametersforCurrentAction {
            get
            {
                return _allAvailableParametersforCurrentAction;
            }

            set
            {
                _allAvailableParametersforCurrentAction = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<string> _availableActionsforCurrentDevice;
        public ObservableCollection<string> AvailableActionsforCurrentDevice {
            get
            {
                return _availableActionsforCurrentDevice;
            }

            set
            {
                _availableActionsforCurrentDevice = value;
               
                RaisePropertyChanged();
            }
        }

        private IActionSettings _currentSelectedAction;
        public IActionSettings CurrentSelectedAction {
            get
            {
                return _currentSelectedAction;
            }

            set
            {
                _currentSelectedAction = value;
            }
        }

        private IActionParameter _selectedParameter;
        public IActionParameter SelectedParameter {
            get { return _selectedParameter; }
            set { _selectedParameter = value; }
        }
        private string _selectedActionType;
        public string SelectedActionType {
            get
            {
                return _selectedActionType;
            }

            set
            {
                _selectedActionType = value;
               
                switch (value)
                {
                    case "Activate Profile":

                        AvailableParametersforCurrentAction = AllAvailableParametersforCurrentAction.Where(x => x.Type == "profile").ToList();
                        
                        break;
                    case "Brightness Control":

                        AvailableParametersforCurrentAction = AllAvailableParametersforCurrentAction.Where(x => x.Type == "brightness").ToList();
                        break;
                }
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
                //if (_currentOutput == value) return;
                //if (_currentOutput != null) _currentOutput.PropertyChanged -= _currentDevice_PropertyChanged;
                _currentOutput = value;
                //if (_currentOutput != null) _currentOutput.PropertyChanged += _currentDevice_PropertyChanged;
                _currentOutput.PropertyChanged += (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(CurrentOutput.OutputNumLEDX):
                        case nameof(CurrentOutput.OutputNumLEDY):
                            if (CurrentOutput.OutputNumLEDX > CurrentOutput.OutputNumLEDY)
                            {
                                //CurrentOutput.OutputRectangle.Height = (int)ShaderBitmap.Width * CurrentOutput.OutputNumLEDY / CurrentOutput.OutputNumLEDX;
                                //CurrentOutput.OutputRectangle.Width = (int)ShaderBitmap.Width;
                            }

                            else
                            {
                                //CurrentOutput.OutputRectangle.Width = (int)ShaderBitmap.Height * CurrentOutput.OutputNumLEDX / CurrentOutput.OutputNumLEDY;
                                //CurrentOutput.OutputRectangle.Height = (int)ShaderBitmap.Height;
                            }

                            //RaisePropertyChanged(nameof(CurrentOutput.OutputPixelWidth));
                            //RaisePropertyChanged(nameof(CurrentOutput.OutputPixelHeight));
                            break;

                        case nameof(CurrentOutput.OutputScreenCapturePositionIndex):
                            switch (CurrentOutput.OutputScreenCapturePositionIndex)
                            {
                                case 0://full screen

                                    CurrentOutput.OutputRectangle = new Rectangle(0, 0, (int)ShaderBitmap.Width, (int)ShaderBitmap.Height);

                                    break;
                                case 1: // left

                                    CurrentOutput.OutputRectangle = new Rectangle(0, 0, (int)ShaderBitmap.Width / 2, (int)ShaderBitmap.Height);

                                    break;
                                case 2: // left

                                    CurrentOutput.OutputRectangle = new Rectangle((int)ShaderBitmap.Width / 2, 0, (int)ShaderBitmap.Width / 2, (int)ShaderBitmap.Height);

                                    break;
                                case 3: // top

                                    CurrentOutput.OutputRectangle = new Rectangle(0, 0, (int)ShaderBitmap.Width, (int)ShaderBitmap.Height / 4);

                                    break;
                                case 4: // bottom

                                    CurrentOutput.OutputRectangle = new Rectangle(0, (int)ShaderBitmap.Height * 3 / 4, (int)ShaderBitmap.Width, (int)ShaderBitmap.Height / 4);

                                    break;
                            }
                            RaisePropertyChanged(nameof(CurrentOutput.OutputRectangle));
                            break;
                        case nameof(CurrentOutput.OutputMusicSensitivity):
                            SensitivityThickness = new Thickness(0, 0, 0, CurrentOutput.OutputMusicSensitivity + 15);
                            RaisePropertyChanged(nameof(SensitivityThickness));
                            break;
                        case nameof(CurrentOutput.OutputMusicVisualizerFreq):
                            if (CurrentOutput.OutputMusicVisualizerFreq == 0) // bass configuration
                            {
                                foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                                {
                                    spot.SetMID(2);
                                }
                            }
                            else if (CurrentOutput.OutputMusicVisualizerFreq == 1)// mid configuration
                            {
                                foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                                {
                                    spot.SetMID(10);
                                }
                            }
                            else if (CurrentOutput.OutputMusicVisualizerFreq == 2)// treble configuration
                            {
                                foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                                {
                                    spot.SetMID(20);
                                }
                            }
                            else if (CurrentOutput.OutputMusicVisualizerFreq == 3)// Full range configuration
                            {
                                foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                                {
                                    spot.SetMID(spot.id);
                                }
                            }
                            RaisePropertyChanged(nameof(SensitivityThickness));
                            break;


                    }
                    WriteDeviceInfoJson();
                };
                RaisePropertyChanged(nameof(CurrentOutput));



            }
        }

        private void _currentDevice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

            if (!IsDashboardType)
            {

                WriteDeviceInfoJson();
            }
            IsSettingsUnsaved = BadgeStatus.Processing;
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
        public ICommand OpenFFTPickerWindowCommand { get; set; }
        public ICommand ScanSerialDeviceCommand { get; set; }
        public ICommand SaveAllAutomationCommand { get; set; }
        public ICommand OpenAddNewAutomationCommand { get; set; }
        public ICommand OpenActionsManagerWindowCommand { get; set; }
        public ICommand OpenActionsEditWindowCommand { get; set; }
        public ICommand OpenAutomationManagerWindowCommand { get; set; }
        public ICommand SaveCurrentSelectedAutomationCommand { get; set; }
        public ICommand AddSelectedWLEDDevicesCommand { get; set; }
        public ICommand CoppyColorCodeCommand { get; set; }
        public ICommand DeleteSelectedSolidColorCommand { get; set; }
        public ICommand DeleteSelectedAutomationCommand { get; set; }
        public ICommand AddNewGradientCommand { get; set; }
        public ICommand ApplyCurrentOutputCapturingPositionCommand { get; set; }
        public ICommand LaunchWBAdjustWindowCommand { get; set; }
        public ICommand ExportCurrentProfileCommand { get; set; }
        public ICommand ImportProfileCommand { get; set; }
        public ICommand SaveCurrentProfileCommand { get; set; }
        public ICommand SaveCurrentSelectedActionCommand { get; set; }
        public ICommand CreateNewProfileCommand { get; set; }
        public ICommand OpenProfileCreateCommand { get; set; }
        public ICommand ExportPIDCommand { get; set; }
        public ICommand ImportPIDCommand { get; set; }
        public ICommand OpenDeviceConnectionSettingsWindowCommand { get; set; }
        public ICommand OpenDeviceFirmwareSettingsWindowCommand { get; set; }
        public ICommand RenameSelectedItemCommand { get; set; }
        public ICommand OpenAdvanceSettingWindowCommand { get; set; }
        public ICommand CancelEditWizardCommand { get; set; }
        public ICommand ShowNameEditWindow { get; set; }
        public ICommand SetBorderSpotActiveCommand { get; set; }
        public ICommand SaveNewUserEditLEDSetup { get; set; }
        public ICommand ResetCountCommand { get; set; }
        public ICommand SetSpotPIDCommand { get; set; }
        public ICommand ResetMaxCountCommand { get; set; }
        public ICommand SetPIDNeutral { get; set; }
        public ICommand SetCurrentSelectedVID { get; set; }
        public ICommand SetCurrentSelectedVIDRange { get; set; }
        public ICommand SetPIDReverseNeutral { get; set; }
        public ICommand JumpToNextWizardStateCommand { get; set; }
        public ICommand JumpToNextAddDeviceWizardStateCommand { get; set; }
        public ICommand AddCurrentSelectedDeviceToDashboard { get; set; }
        public ICommand BackToPreviousAddDeviceWizardStateCommand { get; set; }
        public ICommand BackToPreviousWizardStateCommand { get; set; }
        public ICommand LaunchPositionEditWindowCommand { get; set; }
        public ICommand LaunchPIDEditWindowCommand { get; set; }
        public ICommand LaunchVIDEditWindowCommand { get; set; }
        public ICommand LaunchMIDEditWindowCommand { get; set; }
        public ICommand EditSelectedPaletteCommand { get; set; }
        public ICommand AddNewSolidColorCommand { get; set; }
        public ICommand AddPickedSolidColorCommand { get; set; }
        public ICommand ImportPaletteCardFromFileCommand { get; set; }
        public ICommand ExportCurrentSelectedPaletteToFileCommand { get; set; }
        public ICommand EditSelectedPaletteSaveConfirmCommand { get; set; }
        public ICommand DeleteSelectedPaletteCommand { get; set; }
        public ICommand CreateNewPaletteCommand { get; set; }
        public ICommand CreateNewAutomationCommand { get; set; }
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
        private ObservableCollection<WLEDDevice> _availableWLEDDevices;
        public ObservableCollection<WLEDDevice> AvailableWLEDDevices {
            get { return _availableWLEDDevices; }
            set
            {
                if (_availableWLEDDevices == value) return;
                _availableWLEDDevices = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<IDeviceSettings> _availableSerialDevices;
        public ObservableCollection<IDeviceSettings> AvailableSerialDevices {
            get { return _availableSerialDevices; }
            set
            {
                if (_availableSerialDevices == value) return;
                _availableSerialDevices = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<WLEDDevice> _selectedWLEDDevice;
        public ObservableCollection<WLEDDevice> SelectedWLEDDevice {
            get { return _selectedWLEDDevice; }
            set { _selectedWLEDDevice = value; }
        }
        private ObservableCollection<IDeviceProfile> _availableProfiles;
        public ObservableCollection<IDeviceProfile> AvailableProfiles {
            get { return _availableProfiles; }
            set
            {
                if (_availableProfiles == value) return;
                _availableProfiles = value;

                RaisePropertyChanged();
            }
        }

        private ObservableCollection<IAutomationSettings> _availableAutomations;
        public ObservableCollection<IAutomationSettings> AvailableAutomations {
            get { return _availableAutomations; }
            set
            {
                if (_availableAutomations == value) return;
                _availableAutomations = value;

                RaisePropertyChanged();
            }
        }
        private ObservableCollection<IDeviceProfile> _availableProfilesForCurrentDevice;
        public ObservableCollection<IDeviceProfile> AvailableProfilesForCurrentDevice {
            get { return _availableProfilesForCurrentDevice; }
            set
            {
                if (_availableProfilesForCurrentDevice == value) return;
                _availableProfilesForCurrentDevice = value;
                RaisePropertyChanged();
            }
        }
        private IDeviceProfile _currentSelectedProfile;
        public IDeviceProfile CurrentSelectedProfile {
            get { return _currentSelectedProfile; }
            set
            {
                if (value != null)
                {
                    _currentSelectedProfile = value;
                    LoadProfile();
                    RaisePropertyChanged();
                }



                //RaisePropertyChanged(nameof(CurrentDevice.AvailableOutputs));

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
        private ObservableCollection<IColorPalette> _availablePallete;
        public ObservableCollection<IColorPalette> AvailablePallete {
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
        private ObservableCollection<IModifiersType> _availableModifiers;
        public ObservableCollection<IModifiersType> AvailableModifiers {
            get { return _availableModifiers; }
            set
            {
                if (_availableModifiers == value) return;
                _availableModifiers = value;
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
        private IDeviceSpot[] _previewSpots;
        public IDeviceSpot[] PreviewSpots {
            get => _previewSpots;
            set
            {
                _previewSpots = value;
                RaisePropertyChanged();
            }
        }
        private ILEDSetup _currentLEDSetup;
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
        private IVisualizerProgressBar[] _visualizerFFT;
        public IVisualizerProgressBar[] VisualizerFFT {
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
        private string _setIDMode;
        public string SetIDMode {
            get => _setIDMode;
            set
            {
                _setIDMode = value;
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

        private ObservableCollection<IDeviceCatergory> _availableDeviceCatergoryToAdd;

        public ObservableCollection<IDeviceCatergory> AvailableDeviceCatergoryToAdd { get => _availableDeviceCatergoryToAdd; set => Set(ref _availableDeviceCatergoryToAdd, value); }
        private IDeviceCatergory _currentSelectedCatergoryToAdd;

        public IDeviceCatergory CurrentSelectedCatergoryToAdd { get => _currentSelectedCatergoryToAdd; set => Set(ref _currentSelectedCatergoryToAdd, value); }

        private IDeviceSettings _currentSelectedDeviceToAdd;

        public IDeviceSettings CurrentSelectedDeviceToAdd { get => _currentSelectedDeviceToAdd; set => Set(ref _currentSelectedDeviceToAdd, value); }


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
            var keyboardHookManager = new KeyboardHookManager();
            AvailableModifiers = new ObservableCollection<IModifiersType> { new ModifiersType { Name = "CTRL", ModifierKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.Control, IsChecked = false }, new ModifiersType { Name = "SHIFT", ModifierKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.Shift, IsChecked = false }, new ModifiersType { Name = "ALT", ModifierKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.Alt, IsChecked = false } };
            //keyboardHookManager.Start();

            //// Register virtual key code 0x60 = NumPad0
            //keyboardHookManager.RegisterHotkey(0x78, () =>
            //{
            //    Debug.WriteLine("NumPad0 detected");
            //});
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
            AvailableAutomations = new ObservableCollection<IAutomationSettings>();
            foreach (var automation in LoadAutomationIfExist())
            {
                AvailableAutomations.Add(automation);
            }
            WriteAutomationCollectionJson();

            //register hotkey from loaded automation//
            HotKeyManager.Instance.Start();
            Register();

            //dummy device acts as add new button
            var addNewButton = new DeviceSettings {
                IsDummy = true
            };
            AvailableDevices.Add(addNewButton);

            

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
   
        public void ShaderImageUpdate(byte[] frame, int frameWidth, int frameHeight)
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
                    var MatrixBitmap = new WriteableBitmap(frameWidth, frameHeight, 96, 96, PixelFormats.Bgra32, null);
                    MatrixBitmap.Lock();
                    IntPtr pixelAddress = MatrixBitmap.BackBuffer;
                    Marshal.Copy(CurrentFrame, 0, pixelAddress, CurrentFrame.Length);

                    MatrixBitmap.AddDirtyRect(new Int32Rect(0, 0, frameWidth, frameHeight));

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
        private int _currentAddDeviceWizardState = 0;
        public int CurrentAddDeviceWizardState {
            get => _currentAddDeviceWizardState;
            set
            {
                // _log.Info("PreviewImageSource created.");
                Set(ref _currentAddDeviceWizardState, value);
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
        //#region StaticColor Dependency

        //public bool IsStaticColorGradientChecked {
        //    get => CurrentOutput.OutputStaticColorMode=="gradient";
        //    set
        //    {

        //        CurrentOutput.OutputStaticColorMode = "gradient";
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputStaticColorMode));

        //    }
        //}

        //public bool IsStaticColorBreathingChecked {
        //    get => CurrentOutput.OutputStaticColorMode == "breathing";
        //    set
        //    {

        //        CurrentOutput.OutputStaticColorMode = "breathing";
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputStaticColorMode));



        //    }
        //}



        //public bool IsStaticColorSpectrumCyclingChecked {
        //    get => CurrentOutput.OutputStaticColorMode == "spectrumcycling";
        //    set
        //    {
        //        CurrentOutput.OutputStaticColorMode = "spectrumcycling";
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputStaticColorMode));

        //    }
        //}

        //public bool IsStaticColorSolidChecked {
        //    get => CurrentOutput.OutputStaticColorMode == "solid";
        //    set
        //    {
        //        CurrentOutput.OutputStaticColorMode = "solid";
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputStaticColorMode));



        //    }
        //}


        //public bool IsStaticColorGradientModeFullCycleChecked {
        //    get => CurrentOutput.OutputStaticColorGradientMode == "full";
        //    set
        //    {
        //        CurrentOutput.OutputStaticColorGradientMode = "full";
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputStaticColorGradientMode));

        //    }
        //}

        //public bool IsStaticColorGradientModeFirstChecked {
        //    get => CurrentOutput.OutputStaticColorGradientMode == "first";
        //    set
        //    {
        //        CurrentOutput.OutputStaticColorGradientMode = "first";
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputStaticColorGradientMode));

        //    }
        //}

        //public bool IsStaticColorGradientModeLastChecked {
        //    get => CurrentOutput.OutputStaticColorGradientMode == "last";
        //    set
        //    {
        //        CurrentOutput.OutputStaticColorGradientMode = "last";
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputStaticColorGradientMode));

        //    }
        //}

        //public bool IsStaticColorGradientModeCustomChecked {
        //    get => CurrentOutput.OutputStaticColorGradientMode == "custom";
        //    set
        //    {

        //        CurrentOutput.OutputStaticColorGradientMode = "custom";
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputStaticColorGradientMode));

        //    }
        //}
        //#endregion
        //#region Screen Capture Dependency

        //public bool IsRightScreenRegionChecked {
        //    get => CurrentOutput.OutputScreenCapturePosition == "right";
        //    set
        //    {
        //        CurrentOutput.OutputScreenCapturePosition = "right";
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCapturePosition));
        //        RaisePropertyChanged(() => IsRightScreenRegionChecked);
        //        RaisePropertyChanged(() => IsLeftScreenRegionChecked);
        //        RaisePropertyChanged(() => IsTopScreenRegionChecked);
        //        RaisePropertyChanged(() => IsBottomScreenRegionChecked);
        //        RaisePropertyChanged(() => IsCustomScreenRegionChecked);
        //        RaisePropertyChanged(() => IsFullScreenRegionChecked);
        //    }
        //}

        //public bool IsLeftScreenRegionChecked {
        //    get => CurrentOutput.OutputScreenCapturePosition == "left";
        //    set
        //    {
        //        CurrentOutput.OutputScreenCapturePosition = "left";
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCapturePosition));
        //        RaisePropertyChanged(() => IsRightScreenRegionChecked);
        //        RaisePropertyChanged(() => IsLeftScreenRegionChecked);
        //        RaisePropertyChanged(() => IsTopScreenRegionChecked);
        //        RaisePropertyChanged(() => IsBottomScreenRegionChecked);
        //        RaisePropertyChanged(() => IsCustomScreenRegionChecked);
        //        RaisePropertyChanged(() => IsFullScreenRegionChecked);
        //    }
        //}

        //public bool IsTopScreenRegionChecked {
        //    get => CurrentOutput.OutputScreenCapturePosition == "top";
        //    set
        //    {
        //        CurrentOutput.OutputScreenCapturePosition = "top";
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCapturePosition));
        //        RaisePropertyChanged(() => IsRightScreenRegionChecked);
        //        RaisePropertyChanged(() => IsLeftScreenRegionChecked);
        //        RaisePropertyChanged(() => IsTopScreenRegionChecked);
        //        RaisePropertyChanged(() => IsBottomScreenRegionChecked);
        //        RaisePropertyChanged(() => IsCustomScreenRegionChecked);
        //        RaisePropertyChanged(() => IsFullScreenRegionChecked);
        //    }
        //}

        //public bool IsBottomScreenRegionChecked {
        //    get => CurrentOutput.OutputScreenCapturePosition == "bot";
        //    set
        //    {
        //        CurrentOutput.OutputScreenCapturePosition = "bot";
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCapturePosition));
        //        RaisePropertyChanged(() => IsRightScreenRegionChecked);
        //        RaisePropertyChanged(() => IsLeftScreenRegionChecked);
        //        RaisePropertyChanged(() => IsTopScreenRegionChecked);
        //        RaisePropertyChanged(() => IsBottomScreenRegionChecked);
        //        RaisePropertyChanged(() => IsCustomScreenRegionChecked);
        //        RaisePropertyChanged(() => IsFullScreenRegionChecked);
        //    }
        //}

        //public bool IsCustomScreenRegionChecked {
        //    get => CurrentOutput.OutputScreenCapturePosition == "custom";
        //    set
        //    {
        //        CurrentOutput.OutputScreenCapturePosition = "custom";
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCapturePosition));
        //        RaisePropertyChanged(() => IsRightScreenRegionChecked);
        //        RaisePropertyChanged(() => IsLeftScreenRegionChecked);
        //        RaisePropertyChanged(() => IsTopScreenRegionChecked);
        //        RaisePropertyChanged(() => IsBottomScreenRegionChecked);
        //        RaisePropertyChanged(() => IsCustomScreenRegionChecked);
        //        RaisePropertyChanged(() => IsFullScreenRegionChecked);
        //    }
        //}
        //public bool IsFullScreenRegionChecked {
        //    get => CurrentOutput.OutputScreenCapturePosition == "full";
        //    set
        //    {
        //        CurrentOutput.OutputScreenCapturePosition = "full";
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCapturePosition));
        //        RaisePropertyChanged(() => IsRightScreenRegionChecked);
        //        RaisePropertyChanged(() => IsLeftScreenRegionChecked);
        //        RaisePropertyChanged(() => IsTopScreenRegionChecked);
        //        RaisePropertyChanged(() => IsBottomScreenRegionChecked);
        //        RaisePropertyChanged(() => IsCustomScreenRegionChecked);
        //        RaisePropertyChanged(() => IsFullScreenRegionChecked);
        //    }
        //}

        //public bool IsWarmWBSelected {
        //    get => CurrentOutput.OutputScreenCaptureWB == "warm";
        //    set
        //    {

        //        CurrentOutput.OutputScreenCapturePosition = "warm";
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCaptureWB));
        //    }
        //}


        //public bool IsNaturalWBSelected {
        //    get => CurrentOutput.OutputScreenCaptureWB == "natural";
        //    set
        //    {

        //        CurrentOutput.OutputScreenCapturePosition = "natural";
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCaptureWB));
        //    }
        //}

        //public bool IsColdWBSelected {
        //    get => CurrentOutput.OutputScreenCaptureWB == "cold";
        //    set
        //    {

        //        CurrentOutput.OutputScreenCapturePosition = "cold";
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCaptureWB));
        //    }
        //}

        //public bool IsCustomWBSelected {
        //    get => CurrentOutput.OutputScreenCaptureWB == "custom";
        //    set
        //    {

        //        CurrentOutput.OutputScreenCapturePosition = "custom";
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputScreenCaptureWB));
        //    }
        //}



        //#endregion



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
        private Thickness _sensitivityThickness;
        public Thickness SensitivityThickness {

            get => _sensitivityThickness;
            set
            {
                Set(ref _sensitivityThickness, value);
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
        private bool _isVisualizerWindowOpen;
        public bool IsVisualizerWindowOpen {
            get => _isVisualizerWindowOpen;
            set
            {
                Set(ref _isVisualizerWindowOpen, value);
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
        private ObservableCollection<ILightingMode> _availableLightingMode;
        public ObservableCollection<ILightingMode> AvailableLightingMode {
            get => _availableLightingMode;
            set
            {
                Set(ref _availableLightingMode, value);
                // _log.Info($"IsSettingsWindowOpen is now {_isSettingsWindowOpen}");
            }
        }
        private ILightingMode _currentSelectedLightingMode;
        public ILightingMode CurrentSelectedLightingMode {
            get => _currentSelectedLightingMode;
            set
            {
                Set(ref _currentSelectedLightingMode, value);
                // _log.Info($"IsSettingsWindowOpen is now {_isSettingsWindowOpen}");
            }
        }
        private IAutomationSettings _currentSelectedAutomation;
        public IAutomationSettings CurrentSelectedAutomation {
            get => _currentSelectedAutomation;
            set
            {
                Set(ref _currentSelectedAutomation, value);
                // _log.Info($"IsSettingsWindowOpen is now {_isSettingsWindowOpen}");
            }
        }

        private IColorPalette _currentActivePalette;
        public IColorPalette CurrentActivePalette {
            get { return _currentActivePalette; }
            set
            {
                if (value != null)
                {
                    _currentActivePalette = value;
                    CurrentOutput.OutputCurrentActivePalette = value;
                    RaisePropertyChanged(nameof(CurrentOutput.OutputCurrentActivePalette));
                }

                //SetCurrentDeviceSelectedPalette(value);
            }
        }
        private ObservableCollection<Color> _currentEditingColors;
        public ObservableCollection<Color> CurrentEditingColors {
            get { return _currentEditingColors; }
            set
            {
                _currentEditingColors = value;

                //SetCurrentDeviceSelectedPalette(value);
            }
        }



        private BadgeStatus _isSettingsUnsaved;
        public BadgeStatus IsSettingsUnsaved {
            get
            {
                return _isSettingsUnsaved;
            }

            set
            {
                _isSettingsUnsaved = value;

                RaisePropertyChanged();
            }
        }



        private string _nameToChange;
        public string NameToChange {
            get
            {
                return _nameToChange;
            }

            set
            {
                _nameToChange = value;

                RaisePropertyChanged();
            }
        }
        private string _automationNewName;
        public string AutomationNewName {

            get { return _automationNewName; }
            set {  _automationNewName = value; RaisePropertyChanged(); }    
        }


        private string _newProfileName;
        public string NewProfileName {
            get
            {
                return _newProfileName;
            }

            set
            {
                _newProfileName = value;

                RaisePropertyChanged();
            }
        }
        private string _newProfileOwner;
        public string NewProfileOwner {
            get
            {
                return _newProfileOwner;
            }

            set
            {
                _newProfileOwner = value;

                RaisePropertyChanged();
            }
        }
        private string _newProfileDescription;
        public string NewProfileDescription {
            get
            {
                return _newProfileDescription;
            }

            set
            {
                _newProfileDescription = value;

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
        private Color _selectedSolidColor;
        public Color SelectedSolidColor {
            get
            {
                return _selectedSolidColor;
            }

            set
            {
                _selectedSolidColor = value;

                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// new Palette
        /// </summary>
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

        /// <summary>
        /// 
        /// new Automation
        /// </summary>
        private string _newAutomationName;
        public string NewAutomationName {
            get { return _newAutomationName; }
            set
            {
                _newAutomationName = value;
            }
        }
      

        private int _rangeMinValue = 0;
        public int RangeMinValue {
            get { return _rangeMinValue; }
            set
            {
                _rangeMinValue = value;
            }
        }
        private int _iDMinValue = 0;
        public int IDMinValue {
            get { return _iDMinValue; }
            set
            {
                _iDMinValue = value;
            }
        }
        private int _rangeMaxValue = 100;
        public int RangeMaxValue {
            get { return _rangeMaxValue; }
            set
            {
                _rangeMaxValue = value;
            }
        }
       
        public int IDMaxValue {
            get {
                int totalLEDNum=0;
                foreach(var device in AvailableDevices)
                {
                    if(!device.IsDummy)
                    {
                        foreach (var output in device.AvailableOutputs)
                        {
                            totalLEDNum += output.OutputLEDSetup.Spots.Length;
                        }
                    }
                  
                }
                
                return totalLEDNum; }
            
        }
        private IGradientColorCard _currentSelectedGradient;
        public IGradientColorCard CurrentSelectedGradient {
            get { return _currentSelectedGradient; }
            set
            {
                _currentSelectedGradient = value;
            }
        }
        private Color _currentStartColor;
        public Color CurrentStartColor {
            get { return _currentStartColor; }
            set
            {
                _currentStartColor = value;
            }
        }
        private Color _currentStopColor;
        public Color CurrentStopColor {
            get { return _currentStopColor; }
            set
            {
                _currentStopColor = value;
            }
        }
        private int _adjustingRectangleWidth;
        public int AdjustingRectangleWidth {
            get { return _adjustingRectangleWidth; }
            set
            {
                _adjustingRectangleWidth = value;
                RaisePropertyChanged();
            }
        }

        private int _adjustingRectangleHeight;
        public int AdjustingRectangleHeight {
            get { return _adjustingRectangleHeight; }
            set
            {
                _adjustingRectangleHeight = value;
                RaisePropertyChanged();
            }
        }
        private int _adjustingRectangleTop;
        public int AdjustingRectangleTop {
            get { return _adjustingRectangleTop; }
            set
            {
                _adjustingRectangleTop = value;
                RaisePropertyChanged();
            }
        }
        private int _adjustingRectangleLeft;
        public int AdjustingRectangleLeft {
            get { return _adjustingRectangleLeft; }
            set
            {
                _adjustingRectangleLeft = value;
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
            ChangeCurrentDeviceSelectedPalette = new RelayCommand<IColorPalette>((p) =>
            {
                return true;
            }, (p) =>
            {
                //SetCurrentDeviceSelectedPalette(CurrentActivePalette);
            });

            EditSelectedPaletteCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                EditCurrentPalette(p);
            });
            LaunchPositionEditWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenPositionEditWindow();
            });

            AddNewGradientCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenAddNewGradientWindow();
            });
            OpenAddNewAutomationCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenAddNewAutomationWindowCommand();
            });
            SaveAllAutomationCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SaveAllAutomation();
            });
            AddNewSolidColorCommand = new RelayCommand<string>((p) =>
                 {
                     return true;
                 }, (p) =>
                 {
                     OpenColorPickerWindow();
                 });
            AddPickedSolidColorCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                AvailableSolidColors.Insert(0, CurrentPickedColor);
                WriteSolidColorJson();
            }
            );
            EditSelectedPaletteSaveConfirmCommand = new RelayCommand<string>((p) =>
                 {
                     return true;
                 }, (p) =>
                 {
                     SaveCurrentEditedPalette(p);
                 });
            ApplyCurrentOutputCapturingPositionCommand = new RelayCommand<string>((p) =>
           {
               return true;
           }, (p) =>
           {
               ApplyCurrentOuputCapturingPosition();
           }
            );
            CreateNewPaletteCommand = new RelayCommand<string>((p) =>
                   {
                       return true;
                   }, (p) =>
                   {
                       CreateNewPalette();
                   });


            CreateNewAutomationCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                CreateNewAutomation();
            });
            SaveCurrentProfileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SaveCurrentProfile();
            }
            );
            SaveCurrentSelectedActionCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SaveCurrentSelectedAction();
            }
            );
            SaveCurrentSelectedAutomationCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                SaveCurrentSelectedAutomation();
            }
           );
            ExportCurrentProfileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ExportCurrentProfile();
            }
          );
            ImportProfileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ImportProfile();
            }
          );
            CreateNewProfileCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                CreateNewProfile();
            }
            );
            OpenProfileCreateCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                OpenCreatenewProfileWindow();
            }
           );
            CoppyColorCodeCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                System.Windows.Clipboard.SetText(SelectedSolidColor.ToString());
                Growl.Success("Color code copied to clipboard!");
            }
         );
            DeleteSelectedSolidColorCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                AvailableSolidColors.Remove(SelectedSolidColor);
                WriteSolidColorJson();
            }
      );

            DeleteSelectedPaletteCommand = new RelayCommand<IColorPalette>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeleteSelectedPalette(p);
            });

            DeleteSelectedAutomationCommand = new RelayCommand<IAutomationSettings>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeleteSelectedAutomation(p);
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
            ExportCurrentSelectedPaletteToFileCommand = new RelayCommand<IColorPalette>((p) =>
            {
                return true;
            }, (p) =>
            {
                ExportCurrentSelectedPaletteToFile(p);
            });
            ExportPIDCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ExportCurrentOutputPID();
            });
            ImportPIDCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ImportCurrentOutputPID();
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
            OpenFFTPickerWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                OpenFFTPickerWindow();
            });
            OpenActionsManagerWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                OpenActionsManagerWindow();
            });
            OpenActionsEditWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                OpenActionsEditWindow();
            });
            OpenAutomationManagerWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                OpenAutomationManagerWindow();
            });
            LaunchWBAdjustWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                LaunchWBAdjustWindow();
            });
            LaunchVIDEditWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                LaunchVIDEditWindow();
            });
            LaunchMIDEditWindowCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {

                LaunchMIDEditWindow();
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

            JumpToNextAddDeviceWizardStateCommand = new RelayCommand<string>((p) =>
                   {
                       return p != null;
                   }, (p) =>
                   {
                       CurrentAddDeviceWizardState++;



                   });
            AddCurrentSelectedDeviceToDashboard = new RelayCommand<string>((p) =>
           {
               return p != null;
           }, (p) =>
           {
               AddDevice();



           }
            );
            AddSelectedWLEDDevicesCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                AddWLEDDevices();



            }
            );
            BackToPreviousAddDeviceWizardStateCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                CurrentAddDeviceWizardState--;



            });
            RenameSelectedItemCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                CurrentOutput.OutputName = NameToChange;
                RaisePropertyChanged(nameof(CurrentOutput.OutputName));
            });
            ShowNameEditWindow = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                OpenNameEditWindow();
            });

            OpenDeviceConnectionSettingsWindowCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                OpenDeviceConnectionSettingsWindow();
            });
            OpenDeviceFirmwareSettingsWindowCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                OpenDeviceFirmwareSettingsWindow();
            });
            OpenAdvanceSettingWindowCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                OpenAdvanceSettingWindow();
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
                Count = 0;
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
                Count = 0;
                foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                {
                    spot.SetStroke(0);
                    spot.IsActivated = false;
                    if (spot.XIndex == 0 || spot.YIndex == 0 || spot.XIndex == CurrentOutput.OutputNumLEDX - 1 || spot.YIndex == CurrentOutput.OutputNumLEDY - 1)
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
            SetCurrentSelectedVID = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                if (p != "")
                    ProcessSelectedSpots(p);

            });
            SetCurrentSelectedVIDRange = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {

                ProcessSelectedSpotsWithRange();

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
                ShowAddNewWindow();
            });
            ScanSerialDeviceCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                ScanSerialDevice(p);
            });
            BackCommand = new RelayCommand<string>((p) =>
            {
                return true;
            }, (p) =>
            {
                BackToDashboard();


            });
        }
        private static object _syncRoot = new object();
        private static CancellationToken cancellationtoken;
        private async void ScanSerialDevice(string status)
        {
            ISerialDeviceDetection detector = new SerialDeviceDetection();
           
            

            var jobTask = Task.Run(() => {
                // Organize critical sections around logical serial port operations somehow.
                lock (_syncRoot)
                {
                    return detector.DetectedDevices;
                }
            });
            if (jobTask != await Task.WhenAny(jobTask, Task.Delay(Timeout.Infinite, cancellationtoken)))
            {
                // Timeout;
                return;
            }
            var newDevices = await jobTask;
            foreach (var device in newDevices)
            {
                Debug.WriteLine("Name: " + device.DeviceName);
                Debug.WriteLine("ID: " + device.DeviceSerial);
                Debug.WriteLine("Firmware Version: " + device.FirmwareVersion);
                Debug.WriteLine("---------------");
             
            }



            AvailableSerialDevices = new ObservableCollection<IDeviceSettings>();
            foreach (var device in newDevices)
            {
                AvailableSerialDevices.Add(device);
            }

        }

        private void OpenFFTPickerWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"MIDEditWindow"}") is System.Windows.Window window)
            {

                VisualizerFFT = new VisualizerProgressBar[CurrentOutput.OutputLEDSetup.Spots.Length];
                for (int i = 0; i < VisualizerFFT.Length; i++)
                {
                    VisualizerFFT[i] = new VisualizerProgressBar(0.0f, Color.FromRgb(0, 0, 0));
                }


                IsVisualizerWindowOpen = true;
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private void OpenActionsManagerWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"ActionManagerWindow"}") is System.Windows.Window window)
            {
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private void OpenActionsEditWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"ActionEditWindow"}") is System.Windows.Window window)
            {
                AvailableActionsforCurrentDevice = new ObservableCollection<string>();
                AvailableActionsforCurrentDevice.Add("Activate Profile");
                AvailableActionsforCurrentDevice.Add("Brightness Control");
                AvailableActionsforCurrentDevice.Add("Do Nothing");
                AvailableParametersforCurrentAction = new List<IActionParameter>();
                AllAvailableParametersforCurrentAction = new List<IActionParameter>();
                foreach (var profile in AvailableProfiles)
                {
                    if(profile.DeviceType==CurrentSelectedAction.TargetDeviceType)
                    {
                        AllAvailableParametersforCurrentAction.Add(new ActionParameter { Geometry = profile.Geometry,Name=profile.Name,Type="profile",Value=profile.ProfileUID });
                    }
                }


                ActionParameter brightnessUp = new ActionParameter { Name = "Brightness Up",Geometry= "brightness", Type = "brightness", Value = "up" };
                ActionParameter brightnessDown = new ActionParameter { Name = "Brightness Down",Geometry="brightness", Type = "brightness", Value = "down" };
                AllAvailableParametersforCurrentAction.Add(brightnessUp);
                AllAvailableParametersforCurrentAction.Add(brightnessDown);
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private void OpenAutomationManagerWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AutomationManagerWindow"}") is System.Windows.Window window)
            {

                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }

        private void OpenAddNewGradientWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"GradientEditWindow"}") is System.Windows.Window window)
            {
                CurrentStartColor = CurrentSelectedGradient.StartColor;
                CurrentStopColor = CurrentSelectedGradient.StopColor;
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private void OpenAddNewAutomationWindowCommand()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AddNewAutomationWindow"}") is System.Windows.Window window)
            {
               
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private void LaunchWBAdjustWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"CustomWBWindow"}") is System.Windows.Window window)
            {

                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private void ApplyCurrentOuputCapturingPosition()
        {
            if (AdjustingRectangleTop + AdjustingRectangleHeight <= ShaderBitmap.Height && AdjustingRectangleLeft + AdjustingRectangleWidth <= ShaderBitmap.Width && AdjustingRectangleTop >= 0 && AdjustingRectangleLeft >= 0)
            {
                CurrentOutput.OutputRectangle = new Rectangle(AdjustingRectangleLeft, AdjustingRectangleTop, AdjustingRectangleWidth, AdjustingRectangleHeight);
                RaisePropertyChanged(nameof(CurrentOutput.OutputRectangle));
            }

        }

        private void ImportProfile()
        {

            OpenFileDialog Import = new OpenFileDialog();
            Import.Title = "Chọn Profile";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "Pro";
            Import.Filter = "Text files (*.Pro)|*.Pro";
            Import.FilterIndex = 2;


            Import.ShowDialog();


            if (!string.IsNullOrEmpty(Import.FileName) && File.Exists(Import.FileName))
            {

                var json = File.ReadAllText(Import.FileName);

                var existedprofile = JsonConvert.DeserializeObject<DeviceProfile>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                //ignore existed UID for posibility of conflicting with old UID
                existedprofile.ProfileUID = Guid.NewGuid().ToString();
                AvailableProfiles.Add(existedprofile);
                RaisePropertyChanged(nameof(AvailableProfiles));
                WriteDeviceProfileCollection();
                AvailableProfilesForCurrentDevice.Clear();
                AvailableProfilesForCurrentDevice = ProfileFilter(CurrentDevice);

            }
        }

        private void SaveCurrentProfile()
        {
            for (var i = 0; i < CurrentDevice.AvailableOutputs.Length; i++)
            {
                foreach (PropertyInfo property in typeof(IOutputSettings).GetProperties().Where(p => p.CanWrite))
                {
                    property.SetValue(CurrentSelectedProfile.OutputSettings[i], property.GetValue(CurrentDevice.AvailableOutputs[i], null), null);
                }
            }
            WriteDeviceProfileCollection();
            AvailableProfiles.Clear();
            foreach (var profile in LoadDeviceProfileIfExist())
            {
                AvailableProfiles.Add(profile);
            }
            RaisePropertyChanged(nameof(AvailableProfiles));
            Growl.Success("Profile saved successfully!");
            IsSettingsUnsaved = BadgeStatus.Dot;
        }
        private void SaveCurrentSelectedAction()
        {
         CurrentSelectedAction.ActionType=SelectedActionType.ToString();
            CurrentSelectedAction.ActionParameter = SelectedParameter;
            
            
        }
        private void SaveCurrentSelectedAutomation()
        {
            CurrentSelectedAutomation.Modifiers = new List<IModifiersType>();
            foreach(var modifier in AvailableModifiers)
            {
                if(modifier.IsChecked)
                {
                    CurrentSelectedAutomation.Modifiers.Add(modifier);
                }
            }

            CurrentSelectedAutomation.Name = AutomationNewName; 
            WriteAutomationCollectionJson();
            AvailableAutomations = new ObservableCollection<IAutomationSettings>();
            foreach (var automation in LoadAutomationIfExist())
            {
                AvailableAutomations.Add(automation);
            }
            Unregister();
            Register();

        }
        private void SaveAllAutomation()
        {
           
            
            WriteAutomationCollectionJson();
            AvailableAutomations = new ObservableCollection<IAutomationSettings>();
            foreach (var automation in LoadAutomationIfExist())
            {
                AvailableAutomations.Add(automation);
            }
            Unregister();
            Register();

        }
        private void ExportCurrentProfile()
        {
            SaveFileDialog Export = new SaveFileDialog();
            Export.CreatePrompt = true;
            Export.OverwritePrompt = true;

            Export.Title = "Xuất dữ liệu";
            Export.FileName = CurrentDevice.DeviceName + " Profile";
            Export.CheckFileExists = false;
            Export.CheckPathExists = true;
            Export.DefaultExt = "Pro";
            Export.Filter = "All files (*.*)|*.*";
            Export.InitialDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Export.RestoreDirectory = true;


            var ledsetupjson = JsonConvert.SerializeObject(CurrentSelectedProfile, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });

            if (Export.ShowDialog() == DialogResult.OK)
            {
                Directory.CreateDirectory(JsonPath);
                File.WriteAllText(Export.FileName, ledsetupjson);
                Growl.Success("Profile exported successfully!");
            }
        }

        private void LoadProfile() // change to currentDevice.LoadProfile
        {
            GeneralSettings.IsProfileLoading = true;
            CurrentDevice.ActivateProfile(CurrentSelectedProfile);

            WriteDeviceInfoJson();

            foreach (var output in CurrentDevice.AvailableOutputs)
            {
                output.OutputIsBuildingLEDSetup = true;


            }
            GeneralSettings.IsProfileLoading = false;
        }
        private void CreateNewProfile()
        {
            var newprofile = new DeviceProfile {
                Name = NewProfileName,
                Description = NewProfileDescription,
                Owner = NewProfileOwner,
                DeviceType = CurrentDevice.DeviceType,
                Geometry = "profile",
                ProfileUID = Guid.NewGuid().ToString(),
                OutputSettings = CurrentDevice.AvailableOutputs
            };

            AvailableProfiles.Add(newprofile);

            WriteDeviceProfileCollection();
            AvailableProfiles.Clear();
            foreach (var profile in LoadDeviceProfileIfExist())
            {
                AvailableProfiles.Add(profile);
            }

            AvailableProfilesForCurrentDevice.Clear();
            AvailableProfilesForCurrentDevice = ProfileFilter(CurrentDevice);
            RaisePropertyChanged(nameof(AvailableProfiles));



        }
        private void OpenCreatenewProfileWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AddNewProfileWindow"}") is System.Windows.Window window)
            {

                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        public void HotKeyDetected(string modifier, int keyCode)
        {
            foreach(var automation in AvailableAutomations)
            {
                if(automation.Condition == keyCode)
                //execute the action if valid
                {
                    foreach(var action in automation.Actions)
                    {
                        var targetDevice = AvailableDevices.Where(x => x.DeviceUID == action.TargetDeviceUID).FirstOrDefault();
                        switch (action.ActionType)
                        {
                            case "Activate Profile":
                                var destinationProfile = AvailableProfiles.Where(x => x.ProfileUID == action.ActionParameter.Value).FirstOrDefault();
                                targetDevice.ActivateProfile(destinationProfile);
                                var targetDeviceAvailableProfile = ProfileFilter(targetDevice);
                                targetDevice.ActivatedProfileIndex = targetDeviceAvailableProfile.IndexOf(destinationProfile);
                                break;
                            case "Brightness Control":
                                switch(action.ActionParameter.Value)
                                {
                                    case "up":
                                        foreach(var output in targetDevice.AvailableOutputs)//possible replace with method from IOutputSettings
                                        {
                                            if(output.OutputBrightness<98)
                                            output.OutputBrightness+=2;
                                        }
                                        break;
                                    case "down":
                                        foreach (var output in targetDevice.AvailableOutputs)
                                        {
                                            if(output.OutputBrightness>2)
                                            output.OutputBrightness -= 2;
                                        }
                                        break;
                                }
                                break;
                            case "Do Nothing":
                              // Yeah?? do fking nothing here
                                break;
                        }
                    }
                }
            }
            // scan all available automation with hotkey setup and execconute the command
        }
        private List<Guid?> _identifiers;

        private void Register()
            {
            _identifiers = new List<Guid?>();
            foreach(var automation in AvailableAutomations.Where(x => x.IsEnabled==true))
            {
                var modifierkeys = new List<NonInvasiveKeyboardHookLibrary.ModifierKeys>();
                if(automation.Modifiers!=null)
                {
                    foreach (var modifier in automation.Modifiers)
                    {
                        var modifierkey = modifier.ModifierKey;
                        modifierkeys.Add(modifierkey);

                    }
                }

               
               try
                {
                    Guid? _identifier = HotKeyManager.Instance.RegisterHotkey(modifierkeys.ToArray(), automation.Condition, () =>
                    {
                        foreach (var action in automation.Actions)
                        {
                            var targetDevice = AvailableDevices.Where(x => x.DeviceUID == action.TargetDeviceUID).FirstOrDefault();
                            switch (action.ActionType)
                            {
                                case "Activate Profile":
                                    var destinationProfile = AvailableProfiles.Where(x => x.ProfileUID == action.ActionParameter.Value).FirstOrDefault();
                                    targetDevice.ActivateProfile(destinationProfile);
                                    var targetDeviceAvailableProfile = ProfileFilter(targetDevice);
                                    targetDevice.ActivatedProfileIndex = targetDeviceAvailableProfile.IndexOf(destinationProfile);
                                    break;
                                case "Brightness Control":
                                    switch (action.ActionParameter.Value)
                                    {
                                        case "up":
                                            foreach (var output in targetDevice.AvailableOutputs)//possible replace with method from IOutputSettings
                                            {
                                                if (output.OutputBrightness < 98)
                                                    output.OutputBrightness += 2;
                                            }
                                            break;
                                        case "down":
                                            foreach (var output in targetDevice.AvailableOutputs)
                                            {
                                                if (output.OutputBrightness > 2)
                                                    output.OutputBrightness -= 2;
                                            }
                                            break;
                                    }
                                    break;
                            }
                        }
                        Debug.WriteLine(automation.Name + " excuted");
                    });
                    _identifiers.Add(_identifier);
                }
                catch(NonInvasiveKeyboardHookLibrary.HotkeyAlreadyRegisteredException ex)
                {
                    HandyControl.Controls.MessageBox.Show(automation.Name + " Hotkey is being used by another automation!!!", "HotKey Already Registered", MessageBoxButton.OK, MessageBoxImage.Error);
                    //disable automation
                    automation.IsEnabled = false;
                    WriteAutomationCollectionJson();

                }
               
            }
                
        }

        private void Unregister()
        {
            foreach(var _identifier in _identifiers)
            {
                if (_identifier.HasValue)
                {
                    HotKeyManager.Instance.UnregisterHotkey(_identifier.Value);
                }
            }
           
        }

        private void ImportCurrentOutputPID()
        {
            OpenFileDialog Import = new OpenFileDialog();
            Import.Title = "Chọn LED Setup file";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = "json";
            Import.Filter = "Text files (*.json)|*.json";
            Import.FilterIndex = 2;


            Import.ShowDialog();


            if (!string.IsNullOrEmpty(Import.FileName) && File.Exists(Import.FileName))
            {



                var json = File.ReadAllText(Import.FileName);

                try
                {
                    var ledSetup = JsonConvert.DeserializeObject<LEDSetup>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                    if (ledSetup.Spots.Length != CurrentOutput.OutputLEDSetup.Spots.Length)
                        HandyControl.Controls.MessageBox.Show("Corrupted or incompatible data File!!!", "Incompatible Data File", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                    {
                        CurrentOutput.OutputLEDSetup = ledSetup;
                        RaisePropertyChanged(nameof(CurrentOutput.OutputLEDSetup));
                    }

                }
                catch (FormatException)
                {
                    HandyControl.Controls.MessageBox.Show("Corrupted or incompatible data File!!!");
                }

            }
        }

        private void ExportCurrentOutputPID()
        {
            SaveFileDialog Export = new SaveFileDialog();
            Export.CreatePrompt = true;
            Export.OverwritePrompt = true;

            Export.Title = "Xuất dữ liệu";
            Export.FileName = CurrentOutput.OutputName + " LED Setup";
            Export.CheckFileExists = false;
            Export.CheckPathExists = true;
            Export.DefaultExt = "json";
            Export.Filter = "All files (*.*)|*.*";
            Export.InitialDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Export.RestoreDirectory = true;


            var ledsetupjson = JsonConvert.SerializeObject(CurrentOutput.OutputLEDSetup, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });

            if (Export.ShowDialog() == DialogResult.OK)
            {
                Directory.CreateDirectory(JsonPath);
                File.WriteAllText(Export.FileName, ledsetupjson);
                Growl.Success("LED Setup saved successfully!");
            }
        }

        private void ProcessSelectedSpotsWithRange()
        {
            int counter = 0;


            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
            {

                if (spot.BorderThickness != 0)//spots is selecteds
                {
                    counter++;
                }
            }
            var spacing = (RangeMaxValue - RangeMinValue) / counter;
            if (spacing < 1)
                spacing = 1;
            int offset = RangeMinValue;
            foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
            {

                if (spot.BorderThickness != 0)//spots is selected
                {
                    switch (SetIDMode)
                    {
                        case "VID":
                            spot.SetVID(offset);
                            offset += spacing;
                            break;
                        case "MID":
                            spot.SetMID(offset);
                            offset += spacing;
                            break;
                    }

                }
            }
        }

        private void ProcessSelectedSpots(string userInput)
        {
            switch (SetIDMode)
            {
                case "VID":
                    foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                    {

                        if (spot.BorderThickness != 0)//spots is selected
                        {
                            spot.SetVID(Int32.Parse(userInput));
                        }



                    }
                    break;
                case "MID":
                    foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                    {

                        if (spot.BorderThickness != 0)//spots is selected
                        {
                            spot.SetMID(Int32.Parse(userInput));
                        }



                    }
                    break;
            }

        }

        private void OpenDeviceConnectionSettingsWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"DeviceConnectionSettingsWindow"}") is System.Windows.Window window)
            {

                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private void OpenDeviceFirmwareSettingsWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"DeviceFirmwareSettingsWindow"}") is System.Windows.Window window)
            {

                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private void OpenNameEditWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"RenameItemWindow"}") is System.Windows.Window window)
            {

                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        private void OpenAdvanceSettingWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"OutputAdvanceSettingsWindow"}") is System.Windows.Window window)
            {

                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            }
        }


        private void AddDevice()
        {
            CurrentSelectedDeviceToAdd.DeviceID = AvailableDevices.Count + 1;
            CurrentSelectedDeviceToAdd.DeviceUID = Guid.NewGuid().ToString();
            AvailableDevices.Add(CurrentSelectedDeviceToAdd);
            WriteDeviceInfoJson();
            System.Windows.Forms.Application.Restart();
            Process.GetCurrentProcess().Kill();
        }
        private void AddWLEDDevices()
        {
            if(AvailableWLEDDevices!=null)
            {
                foreach (var wLEDDevice in AvailableWLEDDevices)
                {
                    if (wLEDDevice.IsSelected)
                    {
                        IDeviceSettings convertedDevice = new DeviceSettings();
                        convertedDevice.DeviceName = wLEDDevice.Name;
                        convertedDevice.DeviceType = "WLED";
                        convertedDevice.DeviceConnectionType = "wireless";
                        convertedDevice.OutputPort = wLEDDevice.NetworkAddress;
                        convertedDevice.DeviceDescription = "WLED Device using WARLS protocol";
                        convertedDevice.DeviceID = AvailableDevices.Count + 1;
                        convertedDevice.DeviceUID = Guid.NewGuid().ToString();
                        convertedDevice.AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.GenericLEDStrip(0, 64) };
                        convertedDevice.Geometry = wLEDDevice.Geometry;
                        AvailableDevices.Add(convertedDevice);
                    }

                }
            }
            if(AvailableSerialDevices!=null)
            {
                foreach(var serialDevice in AvailableSerialDevices)
                {
                    if(serialDevice.IsSelected)
                    {
                        serialDevice.DeviceID = AvailableDevices.Count + 1;
                        AvailableDevices.Add(serialDevice);
                    }
                    
                }
            }
           
          
            WriteDeviceInfoJson();
            System.Windows.Forms.Application.Restart();
            Process.GetCurrentProcess().Kill();
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
        private void LaunchVIDEditWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"VIDEditWindow"}") is System.Windows.Window window)
            {
                BackupSpots = new List<IDeviceSpot>();
                foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                {
                    BackupSpots.Add(spot);
                }

                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();

            }
        }
        private void LaunchMIDEditWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"MIDEditWindow"}") is System.Windows.Window window)
            {
                BackupSpots = new List<IDeviceSpot>();
                foreach (var spot in CurrentOutput.OutputLEDSetup.Spots)
                {
                    BackupSpots.Add(spot);
                }

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

        private void OpenPositionEditWindow()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"PositionEditWindow"}") is System.Windows.Window window)
            {
                AdjustingRectangleWidth = CurrentOutput.OutputRectangle.Width;
                AdjustingRectangleHeight = CurrentOutput.OutputRectangle.Height;
                AdjustingRectangleLeft = CurrentOutput.OutputRectangle.Left;
                AdjustingRectangleTop = CurrentOutput.OutputRectangle.Top;
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();

            }
        }

        private void ExportCurrentSelectedPaletteToFile(IColorPalette palette)
        {
            SaveFileDialog Export = new SaveFileDialog();
            Export.CreatePrompt = true;
            Export.OverwritePrompt = true;

            Export.Title = "Xuất dữ liệu";
            Export.FileName = palette.Name + " Color Palette";
            Export.CheckFileExists = false;
            Export.CheckPathExists = true;
            Export.DefaultExt = "col";
            Export.Filter = "All files (*.*)|*.*";
            Export.InitialDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Export.RestoreDirectory = true;


            string[] paletteData = new string[19];
            paletteData[0] = palette.Name;
            paletteData[1] = palette.Owner;
            paletteData[2] = palette.Description;
            for (int i = 0; i < palette.Colors.Length; i++)
            {
                paletteData[i + 3] = palette.Colors[i].ToString();
            }
            if (Export.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllLines(Export.FileName, paletteData);
                Growl.Success("Palette saved successfully!");
            }
        }
        public void SetPreviewVisualizerFFT(float[] fft, Color[] previewStrip)
        {




            for (int i = 0; i < fft.Length; i++)
            {
                VisualizerFFT[i].SetValue(fft[i]);
                VisualizerFFT[i].SetColor(previewStrip[i]);
            }



        }
        private void CreateNewPalette()
        {
            var lastSelectedPaletteIndex = CurrentOutput.OutputSelectedChasingPalette;
            var name = NewPaletteName;
            var owner = NewPaletteOwner;
            var description = NewPaletteDescription;
            var colors = new System.Windows.Media.Color[16];
            int counter = 0;
            foreach (var color in CurrentEditingColors)
            {
                colors[counter++] = color;
            }
            AvailablePallete.Clear();
            foreach (var palette in LoadPaletteIfExists())
            {
                AvailablePallete.Add(palette);
            }
            IColorPalette newpalette = new ColorPalette(name, owner, "RGBPalette16", description, colors);
            AvailablePallete.Add(newpalette);

            WritePaletteCollectionJson();
            AvailablePallete.Clear();
            foreach (var palette in LoadPaletteIfExists())
            {
                AvailablePallete.Add(palette);
            }
            CurrentOutput.OutputSelectedChasingPalette = lastSelectedPaletteIndex;

        }

        private void CreateNewAutomation()
        {
            
            var name = NewAutomationName;
            IAutomationSettings newAutomation = new AutomationSettings { Name = name};
           
            var doAbsolutelyNothing = new List<IActionSettings>(); // doing nothing to all devices
            if (AvailableDevices.Count > 0 && AvailableProfiles.Count > 0)
            {
                foreach (var device in AvailableDevices.Where(x=>x.IsDummy==false))
                {

                    IActionSettings doNothing = new ActionSettings {
                        ActionType = "Do Nothing",
                        TargetDeviceUID = device.DeviceUID,
                        TargetDeviceName = device.DeviceName,
                        TargetDeviceType = device.DeviceType,
                        ActionParameter = new ActionParameter { Name = "Do Nothing", Type = "donothing", Value = "none" },


                    };

                    doAbsolutelyNothing.Add(doNothing);
                    
                }

            }

            newAutomation.Actions = doAbsolutelyNothing;
            AvailableAutomations.Add(newAutomation);
            WriteAutomationCollectionJson();

        }
        private void EditCurrentPalette(string param)
        {
            ////var name = NewPaletteName;
            ////var owner = NewPaletteOwner;
            ////var description = NewPaletteDescription;
            ////var colors = new System.Windows.Media.Color[16];
            ////IColorPalette newpalette = new ColorPalette(name, owner, "RGBPalette16", description, colors);
            //AvailablePallete.Add(newpalette);
            //CurrentActivePalette = AvailablePallete.Last();
            //RaisePropertyChanged(nameof(AvailablePallete));
            //RaisePropertyChanged(nameof(CurrentActivePalette));
            //WritePaletteCollectionJson();
            OpenEditPaletteDialog(param);
        }
        private void DeleteSelectedAutomation(IAutomationSettings automation)
        {
            AvailableAutomations.Remove(automation);
            WriteAutomationCollectionJson();
        }
        private void DeleteSelectedPalette(IColorPalette selectedpalette)
        {
            if (AvailablePallete.Count == 1)
            {
                var result = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo {
                    Message = " Please don't delete all Color Palette in this section, atleast keep one left!!!",
                    Caption = "Xóa dải màu",
                    Button = MessageBoxButton.OK,
                    IconBrushKey = ResourceToken.AccentBrush,
                    IconKey = ResourceToken.WarningGeometry,
                    StyleKey = "MessageBoxCustom"
                });

                return;
            }
            AvailablePallete.Remove(selectedpalette);
            WritePaletteCollectionJson();
            AvailablePallete.Clear();
            foreach (var palette in LoadPaletteIfExists())
            {
                AvailablePallete.Add(palette);
            }
            CurrentOutput.OutputSelectedChasingPalette = 0;
            CurrentActivePalette = AvailablePallete.First();

            //if (AvailablePallete.ElementAt(CurrentOutput.OutputSelectedChasingPalette) == palette && AvailablePallete.ElementAt(CurrentOutput.OutputSelectedMusicPalette) == palette)
            //{
            //    var result = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo {
            //        Message = " Có một chế độ khác đang sử dụng dải màu này, bạn có muốn xóa không ?",
            //        Caption = "Xóa dải màu",
            //        Button = MessageBoxButton.YesNo,
            //        IconBrushKey = ResourceToken.AccentBrush,
            //        IconKey = ResourceToken.AskGeometry,
            //        StyleKey = "MessageBoxCustom"
            //    });
            //    if (result == MessageBoxResult.Yes) // overwrite current palette
            //    {

            //        CurrentOutput.OutputSelectedChasingPalette = 0;
            //        CurrentOutput.OutputSelectedMusicPalette = 0;
            //        RaisePropertyChanged(nameof(CurrentOutput.OutputSelectedChasingPalette));
            //        RaisePropertyChanged(nameof(CurrentOutput.OutputSelectedMusicPalette));
            //        CurrentActivePaletteCard = AvailablePallete.First();
            //        SetCurrentDeviceSelectedPalette(CurrentActivePaletteCard);
            //        RaisePropertyChanged(nameof(CurrentActivePaletteCard));
            //        AvailablePallete.Remove(palette);
            //        RaisePropertyChanged(nameof(AvailablePallete));
            //        WritePaletteCollectionJson();
            //    }
            //    else
            //    {

            //    }

            //}
            //else
            //{
            //    switch (CurrentOutput.OutputSelectedMode)
            //    {
            //        case 1:
            //            CurrentOutput.OutputSelectedChasingPalette = 0;
            //            RaisePropertyChanged(nameof(CurrentOutput.OutputSelectedChasingPalette));
            //            break;
            //        case 3:
            //            CurrentOutput.OutputSelectedMusicPalette = 0;
            //            RaisePropertyChanged(nameof(CurrentOutput.OutputSelectedMusicPalette));
            //            break;
            //    }
            //    CurrentActivePaletteCard = AvailablePallete.First();
            //    SetCurrentDeviceSelectedPalette(CurrentActivePaletteCard);
            //    RaisePropertyChanged(nameof(CurrentActivePaletteCard));
            //    AvailablePallete.Remove(palette);
            //    RaisePropertyChanged(nameof(AvailablePallete));
            //    WritePaletteCollectionJson();

            //}


        }


        private void SaveCurrentEditedPalette(string param)
        {
            //AvailablePallete[CurrentOutput.OutputSelectedChasingPalette] = CurrentActivePalette;
            var lastSelectedPaletteIndex = CurrentOutput.OutputSelectedChasingPalette;
            if (param == "save")
            {
                WritePaletteCollectionJson();
            }

            //reload all available palette;
            AvailablePallete.Clear();

            foreach (var palette in LoadPaletteIfExists())
            {
                AvailablePallete.Add(palette);
            }
            CurrentOutput.OutputSelectedChasingPalette = lastSelectedPaletteIndex;
            //CurrentActivePalette = AvailablePallete[CurrentOutput.OutputSelectedChasingPalette];
            //var result = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo {
            //    Message = "Bạn có muốn ghi đè lên dải màu hiện tại?",
            //    Caption = "Lưu dải màu",
            //    Button = MessageBoxButton.YesNo,
            //    IconBrushKey = ResourceToken.AccentBrush,
            //    IconKey = ResourceToken.AskGeometry,
            //    StyleKey = "MessageBoxCustom"
            //});
            //if (result == MessageBoxResult.Yes) // overwrite current palette
            //{
            //    var activePalette = CurrentOutput.OutputCurrentActivePalette;
            //    CurrentActivePaletteCard.Thumbnail = activePalette;
            //    SetCurrentDeviceSelectedPalette(CurrentActivePaletteCard);
            //    WritePaletteCollectionJson();
            //    //reload all available palette;
            //    AvailablePallete.Clear();
            //    foreach (var palette in LoadPaletteIfExists())
            //    {
            //        AvailablePallete.Add(palette);
            //    }

            //    CurrentCustomZoneColor.Clear();
            //    foreach (var color in CurrentOutput.OutputCurrentActivePalette)
            //    {
            //        CurrentCustomZoneColor.Add(color);
            //    }
            //    RaisePropertyChanged(nameof(CurrentCustomZoneColor));
            //}

            //else // open create new dialog
            //{
            //OpenCreateNewDialog();
            //}
        }

        public void OpenEditPaletteDialog(string praram)
        {
            var window = new PaletteEditWindow(praram);
            window.Owner = System.Windows.Application.Current.MainWindow;
            CurrentEditingColors = new ObservableCollection<Color>();
            foreach (var color in CurrentActivePalette.Colors)
            {
                CurrentEditingColors.Add(color);
            }
            //CurrentCustomZoneColor = palette;

            RaisePropertyChanged(nameof(CurrentEditingColors));
            window.ShowDialog();

        }

        public void OpenCreateNewDialog()
        {
            if (AssemblyHelper.CreateInternalInstance($"View.{"AddNewPaletteWindow"}") is System.Windows.Window window)
            {

                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();

            }
        }
        //public void SetCurrentDeviceSelectedPalette(IColorPaletteCard palette)
        //{
        //    if (palette != null)
        //    {
        //        for (var i = 0; i < CurrentOutput.OutputCurrentActivePalette.Length; i++)
        //        {
        //            CurrentOutput.OutputCurrentActivePalette[i] = palette.Thumbnail[i];

        //        }

        //        RaisePropertyChanged(nameof(AvailablePallete));
        //        RaisePropertyChanged(nameof(CurrentOutput.OutputCurrentActivePalette));
        //        WriteDeviceInfoJson();
        //    }


        //}

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
            CurrentEditingColors[index] = CurrentPickedColor;
            CurrentActivePalette.SetColor(index, CurrentPickedColor);
            CurrentOutput.OutputCurrentActivePalette.SetColor(index, CurrentPickedColor);

            RaisePropertyChanged(nameof(CurrentActivePalette));
            RaisePropertyChanged(nameof(CurrentOutput.OutputCurrentActivePalette));


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


            AvailableLightingMode = new ObservableCollection<ILightingMode>();
            var screencapture = new LightingMode { Name = "Screen Capture", Geometry = "ambilight", Description = "Screen sampling to LED" };
            var palette = new LightingMode { Name = "Color Palette", Geometry = "colorpalette", Description = "Screen sampling to LED" };
            var music = new LightingMode { Name = "Music Reactive", Geometry = "music", Description = "Screen sampling to LED" };
            var staticcolor = new LightingMode { Name = "Static Color", Geometry = "static", Description = "Screen sampling to LED" };
            var gifxelation = new LightingMode { Name = "Gifxelation", Geometry = "gifxelation", Description = "Screen sampling to LED" };
            AvailableLightingMode.Add(screencapture);
            AvailableLightingMode.Add(palette);
            AvailableLightingMode.Add(music);
            AvailableLightingMode.Add(staticcolor);
            AvailableLightingMode.Add(gifxelation);
            AvailableBaudrates = new ObservableCollection<int> {
                9600,
                19200,
                38400,
                57600,
                115200,
                230400,
                460800,
                576000,
                921600,
                1000000,
                1500000,
                2000000,
                2500000,
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
            AvailablePallete = new ObservableCollection<IColorPalette>();
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

            AvailableProfiles = new ObservableCollection<IDeviceProfile>();
            foreach (var profile in LoadDeviceProfileIfExist())
            {
                AvailableProfiles.Add(profile);
            }
            WriteDeviceProfileCollection();
            //CurrentCustomZoneColor = new ObservableCollection<System.Windows.Media.Color>();
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
        public List<IColorPalette> LoadPaletteIfExists()
        {
            if (!File.Exists(JsonPaletteFileNameAndPath))
            {
                //create default palette
                var palettes = new List<IColorPalette>();
                IColorPalette rainbow = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow);
                IColorPalette police = new ColorPalette("Police", "Zooey", "RGBPalette16", "Police Car Light mimic", DefaultColorCollection.police);
                IColorPalette forest = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.forest);
                IColorPalette aurora = new ColorPalette("Police", "Zooey", "RGBPalette16", "Police Car Light mimic", DefaultColorCollection.aurora);
                IColorPalette iceandfire = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.iceandfire);
                IColorPalette scarlet = new ColorPalette("Police", "Zooey", "RGBPalette16", "Police Car Light mimic", DefaultColorCollection.scarlet);

                palettes.Add(rainbow);
                palettes.Add(police);
                palettes.Add(forest);
                palettes.Add(aurora);
                palettes.Add(iceandfire);
                palettes.Add(scarlet);







                return palettes;


            }

            var json = File.ReadAllText(JsonPaletteFileNameAndPath);
            var loadedPaletteCard = new List<IColorPalette>();
            var existPaletteCard = JsonConvert.DeserializeObject<List<ColorPalette>>(json);
            foreach (var paletteCard in existPaletteCard)
            {
                loadedPaletteCard.Add(paletteCard);
            }


            return loadedPaletteCard;
        }
        public List<IDeviceProfile> LoadDeviceProfileIfExist()
        {
            var loadedProfiles = new List<IDeviceProfile>();
            if (!File.Exists(JsonDeviceProfileFileNameAndPath))
            {
                var defaultFanHubProfile = new DeviceProfile {
                    Name = "Fan HUB Default",
                    Owner = "Ambino",
                    ProfileUID = Guid.NewGuid().ToString(),
                    Geometry = "profile",
                    DeviceType = "ABFANHUB",
                    Description = "Default Profile for Ambino Fan HUB",
                    OutputSettings = DefaultDeviceCollection.ambinoFanHub.AvailableOutputs

                };
                var defaultAmbinobasic24 = new DeviceProfile {
                    Name = "Ambino Basic 24inch Default",
                    Owner = "Ambino",
                    ProfileUID = Guid.NewGuid().ToString(),
                    Geometry = "profile",
                    DeviceType = "ABBASIC24",
                    Description = "Default Profile for Ambino Basic",
                    OutputSettings = DefaultDeviceCollection.ambinoBasic24.AvailableOutputs

                };
                var defaultAmbinobasic27 = new DeviceProfile {
                    Name = "Ambino Basic 27inch Default",
                    Owner = "Ambino",
                    ProfileUID = Guid.NewGuid().ToString(),
                    Geometry = "profile",
                    DeviceType = "ABBASIC27",
                    Description = "Default Profile for Ambino Basic",
                    OutputSettings = DefaultDeviceCollection.ambinoBasic27.AvailableOutputs

                };
                var defaultAmbinobasic29 = new DeviceProfile {
                    Name = "Ambino Basic 29inch Default",
                    Owner = "Ambino",
                    ProfileUID = Guid.NewGuid().ToString(),
                    Geometry = "profile",
                    DeviceType = "ABBASIC29",
                    Description = "Default Profile for Ambino Basic",
                    OutputSettings = DefaultDeviceCollection.ambinoBasic29.AvailableOutputs

                };
                var defaultAmbinobasic32 = new DeviceProfile {
                    Name = "Ambino Basic 32inch Default",
                    Owner = "Ambino",
                    ProfileUID = Guid.NewGuid().ToString(),
                    Geometry = "profile",
                    DeviceType = "ABBASIC32",
                    Description = "Default Profile for Ambino Basic",
                    OutputSettings = DefaultDeviceCollection.ambinoBasic32.AvailableOutputs

                };
                var defaultAmbinobasic34 = new DeviceProfile {
                    Name = "Ambino Basic 34inch Default",
                    Owner = "Ambino",
                    ProfileUID = Guid.NewGuid().ToString(),
                    Geometry = "profile",
                    DeviceType = "ABBASIC34",
                    Description = "Default Profile for Ambino Basic",
                    OutputSettings = DefaultDeviceCollection.ambinoBasic34.AvailableOutputs

                };
                var defaultAmbinoedge1m2 = new DeviceProfile {
                    Name = "Ambino EDGE 1m2 Default",
                    Owner = "Ambino",
                    ProfileUID = Guid.NewGuid().ToString(),
                    Geometry = "profile",
                    DeviceType = "ABEDGE1.2",
                    Description = "Default Profile for Ambino EDGE",
                    OutputSettings = DefaultDeviceCollection.ambinoEdge1m2.AvailableOutputs

                };
                var defaultAmbinoedge2m = new DeviceProfile {
                    Name = "Ambino EDGE 2m Default",
                    Owner = "Ambino",
                    ProfileUID = Guid.NewGuid().ToString(),
                    Geometry = "profile",
                    DeviceType = "ABEDGE2.0",
                    Description = "Default Profile for Ambino EDGE",
                    OutputSettings = DefaultDeviceCollection.ambinoEdge2m.AvailableOutputs

                };
                loadedProfiles.Add(defaultFanHubProfile);
                loadedProfiles.Add(defaultAmbinobasic24);
                loadedProfiles.Add(defaultAmbinobasic27);
                loadedProfiles.Add(defaultAmbinobasic29);
                loadedProfiles.Add(defaultAmbinobasic32);
                loadedProfiles.Add(defaultAmbinobasic34);
                loadedProfiles.Add(defaultAmbinoedge1m2);
                loadedProfiles.Add(defaultAmbinoedge2m);


            }
            else
            {
                var json = File.ReadAllText(JsonDeviceProfileFileNameAndPath);

                var existedProfile = JsonConvert.DeserializeObject<List<DeviceProfile>>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                foreach (var profile in existedProfile)
                {
                    loadedProfiles.Add(profile);
                }
            }



            return loadedProfiles;
        }
        public List<IAutomationSettings> LoadAutomationIfExist()
        {
            var loadedAutomations = new List<IAutomationSettings>();

            if (!File.Exists(JsonAutomationFileNameAndPath))
            {
                var brightnessUpAllDevices = new List<IActionSettings>(); // load all profile 0 on the list apply to all devices
                var brightnessDownAllDevices = new List<IActionSettings>(); // load all profile 0 on the list apply to all devices
                if (AvailableDevices.Count > 0 && AvailableProfiles.Count > 0)
                {
                    foreach (var device in AvailableDevices)
                    {
                        
                        IActionSettings brightnessUp = new ActionSettings {
                            ActionType = "Brightness Control",
                            TargetDeviceUID = device.DeviceUID,
                            TargetDeviceName = device.DeviceName,
                            TargetDeviceType = device.DeviceType,
                            ActionParameter = new ActionParameter { Name= "Brightness Up", Type="brightness",Value="up"  },
                           
                            
                        };
                        IActionSettings brightnessDown = new ActionSettings {
                            ActionType = "Brightness Control",
                            TargetDeviceUID = device.DeviceUID,
                            TargetDeviceName = device.DeviceName,
                            TargetDeviceType = device.DeviceType,
                            ActionParameter = new ActionParameter { Name = "Brightness Down", Type = "brightness", Value = "down" },
                           

                        };
                        brightnessUpAllDevices.Add(brightnessUp);
                        brightnessDownAllDevices.Add(brightnessDown);
                    }
                   
                }

                var BrightnessUpAllDevicesAutomation = new AutomationSettings {
                    Name = "Brightness Up",
                    Actions = brightnessUpAllDevices,
                    ConditionTypeIndex = 0,//key stroke detection
                    Modifiers = new List<IModifiersType> { new ModifiersType { Name ="CTRL", ModifierKey=NonInvasiveKeyboardHookLibrary.ModifierKeys.Control,IsChecked=true }, new ModifiersType { Name = "SHIFT", ModifierKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.Shift, IsChecked = true } },
                    Condition = 120// F9 

                };
                var BrightnessDownAllDevicesAutomation = new AutomationSettings {
                    Name = "Brightness Down",
                    Actions = brightnessDownAllDevices,
                    Modifiers = new List<IModifiersType> { new ModifiersType { Name = "CTRL", ModifierKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.Control, IsChecked = true }, new ModifiersType { Name = "SHIFT", ModifierKey = NonInvasiveKeyboardHookLibrary.ModifierKeys.Shift, IsChecked = true } },
                    ConditionTypeIndex = 0,//key stroke detection
                    Condition = 121// F10 

                };
                loadedAutomations.Add(BrightnessUpAllDevicesAutomation);
                loadedAutomations.Add(BrightnessDownAllDevicesAutomation);



            }
            else
            {
                var json = File.ReadAllText(JsonAutomationFileNameAndPath);

                var existedAutomation = JsonConvert.DeserializeObject<List<AutomationSettings>>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                foreach (var automation in existedAutomation)
                {
                    loadedAutomations.Add(automation);
                }
            }



            return loadedAutomations;
        }
        public Color[] LoadSolidColorIfExists()
        {

            if (!File.Exists(JsonSolidColorFileNameAndPath))
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
                WriteSolidColorJson();
            }
            else
            {
                var json = File.ReadAllText(JsonSolidColorFileNameAndPath);
                var existedSolidColor = JsonConvert.DeserializeObject<List<Color>>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                Color[] colors = new Color[existedSolidColor.Count];
                colors = existedSolidColor.ToArray();
                return colors;
            }


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



        }
        public async void ShowAddNewWindow()
        {

            //Device Catergory to group type of device in the add new window, this could be moved to add new window open code
            AvailableDeviceCatergoryToAdd = new ObservableCollection<IDeviceCatergory>();
            IDeviceCatergory ambinoBasic = new DeviceCatergory {
                Description = "Ambino Ambient Lighting Kit",
                Name = "AMBINO BASIC",
                Geometry = "ambinobasic",
                Devices = new DeviceSettings[] { DefaultDeviceCollection.ambinoBasic24,
                                                 DefaultDeviceCollection.ambinoBasic27,
                                                 DefaultDeviceCollection.ambinoBasic29,
                                                 DefaultDeviceCollection.ambinoBasic32,
                                                 DefaultDeviceCollection.ambinoBasic34

                }
            };
            IDeviceCatergory ambinoEDGE = new DeviceCatergory {
                Description = "Ambino Ambient Lighting Kit",
                Name = "AMBINO EDGE",
                Geometry = "ambinoedge",
                Devices = new DeviceSettings[] { DefaultDeviceCollection.ambinoEdge1m2,
                                                 DefaultDeviceCollection.ambinoEdge2m

                }
            };
            IDeviceCatergory ambinoHUB = new DeviceCatergory {
                Description = "Ambino HUB Collection",
                Name = "AMBINO HUB",
                Geometry = "ambinohub",
                Devices = new DeviceSettings[] { DefaultDeviceCollection.ambinoHUBV2,
                                                 DefaultDeviceCollection.ambinoFanHub,
                                                 DefaultDeviceCollection.ambinoHUBV3
                }
            };
            IDeviceCatergory ambinoRainPow = new DeviceCatergory {
                Description = "Ambino RainPow",
                Name = "AMBINO RAINPOW",
                Geometry = "generaldevice",
                Devices = new DeviceSettings[] { DefaultDeviceCollection.ambinoRainPow

                }
            };

            AvailableDeviceCatergoryToAdd.Add(ambinoBasic);
            AvailableDeviceCatergoryToAdd.Add(ambinoEDGE);
            AvailableDeviceCatergoryToAdd.Add(ambinoHUB);
            AvailableDeviceCatergoryToAdd.Add(ambinoRainPow);

          
          

            if (AssemblyHelper.CreateInternalInstance($"View.{"AddNewDeviceWindow"}") is System.Windows.Window window)
            {
                CurrentAddDeviceWizardState = 0;
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();

            }


            //System.Windows.Forms.Application.Restart();
            //    Process.GetCurrentProcess().Kill();


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

            var palettes = new List<IColorPalette>();
            foreach (var palette in AvailablePallete)
            {
                palettes.Add(palette);
            }
            var json = JsonConvert.SerializeObject(palettes, Formatting.Indented);
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonPaletteFileNameAndPath, json);

        }
        public void WriteAutomationCollectionJson()
        {

            var automations = new List<IAutomationSettings>();
            foreach (var automation in AvailableAutomations)
            {
                automations.Add(automation);
            }
            var json = JsonConvert.SerializeObject(automations, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });
            File.WriteAllText(JsonAutomationFileNameAndPath, json);

        }
        public void WriteSolidColorJson()
        {

            var colors = new List<Color>();
            foreach (var color in AvailableSolidColors)
            {
                colors.Add(color);
            }
            var json = JsonConvert.SerializeObject(colors, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonSolidColorFileNameAndPath, json);

        }
        public void WriteDeviceProfileCollection()
        {

            var profiles = new List<IDeviceProfile>();
            foreach (var profile in AvailableProfiles)
            {
                profiles.Add(profile);
            }
            var json = JsonConvert.SerializeObject(profiles, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto
            });
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonDeviceProfileFileNameAndPath, json);

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

                    IColorPalette importedPaletteCard = new ColorPalette(name, owner, "Imported from local file", description, color);
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
        private ObservableCollection<IDeviceProfile>ProfileFilter(IDeviceSettings device)
        {
            var filteredProfiles = new ObservableCollection<IDeviceProfile>();
            foreach (var profile in AvailableProfiles)
            {
                if (profile.DeviceType == device.DeviceType)
                {
                    filteredProfiles.Add(profile);
                }
            }
            return filteredProfiles;
        }
        public void GotoChild(IDeviceSettings selectedDevice)
        {

            SelectedVerticalMenuItem = MenuItems.FirstOrDefault(t => t.Text == lighting);
            IsDashboardType = false;
            CurrentDevice = selectedDevice;
            AvailableProfilesForCurrentDevice = new ObservableCollection<IDeviceProfile>();
            AvailableProfilesForCurrentDevice = ProfileFilter(CurrentDevice);
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

                    case nameof(CurrentDevice.ActivatedProfileIndex):
                        RaisePropertyChanged(nameof(CurrentSelectedProfile));
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
