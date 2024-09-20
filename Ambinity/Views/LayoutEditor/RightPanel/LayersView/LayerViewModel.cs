using System;
using Ambinity.ViewModels;
using AmbinityCore.Models.Geography;
using AmbinityCore.Models.Lighting.Zone;
using Avalonia.Media;
using Draw2D.Core;

namespace Ambinity.Views.LayoutEditor;

public class LayerViewModel : ViewModelBase
{
    public LayerViewModel(ContainerFigure figure)
    {
        _figure = figure;
        Name = figure.ChildItem.GetDisplayName();
        Icon = figure.ChildItem.Icon;
        Color = figure.ChildItem.GetDisplayColor() ?? Colors.White;
        _figure.MouseOverChanged += OnMouseOverChanged;
    }

    public event Action<LayerViewModel, bool> Selected;
    public string Name { get; set; }
    public Color Color { get; set; }
    public bool IsVisible { get; set; }
    public string Icon { get; set; }

    private void OnMouseOverChanged(bool value)
    {
        IsMouseOver = value;
    }

    private bool _isMouseOver;

    public bool IsMouseOver
    {
        get => _isMouseOver;
        set
        {
            _isMouseOver = value;
            OnPropertyChanged(nameof(IsMouseOver));
            OnPropertyChanged(nameof(ShowButtons));
        }
    }

    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged(nameof(IsSelected));
            OnPropertyChanged(nameof(ShowButtons));
        }
    }

    public bool ShowButtons => IsSelected || IsMouseOver;

    public Figure Figure
    {
        get => _figure;
    }

    private Figure _figure;

    public void Update()
    {
        // Name = _zoneFigure.Zone.Name;
        IsSelected = _figure.IsSelected;
    }

    public void OnLayerPointerPress(bool isCtrl)
    {
        Selected?.Invoke(this, isCtrl);
    }
}