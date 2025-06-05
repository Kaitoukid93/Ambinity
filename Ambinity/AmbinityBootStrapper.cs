using System.IO;
using System.Threading.Tasks;
using Ambinity.AppResource;
using Ambinity.QuickAccess;
using Ambinity.Services;
using Ambinity.Stores;
using Ambinity.SystemUtilities;
using Ambinity.Views.AmbinityStore;
using Ambinity.Views.AppTour;
using Ambinity.Views.Configuration.ColorConfiguration;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using Ambinity.Views.Configuration.PositionConfiguration;
using Ambinity.Views.Debug;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.LayoutEditor;
using Ambinity.Views.LayoutEditor.Canvas;
using Ambinity.Views.LayoutEditor.LEDLayoutCreator;
using Ambinity.Views.LayoutEditor.RightPanel.PropertiesView;
using Ambinity.Views.NonClientArea;
using Ambinity.Views.OnlineStore.Library;
using Ambinity.Views.Screens.AppSettings;
using Ambinity.Views.Screens.DeviceLayout;
using Ambinity.Views.Screens.DeviceLayout.Library;
using Ambinity.Views.Screens.DeviceSettings;
using Ambinity.Views.Screens.Home;
using Ambinity.Views.Screens.ProfileEditor;
using Ambinity.Views.Screens.ProfileEditor.Library;
using Ambinity.Views.SideMenu;
using Ambinity.Views.SplashScreen;
using Ambinity.Windows;
using AmbinityCore.CapturingService;
using AmbinityCore.CapturingService.AudioCapturing;
using AmbinityCore.Colors;
using AmbinityCore.DataBase;
using AmbinityCore.Helpers;
using AmbinityCore.LightingEngines;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.Device;
using AmbinityCore.Models.Device.Provider;
using AmbinityCore.Models.Device.Service;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Profile;
using AmbinityCore.Models.ProfileCategory;
using AmbinityCore.OpenRGB;
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
using LibreHardwareMonitor.Software;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using Constants = AmbinityCore.Constants;
using OperatingSystem = System.OperatingSystem;
using RootViewModel = Ambinity.Views.Root.RootViewModel;

namespace Ambinity;

public class AmbinityBootStrapper
{
    #region Properties

    private static KnownTypesBinder _knownTypeBinders { get; set; }
    private static GeneralSettingsManager _generalSettingsManager;

    private IDialogService _dialogService;
    private static Application? _application;
    private static IWindowService _windowService;
    private static AppThemeManager _appThemeManager;

    #endregion
    public static async void Initialize(Application application)
    {
        _application = application;
        //register all Services and ViewModels
        ConfigureIoc();
        //setup debug logging
        SetupDebugLogging();
        //get settings
        _generalSettingsManager = Ioc.Default.GetRequiredService<GeneralSettingsManager>();
        //register auto starts
        // ConfigureAutoStart();
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
        _appThemeManager = Ioc.Default.GetRequiredService<AppThemeManager>();
        _appThemeManager.UpdateAppAccentColor(_generalSettingsManager.Settings.PrimaryColor);
        _appThemeManager.SetAppTheme(_generalSettingsManager.Settings.SelectedTheme);


    }

    // private static void ConfigureAutoStart()
    // {
    //     if (_generalSettingsManager.Settings.AutoStart)
    //     {
    //         StartUpManager.AddApplicationToTaskScheduler("Ambinity Startup Task",_generalSettingsManager.Settings.AutoStartDelay);
    //     }
    // }
    private static void ConfigureIoc()
    {
        //create default frame buffer
        var mainFrameBuffer = new FrameBuffer(750, 500);
        var serviceCollection = new ServiceCollection()
            .AddSingleton<AppThemeManager>()
            .AddSingleton<IMainWindowService, MainWindowService>()
            .AddSingleton<IWindowService, WindowService>()
            .AddSingleton<RootViewModel>()
            .AddSingleton<GeneralSettingsManager>()
            .AddSingleton<RootNavigationStores>()
            .AddSingleton<AppSettingsViewModel>()
            .AddSingleton<UpdateService>()
            .AddSingleton<DeviceSettingsViewModel>()
            .AddSingleton<DeviceSettingsDashboardViewModel>()
            .AddSingleton<HomeViewModelFactory>()
            .AddSingleton<HomeViewModel>()
            .AddSingleton<DeviceFirmwareSettingsViewModel>()
            .AddSingleton<DeviceHardwareLightingViewModel>()
            .AddSingleton<DeviceCoolingSettingsViewModel>()
            .AddSingleton<DeviceConnectionSettingsViewModel>()
            .AddSingleton<DevicePortConfigurationViewModel>()
            .AddSingleton<DeviceSettingsInfoBarViewModel>()
            .AddSingleton<PortDetailViewModel>()
            .AddSingleton<AmbinityDeviceViewModelFactory>()
            .AddSingleton<NonClientAreaContentViewModel>()
            .AddSingleton<AppTourViewModel>()
            .AddSingleton<AppTourElementProvider>()
            //system tray
            .AddSingleton<SystemTrayFlyoutWindowViewModel>()
            .AddSingleton<QuickAccessViewModel>()
            .AddSingleton<QuickAccessNavigationStore>()
            .AddSingleton<ShortcutPageViewModel>()
            .AddSingleton<DevicesPageViewModel>()
            .AddSingleton<QuickAccessViewModelFactory>()
            .AddSingleton<ShortcutEditorViewModel>()
            .AddSingleton<LightingProfilePlayerWidgetViewModel>()
            //splash
            .AddSingleton<SplashViewModel>()
            //layout editor
            .AddSingleton<CanvasViewModelFactory>()
            .AddSingleton<DeviceLayoutEditorViewModel>()
            .AddSingleton<CanvasViewModelBase, DeviceLayoutCanvasViewModel>()
            .AddSingleton<ProfileEditorViewModel>()
            .AddSingleton<ProfileEditorRightPanelViewModel>()
            .AddSingleton<ParameterViewModelFactory>()
            .AddSingleton<ColorConfigurationViewModelFactory>()
            .AddTransient<PositionConfigurationViewModel>()
            .AddSingleton<ColorPaletteAssetsViewModel>()
            .AddSingleton<LightingZonesLibraryViewModel>()
            .AddSingleton<DeviceLayoutAssetsViewModel>()
            .AddSingleton<LightingZoneAssetsViewModel>()
            .AddSingleton<AnimationAssetsViewModel>()
            .AddSingleton<LightingProfileAssetsViewModel>()
            .AddSingleton<ColorPalettesLibraryViewModel>()
            .AddSingleton<DeviceLayoutsLibraryViewModel>()
            .AddSingleton<AnimationLibraryViewModel>()
            .AddSingleton<LightingProfileLibraryViewModel>()
            .AddSingleton<LibraryViewModelFactory>()
            .AddSingleton<Draw2DCanvasInfoBarViewModel>()
            .AddSingleton<AssetItemViewModelFactory>()
            .AddSingleton<DeviceLayoutRightPanelViewModel>()
            .AddSingleton<ConfigurationHeaderViewModel>()
            //device layout
            .AddSingleton<DevicePropertiesViewModel>()
            //Capturing Service

            .AddSingleton<CapturingServiceProvider>()
            .AddSingleton<DeviceBitmapCaptureFactory>()
            .AddSingleton<FanOutputServiceFactory>()
            .AddSingleton<BassAudioDeviceEnumerationService>()
            .AddSingleton<BrightnessProviderFactory>()
            .AddSingleton<AudioDeviceNotificationClient>()
            //lighting engine
            .AddTransient<SelfGeneratedColorEngine>()
            .AddTransient<ScreenCaptureEngine>()
            .AddTransient<GifxelationEngine>()
            .AddTransient<AnimationDecodeEngine>()
            //Side menu
            .AddSingleton<SideMenuViewModel>()
            .AddSingleton<LightingProfileRepository>()
            .AddSingleton<LightingProfileCategoryRepository>()
            .AddSingleton<SideMenuProfilePlayerViewModel>()
            .AddSingleton<SideMenuViewModelFactory>()
            //Dialogs
            .AddSingleton<IDialogService, DialogService>()
            //Profile editor
            .AddSingleton<CanvasViewModelBase,CaptureRegionSelectionCanvasViewModel>()
            .AddSingleton<CanvasViewModelBase, ProfileEditorCanvasViewModel>()
            .AddSingleton<FigureContextMenuProvider>()
            .AddSingleton<ToolsViewModel>()
            .AddSingleton<LayersViewModel>()
            .AddSingleton<DeviceLayoutRightPanelViewModel>()
            .AddSingleton<ZonePropertiesViewModel>()
            .AddTransient<LayersView>()

            //profile Decoder
            .AddSingleton(mainFrameBuffer)
            .AddSingleton<LightingProfileDecoder>()
            .AddSingleton<ColorServiceProvider>()
            //Repository singleton
            .AddSingleton<TutorialsOnlineRepository>()
            .AddSingleton<StaticColorsRepository>()
            .AddSingleton<ColorPaletteRepository>()
            .AddSingleton<AnimationsRepository>()
            .AddSingleton<GifImagesRepository>()
            .AddSingleton<LightingZoneRepository>()
            .AddSingleton<SerialControllerDiscoveryService>()
            .AddSingleton<SerialControllerProvider>()
            .AddSingleton<SerialControllerRepository>()
            .AddSingleton<OpenRGBControllerRepository>()
            .AddSingleton<OpenRGBService>()
            .AddSingleton<OpenRGBControllerProvider>()
            .AddSingleton<OpenRGBControllerDiscoveryService>()
            .AddSingleton<DataStreamProvider>()
            .AddSingleton<AmbinityOpenRGBClient>()
            .AddSingleton<AmbinityDeviceLayoutRepository>()
            .AddSingleton<AmbinityDeviceOnlineRepository>()
            .AddSingleton<LightingZoneOnlineRepository>()
            .AddSingleton<LightingProfileOnlineRepository>()
            .AddSingleton<ColorPaletteOnlineRepository>()
            .AddSingleton<AmbinityDeviceRepository>()
            .AddSingleton<ResourceService>()
            .AddSingleton<AnimationOnlineRepository>()
            .AddSingleton<ShortcutRepository>()

            //Server
            .AddSingleton<AmbinityClient>()
            .AddSingleton<ThumbnailService>()
            .AddSingleton<DownloadService>()
            .AddSingleton<FirmwareService>()
            .AddSingleton<AmbinityStoreItemExportViewModel>()
            .AddSingleton<RepositoryHelpers>()
            .AddSingleton<ProfileStoreViewModel>()
            .AddSingleton<ProfileStoreNonClientAreaContentViewModel>()
            .AddSingleton<AmbinityStoreNavigation>()
            .AddSingleton<AmbinityStoreDetailViewModel>()

            //capture
            .AddSingleton<ScreenCapturingService>()
            .AddSingleton<AudioCapturingService>()
            .AddSingleton<HWMonitorCapturingService>()

            //debug
            .AddSingleton<DebugWindowViewModel>()
            .AddSingleton<AvaloniaViewModelSink>()
        //LED layout creator
            .AddSingleton<CanvasViewModelBase,LEDLayoutCreatorCanvasViewModel>()
            .AddSingleton<LEDLayoutCreatorViewModel>();

        Ioc.Default.ConfigureServices(
            serviceCollection
                .BuildServiceProvider());
    }



    private static async Task ConfigureCoreService(SplashViewModel splashViewModel)
    {
        //Update framebuffer value from settings before any service run
        var framebuffer = Ioc.Default.GetRequiredService<FrameBuffer>();
        framebuffer.FrameWidth = (int)_generalSettingsManager.Settings.CanvasWidth >= 400 ? (int)_generalSettingsManager.Settings.CanvasWidth : 400;
        framebuffer.FrameHeight = (int)_generalSettingsManager.Settings.CanvasHeight >= 320 ? (int)_generalSettingsManager.Settings.CanvasHeight : 320;
        //update setting incase size mismatch
        _generalSettingsManager.Settings.CanvasWidth = framebuffer.FrameWidth;
        _generalSettingsManager.Settings.CanvasHeight = framebuffer.FrameHeight;
        framebuffer.UpdatePixelData();
        //Try download assets from server
        var resourceService = Ioc.Default.GetRequiredService<ResourceService>();
        await Task.Run(async () =>
        {
            await resourceService.DownloadFirstRunResource(_splashViewModel.DownloadProgress);
        });
        var colorPaletteRepository = Ioc.Default.GetRequiredService<ColorPaletteRepository>();
        var solidColorsRepository = Ioc.Default.GetRequiredService<StaticColorsRepository>();
        var gifImagesRepository = Ioc.Default.GetRequiredService<GifImagesRepository>();
        var animationsRepository = Ioc.Default.GetRequiredService<AnimationsRepository>();
        var lightingZoneRepository = Ioc.Default.GetRequiredService<LightingZoneRepository>();
        var lightingProfileRepository = Ioc.Default.GetRequiredService<LightingProfileRepository>();
        var lightingProfileCategoryRepository = Ioc.Default.GetRequiredService<LightingProfileCategoryRepository>();
        var serialControllerRepository = Ioc.Default.GetRequiredService<SerialControllerRepository>();
        //if open rgb enable
        var openRGBControllerRepository = Ioc.Default.GetRequiredService<OpenRGBControllerRepository>();
        var ambinityDeviceRepository = Ioc.Default.GetRequiredService<AmbinityDeviceRepository>();
        var ambinityDeviceLayoutRepository = Ioc.Default.GetRequiredService<AmbinityDeviceLayoutRepository>();
        var shortcutRepository = Ioc.Default.GetRequiredService<ShortcutRepository>();

        splashViewModel.Progress = 5;
        await Task.Run(async () =>
        {
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
            ambinityDeviceLayoutRepository.Init();
            await Task.Delay(100);
            splashViewModel.Progress = 85;
            shortcutRepository.Init();
            await Task.Delay(100);

            serialControllerRepository.Init();
            //only allow Openrgb to run on Windows
            if (OperatingSystem.IsWindows() && _generalSettingsManager.Settings.EnableOpenRGB)
                openRGBControllerRepository.Init();
        });
        //run the profile decoder for rendering to device
        var profileDecoder = Ioc.Default.GetRequiredService<LightingProfileDecoder>();
        profileDecoder.Init();
        //start Serial controller repository to load controllers and devices
        await Task.Delay(1000);
        splashViewModel.Progress = 100;
        //start OpenRGB controller repository
    }

    private static void SetupDebugLogging()
    {
        var debugViewModel = Ioc.Default.GetRequiredService<DebugWindowViewModel>();
        var debugSinkViewModel = Ioc.Default.GetRequiredService<AvaloniaViewModelSink>();
        var logPath = Path.Combine(Constants.AppDataFolder, "Logs");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.Sink(debugSinkViewModel)
            .WriteTo.File(Path.Combine(logPath, "ambinity-.txt"), rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 10, shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information($"DEBUG logging set up!");
    }
}
