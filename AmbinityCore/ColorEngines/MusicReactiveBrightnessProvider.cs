using AmbinityCore.CapturingService;
using AmbinityCore.CapturingService.AudioCapturing;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using Draw2D.Core.Graphic;
using Serilog;


namespace AmbinityCore.LightingEngines;

/// <summary>
/// provie brightnessmap based on audio capture
/// </summary>
public class MusicReactiveBrightnessProvider : IBrightnessProvider
{
    private AudioCapturingService _capturingService;
    private bool _isActivated;
    private int _deviceID;
    private Thread? _workerThread;
    private bool _isActivating;
    private readonly AudioBuffer _buffer;
    private readonly byte[] _internalBuffer;
    private readonly MusicReactiveMotionConfiguration _configuration;
    private int[] _frequencyRange;
    private byte[] _soundData;
    private int _blackFrameCounter;
    private int _sumByte;
    private int _lastSumByte;
    private NoSoundBehaviorEnum _noSoundBehavior;
    private IVisualizerStyle _visualizerStyle;

    public MusicReactiveBrightnessProvider(IMotionConfiguration configuration, AudioCapturingService capturingService)
    {
        _configuration = configuration as MusicReactiveMotionConfiguration;
        _configuration.FrequencyRangeUpdate += OnFrequencyRangeUpdate;
        _configuration.VisualizerStyleUpdate += OnVisualizerStyleUpdate;
        _configuration.NoSoundBehaviorUpdate += OnNoSoundBehaviorUpdate;
        _capturingService = capturingService;
        _deviceID = _configuration.UseDefaultDevice ? _capturingService.DefaultDeviceID : _configuration.AudioDevice.ID;
        _buffer = _capturingService.Buffer;
        _internalBuffer = new byte[32];
        OnFrequencyRangeUpdate();
        OnVisualizerStyleUpdate();
        OnNoSoundBehaviorUpdate();
    }

    private void OnNoSoundBehaviorUpdate()
    {
        _noSoundBehavior = _configuration.NoSoundBehavior;
    }

    private void OnVisualizerStyleUpdate()
    {
        _visualizerStyle = _configuration.Style;
    }

    private void OnFrequencyRangeUpdate()
    {
        _frequencyRange = _configuration.FrequencyRange;
    }

    public void Activate()
    {
        _capturingService.RegisterBrightnessProvider();
    }


    public void Deactivate()
    {
        _capturingService.UnRegisterBrightnessProvider();
    }

    public void Init(IMotionConfiguration config)
    {
        throw new NotImplementedException();
    }

    private double GetAverage(byte[] data)
    {
        int sum = 0;
        for (int i = 0; i < data.Length; i++)
        {
            sum += data[i];
        }

        return sum / data.Length;
    }

    private void FillArray255(byte[] inputArray)
    {
        for (int i = 0; i < inputArray.Length; i++)
        {
            inputArray[i] = 255;
        }
    }

    private void BrightnessManipulation(float[] reusableArray, double brightnessLevel)
    {
        for (int i = 0; i < reusableArray.Length; i++)
        {
            reusableArray[i] = (byte)brightnessLevel;
        }
    }

    private void VUMeterManipulation(float[] reusableArray, double brightnessLevel)
    {
        var vu = _visualizerStyle as VUMetterVisualizerStyle;
        var height = brightnessLevel / 255f;
        var actualHeight = Math.Floor(height * reusableArray.Length);
        var maxHeight = reusableArray.Length;
        switch (vu.VisualizerMode)
        {
            case VUVisualizerMode.Normal:
                for (var i = 0; i < maxHeight; i++)
                {
                    if (i < maxHeight - actualHeight)
                        reusableArray[i] = 0;
                    else
                        reusableArray[i] = 255;
                }

                break;
            case VUVisualizerMode.Inverse:

                for (var i = 0; i < maxHeight; i++)
                {
                    if (i < actualHeight)
                        reusableArray[i] = 255;
                    else
                        reusableArray[i] = 0;
                }

                break;
            case VUVisualizerMode.Floating:
                //first half
                for (var i = 0; i < maxHeight / 2; i++)
                {
                    if (Math.Abs(0 - i) <= actualHeight)
                        reusableArray[i] = 0;
                    else
                        reusableArray[i] = 255;
                }

                //the other half
                for (var i = maxHeight / 2; i < maxHeight; i++)
                {
                    if (Math.Abs(maxHeight / 2 - i) <= actualHeight)
                        reusableArray[i] = 255;
                    else
                        reusableArray[i] = 0;
                }

                break;
        }
    }

    public void GetBrightness(float[] reusableArray)
    {
        //Get audio data
        _buffer.CopyInto(_deviceID, _internalBuffer);
        _sumByte = 0;
        //Get average audio level
        var start = _frequencyRange[0];
        var stop = _frequencyRange[1];
        var length = stop - start;
        if (_soundData == null || _soundData.Length != length)
            _soundData = new byte[length];
        for (int i = 0; i < length; i++)
        {
            _soundData[i] = _internalBuffer[i + start];
            _sumByte += _soundData[i];
        }

        if (_sumByte == _lastSumByte)
            _blackFrameCounter++;
        else
        {
            _lastSumByte = _sumByte;
            _blackFrameCounter = 0;
        }

        if (_blackFrameCounter >= 200 && _noSoundBehavior == NoSoundBehaviorEnum.StayOn) //4s
            FillArray255(_soundData);
        var currentBrightnessLevel = GetAverage(_soundData);

        //manipulate data here

        switch (_visualizerStyle.Type)
        {
            case VisualizerStyleEnum.Brightness:
                BrightnessManipulation(reusableArray, currentBrightnessLevel);
                break;
            case VisualizerStyleEnum.VUMetter:
                VUMeterManipulation(reusableArray, currentBrightnessLevel);
                break;
        }
    }

    public void Dispose()
    {
        Deactivate();
    }
}