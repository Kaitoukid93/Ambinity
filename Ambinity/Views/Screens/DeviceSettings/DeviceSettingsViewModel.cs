using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Stores;
using Ambinity.ViewModels;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;
using AmbinityServer.OnlineItem;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Screens.DeviceSettings;

public class DeviceSettingsViewModel : ViewModelBase
{
    public DeviceSettingsViewModel(RootNavigationStores navigationStores,
        DeviceFirmwareSettingsViewModel firmwareSettingsViewModel,
        DeviceHardwareLightingViewModel hardwareLightingViewModel,
        DevicePortConfigurationViewModel portConfigurationViewModel,
        DeviceConnectionSettingsViewModel connectionSettingsViewModel,
        DeviceCoolingSettingsViewModel coolingSettingsViewModel,ThumbnailService thumbnailService)
    {
        _thumbnailService = thumbnailService;
        _rootNavigationStores = navigationStores;
        PortConfigurationViewModel = portConfigurationViewModel;
        HardwareLightingViewModel = hardwareLightingViewModel;
        ConnectionSettingsViewModel = connectionSettingsViewModel;
        FirmwareSettingsViewModel = firmwareSettingsViewModel;
        CoolingSettingsViewModel = coolingSettingsViewModel;
    }


    private RootNavigationStores _rootNavigationStores;
    private ThumbnailService _thumbnailService;
    public  async Task<bool> Init(IController controller)
    {
        Controller = controller;
        Thumbnail = Controller.Thumbnail ??  _thumbnailService.GetThumbnail("null").Result;
        HasFanControl = Controller.FanController != null;

        OnPropertyChanged(nameof(HasFanControl));
        var result = await HardwareLightingViewModel.Init(controller);
        //init child viewmodel
        PortConfigurationViewModel.Init(controller);
        if(HasFanControl)
            CoolingSettingsViewModel.Init(controller);
        ConnectionSettingsViewModel.Init(controller);
        FirmwareSettingsViewModel.Init(controller);

        CommandSetup();
        if (!result)
        {
            //Dispatcher.UIThread.Invoke(BackToDashboard);
            return false;
        }

        return true;
    }

    private IController _controller;

    public IController Controller
    {
        get => _controller;
        set
        {
            _controller = value;
            OnPropertyChanged();
        }
    }

    private void CommandSetup()
    {
        BackToDashboardCommand = new RelayCommand(BackToDashboard);
    }

    private void BackToDashboard()
    {
        var vm = Ioc.Default.GetRequiredService<DeviceSettingsDashboardViewModel>();
        _rootNavigationStores.CurrentViewModel = vm;
        vm.Init();
    }

    public DeviceConnectionSettingsViewModel ConnectionSettingsViewModel { get; }
    public DeviceFirmwareSettingsViewModel FirmwareSettingsViewModel { get; }
    public DeviceHardwareLightingViewModel HardwareLightingViewModel { get; }
    public DevicePortConfigurationViewModel PortConfigurationViewModel { get; }
    public DeviceCoolingSettingsViewModel CoolingSettingsViewModel { get; }
    public ICommand BackToDashboardCommand { get; set; }
    public bool HasFanControl { get; set; }
    public Bitmap Thumbnail { get; set; }

    public override void Dispose()
    {
        ConnectionSettingsViewModel?.Dispose();
        CoolingSettingsViewModel?.Dispose();
        FirmwareSettingsViewModel?.Dispose();
        HardwareLightingViewModel?.Dispose();
        PortConfigurationViewModel?.Dispose();
    }
}
