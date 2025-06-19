using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.ViewModels;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.LED;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32.TaskScheduler;
using Task = System.Threading.Tasks.Task;

namespace Ambinity.Views.LayoutEditor.LEDLayoutEditor;

public class LEDLayoutEditorViewModel : ViewModelBase
{
    private readonly AmbinityDevice _device;
    public AmbinityDevice Device => _device;
    private AmbinityLEDViewModel _currentSelectedLED;
    private bool isInIndexSetupMode;

    public bool IsInIndexSetupMode
    {
        get => isInIndexSetupMode;
        set
        {
            isInIndexSetupMode = value;
            if (value)
                WindowState = WindowState.Maximized;
            else
            {
                WindowState = WindowState.Normal;
            }
            OnPropertyChanged();
        }
    }
    private WindowState _windowState;
    public WindowState WindowState
    {
        get => _windowState;
        set
        {
            _windowState = value;
            OnPropertyChanged();
        }
    }

    public AmbinityLEDViewModel CurrentSelectedLED
    {
        get => _currentSelectedLED;
        set
        {
            if (_currentSelectedLED != null)
                _currentSelectedLED.IsSelected = false;
            _currentSelectedLED = value;
            if (_currentSelectedLED != null)
            {
                _currentSelectedLED.IsSelected = true;
                if (CurrentSelectedButton != null)
                {
                    _currentSelectedLED.SetColor(CurrentSelectedButton.Color.Color);
                }
            }

            OnPropertyChanged();
        }
    }

    private ColorButtonViewModel _currentSelectedButton;

    public ColorButtonViewModel CurrentSelectedButton
    {
        get => _currentSelectedButton;
        set
        {
            _currentSelectedButton = value;
            if (CurrentSelectedLED != null)
            {
                CurrentSelectedLED.SetColor(_currentSelectedButton.Color.Color);
            }

            OnPropertyChanged();
        }
    }

    public LEDLayoutEditorViewModel(AmbinityDevice device, IWindowService windowService)
    {
        _windowService = windowService;
        _device = device;
        _device.IsIdentifying = true;
        LEDs = [];
        foreach (var led in _device.Leds)
        {
            LEDs.Add(new AmbinityLEDViewModel(led));
        }

        AvailableColors =
        [
            new ColorButtonViewModel(new SolidColorBrush(Colors.Red)),
            new ColorButtonViewModel(new SolidColorBrush(Colors.Green)),
            new ColorButtonViewModel(new SolidColorBrush(Colors.Blue)),
            new ColorButtonViewModel(new SolidColorBrush(Colors.Cyan)),
            new ColorButtonViewModel(new SolidColorBrush(Colors.Magenta)),
            new ColorButtonViewModel(new SolidColorBrush(Colors.Yellow)),
            new ColorButtonViewModel(new SolidColorBrush(Colors.Black))
        ];
        CurrentSelectedButton = AvailableColors.First();
        EnterIndexSetupCommand = new RelayCommand(EnterIndexSetup);
        ExitIndexSetupCommand = new RelayCommand(CancelIndexSetup);
        ResetIndexCommand = new RelayCommand(ResetIndex);
        SaveCurrentIndexSetupCommand = new RelayCommand(SaveCurrentIndexSetup);
        RunTestCommand = new AsyncRelayCommand<string>(RunTest);
    }

    private async Task RunTest(string mode)
    {
        var vm = new ColorTestingViewModel(_device, mode);

        var window = await _windowService.ShowDialogWindow(vm, _currentWindow);
        vm.WindowCloseRequest += () => window.Close();

    }

    private void SaveCurrentIndexSetup()
    {
        if(LEDs.Any(led=>!led.IsSelected))
        return;
        foreach (var led in LEDs)
        {
            led.SaveIndex();
            led.IsSelected = false;
            led.IsIndexVisible = true;
        }
        IsInIndexSetupMode = false;
        //add custom layout to load at startup
        _device.Layout.CustomIndex = new int?[LEDs.Count];
        for (int i = 0; i < LEDs.Count; i++)
        {
            _device.Layout.CustomIndex[i] = LEDs[i].Index;
        }
        Device.IsIdentifying =false;

        OnPropertyChanged(nameof(UseCustomIndex));
    }
    private Window _currentWindow;
    //pass window instance for dialog show
    //todo make dialog window service
    public void SetWindow(Window window)
    {
        _currentWindow = window;
    }
    private void ResetIndex()
    {
        _currentLEDIndex = 0;
        foreach (var led in LEDs)
        {
            led.ResetIndex();
        }
    }

    public void CancelIndexSetup()
    {
        IsInIndexSetupMode = false;
        foreach (var led in LEDs)
        {
            led.IsSelected = false;
            led.IsIndexVisible = true;
            led.RevertIndex();
        }
        Device.IsIdentifying = false;
    }

    private void EnterIndexSetup()
    {
        IsInIndexSetupMode = true;
        ResetIndex();
    }

    public List<AmbinityLEDViewModel> LEDs { get; set; }
    public List<ColorButtonViewModel> AvailableColors { get; set; }
    public ICommand EnterIndexSetupCommand { get; }


    public ICommand ExitIndexSetupCommand { get; }
    public ICommand SaveCurrentIndexSetupCommand { get; }
    public ICommand ResetIndexCommand { get; }

    private int _currentLEDIndex;
    private readonly IWindowService _windowService;

    public void SetIndex(AmbinityLEDViewModel led)
    {
        lock (_device.Lock)
        {
            if (!led.IsSelected)
            {
                led.IsSelected = true;
                if (CurrentSelectedButton != null)
                {
                    led.SetColor(CurrentSelectedButton.Color.Color);
                }
                led.SetIndex(_currentLEDIndex);
                _currentLEDIndex++;
            }
            else
            {
                _currentLEDIndex--;
                led.IsSelected = false;
                led.SetIndex(null);
            }
        }

    }

    public void ToggleLED(AmbinityLEDViewModel led)
    {
        if (CurrentSelectedLED == null || !led.IsSelected)
        {
            CurrentSelectedLED = led;
        }

        else
        {
            CurrentSelectedLED.IsSelected = false;
        }
    }

    public class ColorButtonViewModel
    {
        public ColorButtonViewModel(SolidColorBrush color)
        {
            Color = color;
        }

        public SolidColorBrush Color { get; set; }
    }


    public bool UseCustomIndex => _device.Layout.CustomIndex != null;
    public ICommand RunTestCommand { get; }

    public override void Dispose()
    {
        _device.IsIdentifying = false;
    }
}
