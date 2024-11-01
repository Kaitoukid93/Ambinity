using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Colors;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Repositories;
using CommunityToolkit.Mvvm.Input;
using Serilog;
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

    public AnimationSelectionParameterViewModel(AnimationConfiguration configuration,
        ProfileEditorRightPanelViewModel rightPanelViewModel,
        LibraryViewModelFactory libraryViewModelFactory, IWindowService windowService,AnimationsRepository repository)
    {
        _windowService = windowService;
        _repository = repository;
        _libraryViewModelFactory = libraryViewModelFactory;
        _rightPanelViewModel = rightPanelViewModel;
        _configuration = configuration;
        _configuration.AnimationChanged += OnAnimationChanged;
        OpenLibraryCommand = new RelayCommand(OpenLibrary);
        _configuration?.Animation?.LoadAnimation();
        OnAnimationChanged();
        ImportAnimationCommand = new AsyncRelayCommand(ImportAnimation);
    }
    
    private void OnAnimationChanged()
    {
        if(_configuration.Animation!=null)
        Animation = _configuration.Animation.SkottieAnimation;
    }

    private async Task ImportAnimation()
    {
        string[]? result = await _windowService.CreateOpenFileDialog()
            .HavingFilter(f => f.WithExtension("json").WithName("json file"))
            .ShowAsync();
        if (result == null)
            return;
        var importFilePath = result.First();
        var filename = Path.GetFileNameWithoutExtension(importFilePath);
        var animation = new AmbinityCore.Repositories.Animation(filename);
        _repository.AddItem(animation);
        animation.Save();
        //rename json to config and copy to folder
        File.Copy(importFilePath, Path.Combine(animation.LocalPath,"config.json"),true);
        animation.LoadAnimation();
        _configuration.ChangeAnimation(animation);
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
        if(_libraryViewModel!=null)
        _libraryViewModel.ItemSelected -= OnAnimationSelected;
        
    }

    private void OnAnimationSelected(AssetItemViewModelBase item)
    {
        if (item is AnimationAssetViewModel)
        {
            var animationAsset = item as AnimationAssetViewModel;
            _configuration.ChangeAnimation(animationAsset.Item as AmbinityCore.Repositories.Animation);
        }
        
    }

    public ICommand OpenLibraryCommand { get; }
    public ICommand ImportAnimationCommand { get; }
    public Animation Animation { get; set; }
    private int _frameRate;
    public int FrameRate
    {
        get => _frameRate;
        set
        {
            _configuration.FrameRate = value;
            _frameRate = value;
            OnPropertyChanged();
        }
    } 

    public AnimationConfiguration Configuration => _configuration;

}