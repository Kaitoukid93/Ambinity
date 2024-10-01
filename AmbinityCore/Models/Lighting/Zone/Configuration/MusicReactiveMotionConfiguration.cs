namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public class MusicReactiveMotionConfiguration : IMotionConfiguration
{
    public event Action Update;
    public event Action AudioDeviceChanged;
    public event Action FrequencyRangeUpdate;
    public event Action VisualizerStyleUpdate;
    public event Action NoSoundBehaviorUpdate;
    public MusicReactiveMotionConfiguration()
    {
    }

    public MotionTypeEnum Type => MotionTypeEnum.MusicReactive;
    public string Icon => "LightingConfiguration_MusicReactive";

    /// <summary>
    /// get or set current style 
    /// </summary>
    /// <returns></returns>
    public IVisualizerStyle Style { get; set; } = new BrightnessVisualizerStyle();

    public int[] FrequencyRange { get; set; } = GetDefaultFrequencyRange();

    public AudioDevice AudioDevice { get; set; } = new AudioDevice() { ID = -3 };
    public bool UseDefaultDevice { get; set; } = true;
    public NoSoundBehaviorEnum NoSoundBehavior { get; set; } = NoSoundBehaviorEnum.TurnOff;

    private static int[] GetDefaultFrequencyRange()
    {
        var range = new int[2];
        range[0] = 0;
        range[1] = 31;
        return range;
    }

    public void UpdateConfig()
    {
        Update?.Invoke();
    }

    public void SetAudioDevice(AudioDevice device)
    {
        AudioDevice = device;
        AudioDeviceChanged?.Invoke();
    }

    public void UpdateVisualizerStyle()
    {
        VisualizerStyleUpdate?.Invoke();
    }

    public void UpdateFrequencyRange()
    {
        FrequencyRangeUpdate?.Invoke();
    }

    public void AudioDeviceUseDefaultUpdate()
    {
        if (UseDefaultDevice)
            AudioDeviceChanged?.Invoke();
    }

    public void UpdateSoundBehavior()
    {
        NoSoundBehaviorUpdate?.Invoke();
    }
}