using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media;
using ColorChangedEventArgs = FluentAvalonia.UI.Controls.ColorChangedEventArgs;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public partial class SolidColorPickerView : UserControl
{
    public SolidColorPickerView()
    {
        InitializeComponent();
        _componentList = new List<ColorPickerComponent>();
        _componentList.Add(Spectrum);
        _componentList.Add(Hue);
    }

    private List<ColorPickerComponent> _componentList;
    private void ColorPickerComponent_OnColorChanged(ColorPickerComponent sender, ColorChangedEventArgs args)
    {
        if(args.NewColor.Hue == Spectrum.Color.Hue)
            return;
        UpdateColor(args.NewColor, Spectrum);
    }

    private void Spectrum_OnColorChanged(ColorPickerComponent sender, ColorChangedEventArgs args)
    {
        UpdateColor(args.NewColor, Hue);
    }

    private void UpdateColor(Color2 newColor, ColorPickerComponent target)
    {

        target.Color = newColor;
       HexBox.Text = ((Color)newColor).ToUInt32().ToString("x8", CultureInfo.InvariantCulture).Remove(0,2);
    }

    private void HexBox_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        var lastColor = Spectrum.Color;
        var color = Colors.Red;
        var colorString = "#ff" + HexBox.Text;
        var result = Color.TryParse(colorString,out color);
        if (result)
        {
            UpdateColor(color,Spectrum);
            UpdateColor(color,Hue);
        }
        else
        {
            UpdateColor(lastColor,Spectrum);
            UpdateColor(lastColor,Hue);
        }
            
    }
}