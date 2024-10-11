using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AmbinityCore.CapturingService;
using AmbinityCore.CapturingService.HWMonitorCapturing;
using Avalonia.Media;
using LibreHardwareMonitor.Hardware;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Ambinity.Views.Screens.DeviceSettings;

public class HWMonitorChartViewModel : FanControlItemViewModelBase
{
    private HWMonitorCapturingService _capturingService;
    private readonly DateTimeAxis _customAxis;
    public object Sync { get; } = new object();
    public ISeries[] Series { get; set; }
    private readonly ObservableCollection<double> _values;
    private ISensor _sensor;
    public ISensor Sensor => _sensor;
    public List<Axis> XAxis { get; set; } = new List<Axis>
    {
        new Axis
        {
            IsVisible = false
        }
    };

    public List<Axis> YAxis { get; set; } = new List<Axis>
    {
        new Axis
        {
            IsVisible = false
        }
    };

    public SolidColorPaint ToolTipPaint { get; } = new SolidColorPaint(SKColors.Black);
    public SolidColorPaint ToolTipTextPaint { get; } = new SolidColorPaint(SKColors.Silver);

    public HWMonitorChartViewModel(CapturingServiceProvider capturingServiceProvider, ISensor sensor, Color color)
    {
        _sensor = sensor;
        _capturingService =
            (HWMonitorCapturingService)capturingServiceProvider.GetCapturingService(CapturingType.HWCapture);
        _capturingService.RegisterUse();
        _capturingService.DataUpdate += OnDataUpdated;
        var items = new List<double>();
        for (var i = 0; i < 50; i++)
        {
            items.Add(80);
        }

        _values = new ObservableCollection<double>(items);
        Series =  new ISeries[]
        {
            new LineSeries<double>
            {
                Values = _values,
                GeometryFill = null,
                GeometryStroke = null,
                Fill = new SolidColorPaint(new SKColor(color.R,color.G,color.B,50)),
                LineSmoothness = 0,
                Stroke = new SolidColorPaint(new SKColor(color.R,color.G,color.B),2)
            }
        };
       // XAxes = new Axis[] { new Axis(){ show } };
        OnPropertyChanged(nameof(Series));
    }

    private void OnDataUpdated()
    {
        //get data at sensor index
        var value = _capturingService.Buffer.GetValue(_sensor.Index, 0); //value at index 1
        lock (Sync)
        {
            _values.Add(Math.Ceiling(value));
            if (_values.Count > 50) _values.RemoveAt(0);
        }
    }

    public override void Dispose()
    {
        _capturingService.DataUpdate -= OnDataUpdated;
        _capturingService.UnregisterUse();
    }
}