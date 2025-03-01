using Ambinity.Stores;
using Ambinity.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using ExCSS;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;

namespace Ambinity.QuickAccess;

public partial class QuickAccessView : UserControl
{
    private QuickAccessNavigationStore _quickAccessnavigationStore;

    public QuickAccessView()
    {
        InitializeComponent();
        _quickAccessnavigationStore = Ioc.Default.GetRequiredService<QuickAccessNavigationStore>();
        
        _quickAccessnavigationStore.CurrentViewModelChanged += FrameNavigate;
        _quickAccessnavigationStore.GoBackRequested += OnGoBackRequested;
        QuickAccessFrame.Navigated += OnFrameNavigated;
    }

    private void OnGoBackRequested()
    {
        QuickAccessFrame.GoBack();
    }

    private void OnFrameNavigated(object sender, NavigationEventArgs e)
    {
        //throw new System.NotImplementedException();
    }

    private void FrameNavigate(ViewModelBase vm)
    {
        QuickAccessFrame.NavigateFromObject(vm,new FrameNavigationOptions(){ TransitionInfoOverride = new SlideNavigationTransitionInfo(),IsNavigationStackEnabled = true});
    }
    
}