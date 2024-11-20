using System;
using System.Windows.Input;
using AmbinityServer.AppRelease;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Installer.ViewModels;

public class AppReleaseInfomationViewModel : ViewModelBase
{
    private readonly AppReleaseInformation _infomation;
    public AppReleaseInformation Information => _infomation;

    public AppReleaseInfomationViewModel(AppReleaseInformation information)
    {
        _infomation = information;
        Version = _infomation.Version;
        ReleaseDate = _infomation.ReleaseDate;
        SelectReleaseCommand = new RelayCommand(SelectRelease);
    }

    private void SelectRelease()
    {
        IsSelected = true;
    }

    public string Version { get; set; }
    public DateTime ReleaseDate { get; set; }
    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public ICommand SelectReleaseCommand { get; }
}