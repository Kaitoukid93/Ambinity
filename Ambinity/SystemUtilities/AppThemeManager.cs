
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using FluentAvalonia.Styling;

namespace Ambinity.SystemUtilities;

public static class AppThemeManager
{
    private static ThemeVariant GetThemeVariant(string value)
    {
        switch (value)
        {
            case "Light":
                return ThemeVariant.Light;
            case "Dark":
                return ThemeVariant.Dark;
            case "System":
            default:
                return null;
        }
    }

    public static void SetAppTheme(string themeName)
    {
        var theme = GetThemeVariant(themeName);
        var _faTheme = Application.Current.Styles[0] as FluentAvaloniaTheme;
        Application.Current.RequestedThemeVariant = theme;
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
    }
    public static void UpdateAppAccentColor(Color? color)
    {
        var _faTheme = Application.Current.Styles[0] as FluentAvaloniaTheme;
        _faTheme.CustomAccentColor = color;
    }
    
}