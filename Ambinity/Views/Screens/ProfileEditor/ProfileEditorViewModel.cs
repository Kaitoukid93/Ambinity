using System;
using System.Collections.Generic;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Geography;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Profile;
using AmbinityCore.Repositories;
using Draw2D.Core;

namespace Ambinity.Views.Screens.ProfileEditor;

public class ProfileEditorViewModel : ViewModelBase
{
    public ProfileEditorViewModel(LayoutCanvasViewModel layoutViewModel, ProfileEditorRightPanelViewModel rightPanelViewModel,
        IMainWindowService mainWindowService, ZonePropertiesViewModel propertiesViewModel,
        ToolsViewModel toolsViewModel,
        LightingZoneRepository lightingZoneRepository,
        LightingZoneOnlineRepository lightingZoneOnlineRepository,
        AmbinityDeviceRepository deviceRepository,LightingProfileDecoder decoder)
    {
        LayoutViewModel = layoutViewModel;
        LayoutViewModel.ItemAdded += OnItemAdded;
        LayoutViewModel.ItemRemoved += OnItemRemoved;
        _deviceRepository = deviceRepository;
        RightPanelViewModel = rightPanelViewModel;
        _propertiesViewModel = propertiesViewModel;
        _lightingZoneRepository = lightingZoneRepository;
        _lightingZoneOnlineRepository = lightingZoneOnlineRepository;
        mainWindowService.MainWindowClosed += OnMainWindowClosed;
        _toolsViewModel = toolsViewModel;
        _decoder = decoder;
        
    }

    private void OnItemRemoved(Figure item)
    {
        var zoneFigure = item as LightingZoneFigure;
        _currentProfile.RemoveLightingZone(zoneFigure.ChildItem as LightingZone);
    }

//todo how to properly notify profile to update zone
    private void OnItemAdded(Figure item)
    {
        var zoneFigure = item as LightingZoneFigure;
        _currentProfile.AddLightingZone(zoneFigure.ChildItem as LightingZone);
    }

    private void OnMainWindowClosed(object? sender, EventArgs e)
    {
        Dispose();
    }

    public LayoutCanvasViewModel LayoutViewModel { get; set; }
    public ProfileEditorRightPanelViewModel RightPanelViewModel { get; set; }
    private LightingProfile _currentProfile;
    private ZonePropertiesViewModel _propertiesViewModel;
    private readonly LightingZoneRepository _lightingZoneRepository;
    private readonly LightingZoneOnlineRepository _lightingZoneOnlineRepository;
    private readonly ToolsViewModel _toolsViewModel;
    private readonly AmbinityDeviceRepository _deviceRepository;
    private readonly LightingProfileDecoder _decoder;
    public void Init(LightingProfile profile)
    {
        _currentProfile = profile;
        var items = new List<IPositionAware>();
        //add zones
        var zones = new List<LightingZone>();
        foreach (var zone in profile.Zones)
        {
            zone.IsSelectable = true;
            zone.IsDraggable = true;
            zone.IsResizeable = true;
            zone.IsRotatable = false; // zone cant be rotate
            zone.IsScalable = false; // zone cant be scale, use resize instead
            zone.IsDeleteable = true;
            zones.Add(zone);
        }
        //add zone groups

        //todo
        //add devices
        var devices = new List<AmbinityDevice>();
        foreach (var device in _deviceRepository.Devices)
        {
            //simply lock the device in profile editor canvas, todo implement lock method
            device.IsSelectable = false;
            device.IsDraggable = false;
            device.IsResizeable = false;
            device.IsRotatable = false;
            device.IsScalable = false;
            device.IsDeleteable = false;
            devices.Add(device);
        }
        items.AddRange(devices);
        items.AddRange(zones);
        //init layout canvas
        LayoutViewModel.ShoudDrawBackground = true;
        LayoutViewModel.Init(items);
        _toolsViewModel.InitForProfileEditor(profile);
        RightPanelViewModel.PropertiesViewModel = _propertiesViewModel;
        //init assets
        RightPanelViewModel.Init();
        //tell decoder to update frame until this is disposed
        _decoder.ShouldUpdateFrame =true;
    }

    public override void Dispose()
    {
        LayoutViewModel?.Dispose();
        RightPanelViewModel?.Dispose();
        _decoder.ShouldUpdateFrame = false;
    }
}