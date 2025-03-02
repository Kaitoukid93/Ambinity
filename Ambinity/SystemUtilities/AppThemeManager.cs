
using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using FluentAvalonia.Styling;

namespace Ambinity.SystemUtilities;

public class AppThemeManager
{
    public AppThemeManager()
    {
        Application.Current.ActualThemeVariantChanged+= (s, e) =>
        {
            ThemeChanged?.Invoke(Application.Current.RequestedThemeVariant);
        };
    }
    public event Action<ThemeVariant> ThemeChanged;
    private ThemeVariant GetThemeVariant(string value)
    {
        switch (value)
        {
            case "Light":
                return ThemeVariant.Light;
            case "Dark":
                return ThemeVariant.Dark;
            case "System":
            return Application.Current.RequestedThemeVariant;
            default:
                return ThemeVariant.Default;
        }
    }

    public void SetAppTheme(string themeName)
    {
        var theme = GetThemeVariant(themeName);
        var _faTheme = Application.Current.Styles[0] as FluentAvaloniaTheme;
        Application.Current.RequestedThemeVariant = theme;
        //change backdrop blur tint color too
        if (themeName != "System")
        {                    
            _faTheme.PreferSystemTheme = false;
            _faTheme.PreferUserAccentColor = true;
        }
        else
        {
            _faTheme.PreferSystemTheme = true;
            _faTheme.PreferUserAccentColor = false;
            _faTheme.CustomAccentColor = null;
        }
        ThemeChanged?.Invoke(theme);
    }
    public void UpdateAppAccentColor(Color? color)
    {
        var _faTheme = Application.Current.Styles[0] as FluentAvaloniaTheme;
        _faTheme.CustomAccentColor = color;
    }
    
}