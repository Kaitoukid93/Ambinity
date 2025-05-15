using System.IO;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Models.Collection;
using AmbinityCore.Repositories;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class AnimationAssetViewModel : AssetItemViewModelBase
{
    public AnimationAssetViewModel(ICollectableItem item) : base(item)
    {
        Name = item.Name;

        if (item is GifAnimation)
        {
            AnimationPath = Path.Combine(item.LocalPath, "animation.gif");
            Thumbnail = new GifAnimationThumbnailViewModel(AnimationPath);
        }

        else if (item is LottieJsonAnimation)
        {
            AnimationPath = Path.Combine(item.LocalPath, "config.json");
            Thumbnail = new LottieAnimationThumbnailViewModel(AnimationPath);
        }

        Description = item.Description;
    }

    public string AnimationPath { get; set; }
    public string Description { get; }
    public ViewModelBase Thumbnail { get; set; }
}
