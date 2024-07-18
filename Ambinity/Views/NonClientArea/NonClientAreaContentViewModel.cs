using System.Windows.Input;
using Ambinity.ViewModels;

namespace Ambinity.Views.NonClientArea;

public class NonClientAreaContentViewModel : ViewModelBase
{
    public NonClientAreaContentViewModel(string header, string geometry)
    {
        Geometry = geometry;
        Header = header;
    }

    public NonClientAreaContentViewModel(string content, string geometry, bool showBackButton, ICommand buttonCommand)
    {
        Header = content;
        Geometry = geometry;
        ShowBackButton = showBackButton;
        if (ShowBackButton)
            BackButtonCommand = buttonCommand;
    }

    public string Geometry { get; set; }
    private string _header;

    public string Header
    {
        get { return _header; }
        set
        {
            _header = value;
            RaisePropertyChanged(nameof(Header));
        }
    }

    private bool _showBackButton;
    private ICommand _backButtonCommand;

    public ICommand BackButtonCommand
    {
        get { return _backButtonCommand; }
        set
        {
            _backButtonCommand = value;
            RaisePropertyChanged(nameof(BackButtonCommand));
        }
    }

    public bool ShowBackButton
    {
        get { return _showBackButton; }
        set
        {
            _showBackButton = value;
            RaisePropertyChanged(nameof(ShowBackButton));
        }
    }
}