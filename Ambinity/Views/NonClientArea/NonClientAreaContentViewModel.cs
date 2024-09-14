using System;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Views.AppTour;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.NonClientArea;

public class NonClientAreaContentViewModel : ViewModelBase
{
    public event Action ShowAppTourEvent;
    public NonClientAreaContentViewModel(IWindowService windowService)
    {
        // Geometry = geometry;
        // Header = header;
        _windowService = windowService;;
        _lifeTime = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
    }

    private void ShowAppTour()
    {
        ShowAppTourEvent?.Invoke();
        
    }
    private readonly IClassicDesktopStyleApplicationLifetime _lifeTime;
    // public NonClientAreaContentViewModel(string content, string geometry, bool showBackButton, ICommand buttonCommand)
    // {
    //     Header = content;
    //     Geometry = geometry;
    //     ShowBackButton = showBackButton;
    //     if (ShowBackButton)
    //         BackButtonCommand = buttonCommand;
    // }

    public string Geometry { get; set; }
    private string _header;

    public string Header
    {
        get { return _header; }
        set
        {
            _header = value;
            OnPropertyChanged();
        }
    }

    private bool _showBackButton;
    private ICommand _backButtonCommand;
    private readonly IWindowService _windowService;
    public ICommand BackButtonCommand
    {
        get { return _backButtonCommand; }
        set
        {
            _backButtonCommand = value;
            OnPropertyChanged();
        }
    }

    public bool ShowBackButton
    {
        get { return _showBackButton; }
        set
        {
            _showBackButton = value;
            OnPropertyChanged();
        }
    }
}