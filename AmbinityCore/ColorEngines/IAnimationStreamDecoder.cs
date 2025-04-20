using SkiaSharp;

namespace AmbinityCore.ColorEngines;
public interface IAnimationStreamDecoder : IDisposable
{
    public bool TryDecodeNextFrame(SKBitmap bitmap, SKRect rect);
    public void Dispose();
}
