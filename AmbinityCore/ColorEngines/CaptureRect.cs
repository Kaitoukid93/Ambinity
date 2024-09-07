using Avalonia;

namespace AmbinityCore.LightingEngines;
/// <summary>
/// represent a capture region for led but store original canvas location data
/// </summary>
public class CaptureRect
{
    public CaptureRect(Rect originalRect, Rect translatedRect)
    {
        OriginalRect = originalRect;
        TranslatedRect = translatedRect;
    }

    public Rect OriginalRect { get; set; }
    public Rect TranslatedRect { get; set; }
}