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
    }
    private void FrameNavigate(ViewModelBase vm)
    {
        QuickAccessFrame.NavigateFromObject(vm,new FrameNavigationOptions(){ TransitionInfoOverride = new SlideNavigationTransitionInfo(),IsNavigationStackEnabled = true});
    }
    
}