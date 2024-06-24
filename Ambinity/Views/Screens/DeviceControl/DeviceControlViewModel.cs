using System.Collections.ObjectModel;
using System.Windows.Input;
using Ambinity.Stores;
using Ambinity.ViewModels;
using Ambinity.Views.Screens.CaptureEngine;
using Ambinity.Views.Screens.Dashboard;
using AmbinityCore.DataBase;
using AmbinityCore.Models;
using AmbinityCore.Models.Device;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Screens.DeviceControl;

public class DeviceControlViewModel : ViewModelBase
{
    #region Construct

    public DeviceControlViewModel(RootNavigationStores rootNavigationStores,
        GeneralSettingsManager generalSettingsManager)
    {
        _rootNavigationStores = rootNavigationStores;
        GeneralSettingsManager = generalSettingsManager;
    }

    #endregion

    #region Events

    #endregion

    #region Properties

    private IDevice _device;
    private RootNavigationStores _rootNavigationStores;
    public ObservableCollection<string> AvailableControlModes { get; set; }

    public IDevice Device
    {
        get { return _device; }
        set
        {
            _device = value;
            OnPropertyChanged();
        }
    }

    private GeneralSettingsManager _generalSettingsManager;

    public GeneralSettingsManager GeneralSettingsManager
    {
        get { return _generalSettingsManager; }
        set
        {
            _generalSettingsManager = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    private void CommandSetup()
    {
        BackToDashBoardCommand = new RelayCommand(BackToDashBoard);
        OpenScreenCaptureSettingsCommand = new RelayCommand(OpenScreenCaptureSettings);

    }

    private void BackToDashBoard()
    {
        var vm = Ioc.Default.GetRequiredService<DashboardViewModel>();
        vm.Init();
        _rootNavigationStores.CurrentViewModel = vm;
    }

    private void OpenScreenCaptureSettings()
    {
        var factory = new PageFactory();
        var vm = Ioc.Default.GetRequiredService<ScreenCapturingViewModel>();
        var screencaptureview = factory.GetPageFromObject(vm);
        var wd = new CaptureEngineWindow();
        wd.content.Content = screencaptureview;
        wd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        wd.Closed += (sender, args) => vm.Dispose();
        wd.Show();
    }

    public void Init(object device)
    {
        AvailableControlModes = new ObservableCollection<string>()
        {
            "VU Metter",
            "Brightness",
            "Dance"
        };
        CommandSetup();
        Device = device as IDevice;
    }

    #endregion

    #region Commands

    public ICommand BackToDashBoardCommand { get; set; }
    public ICommand OpenScreenCaptureSettingsCommand { get; set; }

    #endregion
}