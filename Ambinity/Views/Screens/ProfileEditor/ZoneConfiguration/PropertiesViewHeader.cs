namespace Ambinity.Views.Screens.ProfileEditor.ZoneConfiguration;

public class PropertiesViewHeader
{
    public PropertiesViewHeader(string header, string icon, bool visibility = true)
    {
        Header = header;
        Icon = icon;
        Visibility = visibility;
    }

    public string Header { get; set; }
    public string Icon { get; set; }
    public bool Visibility { get; set; } = true;
}