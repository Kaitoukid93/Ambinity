namespace AmbinityCore.CaptureEngines;

public interface ICaptureEngine
{
    ByteFrame[] Frames { get; set; }
    ByteFrame Frame { get; set; }
    void Dispose();
    void RefreshCapturingState();
    object Lock { get; }
    int ServiceRequired { get; set; }
}