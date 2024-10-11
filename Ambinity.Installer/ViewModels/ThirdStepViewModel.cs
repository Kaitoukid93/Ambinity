using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using Ambinity.Installer.Models.Constant;
using Avalonia.Media;

namespace Ambinity.Installer.ViewModels;

public class ThirdStepViewModel : StepViewModelBase
{
    public event Action<Color> AccentColorChanged;
    public ThirdStepViewModel()
    {
        Header="Settings";
        SubHeader="Some initial settings for better experience";
        StepIndex = 3;
        CanBack = false;
        CanCancel = false;
        CanForward = false;
        DefaultColors = DefaultSolidColors.Colors;
    }

    private Color _selectedColor;
    public Color SelectedColor
    {
        get=> _selectedColor;
        set
        {
            _selectedColor = value;
            AccentColorChanged?.Invoke(value);
            OnPropertyChanged();
        }
    }
    public string Header { get; set; }
    public string SubHeader { get; set; }
    public List<Color> DefaultColors { get; set; } = new List<Color>();
}