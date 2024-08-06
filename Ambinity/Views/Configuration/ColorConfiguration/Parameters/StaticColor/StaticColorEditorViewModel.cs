using AmbinityCore.Models.Collection;
using AmbinityCore.Repositories;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

/// <summary>
/// viewmodel for Static Color Editor ( include gradient and solid)
/// </summary>
public class StaticColorEditorViewModel : ValueEditorViewModelBase  
{
    public StaticColorEditorViewModel(ICollectableItem color) : base(color)
    {
        
    }
}