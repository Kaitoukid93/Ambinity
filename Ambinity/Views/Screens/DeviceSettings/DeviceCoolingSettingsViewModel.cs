using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ambinity.ViewModels;
using AmbinityCore.CapturingService;
using AmbinityCore.CapturingService.HWMonitorCapturing;
using AmbinityCore.DataBase;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.GeneralSetting;
using LibreHardwareMonitor.Hardware;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Ambinity.Views.Screens.DeviceSettings;

public class DeviceCoolingSettingsViewModel : ViewModelBase
{
    private IController _fanController;

    private readonly HWMonitorCapturingService _capturingService;
    private readonly CapturingServiceProvider _capturingServiceProvider;
    public List<FanControlItemViewModelBase> Items { get; set; }
    private IGeneralSettings _generalSettings;
    public bool HWmonitorAvailable { get; set; }

    public DeviceCoolingSettingsViewModel(CapturingServiceProvider capturingServiceProvider,
        GeneralSettingsManager generalSettingsManager)
    {
        _generalSettings = generalSettingsManager.Settings;
        _capturingServiceProvider = capturingServiceProvider;
        _capturingService =
            (HWMonitorCapturingService)_capturingServiceProvider.GetCapturingService(CapturingType.HWCapture);
    }

    public void Init(IController controller)
    {
        _fanController = controller;
        Items = new List<FanControlItemViewModelBase>();
        if (_capturingService.IsAvailable)
        {
            HWmonitorAvailable = true;
            foreach (ISensor sensor in _capturingService.FanControlSensors)
            {
                Items.Add(new HWMonitorChartViewModel(_capturingServiceProvider, sensor, _generalSettings.PrimaryColor));
            }
        }
       
        foreach (var output in controller.FanController.Outputs)
        {
           
            var port = new FanPortViewModel(output,HWmonitorAvailable);
            Items.Add(port);
        }


        OnPropertyChanged(nameof(Items));
    }


    public override void Dispose()
    {
        //base.Dispose();
        if (Items != null)
        {
            foreach (var item in Items)
            {
                item?.Dispose();
            }
        }
        
    }
}