using Avalonia;
using Avalonia.Controls;

namespace Ambinity.Views;


public class UIGlobal : AvaloniaObject
{
    public static readonly AttachedProperty<bool> TransparencyEnabledProperty = AvaloniaProperty.RegisterAttached<UIGlobal, Control, bool>("TransparencyEnabled", true, true);
    public static void SetTransparencyEnabled(Control obj, bool value) => obj.SetValue(TransparencyEnabledProperty, value);
    public static bool GetTransparencyEnabled(Control obj) => obj.GetValue(TransparencyEnabledProperty);
}