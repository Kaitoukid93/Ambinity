using System;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Models.Collection;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

/// <summary>
/// base viewmodel for value selection parameter view such as color palette, solid color
/// </summary>
public abstract class ValueSelectionViewModelBase : ParameterViewModelBase
{
    public event Action<ICollectableItem> SelectedValueChanged;

    public ValueSelectionViewModelBase(ICollectableItem item)
    {
        SetValue(item);
    }
    /// <summary>
    /// set value to actual target
    /// </summary>
    
    public void SetValue(ICollectableItem value)
    {
        SelectedValue = value;
        SelectedValueChanged?.Invoke(value);
    }

    public void LoadValue(ICollectableItem value)
    {
        SelectedValue = value;
    }

    private ValueEditorFlyoutViewModel _flyoutViewModel;

    private ICollectableItem _selectedValue;

    public ICollectableItem SelectedValue
    {
        get => _selectedValue;
        set
        {
            _selectedValue = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Open flyout contains editor and asset view
    /// </summary>
    public void OpenAssetFlyout()
    {
        _flyoutViewModel = new ValueEditorFlyoutViewModel(_selectedValue);
        _flyoutViewModel.IsOpen = true;
    }
}