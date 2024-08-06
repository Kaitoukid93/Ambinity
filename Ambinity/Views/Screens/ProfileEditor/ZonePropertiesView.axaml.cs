using System;
using Avalonia.Controls;


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
        public string Name => "Properties";

        public object Content
        {
            get => content;
        }
    }
}