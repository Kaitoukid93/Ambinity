using System.Collections.Generic;
using Ambinity.Services;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.CapturingService;
using AmbinityCore.Colors;
using AmbinityCore.DataBase;
using AmbinityCore.LightingEngines;
using AmbinityCore.Models.GeneralSetting;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Repositories;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

/// <summary>
/// return list of parameter based on lighting configurations
/// and for my laziness, I also add separator viewmodel -_-
/// </summary>
public class ParameterViewModelFactory
{
    public ParameterViewModelFactory(RightPanelViewModel rightPanelViewModel,
        AudioCapturingService audioCapturingService,
        IWindowService windowService, GeneralSettingsManager settingsManager,
        StaticColorsRepository colorsRepository, LibraryViewModelFactory libraryViewModelFactory,
        BrightnessProviderFactory brightnessProviderFactory, ScreenCapturingService screenCapturingService)
    {
        _audioCapturingService = audioCapturingService;
        _libraryViewModelFactory = libraryViewModelFactory;
        _rightPanelViewModel = rightPanelViewModel;
        _colorsRepository = colorsRepository;
        _windowService = windowService;
        _settingsManager = settingsManager;
        _brightnessProviderFactory = brightnessProviderFactory;
        _screenCapturingService = screenCapturingService;
    }

    private readonly RightPanelViewModel _rightPanelViewModel;
    private readonly StaticColorsRepository _colorsRepository;
    private readonly BrightnessProviderFactory _brightnessProviderFactory;
    private readonly AudioCapturingService _audioCapturingService;
    private readonly IWindowService _windowService;
    private readonly GeneralSettingsManager _settingsManager;
    private readonly ScreenCapturingService _screenCapturingService;
    private readonly LibraryViewModelFactory _libraryViewModelFactory;


    public List<ParameterViewModelBase> CreateParameterViewModels(ILightingConfiguration config)
    {
        switch (config.Type)
        {
            case ConfigurationType.Animation:
                return GetAnimationParameters(config);
                break;
            case ConfigurationType.ScreenCapture:
                return GetScreenCaptureParameters(config);
                break;
            case ConfigurationType.SelfGeneratedColor:
                return GetSelfGeneratedColorParameters(config);
                break;
            default:
                return null;
        }
    }

    private List<ParameterViewModelBase> GetSelfGeneratedColorParameters(ILightingConfiguration config)
    {
        var parameters = new List<ParameterViewModelBase>();
        var _configuration = config as SelfGeneratedColorConfiguration;
        var colorSelectorParameter = new FillColorSelectionViewModel(_configuration, _rightPanelViewModel,
            _colorsRepository, _libraryViewModelFactory,_windowService);
        parameters.Add(colorSelectorParameter);

        parameters.Add(new SeparationParameterViewModel());
        var colorBehaviorParameterViewModel = new ColorBehaviorParameterViewModel(_configuration);
        parameters.Add(colorBehaviorParameterViewModel);

        parameters.Add(new SeparationParameterViewModel());
        var colorAppearanceParameter = new ColorAppearanceParameterViewModel(_configuration);
        parameters.Add(colorAppearanceParameter);
        parameters.Add(new SeparationParameterViewModel());
        var motionConfigParamter = new MotionConfigParameterViewModel(_configuration, _audioCapturingService,
            _rightPanelViewModel, _brightnessProviderFactory);
        parameters.Add(motionConfigParamter);
        return parameters;
    }

    private List<ParameterViewModelBase> GetScreenCaptureParameters(ILightingConfiguration configuration)
    {
        var captureParameter = new ScreenRegionSelectionParameterViewModel(configuration as ScreenCaptureConfiguration,
            _windowService, _settingsManager,_screenCapturingService);
        var blackBarDetectionParameter = new BlackBarDetectionParameterViewModel();
        return new List<ParameterViewModelBase>()
            { captureParameter, new SeparationParameterViewModel(), blackBarDetectionParameter };
    }

    private List<ParameterViewModelBase> GetAnimationParameters(ILightingConfiguration configuration)
    {
        var animationSelectionParameter = new AnimationSelectionParameterViewModel(configuration as AnimationConfiguration,_rightPanelViewModel,_libraryViewModelFactory,_windowService);
        return new List<ParameterViewModelBase>()
            { animationSelectionParameter };
    }
}