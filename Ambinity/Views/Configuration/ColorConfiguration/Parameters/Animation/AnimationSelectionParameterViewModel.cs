using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Views.AmbinityStore;
using Ambinity.Views.LayoutEditor;
using Ambinity.Windows;
using AmbinityCore.Colors;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Repositories;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Serilog;
using Tmds.DBus.Protocol;
using Animation = SkiaSharp.Skottie.Animation;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class AnimationSelectionParameterViewModel : ParameterViewModelBase
{
    private AnimationConfiguration _configuration;
    private readonly LibraryViewModelFactory _libraryViewModelFactory;
    private LibraryViewModelBase _libraryViewModel;
    private ProfileEditorRightPanelViewModel _rightPanelViewModel;
    private readonly IWindowService _windowService;
    private readonly AnimationsRepository _repository;
    private readonly AmbinityStoreItemExportViewModel _itemExportViewModel;
    private IAnimation _selectedAnimation;
    private LightingZone _zone;
    private AnimationsRepository _internalRepository => _zone?.ParentProfile?.AnimationRepository;

    public IAnimation SelectedAnimation
    {
        get => _selectedAnimation;
        set
        {
            _selectedAnimation = value;
            OnPropertyChanged();
        }
    }

    public AnimationSelectionParameterViewModel(LightingZone zone,
        ProfileEditorRightPanelViewModel rightPanelViewModel,
        LibraryViewModelFactory libraryViewModelFactory, IWindowService windowService, AnimationsRepository repository,
        AmbinityStoreItemExportViewModel itemExportViewModel, IDialogService dialogService)
    {
        _dialogService = dialogService;
        _itemExportViewModel = itemExportViewModel;
        _windowService = windowService;
        _repository = repository;
        //prepare internal repository for any animation selection
        if (_internalRepository == null)
            zone.ParentProfile.UpdateRepository(ConfigurationType.Animation);
        _libraryViewModelFactory = libraryViewModelFactory;
        _rightPanelViewModel = rightPanelViewModel;
        _zone = zone;
        _configuration = _zone.LightingConfiguration as AnimationConfiguration;
        _configuration.AnimationChanged += OnAnimationChanged;
        OpenLibraryCommand = new RelayCommand(OpenLibrary);
        SelectedAnimation = _internalRepository?.FindAnimation(_configuration.AnimationUID) ??
                            _repository?.FindAnimation(_configuration.AnimationUID);
        OnAnimationChanged();
        ImportAnimationCommand = new AsyncRelayCommand(ImportAnimation);
        ExportAnimationCommand = new AsyncRelayCommand(ExportAnimation);
    }

    private async Task ExportAnimation()
    {
        _itemExportViewModel.Init(SelectedAnimation);
        var window = _windowService.ShowWindow(_itemExportViewModel);
    }

    private void OnAnimationChanged()
    {
        SelectedAnimation = _repository.FindAnimation(_configuration.AnimationUID) ??
                            _internalRepository?.FindAnimation(_configuration.AnimationUID);
                            if (SelectedAnimation == null)
                            return;
        if (SelectedAnimation is GifAnimation)
        {
            var path = Path.Combine(SelectedAnimation.LocalPath, "animation.gif");
            Thumbnail = new GifAnimationThumbnailViewModel(path);

        }

        else if (SelectedAnimation is LottieJsonAnimation)
        {
            var path = Path.Combine(SelectedAnimation.LocalPath, "config.json");
            Thumbnail = new LottieAnimationThumbnailViewModel(path);
        }
       SelectedAnimation.LoadAnimation();
        Size = SelectedAnimation.Size;
        Duration = SelectedAnimation.Duration;
        Fps = SelectedAnimation.Fps;
        Size = SelectedAnimation.Size;
    }



    private async Task ImportAnimation()
    {
        string[]? result = await _windowService.CreateOpenFileDialog()
            .HavingFilter(f => f.WithExtension("json")
            .WithExtension("gif")
            .WithName("json or gif file"))
            .ShowAsync();
        if (result == null)
            return;
        var importFilePath = result.First();
        var filename = Path.GetFileNameWithoutExtension(importFilePath);
        // if (_internalRepository.Items.Any(i => i.Name == filename))
        // {
        //     var existed = _internalRepository.FindAnimation(filename);
        //     existed.LoadAnimation();
        //     _configuration.ChangeAnimation(existed);
        //     return;
        // }
        //remove all animation folders
        _internalRepository.ClearAllAnimations();
        var extension = Path.GetExtension(importFilePath);
        IAnimation animation = null;
        if (extension.Contains("json"))
        {
            animation = new LottieJsonAnimation(filename);
            _internalRepository.AddItem(animation);
            animation.Save();
            //rename json to config and copy to folder
            File.Copy(importFilePath, Path.Combine(animation.LocalPath, "config.json"), true);
        }
        else if (extension.Contains("gif"))
        {
            animation = new GifAnimation(filename);
            _internalRepository.AddItem(animation);
            animation.Save();
            //rename json to config and copy to folder
            File.Copy(importFilePath, Path.Combine(animation.LocalPath, "animation.gif"), true);
        }

        _configuration.ChangeAnimation(animation);
        //ask if user want to add to library
        var confirmationDialogVm = new ConfirmationDialogContentViewModel();
        confirmationDialogVm.DialogClosed += OnConfirmationDialogClosed;
        confirmationDialogVm.Content = "Do you want to add this animation to animation library?";
        await _dialogService.ShowConfirmationDialog(confirmationDialogVm, "New animation imported", "Add",
            "No");
    }

    private void OnConfirmationDialogClosed(object? sender, EventArgs e)
    {
        Log.Information("Adding item to system library");
        var vm = sender as ConfirmationDialogContentViewModel;
        var result = (e as ContentDialogClosedEventArgs).Result;
        if (result == ContentDialogResult.Secondary || result == ContentDialogResult.None)
            return;
        if (result == ContentDialogResult.Primary)
        {
            //copy current selected animation to system library
            var selectedAnimation = _internalRepository.FindAnimation(_configuration.AnimationUID);
            if (selectedAnimation == null)
            {
                Log.Information("Item not found, aborting...");
                return;
            }

            if (_repository.Items.Any(i => i.Name == selectedAnimation.Name))
            {
                Log.Information("Item existed, aborting...");
                return;
            }

            Directory.CreateDirectory(Path.Combine(_repository.LocalFolderPath, _selectedAnimation.Name));
            LocalFileHelpers.CopyDirectory(selectedAnimation.LocalPath,
                Path.Combine(_repository.LocalFolderPath, _selectedAnimation.Name), true);
            Log.Information("Item added to system library, reloading assets...");
            //reload item
            _repository.LoadFromDisk();
        }
    }

    private void OpenLibrary()
    {
        _libraryViewModel = _libraryViewModelFactory.GetLibraryViewModel("Animation");
        _libraryViewModel?.Init();
        _libraryViewModel.ItemSelected += OnAnimationSelected;
        _rightPanelViewModel.OpenFlyout(_libraryViewModel);
    }

    public override void Dispose()
    {
        if (_libraryViewModel != null)
            _libraryViewModel.ItemSelected -= OnAnimationSelected;
    }

    private void OnAnimationSelected(AssetItemViewModelBase item)
    {
        if (item is AnimationAssetViewModel)
        {
            //clear internal repository
            _internalRepository.ClearAllAnimations();
            var animationAsset = item as AnimationAssetViewModel;
            //add this to internal repo by copying item
            LocalFileHelpers.CopyDirectory(animationAsset.Item.LocalPath,
                Path.Combine(_internalRepository.LocalFolderPath, animationAsset.Item.Name), true);
            //reload item
            _internalRepository.LoadFromDisk();
            _configuration.ChangeAnimation(animationAsset.Item as IAnimation);
        }
    }

    public ICommand OpenLibraryCommand { get; }
    public ICommand ImportAnimationCommand { get; }
    private int _frameRate;
    private readonly IDialogService _dialogService;

    public int FrameRate
    {
        get => _frameRate;
        set
        {
            if (value < 0)
                value = 0;
            if (value > 5)
                value = 5;
            _configuration.FrameRate = value;
            _frameRate = value;
            OnPropertyChanged();
        }
    }
    private ViewModelBase _thumbnail;
    public ViewModelBase Thumbnail
    {
        get => _thumbnail; set
        {
            _thumbnail = value;
            OnPropertyChanged();
        }
    }
    private TimeSpan _duration;
    private string _fps;
    private string _size;
    private string _version;
    public TimeSpan Duration
    {
        get => _duration;
        set
        {
            _duration = value;
        }
    }
    public string Fps
    {
        get => _fps;
        set
        {
            _fps = value;
            OnPropertyChanged();
        }
    }
    public string Size
    {
        get => _size;
        set
        {
            _size = value;
            OnPropertyChanged();
        }
    }

    public string Version
    {
        get => _version;
        set
        {
            _version = value;
            OnPropertyChanged();
        }
    }
    public AnimationConfiguration Configuration => _configuration;
    public ICommand ExportAnimationCommand { get; }
}
