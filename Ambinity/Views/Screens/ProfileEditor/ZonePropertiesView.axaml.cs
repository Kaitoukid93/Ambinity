using System;
using Avalonia.Controls;
using Ambinity.Localization;


namespace Ambinity.Views.Screens.ProfileEditor;

public partial class ZonePropertiesView : UserControl
{
    public ZonePropertiesView()
    {
        InitializeComponent();
    }

    public class ZonePropertiesNavigationContent
    {
        private readonly ZonePropertiesView content;

        public ZonePropertiesNavigationContent(ZonePropertiesView content)
        {
            this.content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public int Order => 1;
        public string Icon => "dashboard";
        public string Name => Loc.Get("Properties.RightPanel.Header");

        public object Content
        {
            get => content;
        }
    }
}
