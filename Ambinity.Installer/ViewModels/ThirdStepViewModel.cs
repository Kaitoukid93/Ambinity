using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using Ambinity.Installer.Models;
using Ambinity.Installer.Models.Constant;
using Avalonia.Media;
using FluentAvalonia.Styling;
using Newtonsoft.Json;

namespace Ambinity.Installer.ViewModels;

public class ThirdStepViewModel : StepViewModelBase
{
    public event Action<Color> AccentColorChanged;

    public ThirdStepViewModel(PostInstallationSettings settings)
    {
        Header = "Settings";
        SubHeader = "Some initial settings for better experience";
        StepIndex = 3;
        CanBack = false;
        CanCancel = false;
        CanForward = false;
        DefaultColors = DefaultSolidColors.Colors;
        _initialSettings = settings;
    }
    
    private PostInstallationSettings _initialSettings;
    private Color _selectedColor;

    public Color SelectedColor
    {
        get => _selectedColor;
        set
        {
            _selectedColor = value;
            AccentColorChanged?.Invoke(value);
            _initialSettings.PrimaryColor = value;
            OnPropertyChanged();
        }
    }

    private bool _autoStart = true;

    public bool AutoStart
    {
        get => _autoStart;
        set
        {
            _autoStart = value;
            _initialSettings.AutoStart = value;
            OnPropertyChanged();
        }
    }
    private bool _createDesktopShortcut = true;

    public bool CreateDesktopShortcut
    {
        get => _createDesktopShortcut;
        set
        {
            _createDesktopShortcut = value;
            _initialSettings.CreateDesktopShortcut = value;
            OnPropertyChanged();
        }
    }
    public string Header { get; set; }
    public string SubHeader { get; set; }

    public List<Color> DefaultColors { get; set; } = new List<Color>();
    private bool _openAfterFinish = true;

    public bool OpenAfterFinish
    {
        get => _openAfterFinish;
        set
        {
            _openAfterFinish = value;
            _initialSettings.OpenAfterFinish = value;
            OnPropertyChanged();
        }
    }

}