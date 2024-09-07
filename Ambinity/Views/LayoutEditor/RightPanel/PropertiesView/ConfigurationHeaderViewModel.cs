using Ambinity.ViewModels;

namespace Ambinity.Views.LayoutEditor.RightPanel.PropertiesView;

public class ConfigurationHeaderViewModel : ViewModelBase
{
    public ConfigurationHeaderViewModel(string header, string icon, bool visibility = true)
    {
        Header = header;
        Icon = icon;
        Visibility = visibility;
    }

    public string Header { get; set; }
    public string Icon { get; set; }
    public bool Visibility { get; set; } = true;
}