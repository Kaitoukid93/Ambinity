using System;
using System.IO;
using System.Linq;
using Ambinity.Installer.Models;
using Ambinity.Installer.Services;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Ambinity.Installer.ViewModels;
using Ambinity.Installer.Views;
using AmbinityServer;
using Avalonia.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentAvalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

namespace Ambinity.Installer;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ConfigureIoc();
        SetupDebugLogging();
        var _faTheme = App.Current?.Styles[0] as FluentAvaloniaTheme;
        _faTheme.CustomAccentColor = Colors.LightGray;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Startup += OnStartup;
            var vm = Ioc.Default.GetRequiredService<RootViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnStartup(object sender, ControlledApplicationLifetimeStartupEventArgs e)
    {
        string[]
            args = e.Args; // Process startup arguments
        if (args.Length > 0)
        {
            Log.Information(args.First());
            var installationService = Ioc.Default.GetRequiredService<InstallationService>();
            installationService.Args = args.ToList();
        }

        var rootVm = Ioc.Default.GetRequiredService<RootViewModel>();
        rootVm.Init();
    }

    private static void SetupDebugLogging()
    {
        var logPath = Path.Combine(Constants.AppDataFolder, "installer", "Logs");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(logPath, "ambinity.installer-.txt"), rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 10, shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information($"DEBUG logging set up!");
    }

    private static void ConfigureIoc()
    {
        var postInstallationSettings = new PostInstallationSettings();
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                //Main view
                .AddSingleton<AmbinityClient>()
                .AddSingleton<RootViewModel>()
                .AddSingleton<WelcomeViewModel>()
                .AddSingleton<FirstStepViewModel>()
                .AddSingleton<SecondStepViewModel>()
                .AddSingleton<ThirdStepViewModel>()
                .AddSingleton<InstallationService>()
                .AddSingleton<InstallViewModel>()
                .AddSingleton<UninstallViewModel>()
                .AddSingleton<FirstStepUninstallViewModel>()
                .AddSingleton<SecondStepUninstallViewModel>()
                .AddSingleton<ModifyViewModel>()
                .AddSingleton<FirstStepModifyViewModel>()
                .AddSingleton<ThirdStepUninstallViewModel>()
                .AddSingleton<SelectVersionViewModel>()
                .AddSingleton(postInstallationSettings)
                .BuildServiceProvider());
    }

   
}