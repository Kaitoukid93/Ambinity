using Ambinity.Stores;
using Ambinity.ViewModels;

namespace Ambinity.Services;

public class  NavigationService<TViewModel>
where TViewModel: ViewModelBase
{
    private readonly NavigationStores _navigationStores;
    private readonly TViewModel _viewModel;

    public NavigationService(NavigationStores navigationStores,TViewModel viewModel)
    {
        _navigationStores = navigationStores;
        _viewModel = viewModel;
    }
    public void Navigate()
    {
        _navigationStores.CurrentViewModel = _viewModel;
    }
}