using Ambinity.Stores;
using Ambinity.Views;
using Ambinity.Views.Screens.Dashboard;
using Ambinity.Views.Screens.DeviceControl;
using AmbinityCore.DataBase;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Ambinity
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        #region Properties


        private GeneralSettingsManager _generalSettingsManager;
        #endregion
        private void ConfigureIoc()
        {
            
            Ioc.Default.ConfigureServices(
                new ServiceCollection()
                    .AddSingleton<MainWindowViewModel>()
                    .AddSingleton<DeviceControlViewModel>()
                    .AddSingleton<DashboardViewModel>()
                    .AddSingleton<GeneralSettingsManager>()
                    .AddSingleton<NavigationStores>()
                    .BuildServiceProvider());
        }
        public override void OnFrameworkInitializationCompleted()
        {
            BindingPlugins.DataValidators.RemoveAt(0);
            ConfigureIoc();
            // load general settings
            _generalSettingsManager = Ioc.Default.GetRequiredService<GeneralSettingsManager>();
            var navigationStore = Ioc.Default.GetRequiredService<NavigationStores>();
            navigationStore.CurrentViewModel = Ioc.Default.GetRequiredService<DashboardViewModel>();
            var vm = Ioc.Default.GetRequiredService<MainWindowViewModel>();
            vm.CurrentViewModel.Init();
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = vm,
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}