using System;
using System.Collections.Generic;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using Ambinity.Views.LayoutEditor.Canvas;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Geography;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Profile;
using AmbinityCore.Repositories;
using Draw2D.Core;

namespace Ambinity.Views.Screens.ProfileEditor;

public class ProfileEditorViewModel : ViewModelBase
{
    public ProfileEditorViewModel(CanvasViewModelFactory canvasViewModelFactory, ProfileEditorRightPanelViewModel rightPanelViewModel,
        IMainWindowService mainWindowService, ZonePropertiesViewModel propertiesViewModel, LightingProfileDecoder decoder)
    {
        _canvasViewModelFactory = canvasViewModelFactory;
        RightPanelViewModel = rightPanelViewModel;
        _propertiesViewModel = propertiesViewModel;
        mainWindowService.MainWindowClosed += OnMainWindowClosed;
        _decoder = decoder;

    }


    private void OnMainWindowClosed(object? sender, EventArgs e)
    {
        Dispose();
    }
    private ProfileEditorCanvasViewModel _canvasViewModel;
    public ProfileEditorCanvasViewModel CanvasViewModel
    {
        get => _canvasViewModel;
        set
        {
            _canvasViewModel = value;
            OnPropertyChanged(nameof(CanvasViewModel));

        }
    }
    public ProfileEditorRightPanelViewModel RightPanelViewModel { get; set; }

    private ZonePropertiesViewModel _propertiesViewModel;
    private readonly ToolsViewModel _toolsViewModel;
    private readonly CanvasViewModelFactory _canvasViewModelFactory;
    private readonly LightingProfileDecoder _decoder;
    public void Init(LightingProfile profile)
    {
        CanvasViewModel = _canvasViewModelFactory.Get<ProfileEditorCanvasViewModel>();
        _canvasViewModelFactory.SetCurrent(CanvasViewModel);
        CanvasViewModel.Init(profile);
        RightPanelViewModel.PropertiesViewModel = _propertiesViewModel;
        //init assets
        RightPanelViewModel.Init();
        //tell decoder to update frame until this is disposed
        _decoder.ShouldUpdateFrame = true;
    }

    public override void Dispose()
    {
        CanvasViewModel?.Dispose();
        RightPanelViewModel?.Dispose();
        _decoder.ShouldUpdateFrame = false;
    }
}
