using System.Collections.Generic;
using Ambinity.Services;
using Ambinity.Views.LayoutEditor;
using Ambinity.Windows;
using AmbinityCore.CapturingService;
using AmbinityCore.Colors;
using AmbinityCore.DataBase;
using AmbinityCore.LightingEngines;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Models.Profile;
using AmbinityCore.Repositories;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

/// <summary>
/// return list of parameter based on lighting configurations
/// and for my laziness, I also add separator viewmodel -_-
/// </summary>
public class ParameterViewModelFactory(
    ProfileEditorRightPanelViewModel rightPanelViewModel,
    AudioCapturingService audioCapturingService,
    IWindowService windowService,
    IDialogService dialogService,
    GeneralSettingsManager settingsManager,
    StaticColorsRepository colorsRepository,
    ColorPaletteRepository colorPaletteRepository,
    AnimationsRepository animationsRepository,
    LibraryViewModelFactory libraryViewModelFactory,
    BrightnessProviderFactory brightnessProviderFactory,
    ScreenCapturingService screenCapturingService,
    LightingProfileDecoder decoder)
{
    public List<ParameterViewModelBase> CreateParameterViewModels(ILightingConfiguration config)
    {
        return config.Type switch
        {
            ConfigurationType.Animation => GetAnimationParameters(config),
            ConfigurationType.ScreenCapture => GetScreenCaptureParameters(config),
            ConfigurationType.SelfGeneratedColor => GetSelfGeneratedColorParameters(config)
        };
    }

    private List<ParameterViewModelBase>? GetSelfGeneratedColorParameters(ILightingConfiguration config)
    {
        var parameters = new List<ParameterViewModelBase>();
        if (config is not SelfGeneratedColorConfiguration configuration)
            return null;
        var colorSelectorParameter = new FillColorSelectionViewModel(configuration, rightPanelViewModel,
            colorsRepository, colorPaletteRepository, libraryViewModelFactory, windowService, dialogService);
        parameters.Add(colorSelectorParameter);

        parameters.Add(new SeparationParameterViewModel());
        var colorBehaviorParameterViewModel = new ColorBehaviorParameterViewModel(configuration);
        parameters.Add(colorBehaviorParameterViewModel);

        parameters.Add(new SeparationParameterViewModel());
        var colorAppearanceParameter = new ColorAppearanceParameterViewModel(configuration);
        parameters.Add(colorAppearanceParameter);
        parameters.Add(new SeparationParameterViewModel());
        var motionConfigParameter = new MotionConfigParameterViewModel(configuration, audioCapturingService,
            rightPanelViewModel, brightnessProviderFactory);
        parameters.Add(motionConfigParameter);
        return parameters;
    }

    private List<ParameterViewModelBase>? GetScreenCaptureParameters(ILightingConfiguration config)
    {
        if (config is not ScreenCaptureConfiguration configuration)
            return null;
        var captureParameter = new ScreenRegionSelectionParameterViewModel(configuration,
            windowService, settingsManager, screenCapturingService,decoder);
        var blackBarDetectionParameter = new BlackBarDetectionParameterViewModel();
        return [captureParameter, new SeparationParameterViewModel(), blackBarDetectionParameter];
    }

    private List<ParameterViewModelBase>? GetAnimationParameters(ILightingConfiguration config)
    {
        if (config is not AnimationConfiguration configuration)
            return null;
        var animationSelectionParameter = new AnimationSelectionParameterViewModel(
            configuration, rightPanelViewModel, libraryViewModelFactory, windowService,
            animationsRepository);
        return [animationSelectionParameter];
    }
}