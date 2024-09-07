using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Colors;
using AmbinityCore.Repositories;
using Avalonia.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class SolidColorPickerViewModel : FlyoutContentViewModelBase
{
    
    public SolidColorPickerViewModel(List<SolidColorViewModel> colors, StaticColorsRepository colorRepo)
    {
        _colorRepository = colorRepo;
        _selectedColors = colors;
        ColorShades = new ObservableCollection<Color>();
        Color = colors.First().Color;
        Tabs = new ObservableCollection<string>() { "Custom", "Library" };
        SelectedTab = 0;
       
        SelectShadeCommand = new RelayCommand<Color>(SelectShade);
        SelectPredefinedColorCommand = new RelayCommand<SolidColor>(SelectColor);
        UpdateColorShades(Color);
        ColorsLibrary = new List<SolidColor>();
    }

    private void SelectColor(SolidColor? color)
    {
        Color = color.Color;
    }

    private void SelectShade(Color shade)
    {
        Color = shade;
    }
    public List<SolidColor> ColorsLibrary { get; set; }
    public ObservableCollection<Color> ColorShades { get; set; }
    private Color _color;
    private List<SolidColorViewModel> _selectedColors { get; set; }
    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
           UpdateColorShades(Color);
           UpdateSelectedItemsColor(Color);
        }
    }

    public ObservableCollection<string> Tabs { get; set; }
    private int _selectedTab;
    private readonly StaticColorsRepository _colorRepository;

    public int SelectedTab
    {
        get => _selectedTab;
        set
        {
            _selectedTab = value;
            if (SelectedTab == 1)
            {
                UpdateLibrary();
            }
            OnPropertyChanged();
        }
    }
    public ICommand SelectShadeCommand { get; set; }
    public ICommand SelectPredefinedColorCommand { get; set; }

    private void UpdateSelectedItemsColor(Color color)
    {
        foreach (var solidColorViewModel in _selectedColors)
        {
            solidColorViewModel.Color = color;
        }
    }
    private void UpdateColorShades(Color color)
    {
        ColorShades?.Clear();
        foreach (var c in CreateShades(color))
        {
            ColorShades.Add(c);
        }
    }

    private void UpdateLibrary()
    {
        
        foreach (var item in _colorRepository.Items)
        {
            ColorsLibrary.Add(item as SolidColor);
        }
    }
    private List<Color> CreateShades(Color color)
    {
        var list = new List<Color>();
        float step = 1.8f / 6f;
        float startFactor = -0.8f;
        for (int i = 0; i < 5; i++)
        {
            var newColor = ChangeColorBrightness(color, startFactor);
            list.Add(newColor);
            startFactor += step;
        }
        return list;
    }
    public static Color ChangeColorBrightness(Color color, float correctionFactor)
    {
        float red = (float)color.R;
        float green = (float)color.G;
        float blue = (float)color.B;

        if (correctionFactor < 0)
        {
            correctionFactor = 1 + correctionFactor;
            red *= correctionFactor;
            green *= correctionFactor;
            blue *= correctionFactor;
        }
        else
        {
            red = (255 - red) * correctionFactor + red;
            green = (255 - green) * correctionFactor + green;
            blue = (255 - blue) * correctionFactor + blue;
        }

        return Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
    }

    public override void Dispose()
    {
        ColorsLibrary = null;
        ColorShades = null;
    }
}