using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Colors;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Repositories;
using CommunityToolkit.Mvvm.Input;
using Animation = SkiaSharp.Skottie.Animation;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class AnimationSelectionParameterViewModel : ParameterViewModelBase
{
    private AnimationConfiguration _configuration;
    private readonly LibraryViewModelFactory _libraryViewModelFactory;
    private LibraryViewModelBase _libraryViewModel;
    private RightPanelViewModel _rightPanelViewModel;
    private readonly IWindowService _windowService;

    public AnimationSelectionParameterViewModel(AnimationConfiguration configuration,
        RightPanelViewModel rightPanelViewModel,
        LibraryViewModelFactory libraryViewModelFactory, IWindowService windowService)
    {
        _windowService = windowService;
        _libraryViewModelFactory = libraryViewModelFactory;
        _rightPanelViewModel = rightPanelViewModel;
        _configuration = configuration;
        OpenLibraryCommand = new RelayCommand(OpenLibrary);
        Animation = _configuration.Animation;
        ImportAnimationCommand = new AsyncRelayCommand(ImportAnimation);
    }
    private async Task ImportAnimation()
    {
        string[]? result = await _windowService.CreateOpenFileDialog()
            
            .HavingFilter(f => f.WithExtension("json").WithName("json file"))
            .ShowAsync();

        if (result == null)
            return;
        var importFilePath = result.First();
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

    private void OnAnimationSelected(ICollectableItem obj)
    {
       // throw new System.NotImplementedException();
    }

    public ICommand OpenLibraryCommand { get; }
    public ICommand ImportAnimationCommand { get; }
    public Animation Animation { get; set; }
    
}