namespace AmbinityCore.CaptureEngines;

public interface ICaptureEngine
{
    ByteFrame[] Frames { get; set; }
    ByteFrame Frame { get; set; }
    void Stop();
    void RefreshCapturingState();
    object Lock { get; }
    int ServiceRequired { get; set; }
}