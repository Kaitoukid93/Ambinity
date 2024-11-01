using System;
using System.Windows.Input;
using Ambinity.ViewModels;
using AmbinityCore.Models.Collection;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.LayoutEditor;

public abstract class AssetItemViewModelBase : ViewModelBase
{
    public AssetItemViewModelBase(ICollectableItem item)
    {
        Name = item.Name;
        Item = item;
        SelectItemCommand = new RelayCommand(SelectItem);
    }

    public AssetItemViewModelBase()
    {
        SelectItemCommand = new RelayCommand(SelectItem);
    }
    public event Action<AssetItemViewModelBase> ItemSelected;
    private string _name;
    public ICollectableItem Item { get; set; }
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }
    private void SelectItem()
    {
        ItemSelected?.Invoke(this);
    }
    public ICommand SelectItemCommand { get; set; }
}