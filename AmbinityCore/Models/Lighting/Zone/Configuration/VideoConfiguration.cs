using System;
using AmbinityCore.Repositories;

namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public class VideoConfiguration
{
public VideoConfiguration()
    {
    }

    public VideoConfiguration(Video video)
    {
        if(video!=null)
        VideoUID = video.UID;
    }

    public event Action VideoChanged;
    public ConfigurationType Type => ConfigurationType.Video;
    public string Name => "Video";
    public string Icon => "LightingConfiguration_Animation";

    public string? GetInfo()
    {
        return "XXX";
    }

    /// <summary>
    /// Config the frame rate of this video
    /// </summary>
    public int FrameRate { get; set; } = 1;

    /// <summary>
    /// Config the repeat property
    /// </summary>
    public bool Repeat { get; set; }

    /// <summary>
    /// Set or get the delay frame
    /// </summary>
    public bool DelayFrame { get; set; }

    public void ChangeVideo(Video video)
    {
        VideoUID = video.UID;
        VideoChanged?.Invoke();
    }

    public Guid VideoUID { get; set; }
}
