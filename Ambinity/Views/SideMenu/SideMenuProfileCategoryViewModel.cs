using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Windows;
using AmbinityCore;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Profile;
using AmbinityCore.Models.ProfileCategory;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using MathNet.Numerics.Distributions;
using Newtonsoft.Json;
using Serilog;
using Ambinity.Localization;

namespace Ambinity.Views.SideMenu;

public class SideMenuProfileCategoryViewModel : ViewModelBase
{
    public event Action<SideMenuProfileViewModel> SelectionChanged;
    public event Action<SideMenuProfileCategoryViewModel> Delete;
    private IWindowService _windowService;

    public SideMenuProfileCategoryViewModel(LightingProfileCategory category,
        IDialogService dialogService,
        LightingProfileDecoder decoder,
        LightingProfileRepository profileRepository,
        IWindowService windowService,
        ThumbnailService thumbnailService, SideMenuViewModelFactory vmFactory)
    {
        _windowService = windowService;
        _vmFactory = vmFactory;
        _thumbnailService = thumbnailService;
        _profileRepository = profileRepository;
        _profileRepository.ItemDownloaded += OnNewProfileDownloaded;
        _profileCategory = category;
        Content = _profileCategory.Name;
        _dialogService = dialogService;
        _decoder = decoder;
        Profiles = new ObservableCollection<SideMenuProfileViewModel>();
        ToggleCollapsed = new RelayCommand(ExecuteToggleCollapsed);
        ToggleSuspended = new RelayCommand(ExecuteToggleSuspended);
        AddProfile = new AsyncRelayCommand(ExecuteAddProfile);
        ImportProfile = new AsyncRelayCommand(ExecuteImportProfile);
        MoveUp = new RelayCommand(ExecuteMoveUp);
        MoveDown = new RelayCommand(ExecuteMoveDown);
        RenameCategory = new AsyncRelayCommand(ExecuteRenameCategory);
        RenameCategory = new AsyncRelayCommand(ExecuteRenameCategory);
        DeleteCategory = new AsyncRelayCommand(ExecuteDeleteCategory);
        Init();
    }

    /// <summary>
    /// import or update an item from downloaded path
    /// </summary>
    /// <param name="path"></param>
    private void OnNewProfileDownloaded(LightingProfile profile)
    {
        if (this.Category.Name != "Download")
            return;

        //write new file
        var profileVm = _vmFactory.GetProfileViewModel(profile, this);
        _profileCategory.AddProfile(profile);
        Profiles.Add(profileVm);
        profile.UpdateIcon();
        Log.Information("Successfully downloaded" + " " + profile.Name + "!");
    }

    public LightingProfileCategory Category => _profileCategory;
    private LightingProfileCategory _profileCategory;
    private IDialogService _dialogService;
    private LightingProfileDecoder _decoder;
    private string _content = "New Catergory";
    private bool? _isCollapsed;
    private ObservableCollection<SideMenuProfileViewModel> _profiles;

    public ObservableCollection<SideMenuProfileViewModel> Profiles
    {
        get => _profiles;
        set
        {
            _profiles = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Display Name
    /// </summary>
    public string Content
    {
        get => _content;
        set
        {
            _content = value;
            OnPropertyChanged();
        }
    }

    private bool _isExpanded = true;

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            _isExpanded = value;
            OnPropertyChanged();
        }
    }

    private bool _isSuspended = false;

    public bool IsSuspended
    {
        get => _isSuspended;
        set
        {
            _isSuspended = value;
            OnPropertyChanged();
        }
    }

    private SideMenuProfileViewModel _selectedProfile;
    private readonly LightingProfileRepository _profileRepository;
    private readonly ThumbnailService _thumbnailService;
    private readonly SideMenuViewModelFactory _vmFactory;

    public SideMenuProfileViewModel SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            _selectedProfile = value;
            OnPropertyChanged();
            if (value != null)
            {
                SelectionChanged?.Invoke(value);
            }
        }
    }

    #region Methods

    public void RemoveProfile(SideMenuProfileViewModel profile)
    {
        Profiles.Remove(profile);
        Category.RemoveProfile(profile.Profile);
        _profileRepository.RemoveItem(profile.Profile);
    }

    public void Duplicate(SideMenuProfileViewModel profile)
    {
        var clone = ObjectHelpers.Clone(profile.Profile);
        //make sure there is no duplicate playing
        clone.IsPlaying = false;
        clone.Name = profile.Profile.Name + " -copy";
        clone.ID = Guid.NewGuid();
        _profileRepository.AddItem(clone);
        _profileCategory.AddProfile(clone);
        var cloneVm = _vmFactory.GetProfileViewModel(clone, this);
        Profiles.Add(cloneVm);
        Log.Information("Successfully clone" + " " + profile.Profile.Name + "!");
    }

    private void Init()
    {
        if (_profileCategory == null)
            return;
         Content = Loc.TryGetTranslated(_profileCategory.Name + ".ProfileCategory.Name", _profileCategory.Name);
        _profileCategory.FindChild(_profileRepository.Items.ToList());
        var profiles = new ObservableCollection<SideMenuProfileViewModel>();
        foreach (var profile in _profileCategory.Profiles)
        {
            var profileViewModel = _vmFactory.GetProfileViewModel(profile, this);
            profiles.Add(profileViewModel);
        }

        Profiles = new ObservableCollection<SideMenuProfileViewModel>(profiles.OrderBy(i => i.Profile.Name).ToList());
    }

    private async Task ExecuteRenameCategory()
    {
        var vm = new InputDialogContentViewModel();
        vm.DialogClosed += OnRenameDialogClosed;
        await _dialogService.ShowInputDialog(vm, "Rename", "Ok", "Cancel");
    }

    private void OnRenameDialogClosed(object? sender, EventArgs e)
    {
        var vm = sender as InputDialogContentViewModel;
        var result = (e as ContentDialogClosedEventArgs).Result;
        if (result == ContentDialogResult.Secondary || result == ContentDialogResult.None)
            return;
        if (result == ContentDialogResult.Primary)
        {
            _profileCategory.Name = vm.UserInput;
            _profileCategory.Save();
            Content = vm.UserInput;
        }
    }

    private async Task ExecuteDeleteCategory()
    {
        var vm = new ConfirmationDialogContentViewModel();
        vm.DialogClosed += OnDeleteDialogClosed;
        vm.Content = "Do you want to delete this category and it's profiles?";
        await _dialogService.ShowConfirmationDialog(vm, "Delete", "Ok", "Cancel");
    }

    private void OnDeleteDialogClosed(object? sender, EventArgs e)
    {
        var vm = sender as DeleteDialogContentViewModel;
        var result = (e as ContentDialogClosedEventArgs).Result;
        if (result == ContentDialogResult.Secondary || result == ContentDialogResult.None)
            return;
        if (result == ContentDialogResult.Primary)
        {
            //delete category
            Delete?.Invoke(this);
        }
    }

    private async Task ExecuteAddProfile()
    {
        //load available template from resource
        var vm = new NewProfileDialogContentViewModel();
        vm.DialogClosed += OnCreateNewProfileDialogClosed;
        await _dialogService.ShowCreateNewProfileDialog(vm);
    }

    private void OnCreateNewProfileDialogClosed(object? sender, EventArgs e)
    {
        var vm = sender as NewProfileDialogContentViewModel;
        var result = (e as ContentDialogClosedEventArgs).Result;
        if (result == ContentDialogResult.Secondary || result == ContentDialogResult.None)
            return;
        if (result == ContentDialogResult.Primary)
        {
            //create new profile
            var template = vm.AvailableTemplates.Where(t => t.IsSelected).FirstOrDefault();
            if(template==null)
                return;
            var profile = template.Profile;
            profile.Name = vm.UserInput;
            profile.IsDefault = false;
            profile.ID = Guid.NewGuid();
            profile.CategoryID = this.Category.ID;
            profile.Category = this.Category;
            //add to repo
            var profileViewModel = _vmFactory.GetProfileViewModel(profile, this);
            Profiles.Add(profileViewModel);
            _profileRepository.AddItem(profile);
        }
    }

    /// <summary>
    /// import from zip executed from side menu, profile need category information
    /// </summary>
    private async Task ExecuteImportProfile()
    {
        string[]? result = await _windowService.CreateOpenFileDialog()
            .HavingFilter(f => f.WithExtension("zip").WithName("Zip archive"))
            .ShowAsync();
        if (result == null)
            return;
        var importFilePath = result.First();
        _profileRepository.ImportZipProfile(importFilePath,this.Category);
        //reload item
        Init();
    }

    private void ExecuteToggleSuspended()
    {
        throw new System.NotImplementedException();
    }

    private void ExecuteToggleCollapsed()
    {
        if (IsExpanded)
            IsExpanded = false;
        else
        {
            IsExpanded = true;
        }
    }

    private void ExecuteMoveDown()
    {
        throw new System.NotImplementedException();
    }

    private void ExecuteMoveUp()
    {
        throw new System.NotImplementedException();
    }

    #endregion

    public AsyncRelayCommand ImportProfile { get; }
    public RelayCommand ToggleCollapsed { get; }
    public RelayCommand ToggleSuspended { get; }
    public AsyncRelayCommand AddProfile { get; }
    public RelayCommand MoveUp { get; }
    public RelayCommand MoveDown { get; }
    public AsyncRelayCommand RenameCategory { get; }
    public AsyncRelayCommand DeleteCategory { get; }
}
