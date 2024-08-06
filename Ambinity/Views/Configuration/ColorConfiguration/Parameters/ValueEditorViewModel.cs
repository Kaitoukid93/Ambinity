using Ambinity.ViewModels;
using AmbinityCore.Models.Collection;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

/// <summary>
/// Base class for editing value such as color, palette..
/// </summary>
public class ValueEditorViewModelBase : ViewModelBase
{
    public ValueEditorViewModelBase(ICollectableItem value)
    {
        _value = value;
    }

    private ICollectableItem _value;
}