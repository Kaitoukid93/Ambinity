using Ambinity.Stores;
using Ambinity.ViewModels;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;

namespace Ambinity.Views.AmbinityStore;

public partial class ProfileStoreView : Window
{
    private AmbinityStoreNavigation _storeNavigation;
    public ProfileStoreView()
    {
        InitializeComponent();
        _storeNavigation = Ioc.Default.GetRequiredService<AmbinityStoreNavigation>();
        
        _storeNavigation.CurrentViewModelChanged += FrameNavigate;
        _storeNavigation.GoBackRequested += OnGoBackRequested;
        StoreNavigationFrame.Navigated += OnFrameNavigated;
    }

    private void OnGoBackRequested()
    {
        StoreNavigationFrame.GoBack();
    }

    private void OnFrameNavigated(object sender, NavigationEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void FrameNavigate(ViewModelBase vm)
    {
        StoreNavigationFrame.NavigateFromObject(vm,new FrameNavigationOptions(){ TransitionInfoOverride = new SlideNavigationTransitionInfo(),IsNavigationStackEnabled = true});
    }
}