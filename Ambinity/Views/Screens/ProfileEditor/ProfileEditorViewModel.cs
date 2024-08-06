using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Geography;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Profile;
using AmbinityServer.OnlineItem;
using CommunityToolkit.Mvvm.DependencyInjection;
using Draw2D.Core;

namespace Ambinity.Views.Screens.ProfileEditor;

public class ProfileEditorViewModel : ViewModelBase
{
    public ProfileEditorViewModel(LayoutCanvasViewModel layoutViewModel, RightPanelViewModel rightPanelViewModel,
        IMainWindowService mainWindowService, ZonePropertiesViewModel propertiesViewModel,
        SerialControllerRepository serialControllerRepository)
    {
        LayoutViewModel = layoutViewModel;
        LayoutViewModel.ItemAdded += OnItemAdded;
        LayoutViewModel.ItemRemoved += OnItemRemoved;
        _serialControllerRepository = serialControllerRepository;
        RightPanelViewModel = rightPanelViewModel;
        _propertiesViewModel = propertiesViewModel;
        mainWindowService.MainWindowClosed += OnMainWindowClosed;
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
        // throw new NotImplementedException();
    }

    public LayoutCanvasViewModel LayoutViewModel { get; set; }
    public RightPanelViewModel RightPanelViewModel { get; set; }
    private SerialControllerRepository _serialControllerRepository;
    private LightingProfile _currentProfile;
    private ZonePropertiesViewModel _propertiesViewModel;

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

        //add devices
        var devices = new List<AmbinityDevice>();
        foreach (SerialController controller in _serialControllerRepository.Items)
        {
            foreach (var output in controller.LedController.Outputs)
            {
                //simply lock the device in profile editor canvas, todo implement lock method
                var device = output.Device;
                device.IsSelectable = false;
                device.IsDraggable = false;
                device.IsResizeable = false;
                device.IsRotatable = false;
                device.IsScalable = false;
                device.IsDeleteable = false;
                devices.Add(device);
            }
        }

        items.AddRange(devices);
        items.AddRange(zones);
        var localRepository = Ioc.Default.GetRequiredService<LightingZoneRepository>();
        //init layout canvas
        LayoutViewModel.ShoudDrawBackground = true;
        LayoutViewModel.Init(items);
        RightPanelViewModel.PropertiesViewModel = _propertiesViewModel;
        //init assets
        var onlineRepository = Ioc.Default.GetRequiredService<LightingZoneOnlineRepository>();
        RightPanelViewModel.Init(localRepository, onlineRepository);
    }
}