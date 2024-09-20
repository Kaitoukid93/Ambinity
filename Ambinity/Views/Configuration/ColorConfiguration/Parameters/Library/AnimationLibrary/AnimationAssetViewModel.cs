using System.IO;
using System.Windows.Input;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Models.Collection;
using AmbinityCore.Repositories;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class AnimationAssetViewModel : AssetItemViewModelBase
{
    public AnimationAssetViewModel(ICollectableItem item): base(item)
    {
        Name = item.Name;
        AnimationJsonPath = Path.Combine(item.LocalPath, "config.json");
        Description = (item as Animation).Description;
    }

    public string AnimationJsonPath { get; set; }
    public string Description { get; }
}