using Ambinity.Services;
using Ambinity.ViewModels;
namespace Ambinity.Commands;

public class NavigateCommand<TViewModel> : CommandBase
where TViewModel : ViewModelBase 
{
    public NavigateCommand(NavigationService<TViewModel> navigationService)
    {
        _navigationService = navigationService;
    }

    private readonly NavigationService<TViewModel> _navigationService;
    public override void Execute(object parameter)
    {
        _navigationService.Navigate();
    }
}