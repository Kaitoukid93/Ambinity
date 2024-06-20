using System.Collections.ObjectModel;
using System.Windows.Input;
using Ambinity.Commands;
using Ambinity.Services;
using Ambinity.Stores;
using Ambinity.ViewModels;
using Ambinity.Views.Screens.Dashboard;
using AmbinityCore.DataBase;
using AmbinityCore.Models;
using AmbinityCore.Models.Device;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views.Screens.DeviceControl;

public class DeviceControlViewModel : ViewModelBase
{
    #region Construct

    public DeviceControlViewModel(NavigationStores navigationStores,GeneralSettingsManager generalSettingsManager)
    {
        _navigationStores = navigationStores;
        GeneralSettingsManager = generalSettingsManager;
    }

    #endregion

    #region Events

    #endregion

    #region Properties
    
    private IDevice _device;
    private NavigationStores _navigationStores;
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
        var vm = Ioc.Default.GetRequiredService<DashboardViewModel>();
        BackToDashBoardCommand = new NavigateCommand<DashboardViewModel>(
            new NavigationService<DashboardViewModel>(_navigationStores,vm));
    }
    public void Init(IDevice device)
    {
        AvailableControlModes = new ObservableCollection<string>()
        {
            "Zoe",
            "123",
            "456",
            "The Quick Brown Fox"
        };
        CommandSetup();
        Device = device;
    }

    #endregion

    #region Commands

    public ICommand BackToDashBoardCommand { get; set; }

    #endregion
}