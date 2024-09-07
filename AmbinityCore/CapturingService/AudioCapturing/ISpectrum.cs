namespace AmbinityCore.CapturingService.AudioCapturing;

public interface ISpectrum : IEnumerable<Band>
{
    int BandCount { get; }

    Band this[int index] { get; }
    Band this[float frequency] { get; }
    Band[] this[float minFrequency, float maxFrequency] { get; }
}