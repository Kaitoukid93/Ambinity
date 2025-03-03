using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CoreFoundation;
using CoreMedia;
using HPPH;
using ObjCRuntime;
using ScreenCapture.NET.SCK;
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

public sealed class SCKScreenCapture : AbstractScreenCapture<ColorBGRA>
{
    private SCDisplay _selectedDisplay;
    private byte[]? _buffer;
    private int _stride;
    public double ScalingFactor => _scalingFactor;
    private double _scalingFactor = 0.25d;
    private SCContentFilter _filter;
    private SCStreamConfiguration _streamConfig;
    private SCStream _stream;
    private ScreenCaptureDelegate _delegate;
    public readonly object _captureLock = new();
    public int BufferReceived => _delegate.BufferReceived;
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DX9ScreenCapture"/> class.
    /// </summary>
    /// <param name="display">The <see cref="Display"/> to duplicate.</param>
    /// <param name="scalingFactor">The scale factor value to start new video stream at.</param>
    internal SCKScreenCapture(Display display, double scalingFactor)
        : base(display)
    {
        _scalingFactor = scalingFactor;
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
        lock (_captureLock)
        {
            SCShareableContent.GetShareableContent((SCShareableContent content, NSError error) =>
                  {
                      if (error != null)
                      {
                          Console.WriteLine("Error while requesting shareable content " + error.Code);
                          return;
                      }
                      //get tartget display
                      foreach (var display in content.Displays)
                      {
                          if (display.DisplayId == Display.Index+1) // I have no idea why apple using 1 as the offset for display index...Í
                          {
                              _selectedDisplay = display;
                              break;
                          }
                      }

                      _stride = (int)(Display.Width * _scalingFactor * ColorBGRA.ColorFormat.BytesPerPixel);
                      _buffer = new byte[(int)(Display.Height * _scalingFactor * _stride)];
                      var apps = content.Applications;
                      //config new sreen capture session
                      _filter = new SCContentFilter(_selectedDisplay, [], SCContentFilterOption.Exclude);
                      _streamConfig = new SCStreamConfiguration
                      {
                          Width = (nuint)(Display.Width * _scalingFactor),
                          Height = (nuint)(Display.Height * _scalingFactor),
                          MinimumFrameInterval = new CoreMedia.CMTime(1, 20), // 60 FPS
                          QueueDepth = 5,
                          PixelFormat = CoreVideo.CVPixelFormatType.CV32BGRA,
                          ScalesToFit = false,
                          SourceRect = new CGRect(0, 0, Display.Width, Display.Height),
                          ShowsCursor = false,
                          CaptureResolution = SCCaptureResolutionType.Best,
                          CapturesAudio = false,
                          StreamName = "SCKScreenCapture.NET"

                      };
                      //update registerd zones
                      _delegate = new ScreenCaptureDelegate(_buffer);
                      _stream = new SCStream(_filter, _streamConfig, _delegate);
                      var streamError = new NSError();
                      _stream.AddStreamOutput(_delegate, SCStreamOutputType.Screen, null, out streamError);
                      _stream.StartCapture(OnStreamComplete);
                  });

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
        RefImage<ColorBGRA>.Wrap(_buffer, (int)(Display.Width * _scalingFactor), (int)(Display.Height * _scalingFactor), _stride)[captureZone.X, captureZone.Y, captureZone.Width, captureZone.Height]
                           .CopyTo(MemoryMarshal.Cast<byte, ColorBGRA>(buffer));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DownscaleZone(CaptureZone<ColorBGRA> captureZone, Span<byte> buffer)
    {
        RefImage<ColorBGRA> source = RefImage<ColorBGRA>.Wrap(_buffer, (int)(Display.Width * _scalingFactor), (int)(Display.Height * _scalingFactor), _stride)[captureZone.X, captureZone.Y, captureZone.UnscaledWidth, captureZone.UnscaledHeight];
        Span<ColorBGRA> target = MemoryMarshal.Cast<byte, ColorBGRA>(buffer);

        int blockSize = 1 << captureZone.DownscaleLevel;

        int width = captureZone.Width;
        int height = captureZone.Height;

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                target[(y * width) + x] = source[x * blockSize, y * blockSize, blockSize, blockSize].Average();
    }

}
