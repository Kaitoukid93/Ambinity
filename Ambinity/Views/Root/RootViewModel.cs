using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.Stores;
using Ambinity.ViewModels;
using Ambinity.Views.AppTour;
using Ambinity.Views.NonClientArea;
using Ambinity.Views.SideMenu;
using Ambinity.Views.SplashScreen;
using AmbinityCore.Colors;
using AmbinityCore.DataBase;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.GeneralSetting;
using AmbinityCore.Models.Profile;
using AmbinityCore.Models.ProfileCategory;
using AmbinityCore.Repositories;
using AmbinityServer;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Serilog;


namespace Ambinity.Views.Root;

public partial class RootViewModel : ViewModelBase, IMainWindowProvider
{
    public event EventHandler? AppExitRequested;

    #region Construct

    public RootViewModel(RootNavigationStores rootNavigationStores,
        SideMenuViewModel sideMenu,
        ColorPaletteRepository colorPaletteRepository,
        StaticColorsRepository staticColorsRepository,
        GifImagesRepository gifImagesRepository,
        AnimationsRepository animationsRepository,
        LightingProfileRepository lightingProfileRepository,
        LightingProfileCategoryRepository lightingProfileCategoryRepository,
        SerialControllerRepository serialControllerRepository,
        OpenRGBControllerRepository openRgbControllerRepository,
        IMainWindowService mainWindowService,
        GeneralSettingsManager settingsManager,
        AmbinityClient ambinityClient, NonClientAreaContentViewModel nonClientAreaContentViewModel,
        AppTourViewModel appTourViewModel)
    {
        _rootNavigationStores = rootNavigationStores;
        CommandSetup();
        _repositories = new List<CollectableItemRepository>
        {
            colorPaletteRepository,
            staticColorsRepository,
            gifImagesRepository,
            animationsRepository,
            lightingProfileRepository,
            lightingProfileCategoryRepository,
            serialControllerRepository,
            openRgbControllerRepository
        };
        SideMenu = sideMenu;
        _lifeTime = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
        _settings = settingsManager.Settings;
        _ambinityClient = ambinityClient;
        mainWindowService.ConfigureMainWindowProvider(this);
        NonClientAreaContentViewModel = nonClientAreaContentViewModel;
        AppTourViewModel = appTourViewModel;
        //show UI if requested
        if (ShouldShowUI())
        {
            OpenMainWindow();
        }
    }

    public AppTourViewModel AppTourViewModel { get; set; }


    private bool ShouldShowUI()
    {
        return !_settings.StartMinimized;
    }

    /// <inheritdoc />
    public bool IsMainWindowOpen => _lifeTime.MainWindow != null;

    /// <inheritdoc />
    public bool IsMainWindowFocused { get; private set; }

    public void OpenMainWindow()
    {
        if (_lifeTime.MainWindow == null)
        {
            _lifeTime.MainWindow = new MainWindow { DataContext = this };
            _lifeTime.MainWindow.Show();
            _lifeTime.MainWindow.Closing += CurrentMainWindowOnClosing;
        }

        _lifeTime.MainWindow.Activate();
        if (_lifeTime.MainWindow.WindowState == WindowState.Minimized)
            _lifeTime.MainWindow.WindowState = WindowState.Normal;
        SideMenu.Init();
        OnMainWindowOpened();
    }

    public void CloseMainWindow()
    {
        Dispatcher.UIThread.Post(() => { _lifeTime.MainWindow?.Close(); });
    }


    private void CurrentMainWindowOnClosing(object? sender, EventArgs e)
    {
        //WindowSizeSetting?.Save();
        _lifeTime.MainWindow = null;
        OnMainWindowClosed();
    }

    public void Focused()
    {
        IsMainWindowFocused = true;
        OnMainWindowFocused();
    }

    public void Unfocused()
    {
        IsMainWindowFocused = false;
        OnMainWindowUnfocused();
    }

    /// <inheritdoc />
    public event EventHandler? MainWindowOpened;

    /// <inheritdoc />
    public event EventHandler? MainWindowClosed;

    /// <inheritdoc />
    public event EventHandler? MainWindowFocused;

    /// <inheritdoc />
    public event EventHandler? MainWindowUnfocused;

    protected virtual void OnMainWindowFocused()
    {
        MainWindowFocused?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnMainWindowUnfocused()
    {
        MainWindowUnfocused?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Events

    protected virtual void OnMainWindowOpened()
    {
        MainWindowOpened?.Invoke(this, EventArgs.Empty);
    }

    protected virtual async void OnMainWindowClosed()
    {
        MainWindowClosed?.Invoke(this, EventArgs.Empty);
        await SaveRepositories();
        _sideMenu?.Dispose();
        try
        {
            _ambinityClient.Disconnect();
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
            throw;
        }
    }

    protected virtual void OnApplicationExit()
    {
        AppExitRequested?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Properties

    private readonly IClassicDesktopStyleApplicationLifetime _lifeTime;
    private readonly IGeneralSettings _settings;
    private SideMenuViewModel _sideMenu;
    private AmbinityClient _ambinityClient;

    public SideMenuViewModel SideMenu
    {
        get { return _sideMenu; }
        set
        {
            _sideMenu = value;
            OnPropertyChanged();
        }
    }

    private readonly RootNavigationStores _rootNavigationStores;
    public ViewModelBase CurrentViewModel => _rootNavigationStores.CurrentViewModel;

    #endregion

    #region Methods

    private void CommandSetup()
    {
        OpenUiCommand = new RelayCommand<string>(OpenUI);
        ExitAppCommand = new RelayCommand(ExitApp);
    }

    private void OpenUI(string ui)
    {
        OpenMainWindow();
    }

    private void ExitApp()
    {
        //todo wait for 1 second before exiting because process need to dispose
        if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime
            controlledApplicationLifetime)
            Dispatcher.UIThread.Post(() => controlledApplicationLifetime.Shutdown());
    }

    private List<CollectableItemRepository> _repositories;

    private async Task SaveRepositories()
    {
        foreach (var repo in _repositories)
        {
            await Task.Run(() => repo.SaveToDisk());
        }
    }

    #endregion

    #region Command

    public ICommand OpenUiCommand { get; set; }
    public ICommand ExitAppCommand { get; set; }
    public NonClientAreaContentViewModel NonClientAreaContentViewModel { get; }

    #endregion
}