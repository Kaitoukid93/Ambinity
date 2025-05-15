using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AVFoundation;
using CoreFoundation;
using CoreMedia;
using CoreVideo;
using HPPH;
using ObjCRuntime;
using ScreenCapture.NET.AV;
using ScreenCaptureKit;

namespace ScreenCapture.NET;
/// <summary>
/// Follow the architecture of this library created by author
/// This class create a session require for screen content (in mac os)
/// Capture screen for whatever interval (you have to implement a loop in calling class)
/// Then store at a buffer with the size calculated by color space and screen resolution
/// Everytime user register a new capture zone, a buffer corresponding for that zone is created
///
/// </summary>

public sealed class AVScreenCapture : AbstractScreenCapture<ColorBGRA>
{
    private SCShareableContent _shareableContent;
    private SCDisplay _selectedDisplay;
    private byte[]? _buffer;
    private int _stride;
    private SCContentFilter _filter;
    private SCStreamConfiguration _streamConfig;
    private SCStream _stream;
    private AVCaptureSession _session;
    private AVCaptureScreenInput _screenInput;
    private AVCaptureVideoDataOutput _videoOutput;
    private OutputRecorder _delegate;
    public readonly object _captureLock = new();
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DX9ScreenCapture"/> class.
    /// </summary>
    /// <param name="direct3D9">The D3D9 instance used.</param>
    /// <param name="display">The <see cref="Display"/> to duplicate.</param>
    internal AVScreenCapture(Display display)
        : base(display)
    {
        Restart();
        OnStreamComplete += StreamComplete;
    }

    private void StreamComplete(NSError error)
    {
        Console.WriteLine(error);
    }

    public NativeHandle Handle { get; set; }

    #endregion

    /// <summary>
    /// Perform a clean restart
    /// </summary>
    public override void Restart()
    {
        base.Restart();
        //get permission
        _session = new AVCaptureSession()
        {
            SessionPreset = AVCaptureSession.PresetMedium
        };
        _screenInput = new AVCaptureScreenInput(displayID: (uint)(Display.Index));
        _screenInput.CapturesCursor = false;
        _screenInput.CapturesMouseClicks = false;
        _videoOutput = new AVCaptureVideoDataOutput();
        lock (_captureLock)
        {
            if (_session.Running)
            {
                StopScreenCapture();
            }

            _screenInput.ScaleFactor = 1.0f; // Capture at full resolution
            _session.BeginConfiguration();
            _session.AddInput(_screenInput);

            var videoSettings = new AVVideoSettingsUncompressed
            {
                PixelFormatType = CVPixelFormatType.CV32BGRA
            };
            _stride = Display.Width * ColorBGRA.ColorFormat.BytesPerPixel;
            var queue = new CoreFoundation.DispatchQueue("myQueue");
            _buffer = new byte[Display.Height * _stride];
            _videoOutput.AlwaysDiscardsLateVideoFrames = true;
            _videoOutput.MinFrameDuration = new CMTime(1, 60);
            _videoOutput.WeakVideoSettings = videoSettings.Dictionary;
            _delegate = new OutputRecorder(_buffer);
            _videoOutput.SetSampleBufferDelegate(_delegate, DispatchQueue.MainQueue);
            _session.AddOutput(_videoOutput);
            _session.CommitConfiguration();
            _session.StartRunning();
        }


    }



    private void StopScreenCapture()
    {
        if (_session.Running)
        {
            _session.StopRunning();
        }
    }
    private Action<NSError> OnStreamComplete;

    /// <inheritdoc />
    protected override void PerformCaptureZoneUpdate(CaptureZone<ColorBGRA> captureZone, Span<byte> buffer)
    {
        if (_buffer == null) return;

        using IDisposable @lock = captureZone.Lock();
        {
            if (captureZone.DownscaleLevel == 0)
                CopyZone(captureZone, buffer);
            else
                DownscaleZone(captureZone, buffer);
        }
    }

    protected override bool PerformScreenCapture()
    {
        bool result = true;
        // SCScreenshotManager.CaptureSampleBuffer(_filter, _streamConfig, (CMSampleBuffer buffer , NSError error) =>
        // {
        //     if (error != null)
        //     {
        //         Console.WriteLine("Error: " + error.ToString());
        //         result = false;
        //     }
        //     else
        //     {
        //         //process image, put data to _buffer to process later
        //         BufferReceived++;
        //     }
        // });
        return result;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyZone(CaptureZone<ColorBGRA> captureZone, Span<byte> buffer)
    {
        RefImage<ColorBGRA>.Wrap(_buffer, Display.Width, Display.Height, _stride)[captureZone.X, captureZone.Y, captureZone.Width, captureZone.Height]
                           .CopyTo(MemoryMarshal.Cast<byte, ColorBGRA>(buffer));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DownscaleZone(CaptureZone<ColorBGRA> captureZone, Span<byte> buffer)
    {
        RefImage<ColorBGRA> source = RefImage<ColorBGRA>.Wrap(_buffer, Display.Width, Display.Height, _stride)[captureZone.X, captureZone.Y, captureZone.UnscaledWidth, captureZone.UnscaledHeight];
        Span<ColorBGRA> target = MemoryMarshal.Cast<byte, ColorBGRA>(buffer);

        int blockSize = 1 << captureZone.DownscaleLevel;

        int width = captureZone.Width;
        int height = captureZone.Height;

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                target[(y * width) + x] = source[x * blockSize, y * blockSize, blockSize, blockSize].Average();
    }
    public class OutputRecorder : AVCaptureVideoDataOutputSampleBufferDelegate
    {
        private byte[] _buffer;
        public OutputRecorder(byte[] buffer)
        {
            _buffer = buffer;
        }
        public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
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


    }
}
