using System;
using System.IO;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Models.Collection;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class LottieAnimationThumbnailViewModel : ViewModelBase
{
    public LottieAnimationThumbnailViewModel(string thumbnailPath)
    {

        if (OperatingSystem.IsMacOS())
        {
            ThumbnailPath = new Uri("file://" + Path.GetFullPath(thumbnailPath)).ToString();
        }
        else
        {
            ThumbnailPath = Path.GetFullPath(thumbnailPath);
        }
    }

    public string ThumbnailPath { get; set; }
}
