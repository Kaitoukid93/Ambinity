using System.Collections.Generic;
using Ambinity.ViewModels;
using AmbinityCore.DataBase;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.Service;
using AmbinityCore.Models.GeneralSetting;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using FluentAvalonia.Styling;

namespace Ambinity.Views.Screens.DeviceSettings;

public class DeviceSettingsInfoBarViewModel : ViewModelBase
{
    
    public DeviceSettingsInfoBarViewModel(SerialControllerRepository controllerRepository, GeneralSettingsManager settingsManager,
        SerialControllerDiscoveryService discoveryService)
    {
        _generalSettings = settingsManager.Settings;
        _controllerRepository = controllerRepository;
        _discoveryService = discoveryService;
        _discoveryService.NewComportDetected += OnNewComPortDetected;
        _controllerRepository.OldDeviceDetected += OnOldSerilaPortDetected;
        _controllerRepository.ControllerDisconnected += OnSerialControllerDisconnected;
        _controllerRepository.LoadingFromDisk += OnLoadingFromDisk;
    }

    private void OnLoadingFromDisk(IController controller)
    {
        if (IsOpen)
            IsOpen = false;
        Dispatcher.UIThread.Invoke(() => ForeGround = new SolidColorBrush(_generalSettings.PrimaryColor));
        Title = "Loading...";
        string content = controller.SerialPort;
        IsLoading = true;
        Content = controller.Name;
        SubContent = content;
        IsOpen = true;
    }

    private void OnSerialControllerDisconnected(IController controller)
    {
        if (IsOpen)
            IsOpen = false;
        Title = "Disconnected!";
        string content = controller.SerialPort;
        Dispatcher.UIThread.Invoke(() => ForeGround = new SolidColorBrush(Colors.Red));
        IsLoading = false;
        Content = controller.Name;
        SubContent = content;
        IsOpen = true;
    }

    private void OnOldSerilaPortDetected(IController controller)
    {
        if (IsOpen)
            IsOpen = false;
        Dispatcher.UIThread.Invoke(() => ForeGround = new SolidColorBrush(_generalSettings.PrimaryColor));
        Title = "Connecting...";
        string content = controller.SerialPort;
        IsLoading = true;
        Content = controller.Name;
        SubContent = content;
        IsOpen = true;
    }

    private void OnNewComPortDetected(string port)
    {
        if (IsOpen)
            IsOpen = false;
        ForeGround = new SolidColorBrush(_generalSettings.PrimaryColor);
        Title = "Compatible Device Detected";
        string content = port;
        IsLoading = true;

        Content = "Serial Device";
        SubContent = content;
        IsOpen = true;
    }

    private SerialControllerRepository _controllerRepository;
    private SerialControllerDiscoveryService _discoveryService;
    
    private bool _isOpen;
    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            _isOpen = value;
            OnPropertyChanged();
        }
    }

    private string _title;

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged();
        }
    }
    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }
    private string _content;

    public string Content
    {
        get => _content;
        set
        {
            _content = value;
            OnPropertyChanged();
        }
    }
    private string _subContent;

    public string SubContent
    {
        get => _subContent;
        set
        {
            _subContent = value;
            OnPropertyChanged();
        }
    }

    private SolidColorBrush _foreGround = new SolidColorBrush(Colors.Red);
    private readonly IGeneralSettings _generalSettings;

    public SolidColorBrush ForeGround
    {
        get => _foreGround;
        set
        {
            _foreGround = value;
            OnPropertyChanged();
        }
    }
}