using Ambinity.Stores;
using Ambinity.Views;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.Screens.CaptureEngine;
using Ambinity.Views.Screens.Dashboard;
using Ambinity.Views.Screens.ProfileEditor;
using Ambinity.Views.Screens.ProfileEditor.ZoneConfiguration;
using Ambinity.Views.SideMenu;
using Ambinity.Windows;
using AmbinityCore.CaptureEngines;
using AmbinityCore.CapturingService;
using AmbinityCore.Colors;
using AmbinityCore.DataBase;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Profile;
using AmbinityCore.Models.ProfileCategory;
using AmbinityCore.Repositories;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentAvalonia.Styling;
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
                    //Main view
                    .AddSingleton<MainWindowViewModel>()
                    .AddSingleton<DashboardViewModel>()
                    .AddSingleton<GeneralSettingsManager>()
                    .AddSingleton<RootNavigationStores>()
                    //Capturing Service
                    .AddSingleton<ScreenCapturingService>()
                    .AddSingleton<AudioCapturingService>()
                    //Side menu
                    .AddSingleton<SideMenuViewModel>()
                    .AddSingleton<LightingProfileRepository>()
                    .AddSingleton<LightingProfileCategoryRepository>()
                    //Dialogs
                    .AddSingleton<IDialogService,DialogService>()
                    //Profile editor
                    .AddSingleton<ProfileEditorViewModel>()
                    .AddSingleton<ZoneMappingViewModel>()
                    .AddSingleton<ZoneConfigurationViewModel>()
                    .AddSingleton<Draw2DCanvasViewModel>()
                    .AddSingleton<ZoneMappingToolsViewModel>()
                    .AddSingleton<LayersViewModel>()
                    //Repository singleton
                    .AddSingleton<SolidColorsRepository>()
                    .AddSingleton<ColorPaletteRepository>()
                    .AddSingleton<AnimationsRepository>()
                    .AddSingleton<GradientColorsRepository>()
                    .AddSingleton<GifImagesRepository>()
                    .AddSingleton<LightingZoneRepository>()
                    //
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
        }
    }
}