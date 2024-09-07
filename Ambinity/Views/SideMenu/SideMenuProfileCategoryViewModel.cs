using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Windows;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Profile;
using AmbinityCore.Models.ProfileCategory;
using AmbinityServer.OnlineItem;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Serilog;

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
        ThumbnailService thumbnailService,SideMenuViewModelFactory vmFactory)
    {
        _windowService = windowService;
        _vmFactory = vmFactory;
        _thumbnailService = thumbnailService;
        _profileRepository = profileRepository;
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
        DeleteCategory = new AsyncRelayCommand(ExecuteDeleteCategory);
        Init();
    }

    public LightingProfileCategory Category => _profileCategory;
    private LightingProfileCategory _profileCategory;
    private IDialogService _dialogService;
    private LightingProfileDecoder _decoder;
    private string _content = "New Catergory";
    private bool? _isCollapsed;
    public ObservableCollection<SideMenuProfileViewModel> Profiles { get; set; }

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

    private bool _isExpanded = false;

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
        _profileCategory.AddProfile(clone);
        _profileRepository.AddItem(clone);
        var cloneVm = _vmFactory.GetProfileViewModel(clone, this);
        Profiles.Add(cloneVm);
        Log.Information("Successfully clone" + " "+ profile.Profile.Name + "!");
    }
    private void Init()
    {
        if (_profileCategory == null)
            return;
        Content = _profileCategory.Name;
        _profileCategory.FindChild(_profileRepository.Items.ToList());
        foreach (var profile in _profileCategory.Profiles)
        {
            var profileViewModel = _vmFactory.GetProfileViewModel(profile,this);
            Profiles.Add(profileViewModel);
        }
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
            //create new profile
            _profileCategory.Name = vm.UserInput;
            _profileCategory.Save();
            Content = vm.UserInput;
        }
    }

    private async Task ExecuteDeleteCategory()
    {
        var vm = new DeleteDialogContentViewModel();
        vm.DialogClosed += OnDeleteDialogClosed;
        vm.Content = "Do you want to delete this category and it's profiles?";
        await _dialogService.ShowDeleteDialog(vm, "Delete", "Ok", "Cancel");
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
        var vm = new InputDialogContentViewModel();
       // vm.DialogClosed += OnCreateNewProfileDialogClosed;
        await _dialogService.ShowInputDialog(vm, "New profile", "Ok", "Cancel");
    }
    private void OnCreateNewProfileDialogClosed(object? sender, EventArgs e)
    {
        var vm = sender as InputDialogContentViewModel;
        var result = (e as ContentDialogClosedEventArgs).Result;
        if (result == ContentDialogResult.Secondary || result == ContentDialogResult.None)
            return;
        if (result == ContentDialogResult.Primary)
        {
            //create new profile
            var category = new LightingProfileCategory();
            category.Name = vm.UserInput;
            category.IsDefault = false;
            category.ID = Guid.NewGuid();
            //add to repo
        }
    }
    private async Task ExecuteImportProfile()
    {
        string[]? result = await _windowService.CreateOpenFileDialog()
            
            .HavingFilter(f => f.WithExtension("zip").WithName("Zip archive"))
            .ShowAsync();

        if (result == null)
            return;
        var importFilePath = result.First();
        //execute import
        
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