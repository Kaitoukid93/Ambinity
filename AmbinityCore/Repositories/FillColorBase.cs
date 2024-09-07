using AmbinityCore.Models.Collection;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Repositories;

/// <summary>
/// base class for static color
/// </summary>
public abstract class FillColorBase : ObservableObject
{
    public FillColorBase()
    {
    }

    /// <summary>
    /// get display brush
    /// </summary>
    /// <returns></returns>
    public virtual List<Brush> GetBrush()
    {
        return null;
    }
}