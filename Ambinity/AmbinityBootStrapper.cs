using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Ambinity.Services;
using Ambinity.Stores;
using Ambinity.Views;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.LayoutEditor;
using Ambinity.Views.Root;
using Ambinity.Views.Screens.Dashboard;
using Ambinity.Views.Screens.DeviceLayout;
using Ambinity.Views.Screens.DeviceSettings;
using Ambinity.Views.Screens.ProfileEditor;
using Ambinity.Views.SideMenu;
using Ambinity.Views.SplashScreen;
using Ambinity.Windows;
using AmbinityCore.CapturingService;
using AmbinityCore.Colors;
using AmbinityCore.DataBase;
using AmbinityCore.Helpers;
using AmbinityCore.LightingEngines;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.Device;
using AmbinityCore.Models.Device.Provider;
using AmbinityCore.Models.Device.Service;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Profile;
using AmbinityCore.Models.ProfileCategory;
using AmbinityCore.Repositories;
using AmbinityServer;
using AmbinityServer.OnlineItem;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using Draw2D.Core.Graphic;
using FluentAvalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RootViewModel = Ambinity.Views.Root.RootViewModel;

namespace Ambinity;

public class AmbinityBootStrapper
{
    private string JsonPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Ambinity\\");

    private string ResourceLocalFolderpath => Path.Combine(JsonPath, "Resources");

    #region Properties

    private static KnownTypesBinder _knownTypeBinders { get; set; }
    private static GeneralSettingsManager _generalSettingsManager;
    private static FluentAvaloniaTheme _faTheme;
    private IDialogService _dialogService;
    private static Application? _application;
    private static IWindowService _windowService;

    #endregion

    public static async void Initialize(Application application)
    {
        _application = application;
        //register all Services and ViewModels
        ConfigureIoc();
        //set theme and color
        ConfigureTheme();
        //configure json settings for all Serialize and Deserialize action ( this need for legacy adrilight json)
        ConfigureJson();
        if (_application.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;
        // Don't shut down when the last window closes, just minimize to the system tray
        desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        // Show Splash screen
        _windowService = Ioc.Default.GetRequiredService<IWindowService>();
        _splashViewModel = ShowSplashScreen();
        // Configuring core service
        await ConfigureCoreService(_splashViewModel);
        // Close Splash screen
        _splashView?.Close();
        // Activate MainWindow;
        RootViewModel rootViewModel = Ioc.Default.GetRequiredService<RootViewModel>();
        // Tray icon using rootviewmodel instead of mainviewmodel as adrilight
        _application.DataContext = rootViewModel;
    }

    private static SplashViewModel _splashViewModel;
    private static Window _splashView;

    private static SplashViewModel ShowSplashScreen()
    {
        _splashView = _windowService.ShowWindow(out SplashViewModel vm);
        return vm;
    }

    private static void CloseSplashScreen()
    {
    }

    private static void ConfigureJson()
    {
        _knownTypeBinders = new KnownTypesBinder();
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            { TypeNameHandling = TypeNameHandling.Objects, SerializationBinder = _knownTypeBinders };
    }

    private static void ConfigureTheme()
    {
        _generalSettingsManager = Ioc.Default.GetRequiredService<GeneralSettingsManager>();
        _faTheme = App.Current?.Styles[0] as FluentAvaloniaTheme;
        UpdateAppAccentColor(_generalSettingsManager.Settings.PrimaryColor);
    }

    private static void ConfigureIoc()
    {
        var mainFrameBuffer = new FrameBuffer(500, 250);
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                //Main view
                .AddSingleton<IMainWindowService, MainWindowService>()
                .AddSingleton<IWindowService, WindowService>()
                .AddSingleton<RootViewModel>()
                .AddSingleton<DashboardViewModel>()
                .AddSingleton<GeneralSettingsManager>()
                .AddSingleton<RootNavigationStores>()
                .AddSingleton<DeviceSettingsViewModel>()
                .AddSingleton<DeviceSettingsDashboardViewModel>()
                .AddSingleton<DeviceSettingsInfoBarViewModel>()
                //splash
                .AddSingleton<SplashViewModel>()
                //layout editor
                .AddSingleton<DeviceLayoutEditorViewModel>()
                .AddSingleton<ProfileEditorViewModel>()
                .AddSingleton<LayoutCanvasViewModel>()
                .AddSingleton<RightPanelAssetsViewModel>()
                //device layout
                .AddSingleton<DevicePropertiesViewModel>()
                //Capturing Service
                .AddSingleton<ScreenCapturingService>()
                .AddSingleton<AudioCapturingService>()
                .AddSingleton<CapturingServiceProvider>()
                //lighting engine
                .AddTransient<ColorPaletteEngine>()
                .AddTransient<StaticColorEngine>()
                .AddTransient<ScreenCaptureEngine>()
                .AddTransient<AnimationEngine>()
                .AddTransient<GifxelationEngine>()
                .AddTransient<MusicReactiveEngine>()
                //Side menu
                .AddSingleton<SideMenuViewModel>()
                .AddSingleton<LightingProfileRepository>()
                .AddSingleton<LightingProfileCategoryRepository>()
                .AddSingleton<SideMenuProfilePlayerViewModel>()
                //Dialogs
                .AddSingleton<IDialogService, DialogService>()
                //Profile editor
                .AddSingleton<Draw2DCanvasViewModel>()
                .AddSingleton<FigureContextMenuProvider>()
                .AddSingleton<ToolsViewModel>()
                .AddSingleton<LayersViewModel>()
                .AddSingleton<RightPanelViewModel>()
                .AddSingleton<ZonePropertiesViewModel>()
                .AddTransient<LayersView>()
                .AddSingleton<ZoneMappingRenderControllerViewModel>()
                //profile Decoder
                .AddSingleton(mainFrameBuffer)
                .AddSingleton<LightingProfileDecoder>()
                .AddSingleton<ColorEngineProvider>()
                //Repository singleton
                .AddSingleton<StaticColorsRepository>()
                .AddSingleton<ColorPaletteRepository>()
                .AddSingleton<AnimationsRepository>()
                .AddSingleton<GradientColorsRepository>()
                .AddSingleton<GifImagesRepository>()
                .AddSingleton<LightingZoneRepository>()
                .AddSingleton<SerialControllerDiscoveryService>()
                .AddSingleton<SerialControllerProvider>()
                .AddSingleton<SerialControllerRepository>()
                .AddSingleton<AmbinityDeviceLayoutRepository>()
                .AddSingleton<AmbinityDeviceOnlineRepository>()
                .AddSingleton<LightingZoneOnlineRepository>()
                //Server
                .AddSingleton<AmbinityClient>()
                .AddSingleton<ThumbnailService>()
                .BuildServiceProvider());
    }

    private static void UpdateAppAccentColor(Color? color)
    {
        _faTheme.CustomAccentColor = color;
    }

    private static async Task ConfigureCoreService(SplashViewModel splashViewModel)
    {
        //Load All repository
        var colorPaletteRepository = Ioc.Default.GetRequiredService<ColorPaletteRepository>();
        var solidColorsRepository = Ioc.Default.GetRequiredService<StaticColorsRepository>();
        var gifImagesRepository = Ioc.Default.GetRequiredService<GifImagesRepository>();
        var animationsRepository = Ioc.Default.GetRequiredService<AnimationsRepository>();
        var lightingZoneRepository = Ioc.Default.GetRequiredService<LightingZoneRepository>();
        var lightingProfileRepository = Ioc.Default.GetRequiredService<LightingProfileRepository>();
        var lightingProfileCategoryRepository = Ioc.Default.GetRequiredService<LightingProfileCategoryRepository>();
        var serialControllerRepository = Ioc.Default.GetRequiredService<SerialControllerRepository>();
        var ambinityDeviceLayoutRepository = Ioc.Default.GetRequiredService<AmbinityDeviceLayoutRepository>();
        var ambinityClient = Ioc.Default.GetRequiredService<AmbinityClient>();
        splashViewModel.Progress = 5;
        await Task.Run(async () =>
        {
            if (!ambinityClient.Init())
            {
                splashViewModel.Status = "Ambinity server is not available";
                await Task.Delay(2000);
            }
            
            splashViewModel.Status = "Downloading assets";
            if(!ambinityClient.Init());
            //throw
            await ambinityClient.DownloadAssets(null);
            splashViewModel.Progress = 15;
            colorPaletteRepository.Init();
            await Task.Delay(100);
            splashViewModel.Progress = 25;
            solidColorsRepository.Init();
            await Task.Delay(100);
            splashViewModel.Progress = 35;
            gifImagesRepository.Init();
            await Task.Delay(100);
            splashViewModel.Progress = 45;
            animationsRepository.Init();
            await Task.Delay(100);
            splashViewModel.Progress = 55;
            lightingZoneRepository.Init();
            lightingProfileRepository.Init();
            await Task.Delay(100);
            splashViewModel.Progress = 65;
            lightingProfileCategoryRepository.Init();
            await Task.Delay(100);
            splashViewModel.Progress = 75;
            serialControllerRepository.Init();
            await Task.Delay(100);
            splashViewModel.Progress = 85;
            ambinityDeviceLayoutRepository.Init();
        });
        //run the profile decoder for rendering to device
        var profileDecoder = Ioc.Default.GetRequiredService<LightingProfileDecoder>();
        //start Serial controller repository to load controllers and devices
        await Task.Delay(1000);
        splashViewModel.Progress = 100;
        //start OpenRGB controller repository
    }
}