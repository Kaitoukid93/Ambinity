using Ambinity.Installer.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Installer.Views;

public partial class MainWindow : Window
{
    private RootViewModel viewModel;
    public MainWindow()
    {
        InitializeComponent();
        viewModel = Ioc.Default.GetRequiredService<RootViewModel>();
        viewModel.Close += OnCloseWindowRequested;

    }

    private void OnCloseWindowRequested()
    {
        this.Close();
    }
}