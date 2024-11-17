using System.Collections.Generic;
using Ambinity.Services;
using Ambinity.Views.AmbinityStore;
using Ambinity.Views.LayoutEditor;
using Ambinity.Windows;
using AmbinityCore.CapturingService;
using AmbinityCore.Colors;
using AmbinityCore.DataBase;
using AmbinityCore.LightingEngines;
using AmbinityCore.Models.Lighting.Zone;
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
    CapturingServiceProvider capturingServiceProvider,
    IWindowService windowService,
    IDialogService dialogService,
    GeneralSettingsManager settingsManager,
    StaticColorsRepository colorsRepository,
    ColorPaletteRepository colorPaletteRepository,
    AnimationsRepository animationsRepository,
    LibraryViewModelFactory libraryViewModelFactory,
    BrightnessProviderFactory brightnessProviderFactory,
    ScreenCapturingService screenCapturingService,
    LightingProfileDecoder decoder,
    AmbinityStoreItemExportViewModel exportViewModel)
{
    public List<ParameterViewModelBase> CreateParameterViewModels(LightingZone zone)
    {
        return zone.LightingConfiguration.Type switch
        {
            ConfigurationType.Animation => GetAnimationParameters(zone),
            ConfigurationType.ScreenCapture => GetScreenCaptureParameters(zone),
            ConfigurationType.SelfGeneratedColor => GetSelfGeneratedColorParameters(zone)
        };
    }

    private List<ParameterViewModelBase>? GetSelfGeneratedColorParameters(LightingZone zone)
    {
        var parameters = new List<ParameterViewModelBase>();
        if (zone.LightingConfiguration is not SelfGeneratedColorConfiguration configuration)
            return null;
        var colorSelectorParameter = new FillColorSelectionViewModel(configuration, rightPanelViewModel,
            colorsRepository, colorPaletteRepository, libraryViewModelFactory, windowService, dialogService,exportViewModel);
        parameters.Add(colorSelectorParameter);

        parameters.Add(new SeparationParameterViewModel());
        var colorBehaviorParameterViewModel = new ColorBehaviorParameterViewModel(configuration);
        parameters.Add(colorBehaviorParameterViewModel);

        parameters.Add(new SeparationParameterViewModel());
        var colorAppearanceParameter = new ColorAppearanceParameterViewModel(configuration);
        parameters.Add(colorAppearanceParameter);
        parameters.Add(new SeparationParameterViewModel());
        var motionConfigParameter = new MotionConfigParameterViewModel(configuration, capturingServiceProvider,
            rightPanelViewModel, brightnessProviderFactory);
        parameters.Add(motionConfigParameter);
        return parameters;
    }

    private List<ParameterViewModelBase>? GetScreenCaptureParameters(LightingZone zone)
    {
        if (zone.LightingConfiguration is not ScreenCaptureConfiguration configuration)
            return null;
        var captureParameter = new ScreenRegionSelectionParameterViewModel(configuration,
            windowService, settingsManager, screenCapturingService,decoder);
        var blackBarDetectionParameter = new BlackBarDetectionParameterViewModel();
        return [captureParameter, new SeparationParameterViewModel(), blackBarDetectionParameter];
    }

    private List<ParameterViewModelBase>? GetAnimationParameters(LightingZone zone)
    {
        if (zone.LightingConfiguration is not AnimationConfiguration configuration)
            return null;
        var animationSelectionParameter = new AnimationSelectionParameterViewModel(
            zone, rightPanelViewModel, libraryViewModelFactory, windowService,
            animationsRepository,exportViewModel,dialogService);
        return [animationSelectionParameter];
    }
}