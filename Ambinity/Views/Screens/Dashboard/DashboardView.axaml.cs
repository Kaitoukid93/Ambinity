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
}