using System.IO;
using System.Threading.Tasks;
using Ambinity.AppResource;
using Ambinity.Services;
using Ambinity.Stores;
using Ambinity.Views.AppTour;
using Ambinity.Views.Configuration.ColorConfiguration;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using Ambinity.Views.Configuration.PositionConfiguration;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.LayoutEditor;
using Ambinity.Views.NonClientArea;
using Ambinity.Views.Screens.DeviceLayout;
using Ambinity.Views.Screens.DeviceLayout.Library;
using Ambinity.Views.Screens.DeviceSettings;
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
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using Constants = AmbinityCore.Constants;
using RootViewModel = Ambinity.Views.Root.RootViewModel;

namespace Ambinity;

public class AmbinityBootStrapper
{
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
        SetupDebugLogging();
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
        var mainFrameBuffer = new FrameBuffer(750, 500);
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                //Main view
                .AddSingleton<IMainWindowService, MainWindowService>()
                .AddSingleton<IWindowService, WindowService>()
                .AddSingleton<RootViewModel>()
                .AddSingleton<GeneralSettingsManager>()
                .AddSingleton<RootNavigationStores>()
                .AddSingleton<DeviceSettingsViewModel>()
                .AddSingleton<DeviceSettingsDashboardViewModel>()
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
                //splash
                .AddSingleton<SplashViewModel>()
                //layout editor
                .AddSingleton<DeviceLayoutEditorViewModel>()
                .AddSingleton<ProfileEditorViewModel>()
                .AddSingleton<LayoutCanvasViewModel>()
                .AddSingleton<ProfileEditorRightPanelViewModel>()
                .AddSingleton<ParameterViewModelFactory>()
                .AddSingleton<ColorConfigurationViewModelFactory>()
                .AddTransient<PositionConfigurationViewModel>()
                .AddSingleton<ColorPaletteAssetsViewModel>()
                .AddSingleton<LightingZonesLibraryViewModel>()
                .AddSingleton<DeviceLayoutAssetsViewModel>()
                .AddSingleton<LightingZoneAssetsViewModel>()
                .AddSingleton<AnimationAssetsViewModel>()
                .AddSingleton<ColorPalettesLibraryViewModel>()
                .AddSingleton<DeviceLayoutsLibraryViewModel>()
                .AddSingleton<AnimationLibraryViewModel>()
                .AddSingleton<LibraryViewModelFactory>()
                .AddSingleton<Draw2DCanvasInfoBarViewModel>()
                .AddSingleton<AssetItemViewModelFactory>()
                .AddSingleton<DeviceLayoutRightPanelViewModel>()
                //device layout
                .AddSingleton<DevicePropertiesViewModel>()
                //Capturing Service
                .AddSingleton<ScreenCapturingService>()
                .AddSingleton<AudioCapturingService>()
                .AddSingleton<CapturingServiceProvider>()
                .AddSingleton<DeviceBitmapCaptureFactory>()
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
                .AddSingleton<Draw2DCanvasViewModel>()
                .AddSingleton<FigureContextMenuProvider>()
                .AddSingleton<ToolsViewModel>()
                .AddSingleton<LayersViewModel>()
                .AddSingleton<DeviceLayoutRightPanelViewModel>()
                .AddSingleton<ZonePropertiesViewModel>()
                .AddTransient<LayersView>()

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
                .AddSingleton<OpenRGBControllerRepository>()
                .AddSingleton<OpenRGBControllerProvider>()
                .AddSingleton<OpenRGBControllerDiscoveryService>()
                .AddSingleton<DataStreamProvider>()
                .AddSingleton<AmbinityOpenRGBClient>()
                .AddSingleton<AmbinityDeviceLayoutRepository>()
                .AddSingleton<AmbinityDeviceOnlineRepository>()
                .AddSingleton<LightingZoneOnlineRepository>()
                .AddSingleton<ColorPaletteOnlineRepository>()
                .AddSingleton<AmbinityDeviceRepository>()
                .AddSingleton<ResourceService>()
                .AddSingleton<AnimationsRepository>()
                .AddSingleton<AnimationOnlineRepository>()

                //Server
                .AddSingleton<AmbinityClient>()
                .AddSingleton<ThumbnailService>()
                .AddSingleton<DownloadService>()
                .AddSingleton<FirmwareService>()
                .BuildServiceProvider());
    }

    private static void UpdateAppAccentColor(Color? color)
    {
        _faTheme.CustomAccentColor = color;
    }

    private static async Task ConfigureCoreService(SplashViewModel splashViewModel)
    {
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
            serialControllerRepository.Init();
            openRGBControllerRepository.Init();
        });
        //run the profile decoder for rendering to device
        var profileDecoder = Ioc.Default.GetRequiredService<LightingProfileDecoder>();
        //start Serial controller repository to load controllers and devices
        await Task.Delay(1000);
        splashViewModel.Progress = 100;
        //start OpenRGB controller repository
    }

    private static void SetupDebugLogging()
    {
        var logPath = Path.Combine(Constants.AppDataFolder, "Logs");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(logPath, "ambinity-.txt"), rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 10, shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information($"DEBUG logging set up!");
    }
}