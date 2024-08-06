using Ambinity.ViewModels;

namespace Ambinity.Views.LayoutEditor;

public abstract class AssetItemViewModelBase : ViewModelBase
{
    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }
}