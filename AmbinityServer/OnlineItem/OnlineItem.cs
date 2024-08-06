using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityServer.OnlineItem;

/// <summary>
/// represent online item stored on Ambinity server
/// </summary>
public class OnlineItem : ObservableObject
{
    public OnlineItem()
    {
    }
    
    public string Name { get; set; }
    public string Owner { get; set; }
    public OnlineItemTypeEnum Type { get; set; }
    public string Description { get; set; }
    public string Path { get; set; }
    public string ThumbnailPath { get; set; }
    public string Version { get; set; }
    public DateTime LastUpdate { get; set; }
    
}