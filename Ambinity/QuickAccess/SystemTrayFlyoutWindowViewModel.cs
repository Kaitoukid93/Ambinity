using System.Collections.Generic;
using System.ComponentModel;
using Ambinity.SystemUtilities;
using Ambinity.ViewModels;
using AmbinityCore.DataBase;
using AmbinityCore.Models.GeneralSetting;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using FluentAvalonia.Core;

namespace Ambinity.QuickAccess;

public class SystemTrayFlyoutWindowViewModel : ViewModelBase
{
    public QuickAccessViewModel QuickAccessViewModel => _quickAccessViewModel;
    private readonly QuickAccessViewModel _quickAccessViewModel;
    private readonly IGeneralSettings _settings;
    private readonly AppThemeManager _appThemeManager;

    public SystemTrayFlyoutWindowViewModel(QuickAccessViewModel quickAccessViewModel, GeneralSettingsManager settingsManager, AppThemeManager appThemeManager) 
    {
        _quickAccessViewModel = quickAccessViewModel;
        _settings = settingsManager.Settings;
        _settings.PropertyChanged += OnGeneralSettingsChanged;
        ChangeWindowTransparencyLevel(_settings.EnableMica);
         IsDarkTheme = Application.Current.RequestedThemeVariant == ThemeVariant.Dark;
        _appThemeManager = appThemeManager;
        _appThemeManager.ThemeChanged += (theme) =>
        {
            IsDarkTheme = theme == ThemeVariant.Dark;
        };
    }
    private bool _isDarkTheme;
    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            _isDarkTheme = value;
            OnPropertyChanged();
        }
    }
    public void Init()
    {
        _quickAccessViewModel.Init();
    }

    public override void Dispose()
    {
        _quickAccessViewModel?.Dispose();
    }
    private void OnGeneralSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_settings.EnableMica):
                ChangeWindowTransparencyLevel(_settings.EnableMica);
                break;
        }
    }
    private void ChangeWindowTransparencyLevel(bool value)
    {
        if (value && TransparencyLevel.Contains(WindowTransparencyLevel.AcrylicBlur))
            return;

        TransparencyLevel = value
            ? [WindowTransparencyLevel.AcrylicBlur]
            : [];
        OnPropertyChanged(nameof(TransparencyLevel));
        // Application.Current!.Resources["TransparencyEnabled"] = value;
    }
    public IReadOnlyList<WindowTransparencyLevel> TransparencyLevel { get; set; } = [WindowTransparencyLevel.AcrylicBlur];
}