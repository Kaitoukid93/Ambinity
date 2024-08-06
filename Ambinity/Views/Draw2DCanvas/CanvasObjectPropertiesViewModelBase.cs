using Ambinity.ViewModels;
using Draw2D.Core;

namespace Ambinity.Views.Draw2DCanvas;

public abstract class CanvasObjectPropertiesViewModelBase : ViewModelBase
{
    public CanvasObjectPropertiesViewModelBase()
    {
      
    }
    
    public virtual void Init()
    {
        DisableEdit();
    }
    /// <summary>
    /// Update when selection Changed
    /// </summary>
    public virtual void UpdateObjectProperties()
    {
    }

    /// <summary>
    /// Disable Properties editor
    /// </summary>
    public virtual void DisableEdit()
    {
    }
}