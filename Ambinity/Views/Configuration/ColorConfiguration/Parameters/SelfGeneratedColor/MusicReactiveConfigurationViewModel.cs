using System.Collections.Generic;
using System.Linq;
using Ambinity.ViewModels;
using AmbinityCore.CapturingService;
using AmbinityCore.CapturingService.AudioCapturing;
using AmbinityCore.LightingEngines;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using Avalonia.Media;
using Draw2D.Core.Geo;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class MusicReactiveConfigurationViewModel : MotionConfigurationViewModelBase
{
    private AudioBuffer _buffer;

    public MusicReactiveConfigurationViewModel(IMotionConfiguration config,
        AudioCapturingService audioCapturingService)
    {
        _audioCapturingService = audioCapturingService;
        _buffer = _audioCapturingService.Buffer;
        _audioCapturingService.VisualizerUpdate += OnDataUpdate;
        _audioCapturingService.DefaultDeviceChanged += OnDefaultDeviceChanged;


        Configuration = config;
        Type = config.Type;
        Icon = Configuration.Icon;
        Name = "Music Reactive";
        ShowSettingsButton = true;
        Description = "Brightness change based on audio level";
        VUModes = new List<string>()
        {
            "Normal",
            "Floating",
            "Reverse"
        };
        var data = new VisualizerBarViewModel[32];
        for (int i = 0; i < 32; i++)
        {
            data[i] = new VisualizerBarViewModel();
        }

        Frequencies = data.ToList();
        _startFrequency = (config as MusicReactiveMotionConfiguration).FrequencyRange[0];
        _stopFrequency = (config as MusicReactiveMotionConfiguration).FrequencyRange[1];

        UpdateDeviceList();
        RegisterVisualizer();
        UpdateFrequencyRange();
        SetupVisualStyle();
        _visualizerStyle = AvailableVisualStyles.Where(v => v.Type == _configuration.Style.Type).FirstOrDefault();
        if (_visualizerStyle is VUMetterVisualizerStyle)
            _vuMode = (int)(_configuration.Style as VUMetterVisualizerStyle).VisualizerMode;
        _noSoundBehavior = (int)(_configuration.NoSoundBehavior);
    }

    private void SetupVisualStyle()
    {
        AvailableVisualStyles = new List<IVisualizerStyle>()
        {
            new BrightnessVisualizerStyle(),
            new VUMetterVisualizerStyle()
        };
    }

    private void OnDefaultDeviceChanged()
    {
        UpdateDeviceList();
        UnregisterVisualizer();
        RegisterVisualizer();
    }

    public List<IVisualizerStyle> AvailableVisualStyles { get; set; }
    private IVisualizerStyle _visualizerStyle;

    public IVisualizerStyle VisualizerStyle
    {
        get => _visualizerStyle;
        set
        {
            _visualizerStyle = value;
            _configuration.Style = value;
            _configuration.UpdateVisualizerStyle();
            ;
            OnPropertyChanged();
        }
    }

    private int _vuMode;

    public int VuMode
    {
        get => _vuMode;
        set
        {
            _vuMode = value;
            if (_configuration.Style is not VUMetterVisualizerStyle)
                return;
            (_configuration.Style as VUMetterVisualizerStyle).VisualizerMode = (VUVisualizerMode)value;
            _configuration.UpdateVisualizerStyle();
            OnPropertyChanged();
        }
    }

    private void UpdateDeviceList()
    {
        AvailableAudioDevices = new List<AudioDevice>();
        foreach (var device in _audioCapturingService.AvalableAudioCaptures)
        {
            AvailableAudioDevices.Add(device.Device);
            // todo make capture viewmodel equalizer AvailableAudioDevices.Add(device);
        }

        _isDefault = _configuration.UseDefaultDevice;
        if (_configuration.UseDefaultDevice)
            _selectedAudioDevice = AvailableAudioDevices.Where(d => d.Name.Contains("[default]")).FirstOrDefault();
        else
        {
            _selectedAudioDevice =
                AvailableAudioDevices.Where(d => d.ID == _configuration.AudioDevice.ID).FirstOrDefault();
        }
    }

    private void RegisterVisualizer()
    {
        _audioCapturingService.RegisterVisualizer();
    }

    private void UnregisterVisualizer()
    {
        _audioCapturingService.UnregisterVisualizer();
    }

    private MusicReactiveMotionConfiguration _configuration => Configuration as MusicReactiveMotionConfiguration;

    private void OnDataUpdate()
    {
        int count = 0;
        if (Frequencies == null)
            return;
        lock (Frequencies)
        {
            foreach (var freq in Frequencies)
            {
                freq.Value = _buffer.GetByte(SelectedAudioDevice.ID, count++);
            }
        }
    }

    private int _noSoundBehavior;

    public int NoSoundBehavior
    {
        get => _noSoundBehavior;
        set
        {
            _noSoundBehavior = value;
            _configuration.NoSoundBehavior = value == 0 ? NoSoundBehaviorEnum.TurnOff : NoSoundBehaviorEnum.StayOn;
            _configuration.UpdateSoundBehavior();
            OnPropertyChanged();
        }
    }

    private AudioDevice _selectedAudioDevice;

    public AudioDevice SelectedAudioDevice
    {
        get => _selectedAudioDevice;
        set
        {
            _selectedAudioDevice = value;
            //update the preview too
            _configuration.SetAudioDevice(value);
            UnregisterVisualizer();
            RegisterVisualizer();
            OnPropertyChanged();
        }
    }

    public List<AudioDevice> AvailableAudioDevices { get; set; }
    public List<string> VUModes { get; set; }
    public string Icon { get; set; }
    private int[] _selectedFrequencies;

    public int[] SelectedFrequencies
    {
        get => _selectedFrequencies;
        set
        {
            _selectedFrequencies = value;
            UpdateFrequencyRange();
            OnPropertyChanged();
        }
    }

    private void UpdateFrequencyRange()
    {
        ClearSelection();
        for (int i = _startFrequency; i <= StopFrequency; i++)
        {
            Frequencies[i].ForeGround = Colors.Green;
        }
    }

    private int _startFrequency;

    public int StartFrequency
    {
        get => _startFrequency;
        set
        {
            if (value < 0 || value >= 32)
            {
                return;
            }

            _startFrequency = value;
            _configuration.FrequencyRange[0] = value;
            SetFrequency();
            OnPropertyChanged();
        }
    }

    private int _stopFrequency;
    private readonly AudioCapturingService _audioCapturingService;

    public int StopFrequency
    {
        get => _stopFrequency;
        set
        {
            _stopFrequency = value;
            _configuration.FrequencyRange[1] = value;
            SetFrequency();
            OnPropertyChanged();
        }
    }

    private void ClearSelection()
    {
        foreach (var freq in Frequencies)
        {
            freq.ForeGround = Colors.Gray;
        }
    }

    private void SetFrequency()
    {
        var freqs = new int[StopFrequency - StartFrequency];
        int count = 0;
        for (int i = _startFrequency; i < StopFrequency; i++)
        {
            freqs[count++] = i;
        }

        _configuration.UpdateFrequencyRange();
        SelectedFrequencies = freqs;
    }

    public List<VisualizerBarViewModel> Frequencies { get; set; }
    private bool _isDefault;

    public bool IsDefault
    {
        get => _isDefault;
        set
        {
            _isDefault = value;
            _configuration.UseDefaultDevice = value;
            _configuration.AudioDeviceUseDefaultUpdate();
            if (value)
            {
                UpdateDeviceList();
                UnregisterVisualizer();
                RegisterVisualizer();
                OnPropertyChanged(nameof(SelectedAudioDevice));
            }

            OnPropertyChanged();
        }
    }

    public override void Dispose()
    {
        UnregisterVisualizer();
        Frequencies = null;
    }

    public class VisualizerBarViewModel : ViewModelBase
    {
        public VisualizerBarViewModel()
        {
        }

        private int _value;

        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        private Color _foreGround = Colors.Gray;

        public Color ForeGround
        {
            get => _foreGround;
            set
            {
                _foreGround = value;
                OnPropertyChanged();
            }
        }
    }

    public string NoSoundTeachingTipSubTitle =>"##### How the LEDs behavior when there is no sound captured for the period of time\n * `Keep LEDs off` : The LEDs will stay off as there is no sound present\n * `Return to normal lighting` : The LEDs will show full brightness and will react to music when the sound data is available in the buffer";
}