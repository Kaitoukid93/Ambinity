using System.Collections.Generic;
using AmbinityCore.Colors;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Repositories;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ambinity.Views.Screens.ProfileEditor.ZoneConfiguration;

public class ParameterHelper
{
    public static List<ParameterViewModelBase> GetParameters(ILightingConfiguration config)
    {
        switch (config.Type)
        {
            case ConfigurationType.ScreenCapture:
                return GetScreencaptureParameterViewModels(config);
                break;
        }

        return null;
    }

    private static List<ParameterViewModelBase> GetScreencaptureParameterViewModels(ILightingConfiguration config)
    {
         var parameterViewModels = new List<ParameterViewModelBase>();
         //brightness param
         var brightnessParam = new SliderParameterViewModel();
         brightnessParam.Header = "Brightness";
         brightnessParam.Description = "Adjust brightness of this zone";
         var brightnessCorrectionParam = new ToggleParameterViewModel();
         brightnessCorrectionParam.Header = "Brightness Correction";
         brightnessCorrectionParam.Description = "Turn on or off Brightness Correction";
        return parameterViewModels;

    }

    private static ParameterViewModelBase GetColorSelectionParameterViewModel()
    {
        var parameterViewModel = new DataSourceParameterViewModel();
        var solidColorRepository = Ioc.Default.GetRequiredService<SolidColorsRepository>();
        var colorPaletteRepository = Ioc.Default.GetRequiredService<ColorPaletteRepository>();
        //get color repositories
        parameterViewModel.Datarepositories.Add(solidColorRepository);
        parameterViewModel.Datarepositories.Add(colorPaletteRepository);
        return parameterViewModel;
    }
}