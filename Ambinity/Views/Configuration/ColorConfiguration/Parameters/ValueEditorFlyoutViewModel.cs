using System.Collections;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Models.Collection;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class ValueEditorFlyoutViewModel : ViewModelBase
{
    public ValueEditorFlyoutViewModel(ICollectableItem value)
    {
        _value = value;
        _onlineItemRepository = _value.GetOnlineRerpository();
        _localRepository = _value.GetLocalRepository();
        ValueEditorViewModel = ValueEditorViewModelFactory.GetViewModel(_value);
        SelectedTab = 0;
    }

    public ValueEditorViewModelBase ValueEditorViewModel { get; set; }
    public ValueEditorFlyoutAssetViewModel AssetsViewModel { get; set; }
    private OnlineItemRepository _onlineItemRepository;
    private CollectableItemRepository _localRepository;
    private ICollectableItem _value;
    private bool _isOpen;

    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            _isOpen = value;
            OnPropertyChanged();
        }
    }

    private int _selectedTab;

    public int SelectedTab
    {
        get => _selectedTab;
        set
        {
            _selectedTab = value;
            OnPropertyChanged();
            UpdateTabContent();
        }
    }

    // public void Init()
    // {
    //     SelectedTab = 0;
    //
    // }
    private ViewModelBase _currentTabContent;

    public ViewModelBase CurrentTabContent
    {
        get => _currentTabContent;
        set
        {
            _currentTabContent = value;
            OnPropertyChanged();
        }
    }

    public async Task UpdateTabContent()
    {
        switch (SelectedTab)
        {
            case 0:
                CurrentTabContent = ValueEditorViewModel;
                break;
            case 1:
                CurrentTabContent = AssetsViewModel;
                await AssetsViewModel.Init(_localRepository, _onlineItemRepository);
                break;
        }
    }
}