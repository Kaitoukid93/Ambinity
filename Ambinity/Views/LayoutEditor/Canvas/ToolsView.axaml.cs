using System.Linq;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using Ambinity.Views.AppTour;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views.LayoutEditor;
public partial class ToolsView : UserControl
{
    
    private AppTourViewModel _appTourViewModel;
    private readonly IClassicDesktopStyleApplicationLifetime? _lifeTime;
    private readonly Window? _mainWindow;
    public ToolsView()
    {
        _appTourViewModel = Ioc.Default.GetService<AppTourViewModel>();
        _appTourViewModel.NextStepActivated += OnAppTourStepChanged;
        _lifeTime = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
        _mainWindow = _lifeTime.MainWindow;
        InitializeComponent();
    }

    private async void OnAppTourStepChanged(ViewModelBase element)
    {
        if (element is ToolsViewModel)
        {
            await ActivateGuide();
        }
    }
    private async Task ActivateGuide()
    {
        //focus on this category
        while (!tools.IsLoaded)
        {
            await Task.Delay(100);
        }
        var firstTool = new AppTourElement(tools.ContainerFromItem(tools.Items.First()), "Add Colors Zone",
            "Click to add new color zone, there are three zone shapes available : Rectangle, Ellipse and Polyline");
        var firstToolVm = AppTourElementHelper.GetAppTourElements(firstTool,this._mainWindow);
        if(firstToolVm ==null)
            return;
        _appTourViewModel?.Show(firstToolVm,true,this._mainWindow);
        //wait for user to interact
    }

}