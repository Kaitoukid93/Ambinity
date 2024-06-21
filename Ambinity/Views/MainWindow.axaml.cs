using System.Diagnostics;
using Ambinity.Stores;
using Ambinity.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentAvalonia.UI.Media.Animation;

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