using System;
using System.Runtime.InteropServices;
using AVFoundation;
using CoreMedia;
using CoreVideo;

namespace ScreenCapture.NET.AV;

public class AVScreenCaptureDelegate : NSObject, IAVCaptureVideoDataOutputSampleBufferDelegate
{
    private byte[] _buffer;
    private object _captureLock;
    public AVScreenCaptureDelegate(byte[] buffer, object captureLock)
    {
        this._buffer = buffer;
        _captureLock = captureLock;
    }
    public int ReceivedBuffer { get; set; }
    [Export("captureOutput:didOutputSampleBuffer:fromConnection:")]
    public void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
    {
        lock (_captureLock)
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
                ReceivedBuffer++;
                imageBuffer.Unlock(CVPixelBufferLock.ReadOnly);

                // Use the buffer as needed (e.g., save to a file, process further, etc.)
            }
        }

    }
}
