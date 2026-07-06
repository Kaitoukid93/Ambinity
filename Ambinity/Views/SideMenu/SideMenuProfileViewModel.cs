using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Views.AmbinityStore;
using Ambinity.Windows;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Profile;
using AmbinityServer.OnlineItem;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using Ambinity.Localization;

namespace Ambinity.Views.SideMenu;

public class SideMenuProfileViewModel : ViewModelBase
{
    public SideMenuProfileViewModel(LightingProfileItem profile,
     SideMenuProfileCategoryViewModel category,
        LightingProfileDecoder decoder,
         ThumbnailService thumbnailService,
          IDialogService dialogService,
        SideMenuViewModelFactory vmFactory,
         IWindowService windowService,
         AmbinityStoreItemExportViewModel exportViewModel)
    {
        _exportViewModel = exportViewModel;
        _vmFactory = vmFactory;
        Profile = profile;
        Profile.PropertyChanged += OnItemChanged;
        Catergory = category;
        _decoder = decoder;
        _decoder.RenderingStatusChanged += OnRenderingStatusChanged;
        SelfDeleteCommand = new RelayCommand(SelfDelete, CanDelete);
        OpenPropertiesEditorCommand = new RelayCommand(OpenPropertiesEditor);
        DuplicateCommand = new RelayCommand(SelfDuplicate);
        ExportCommand = new AsyncRelayCommand(ExportProfile);
        _thumbnailService = thumbnailService;
        _dialogService = dialogService;
        _windowService = windowService;
        Init();
        CommandSetup();
    }
    private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 🔥 forward changes to UI
        OnPropertyChanged(e.PropertyName);
    }

    private async Task ExportProfile()
    {
        //prepare profile for exporting
        // copy asset if any zone required it to asset folder
        _exportViewModel.Init(Profile);
        var window = _windowService.ShowWindow(_exportViewModel);
        //zip
        //save
    }

    private void SelfDuplicate()
    {
        Catergory.Duplicate(this);
    }

    private void OpenPropertiesEditor()
    {
        var vm = _vmFactory.GetProfilePropertiesViewModel(Profile);
        _dialogService.ShowWindowDialog(vm, "Edit Properties", "Save", "Cancel");
    }

    private bool CanDelete()
    {
        return !_isPlaying;
    }

    private void SelfDelete()
    {
        Catergory.RemoveProfile(this);
        //profile category should be aware of this, we keep the file in the profile repository
        //because the category own this profile will not init this profile anymore but others does
    }

    private void OnRenderingStatusChanged(bool isplaying, string profileID)
    {
        if (this.Profile.Id != profileID)
            return;
        IsPlaying = Profile.IsPlaying;
    }

    public LightingProfileItem Profile { get; }
    public string Content => Loc.TryGetTranslated(Profile.Name + ".Profile.Name", Profile.Name);
    public string Icon => Profile.Icon;



    private LightingProfileDecoder _decoder;
    private ThumbnailService _thumbnailService;
    public SideMenuProfileCategoryViewModel Catergory { get; set; }

    private bool _isSelected;

    /// <summary>
    /// Indicate this profile is selected on side menu
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }


    private readonly SideMenuViewModelFactory _vmFactory;
    private readonly IDialogService _dialogService;
    private readonly IWindowService _windowService;
    private bool _isPlaying;
    private readonly AmbinityStoreItemExportViewModel _exportViewModel;

    public bool IsPlaying
    {
        get => _isPlaying;
        set
        {
            _isPlaying = value;
            OnPropertyChanged();
        }
    }

    #region Methods

    private async Task Init()
    {
        if (Profile == null)
            return;
        IsPlaying = Profile.IsPlaying;
    }

    private void CommandSetup()
    {
        TogglePlayPauseProfileCommand = new AsyncRelayCommand(TogglePlayPause);
    }

    private async Task TogglePlayPause()
    {
        await _decoder.Toggle(this.Profile.ID);
        // IsPlaying = !IsPlaying;
        // if (!IsPlaying)
        // {
        //     _decoder.Stop();
        // }
        // else
        // {
        //     if (this.Profile == _decoder.CurrentPlayingProfile)
        //         _decoder.Resume();
        //     else
        //     {
        //         await _decoder.Stop();
        //         _decoder.Init(this.Profile);
        //     }
        // }
    }

    public Task<Bitmap> GetThumbnail => GetThumbnailAsync();

    private async Task<Bitmap> GetThumbnailAsync()
    {
        var thumbnailPath = Path.Combine(Profile.LocalPath, "icon.png");
        if (!File.Exists(thumbnailPath))
            return null;
        var thumb = await _thumbnailService.LoadThumbnail(thumbnailPath);
        return thumb;
    }

    public ICommand TogglePlayPauseProfileCommand { get; set; }
    public ICommand OpenPropertiesEditorCommand { get; set; }
    public ICommand SelfDeleteCommand { get; set; }
    public ICommand DuplicateCommand { get; set; }
    public ICommand ExportCommand { get; set; }

    #endregion
}
