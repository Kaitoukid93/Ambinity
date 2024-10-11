using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Ambinity.Installer.ViewModels;
using Ambinity.Installer.Views;
using Avalonia.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentAvalonia.Styling;
using Microsoft.Extensions.DependencyInjection;

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
        var _faTheme = App.Current?.Styles[0] as FluentAvaloniaTheme;
        _faTheme.CustomAccentColor = Colors.LimeGreen;
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = Ioc.Default.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm,
            };
            vm.Init();
        }

        base.OnFrameworkInitializationCompleted();
    }
    private static void ConfigureIoc()
    {
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                //Main view
                .AddSingleton<MainWindowViewModel>()
                .AddSingleton<WelcomeViewModel>()
                .AddSingleton<FirstStepViewModel>()
                .AddSingleton<SecondStepViewModel>()
                .AddSingleton<ThirdStepViewModel>()
                .BuildServiceProvider());
    }
}