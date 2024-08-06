using AmbinityCore.Models.Collection;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Repositories;
/// <summary>
/// base class for static color
/// </summary>
public class StaticColor :ObservableObject
{
    public StaticColor()
    {
        
    }

    public virtual Brush GetBrush()
    {
        return null;
    }

}