using Ambinity.Stores;
using Ambinity.Views;
using Ambinity.Views.Screens.CaptureEngine;
using Ambinity.Views.Screens.Dashboard;
using Ambinity.Views.Screens.DeviceControl;
using Ambinity.Views.Screens.DeviceControl.DeviceCanvas;
using AmbinityCore.CaptureEngines;
using AmbinityCore.DataBase;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentAvalonia.Styling;
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
        private FluentAvaloniaTheme _faTheme;

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
                    .AddSingleton<DeviceCanvasViewModel>()
                    .BuildServiceProvider());
        }
        private void UpdateAppAccentColor(Color? color)
        {
            _faTheme.CustomAccentColor = color;
        }
        public override void OnFrameworkInitializationCompleted()
        {
            
            BindingPlugins.DataValidators.RemoveAt(0);
            ConfigureIoc();
            // load general settings
            _generalSettingsManager = Ioc.Default.GetRequiredService<GeneralSettingsManager>();
         
            //set theme
            
             _faTheme = App.Current.Styles[0] as FluentAvaloniaTheme;
            UpdateAppAccentColor(_generalSettingsManager.Settings.PrimaryColor);
            

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = Ioc.Default.GetRequiredService<MainWindowViewModel>()
                };
            }
            var navigationStore = Ioc.Default.GetRequiredService<RootNavigationStores>();
            var dasboardViewModel = Ioc.Default.GetRequiredService<DashboardViewModel>();
            dasboardViewModel.Init();
            navigationStore.CurrentViewModel = dasboardViewModel;
            base.OnFrameworkInitializationCompleted();
        }
    }
}