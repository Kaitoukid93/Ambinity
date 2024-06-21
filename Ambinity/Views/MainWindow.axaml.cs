using Ambinity.Stores;
using Ambinity.ViewModels;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
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
}