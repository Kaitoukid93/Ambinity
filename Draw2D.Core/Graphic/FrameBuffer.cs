using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;

namespace Draw2D.Core.Graphic;

public class FrameBuffer
{
    public event Action FrameUpdated; 
    public object FrameLock;
    public FrameBuffer(int frameWidth, int frameHeight)
    {
        FrameWidth = frameWidth;
        FrameHeight = frameHeight;
        FrameLock = new object();
        UpdatePixelData();
    }
    /// <summary>
    /// Width of the image this frame is backing
    /// </summary>
    public int FrameWidth { get; set; }

    /// <summary>
    /// Height of the image this frame is backing
    /// </summary>
    public int FrameHeight { get; set; }

    public byte[] PixelData { get; set; }

    public void UpdatePixelData()
    {
        PixelData = new byte[FrameWidth * FrameHeight * 4];
    }
    public void GetBitmap(WriteableBitmap reusableBitmap)
    {
        if (PixelData == null)
            return;
        using (var frameBuffer = reusableBitmap.Lock())
        {
            Marshal.Copy(PixelData, 0, frameBuffer.Address, PixelData.Length);
        }
    }
}