using System.Runtime.InteropServices;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using Serilog;
using ManagedBass;
using ManagedBass.Wasapi;

namespace AmbinityCore.CapturingService.AudioCapturing;

public class AudioCaptureBasic
{
    private float[] _fft;
    private static float _speed1 = 1.0F, _speed2 = 0.20F;
    private AudioDevice _device;
    private bool _isFreed = true;
    private byte[] _smallBuffer;
    private bool _isDisposing;
    private AudioBuffer _buffer;
    public AudioDevice Device => _device;
    public AudioCaptureBasic(AudioDevice device, AudioBuffer buffer)
    {
        _device = device;
        _buffer = buffer;

        _fft = new float[1024];
        _smallBuffer = new byte[32];
    }
    
    public void FreeBassWasapi()
    {
        if (_isFreed)
            return;
        BassWasapi.CurrentDevice = _device.ID;
        bool res = BassWasapi.Free();
        if (!res)
        {
            var err = Bass.LastError;
            Log.Error(err.ToString());
        }

        _isFreed = true;
    }

    public void Dispose()
    {
        if (_isDisposing)
            return;
        FreeBassWasapi();
        _isDisposing = true;
    }
    public void StartBassWasapi()
    {
        BassWasapi.CurrentDevice = _device.ID;
        BassWasapi.Start();
        _isFreed = false;
    }

    public void Init(WasapiProcedure proc)
    {
        bool result =
            BassWasapi.Init(_device.ID, 44100, 0, WasapiInitFlags.Buffer | WasapiInitFlags.AutoFormat, 1f, 0.05f,
                proc);
        if (!result)
        {
            var err = Bass.LastError;
            Log.Error(err.ToString());
        }
        Log.Information("Wasapi device Init OK: " + _device.Name);
    }

    public void Capture()
    {
        try
        {
            var result = GetCurrentFFTFrame();
            if (!result)
            {
                _smallBuffer = new byte[32];
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    private bool GetCurrentFFTFrame()
    {
        BassWasapi.GetData(_fft, (int)DataFlags.FFT2048);

        // Scale to 32 bands
        List<byte> spectrumdata = new List<byte>();
        int x, y;
        int b0 = 0;
        //computes the spectrum data, the code is taken from a bass_wasapi sample.
        for (x = 0; x < 32; x++)
        {
            float peak = 0;
            int b1 = (int)Math.Pow(2, x * 10.0 / (32 - 1));
            if (b1 > 1023) b1 = 1023;
            if (b1 <= b0) b1 = b0 + 1;
            for (; b0 < b1; b0++)
            {
                if (peak < _fft[1 + b0]) peak = _fft[1 + b0];
            }

            y = (int)(Math.Sqrt(peak) * 3 * 250 - 4);
            if (y > 255) y = 255;
            if (y < 10) y = 0;
            spectrumdata.Add((byte)y);
        }

        for (int i = 0; i < 32; i++)
        {
            if (spectrumdata[i] > _smallBuffer[i])
            {
                _smallBuffer[i] += (byte)(_speed1 * (spectrumdata[i] - _smallBuffer[i]));
            }
        
            if (spectrumdata[i] < _smallBuffer[i])
            {
                _smallBuffer[i] -= (byte)(_speed2 * (_smallBuffer[i] - spectrumdata[i]));
            }
        }

        _buffer.Put(_device.ID, _smallBuffer);
        return true;
    }
}