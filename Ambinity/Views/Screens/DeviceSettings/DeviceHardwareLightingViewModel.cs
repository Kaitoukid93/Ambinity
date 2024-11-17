using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using Ambinity.Views.LayoutEditor;
using Ambinity.Windows;
using AmbinityCore.DataStream;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.Service;
using AmbinityCore.Repositories;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Screens.DeviceSettings;

public class DeviceHardwareLightingViewModel : ViewModelBase
{
    private SerialControllerDiscoveryService _discoveryService;
    private SerialControllerHelpers _serialControllerHelpers;
    private SerialControllerRepository _controllerRepository;
    private IDataStream _serialStream;
    private IDialogService _dialogService;
    public event Action OpenFlyoutEvent;
    public event Action CloseFlyoutEvent;

    public DeviceHardwareLightingViewModel(SerialControllerDiscoveryService discoveryService,
        SerialControllerRepository controllerRepository, ColorPalettesLibraryViewModel colorPalettesLibraryViewModel,
        IDialogService dialogService)
    {
        _dialogService = dialogService;
        _colorPalettesLibraryViewModel = colorPalettesLibraryViewModel;
        _discoveryService = discoveryService;
        _controllerRepository = controllerRepository;
        _serialControllerHelpers = new SerialControllerHelpers();
        AvailableHWLModes = new List<string>() { "Static Color", "Breathing", "Color Palette", "Disabled" };
        OpenColorPaletteLibraryCommand = new AsyncRelayCommand(OpenColorPaletteLibrary);
        ApplyHardwareSettingsCommand = new AsyncRelayCommand(ApplyHardwareSettings);
    }

    private async Task ApplyHardwareSettings()
    {
        _serialStream?.Stop();
        var dialogvm = new LoadingDialogViewModel();
        _dialogService.ShowLoadingDialog(dialogvm, "Applying");
        await _serialStream?.Stop();
        var result = await _serialControllerHelpers.SendHardwareSettings(_controller as SerialController);
        if (result)
        {
            dialogvm.ShowSuccess("Settings saved to device memory");
        }
        else
        {
            dialogvm.ShowError("Error when applying hardware settings");
        }
        _serialStream.Init();
    }

    private async Task OpenColorPaletteLibrary()
    {
        CurrentFlyoutViewModel = _colorPalettesLibraryViewModel;
        CurrentFlyoutViewModel.ItemSelected += OnColorPaletteSelected;
        CurrentFlyoutViewModel?.Init();
        OpenFlyoutEvent?.Invoke();
    }

    public void OnFlyoutClosing()
    {
        CurrentFlyoutViewModel.Dispose();
        CurrentFlyoutViewModel = null;
    }

    private void OnColorPaletteSelected(AssetItemViewModelBase item)
    {
        if (item is AssetItemViewModelBase paletteAsset)
        {
            var palette = paletteAsset.Item as ColorPalette;
            var usableColor = palette.Resize(8);
            SelectedPalette = new ColorPaletteAssetViewModel(new ColorPalette(usableColor));
            _ledHardwareSettings.HWL_palette = SelectedPalette.Colors.ToArray();
        }
    }

    public async Task<bool> Init(IController controller)
    {
        _discoveryService.Hold();
        //wait for discovery service to stop
        await Task.Delay(1000);
        _controller = controller;
        if (controller is OpenRGBController)
        {
            IsAvailable = false;
            await Task.Delay(1000);
            return true;
        }

        _serialStream = _controllerRepository.GetSerialStream(controller);
        await _serialStream?.Stop();
        var result = await _serialControllerHelpers.GetHardwareSettings(false, _controller as SerialController);
        if (!result)
        {
            //ShowDeviceConnectionErrorDialog();
            IsAvailable = false;
        }
        else
        {
            IsAvailable = true;
            _ledHardwareSettings = _controller.LedController.HardwareSettings as SerialLEDControllerHardwareSettings;
            if (HasFanControl)
                _fanHardwareSettings = _controller.FanController.HardwareSettings;
            if (!_ledHardwareSettings.HWL_enable)
                _selectedHWLMode = AvailableHWLModes.Where(m => GetHWLMode(m) == 3)
                    .FirstOrDefault();
            else
            {
                SelectedHWLMode = AvailableHWLModes.Where(m => GetHWLMode(m) == _ledHardwareSettings.HWL_effectMode)
                    .FirstOrDefault();
            }

            SelectedPalette = new ColorPaletteAssetViewModel(new ColorPalette(_ledHardwareSettings.HWL_palette));
            SelectedColor = _ledHardwareSettings.HWL_singleColor;
        }
        
        OnPropertyChanged(nameof(LedHardwareSettings));
        OnPropertyChanged(nameof(EnableHWLExpand));
        OnPropertyChanged(nameof(HasFanControl));
        _serialStream.Init();
        return IsAvailable;
    }

    private void ShowDeviceConnectionErrorDialog()
    {
        var vm = new ErrorDialogViewModel();
        vm.ErrorMessage = "Device is disconnected or not supported";
        Dispatcher.UIThread.Invoke(() => _dialogService.ShowErrorDialog(vm, "Device disconnected", "Return"));
    }

    public List<string> AvailableHWLModes { get; set; }
    private bool _isAvailable;

    public bool IsAvailable
    {
        get => _isAvailable;
        set
        {
            _isAvailable = value;
            OnPropertyChanged();
        }
    }

    private IController _controller;
    private SerialLEDControllerHardwareSettings _ledHardwareSettings;
    private SerialFanControllerHardwareSettings _fanHardwareSettings;
    public SerialLEDControllerHardwareSettings LedHardwareSettings => _ledHardwareSettings;
    public SerialFanControllerHardwareSettings FanHardwareSettings => _fanHardwareSettings;
    private string _selectedHWLMode;

    public string SelectedHWLMode
    {
        get => _selectedHWLMode;
        set
        {
            _selectedHWLMode = value;
            SetHWLMode();
            OnPropertyChanged();
        }
    }

    private void SetHWLMode()
    {
        switch (_selectedHWLMode)
        {
            case "Color Palette":
                _ledHardwareSettings.HWL_enable = true;
                _ledHardwareSettings.HWL_effectMode = 1;
                EnableColorSpeed = true;
                EnableColorIntensity = true;
                EnableColorPalette = true;
                break;
            case "Static Color":
                _ledHardwareSettings.HWL_enable = true;
                EnableColorSpeed = false;
                EnableColorIntensity = false;
                EnableColorPalette = false;
                _ledHardwareSettings.HWL_effectMode = 0;
                break;
            case "Breathing":
                _ledHardwareSettings.HWL_enable = true;
                EnableColorSpeed = true;
                EnableColorIntensity = false;
                EnableColorPalette = false;
                _ledHardwareSettings.HWL_effectMode = 2;
                break;
            case "Disabled":
                _ledHardwareSettings.HWL_enable = false;
                break;
        }

        OnPropertyChanged(nameof(EnableHWLExpand));
    }

    private byte GetHWLMode(string mode)
    {
        switch (mode)
        {
            case "Color Palette":
                return 1;
                break;
            case "Static Color":
                return 0;
                break;
            case "Breathing":
                return 2;
                break;
            case "Disabled":
                return 3;
                break;
            default: return 0;
        }
    }

    public bool EnableHWLExpand => _ledHardwareSettings.HWL_enable;
    private bool _enableColorIntensity;

    public bool EnableColorIntensity
    {
        get => _enableColorIntensity;
        set
        {
            _enableColorIntensity = value;
            OnPropertyChanged();
        }
    }

    private bool _enableColorSpeed;

    public bool EnableColorSpeed
    {
        get => _enableColorSpeed;
        set
        {
            _enableColorSpeed = value;
            OnPropertyChanged();
        }
    }

    private bool _enableColorPalette;
    private readonly ColorPalettesLibraryViewModel _colorPalettesLibraryViewModel;

    public bool EnableColorPalette
    {
        get => _enableColorPalette;
        set
        {
            _enableColorPalette = value;
            OnPropertyChanged();
        }
    }

    private ColorPaletteAssetViewModel _selectedPalette;

    public ColorPaletteAssetViewModel SelectedPalette
    {
        get => _selectedPalette;
        set
        {
            _selectedPalette = value;
            OnPropertyChanged();
        }
    }

    private Color _selectedColor;

    public Color SelectedColor
    {
        get => _selectedColor;
        set
        {
            _selectedColor = value;
            _ledHardwareSettings.HWL_singleColor = value;
            OnPropertyChanged();
        }
    }

    public LibraryViewModelBase CurrentFlyoutViewModel { get; set; }
    public ICommand OpenColorPaletteLibraryCommand { get; set; }
    public ICommand ApplyHardwareSettingsCommand { get; set; }
    public bool HasFanControl => _controller.FanController != null && IsAvailable;

    public override void Dispose()
    {
        //serial stream must be enabled right away
        //re-enable discovery service after 5s to prevent user spam click
        _discoveryService.Resume();
        if (_colorPalettesLibraryViewModel != null)
            _colorPalettesLibraryViewModel.ItemSelected -= OnColorPaletteSelected;
    }
}