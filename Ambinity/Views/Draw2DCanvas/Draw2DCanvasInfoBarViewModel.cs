using System.Windows.Input;
using Ambinity.ViewModels;
using AmbinityCore.DataBase;
using AmbinityCore.Models.GeneralSetting;
using AmbinityCore.Models.Profile;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Views.Draw2DCanvas;

public class Draw2DCanvasInfoBarViewModel : ViewModelBase
{
    public Draw2DCanvasInfoBarViewModel(LightingProfileDecoder decoder, GeneralSettingsManager generalSettingsManager)
    {
        _decoder = decoder;
        _generalSettings = generalSettingsManager.Settings;
        _decoder.RenderingStatusChanged += OnRenderingStatusChanged;
        OnRenderingStatusChanged();
        ShowCanvasLockedInfoCheck = new RelayCommand<bool>(ShowCanvasLockedInfoStatusChanged);
        StopRenderingCommand = new RelayCommand(StopRendering);
    }

    private void StopRendering()
    {
        _decoder.Stop();
    }

    /// <summary>
    /// Call everytime new canvas show
    /// </summary>
    public void Init()
    {
        OnRenderingStatusChanged();
    }

    private void ShowCanvasLockedInfoStatusChanged(bool status)
    {
        _generalSettings.ShowCanvasLockedInfo = !status;
    }

    private IGeneralSettings _generalSettings;

    private void OnRenderingStatusChanged()
    {
        if (_decoder.IsRendering && _generalSettings.ShowCanvasLockedInfo)
            ShowCanvasLockedInfo();
        else
        {
            SelfClear();
        }
    }

    private LightingProfileDecoder _decoder;

    public void ShowCanvasLockedInfo()
    {
        Message = "Canvas is locked while rendering! Please stop the profile to move, add or delete items";
        Severity = InfoBarSeverity.Informational;
        IsOpen = true;
    }

    public bool ShouldCanvasLockedInfo => _generalSettings.ShowCanvasLockedInfo;

    public void ShowCanvasEmptyError()
    {
    }

    public void ShowCanvasLayoutError()
    {
    }

    public void SelfClear()
    {
        IsOpen = false;
    }

    private string _message;

    public string Message
    {
        get => _message;
        set
        {
            _message = value;
            OnPropertyChanged();
        }
    }

    private string _title;

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged();
        }
    }

    private InfoBarSeverity _severity;

    public InfoBarSeverity Severity
    {
        get => _severity;
        set
        {
            _severity = value;
            OnPropertyChanged();
        }
    }

    private bool _isOpen;

    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            _isOpen = value;
            OnPropertyChanged();
        }
    }

    public ICommand ShowCanvasLockedInfoCheck { get; set; }
    public ICommand StopRenderingCommand { get; set; }
}