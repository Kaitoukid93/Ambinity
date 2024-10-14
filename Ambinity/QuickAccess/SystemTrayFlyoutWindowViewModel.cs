using Ambinity.ViewModels;

namespace Ambinity.QuickAccess;

public class SystemTrayFlyoutWindowViewModel : ViewModelBase
{
    public QuickAccessViewModel QuickAccessViewModel => _quickAccessViewModel;
    private readonly QuickAccessViewModel _quickAccessViewModel;

    public SystemTrayFlyoutWindowViewModel(QuickAccessViewModel quickAccessViewModel)
    {
        _quickAccessViewModel = quickAccessViewModel;
    }

    public void Init()
    {
        _quickAccessViewModel.Init();
    }

    public override void Dispose()
    {
        _quickAccessViewModel?.Dispose();
    }
}