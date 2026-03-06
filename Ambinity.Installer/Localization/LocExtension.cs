using Avalonia;
using Avalonia.Markup.Xaml;
using System;

namespace Ambinity.Installer.Localization
{
    public class LocExtension : MarkupExtension
    {
        public string Key { get; set; } = string.Empty;

        public LocExtension() { }

        public LocExtension(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Subscribe to dynamic updates
            Loc.LanguageChanged += () =>
            {
                if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget target &&
                    target.TargetObject is AvaloniaObject ao &&
                    target.TargetProperty is AvaloniaProperty ap)
                {
                    ao.SetValue(ap, Loc.Get(Key));
                }
            };

            return Loc.Get(Key);
        }
    }
}
