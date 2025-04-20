using System;
using AmbinityCore.LightingEngines;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Repositories;
using Draw2D.Core.Graphic;
using FFmpeg.AutoGen.Abstractions;
using SkiaSharp;
using Serilog;

namespace AmbinityCore.ColorEngines;

public class VideoDecoderEngine : IColorEngine
{

    private int _startIndex = 0;
    public LightingZone Zone => _zone;
    private LightingZone _zone;
    public bool IsAvailable { get; private set; } = true;
    private FrameBuffer _buffer;

    // private string _animationFilePath = "C:\\Users\\AMBINO\\Downloads\\moving circle.json";
    private byte[] _reusableRow;
    private VideoAnimation _video;
    private AnimationConfiguration _config;
    private bool _loadingVideo;
    private int _frameRate = 1;
    private readonly AnimationsRepository _videoRepository;
    private AnimationsRepository _currentWorkingRepository;
    private unsafe AVCodecContext* _codecContext;
    private unsafe AVFrame* _frame;
    private unsafe AVPacket* _packet;
    private unsafe AVFormatContext* _formatContext;
    private int _videoStreamIndex;
    private AVHWDeviceType _hwType;
    private bool _useHardwareAcceleration = true;
    public VideoDecoderEngine(FrameBuffer buffer, AnimationsRepository repository)
    {
        _videoRepository = repository;
        _buffer = buffer;
        ConfigureHWDecoder(out _hwType);
        DecodeAllFramesToImages();
    }

    private void ConfigureHWDecoder(out AVHWDeviceType HWtype)
    {
        HWtype = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
        var availableHWDecoders = new Dictionary<int, AVHWDeviceType>();

        if (_useHardwareAcceleration)
        {
            Console.WriteLine("Select hardware decoder:");
            var type = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
            var number = 0;

            while ((type = ffmpeg.av_hwdevice_iterate_types(type)) != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
            {
                Console.WriteLine($"{++number}. {type}");
                availableHWDecoders.Add(number, type);
            }

            if (availableHWDecoders.Count == 0)
            {
                Console.WriteLine("Your system have no hardware decoders.");
                HWtype = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
                return;
            }

            var decoderNumber = availableHWDecoders
                .SingleOrDefault(t => t.Value == AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2).Key;
            if (decoderNumber == 0)
                decoderNumber = availableHWDecoders.First().Key;
            availableHWDecoders.TryGetValue(decoderNumber,
                out HWtype);
        }
    }

    public void Render()
    {
        if (_loadingVideo)
            return;
        int width = (int)_zone.Width;
        int height = (int)_zone.Height;
        int frameCount = 1000;
        //run frame decoder in a loop
        _startIndex += _config.FrameRate;
        ;
        if (_startIndex >= frameCount)
            _startIndex = 0;
        //update frame if needed
        // _zone.UpdateFrame();
    }

    public void Init(LightingZone zone)
    {
        _zone = zone;
        _config = zone.LightingConfiguration as AnimationConfiguration;
        if (_zone.ParentProfile.Assets.Count == 0 || _zone.ParentProfile.AnimationRepository == null)
        {
            _currentWorkingRepository = _videoRepository;
        }
        else
        {
            _currentWorkingRepository = _zone.ParentProfile.AnimationRepository;
        }
        _config.AnimationChanged += OnVideoChanged;
        OnVideoChanged();
    }

    private void OnVideoChanged()
    {
        if (_config.AnimationUID == null || _config.AnimationUID == Guid.Empty)
            return;
        _loadingVideo = true;
        //resolve animation from repo
        var video = _currentWorkingRepository.FindAnimation(_config.AnimationUID);
        if (video == null)
        {
            Log.Error("Resource not found in Animation Repository");
            return;
        }

        video.LoadAnimation();
        // _video = animation.SkottieAnimation;
        // _loadingAnimation = false;
    }
    private unsafe void DecodeAllFramesToImages()
    {
        // decode all frames from url, please not it might local resorce, e.g. string url = "../../sample_mpeg4.mp4";

        var url = "Users/zoe/Desktop/Screen Recording 2025-03-06 at 17.51.53.mov"; // be advised this file holds 1440 frames
        using var vsd = new VideoStreamDecoder(null, null);

        Console.WriteLine($"codec name: {vsd.CodecName}");

        var info = vsd.GetContextInfo();
        info.ToList().ForEach(x => Console.WriteLine($"{x.Key} = {x.Value}"));

        var sourceSize = new Avalonia.Size(vsd.FrameSize.Width, vsd.FrameSize.Height);
        var sourcePixelFormat = _hwType == AVHWDeviceType.AV_HWDEVICE_TYPE_NONE
            ? vsd.PixelFormat
            : GetHWPixelFormat(_hwType);
        var destinationSize = sourceSize;
        var destinationPixelFormat = AVPixelFormat.@AV_PIX_FMT_BGRA;
        using var vfc = new VideoFrameConverter(sourceSize, sourcePixelFormat, destinationSize, destinationPixelFormat);

        var frameNumber = 0;
        // while (vsd.TryDecodeNextFrame(out var frame))
        // {
        //     var convertedFrame = vfc.Convert(frame);
        //     WriteFrame(convertedFrame, frameNumber);

        //     Console.WriteLine($"frame: {frameNumber}");
        //     frameNumber++;
        //     if (frameNumber > 1000) break;
        // }
    }
    private static AVPixelFormat GetHWPixelFormat(AVHWDeviceType hWDevice)
    {
        return hWDevice switch
        {
            AVHWDeviceType.AV_HWDEVICE_TYPE_NONE => AVPixelFormat.AV_PIX_FMT_NONE,
            AVHWDeviceType.AV_HWDEVICE_TYPE_VDPAU => AVPixelFormat.AV_PIX_FMT_VDPAU,
            AVHWDeviceType.AV_HWDEVICE_TYPE_CUDA => AVPixelFormat.AV_PIX_FMT_CUDA,
            AVHWDeviceType.AV_HWDEVICE_TYPE_VAAPI => AVPixelFormat.AV_PIX_FMT_VAAPI,
            AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2 => AVPixelFormat.AV_PIX_FMT_NV12,
            AVHWDeviceType.AV_HWDEVICE_TYPE_QSV => AVPixelFormat.AV_PIX_FMT_QSV,
            AVHWDeviceType.AV_HWDEVICE_TYPE_VIDEOTOOLBOX => AVPixelFormat.AV_PIX_FMT_VIDEOTOOLBOX,
            AVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA => AVPixelFormat.AV_PIX_FMT_NV12,
            AVHWDeviceType.AV_HWDEVICE_TYPE_DRM => AVPixelFormat.AV_PIX_FMT_DRM_PRIME,
            AVHWDeviceType.AV_HWDEVICE_TYPE_OPENCL => AVPixelFormat.AV_PIX_FMT_OPENCL,
            AVHWDeviceType.AV_HWDEVICE_TYPE_MEDIACODEC => AVPixelFormat.AV_PIX_FMT_MEDIACODEC,
            _ => AVPixelFormat.AV_PIX_FMT_NONE
        };
    }

    public void Dispose()
    {
        _config.AnimationChanged -= OnVideoChanged;
    }


    private  unsafe SKBitmap ReadFrame(string frameFile)
    {
        using var codec = SKCodec.Create(frameFile);
        return SKBitmap.Decode(codec);
    }

    public bool IsDisposed { get; }
}
