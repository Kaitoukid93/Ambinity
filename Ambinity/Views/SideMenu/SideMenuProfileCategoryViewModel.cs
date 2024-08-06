using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using Ambinity.Windows;
using AmbinityCore.Models.Profile;
using AmbinityCore.Models.ProfileCategory;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using ReactiveUI;

namespace Ambinity.Views.SideMenu;

public class SideMenuProfileCategoryViewModel : ViewModelBase
{
    public event Action<SideMenuProfileViewModel> SelectionChanged;

    public SideMenuProfileCategoryViewModel(LightingProfileCategory category, IDialogService dialogService,LightingProfileDecoder decoder)
    {
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

    private void Init()
    {
        if (_profileCategory == null)
            return;
        Content = _profileCategory.Name;
        foreach (var profile in _profileCategory.Profiles)
        {
            var profileViewModel = new SideMenuProfileViewModel(profile, this,_decoder);
            Profiles.Add(profileViewModel);
        }
    }

    private async Task ExecuteRenameCategory()
    {
        var vm = new InputDialogContentViewModel();
        await _dialogService.ShowInputDialog(vm, "Rename", "Ok", "Cancel");
    }

    private async Task ExecuteDeleteCategory()
    {
        // if (await _windowService.ShowConfirmContentDialog($"Delete {ProfileCategory.Name}",
        //         "Do you want to delete this category and all its profiles?"))
        // {
        //     if (ProfileCategory.ProfileConfigurations.Any(c => _profileService.FocusProfile == c))
        //         await _router.Navigate("home");
        //     _profileService.DeleteProfileCategory(ProfileCategory);
        // }
    }

    private async Task ExecuteAddProfile()
    {
        // ProfileConfiguration? result =
        //     await _windowService.ShowDialogAsync<ProfileConfigurationEditViewModel, ProfileConfiguration?>(
        //         ProfileCategory, ProfileConfiguration.Empty);
        // if (result != null)
        // {
        //     SidebarProfileConfigurationViewModel viewModel = _vmFactory.SidebarProfileConfigurationViewModel(result);
        //     SelectedProfileConfiguration = viewModel;
        // }
    }

    private async Task ExecuteImportProfile()
    {
        throw new System.NotImplementedException();
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