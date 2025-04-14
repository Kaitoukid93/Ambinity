using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Ambinity.ViewModels;
using AmbinityCore.Models.Device;
using Avalonia.Controls.Documents;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32.TaskScheduler;
using Vortice.Mathematics;
using Action = System.Action;
using Color = Avalonia.Media.Color;
using Task = System.Threading.Tasks.Task;

namespace Ambinity.Views.LayoutEditor.LEDLayoutEditor;

public class ColorTestingViewModel : ViewModelBase
{
    private string _currentTestMode = "basic";
    public event Action WindowCloseRequest;

    public ColorTestingViewModel(AmbinityDevice device, string testMode)
    {
        device.IsIdentifying =true;
        _currentTestMode = testMode;
        Device = device;
        LEDs = new List<AmbinityLEDViewModel>();
        foreach (var led in Device.Leds)
        {
            LEDs.Add(new AmbinityLEDViewModel(led));
        }

        RunTestSequence();
    }

    private void Close()
    {
        //turn off all leds
        foreach (var led in LEDs)
        {
            led.SetColor(new Color(255, 0, 0, 0));
        }
        Device.IsIdentifying =false;

    }

    public ICommand CloseCommand { get; set; }
    public AmbinityDevice Device { get; set; }
    public List<AmbinityLEDViewModel> LEDs { get; set; }
    private bool _isTesting;

    public bool IsTesting
    {
        get => _isTesting;
        set
        {
            _isTesting = value;
            if (!value)
            {
                 Close();
                  WindowCloseRequest?.Invoke();
            }

            OnPropertyChanged();
        }
    }

    private void RunTestSequence()
    {
        switch (_currentTestMode)
        {
            case "basic":
                RunBasicTest();
                break;
            case "order":
                RunOrderTest();
                break;
            case "white":
                RunWhiteTest();
                break;
        }
    }

    private async Task RunWhiteTest()
    {
        IsTesting = true;
        foreach (var led in LEDs)
        {
            led.SetColor(new Color(255, 255, 255, 255));
        }

        await Task.Delay(2000);
        IsTesting = false;
    }

    private async Task RunOrderTest()
    {
        IsTesting = true;
        foreach (var led in LEDs.OrderBy(i => i.Index).ToList())
        {
            led.SetColor(new Color(255, 255, 0, 0));
            await Task.Delay(100);
        }

        foreach (var led in LEDs)
        {
            led.SetColor(new Color(255, 0, 0, 0));
        }

        IsTesting = false;
    }

    private async Task RunBasicTest()
    {
        IsTesting = true;
        foreach (var led in LEDs)
        {
            led.SetColor(new Color(255, 255, 0, 0));
        }

        await Task.Delay(1000);
        foreach (var led in LEDs)
        {
            led.SetColor(new Color(255, 0, 255, 0));
        }

        await Task.Delay(1000);
        foreach (var led in LEDs)
        {
            led.SetColor(new Color(255, 0, 0, 255));
        }

        await Task.Delay(1000);
        foreach (var led in LEDs)
        {
            led.SetColor(new Color(255, 0, 0, 0));
        }

        IsTesting = false;
    }
}
