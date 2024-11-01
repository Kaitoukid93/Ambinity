using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

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
    [JsonIgnore] public string Path { get; set; }
    [JsonIgnore] public string ThumbnailPath { get; set; }
    public string Version { get; set; }
    [JsonIgnore] public string FileSize { get; set; }
    [JsonIgnore] public string LastUpdate { get; set; }
    public string[] Tags { get; set; }
    public string Hyperlink { get; set; }
}