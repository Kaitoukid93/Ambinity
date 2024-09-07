namespace AmbinityCore.CapturingService.AudioCapturing;


public interface IAudioInput : IDisposable
{
    int SampleRate { get; }
    float MasterVolume { get; }
    void Initialize();
}