using System;
using System.Collections.Generic;
using Ambinity.ViewModels;
using Ambinity.Views.SideMenu;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Profile;
using DynamicData;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Windows;

public class NewProfileDialogContentViewModel : ViewModelBase
{
    private ContentDialog _dialog;
    public EventHandler? DialogClosed;

    public NewProfileDialogContentViewModel()
    {
    }

    public void Init(ContentDialog dialog)
    {
        if (dialog is null)
        {
            throw new ArgumentNullException(nameof(dialog));
        }

        _dialog = dialog;
        dialog.Closed += DialogOnClosed;
        var colorPaletteProfile =
            ResourceLoader.LoadJsonResource<LightingProfile>(
                "avares://Ambinity/Assets/Templates/ColorPaletteTemplate.json");
        var staticColorProfile =
            ResourceLoader.LoadJsonResource<LightingProfile>(
                "avares://Ambinity/Assets/Templates/StaticColorTemplate.json");
        var musicReactiveProfile =
            ResourceLoader.LoadJsonResource<LightingProfile>(
                "avares://Ambinity/Assets/Templates/MusicReactiveTemplate.json");
        //load profile template
        var colorPaletteTemplate = new ProfileTemplateViewModel(colorPaletteProfile, "Color palette",
            "Create new profile based on color palette fx");
        var staticColorTemplate = new ProfileTemplateViewModel(staticColorProfile, "Static Color",
            "Create new profile based on a solid color");
         var musicReactiveTemplate = new ProfileTemplateViewModel(musicReactiveProfile, "Music Reactive",
            "Create new profile with color react to music");
        AvailableTemplates.Add(colorPaletteTemplate);
        AvailableTemplates.Add(staticColorTemplate);
        AvailableTemplates.Add(musicReactiveTemplate);
    }

    private void DialogOnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _dialog.Closed -= DialogOnClosed;

        DialogClosed?.Invoke(this, args);
    }

    private string _UserInput;

    /// <summary>
    /// Gets or sets the user input to check 
    /// </summary>
    public string UserInput
    {
        get => _UserInput;
        set { SetProperty(ref _UserInput, value); }
    }

    public List<ProfileTemplateViewModel> AvailableTemplates { get; set; } = [];

    private static readonly string[] _AvailableKeyWords = new[]
    {
        "Static",
        "Rainbow",
        "Party",
        "Music",
        "Favorite",
    };

    public string[] AvailableKeyWords => _AvailableKeyWords;
}