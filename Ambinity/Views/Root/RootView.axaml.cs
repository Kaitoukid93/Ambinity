using Ambinity.Stores;
using Ambinity.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views.Root;

public partial class RootView : UserControl
{
    public RootView()
    {
        this.InitializeComponent();
        _rootNavigationStores = Ioc.Default.GetRequiredService<RootNavigationStores>();
        _rootNavigationStores.CurrentViewModelChanged += FrameNavigate;
    }

    private RootNavigationStores _rootNavigationStores;

    private void FrameNavigate(ViewModelBase vm)
    {
        RootFrame.NavigateFromObject(vm);
    }
    
}