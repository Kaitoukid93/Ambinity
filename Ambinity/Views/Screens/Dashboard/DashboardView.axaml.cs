using System;
using Ambinity.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views.Screens.Dashboard;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetRequiredService<DashboardViewModel>();
    }
    public class DashboardViewPage : ISelectablePage
    {
        private readonly Lazy<DashboardView> lazyContent;

        public DashboardViewPage(Lazy<DashboardView> lazyContent)
        {
            this.lazyContent = lazyContent ?? throw new ArgumentNullException(nameof(lazyContent));
        }

        public int Order => 10;

        public string PageName => "Devices Advance Settings";
        public string Geometry => "";

        public object Content { get => lazyContent.Value; }
    }
}