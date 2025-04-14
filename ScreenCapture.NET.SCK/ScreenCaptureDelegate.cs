using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using CoreMedia;
using CoreVideo;
using ObjCRuntime;
using ScreenCaptureKit;

namespace ScreenCapture.NET.SCK;
public class ScreenCaptureDelegate : NSObject, ISCStreamOutput, INativeObject, IDisposable, ISCStreamDelegate
{
    public ScreenCaptureDelegate(byte[] buffer)
    {
        _buffer = buffer;
    }
    public event Action StreamStopped;
    public int BufferReceived { get; set; }
    private byte[] _buffer;
    [Export("init")]
    public ScreenCaptureDelegate()
    {

    }
    [Foundation.Export("stream:didOutputSampleBuffer:ofType:")]
    public void DidOutputSampleBuffer(SCStream stream, CMSampleBuffer sampleBuffer, SCStreamOutputType type)
    {

        try
        {

            var imageBuffer = sampleBuffer.GetImageBuffer() as CVPixelBuffer;
            if (imageBuffer != null)
            {

                imageBuffer.Lock(lockFlags: CVPixelBufferLock.ReadOnly);
                IntPtr baseAddress = imageBuffer.BaseAddress;
                int bytesPerRow = (int)imageBuffer.BytesPerRow;
                int width = (int)imageBuffer.Width;
                int height = (int)imageBuffer.Height;

                // byte[] buffer = new byte[height * bytesPerRow];
                Marshal.Copy(baseAddress, _buffer, 0, _buffer.Length);
                imageBuffer.Unlock(CVPixelBufferLock.ReadOnly);

                // Use the buffer as needed (e.g., save to a file, process further, etc.)
                sampleBuffer.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

    }
    [Export("stream:didStopWithError:")]
    void DidStop(SCStream stream, NSError error)
    {
        if(error!=null)
        {
            Console.WriteLine(error);
            StreamStopped?.Invoke();
        }
    }
}
