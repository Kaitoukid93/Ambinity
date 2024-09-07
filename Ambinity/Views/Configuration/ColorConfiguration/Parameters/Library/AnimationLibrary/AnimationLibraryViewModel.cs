using AmbinityCore.Repositories;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class AnimationLibraryViewModel : LibraryViewModelBase
{
    public AnimationLibraryViewModel(AnimationsRepository animationRepository,
        AnimationOnlineRepository animationOnlineRepository, AnimationAssetsViewModel assetesViewModel) : base(
        animationRepository, animationOnlineRepository, assetesViewModel)
    {
    }
}