using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Windows;
using AmbinityCore;
using AmbinityCore.DataStream;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.Service;
using AmbinityServer.OnlineItem;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace Ambinity.Views.Screens.DeviceSettings;

public class DeviceFirmwareSettingsViewModel : ViewModelBase
{
    private FirmwareService _firmwareService;
    private SerialController _controller;
    public ICommand UpdateDeviceFirmwareCommand { get; set; }
    private List<FirmwareInformation> _availableFirmwares;
    private string _header = "You're up to date";
    private readonly IDialogService _dialogService;
    private SerialControllerRepository _controllerRepository;
    private IDataStream _serialStream;
    public DeviceFirmwareSettingsViewModel(FirmwareService firmwareService, IDialogService dialogService,SerialControllerRepository controllerRepository)
    {
        _serialControllerHelpers = new SerialControllerHelpers();
        _controllerRepository = controllerRepository;
        _firmwareService = firmwareService;
        _dialogService = dialogService;
        CheckForUpdateCommand = new AsyncRelayCommand(CheckForUpdate);
        UpdateDeviceFirmwareCommand = new AsyncRelayCommand<FirmwareInformation>(UpdateDeviceFirmware);
    }

    private async Task UpdateDeviceFirmware(FirmwareInformation firmwareInformation)
    {
        //download selected firmware
        //get firmware file
        _serialStream = _controllerRepository.GetSerialStream(_controller);
        await _serialStream?.Stop();
        DownloadingFirmware = true;
        var downloadVm = new DownloadDialogViewModel("Downloading");
        _dialogService.ShowDownloadDialog(downloadVm, true);
        var fwPath = await _firmwareService.DownloadFirmware(firmwareInformation);
        await _firmwareService.UpdateFirmware(fwPath, firmwareInformation, _controller, downloadVm.ProgressInformation);
        //update info
        var result = await _serialControllerHelpers.GetHardwareSettings(false, _controller);
        if (!result)
        {
            //device could need repower
        }
        DownloadingFirmware = false;
        UpdateAvailable = false;
    }

    private async Task CheckForUpdate()
    {
        CheckingForUpdate = true;
        Header = "Checking for update...";
        await Task.Delay(2000);
        var (updateAvailable, latestFirmware, availableFirmwares) = await _firmwareService.CheckForUpdate(_controller);
        if (updateAvailable)
        {
            UpdateAvailable = true;
            LatestFirmware = latestFirmware;
            Log.Information("Update available: " + latestFirmware.Version);
        }
        else
        {
            UpdateAvailable = false;
        }

        AvailableFirmwares = availableFirmwares.OrderByDescending(f=>f.ReleaseDate).ToList();

        Header = "You're up to date";
        CheckingForUpdate = false;
    }

    public void Init(IController controller)
    {
        if (controller is OpenRGBController)
        {
            IsAvailable = false;
            return;
        }
        _controller = controller as SerialController;
        UpdateAvailable = false;
        AvailableFirmwares = new List<FirmwareInformation>();
        _controller = controller as SerialController;
        IsAvailable = true;
    }


    public ICommand CheckForUpdateCommand { get; }
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

    private FirmwareInformation _latestFirmware;

    public FirmwareInformation LatestFirmware
    {
        get => _latestFirmware;
        set
        {
            _latestFirmware = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LatestFirmwareString));
        }
    }

    public string LatestFirmwareString => "Firmware version: " + LatestFirmware.Version;

    private bool _updateAvailable;

    public bool UpdateAvailable
    {
        get => _updateAvailable;
        set
        {
            _updateAvailable = value;
            OnPropertyChanged();
        }
    }

    private bool _checkingForUpdate;

    public bool CheckingForUpdate
    {
        get => _checkingForUpdate;
        set
        {
            _checkingForUpdate = value;
            OnPropertyChanged();
        }
    }

    private bool _downloadingFirmware;
    private readonly SerialControllerHelpers _serialControllerHelpers;

    public bool DownloadingFirmware
    {
        get => _downloadingFirmware;
        set
        {
            _downloadingFirmware = value;
            OnPropertyChanged();
        }
    }

    public string Header
    {
        get => _header;
        set
        {
            _header = value;
            OnPropertyChanged();
        }
    }


    public List<FirmwareInformation> AvailableFirmwares
    {
        get => _availableFirmwares;
        set
        {
            _availableFirmwares = value;
            OnPropertyChanged();
        }
    }
}