using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Windows;
using AmbinityCore;
using AmbinityCore.Models.Device.Controller;
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

    public DeviceFirmwareSettingsViewModel(FirmwareService firmwareService, IDialogService dialogService)
    {
        _firmwareService = firmwareService;
        _dialogService = dialogService;
        CheckForUpdateCommand = new AsyncRelayCommand(CheckForUpdate);
        UpdateDeviceFirmwareCommand = new AsyncRelayCommand<FirmwareInformation>(UpdateDeviceFirmware);
    }

    private async Task UpdateDeviceFirmware(FirmwareInformation firmwareInformation)
    {
        //download selected firmware
        //get firmware file
        DownloadingFirmware = true;
        var downloadVm = new DownloadDialogViewModel("Downloading");
        _dialogService.ShowDownloadDialog(downloadVm, true);
        var fwPath = await _firmwareService.DownloadFirmware(firmwareInformation);
        await _firmwareService.UpdateFirmware(fwPath, firmwareInformation, _controller, downloadVm.ProgressInformation);
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

        AvailableFirmwares = availableFirmwares;
        AvailableFirmwares?.Reverse();

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