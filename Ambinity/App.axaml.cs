using Ambinity.Stores;
using Ambinity.Views;
using Ambinity.Views.Screens.CaptureEngine;
using Ambinity.Views.Screens.Dashboard;
using Ambinity.Views.Screens.DeviceControl;
using AmbinityCore.CaptureEngines;
using AmbinityCore.DataBase;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using HotAvalonia;
namespace Ambinity
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            this.EnableHotReload(); // Ensure this line **precedes** `AvaloniaXamlLoader.Load(this);`
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
                    .AddSingleton<RootNavigationStores>()
                    .AddSingleton<DesktopCapturingEngine>()
                    .AddTransient<ScreenCapturingViewModel>()
                    .BuildServiceProvider());
        }
        public override void OnFrameworkInitializationCompleted()
        {
            BindingPlugins.DataValidators.RemoveAt(0);
            ConfigureIoc();
            // load general settings
            _generalSettingsManager = Ioc.Default.GetRequiredService<GeneralSettingsManager>();
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = Ioc.Default.GetRequiredService<MainWindowViewModel>()
                };
            }
            var navigationStore = Ioc.Default.GetRequiredService<RootNavigationStores>();
            var dashboardViewModel = Ioc.Default.GetRequiredService<DashboardViewModel>();
            dashboardViewModel.Init();
            navigationStore.CurrentViewModel = dashboardViewModel;
            base.OnFrameworkInitializationCompleted();
        }
    }
}