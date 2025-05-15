using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AmbinityCore.Repositories;
using Avalonia;
using Serilog;
using SkiaSharp;
using SkiaSharp.Skottie;

namespace AmbinityCore.ColorEngines
{
    /// <summary>
    /// A simple wrapper to make Lottie animation work with
    /// the same animation decoder engine.
    /// In the future, if you have another animation type such as GIF, just
    /// add a new wrapper.
    /// </summary>
    public sealed unsafe class LottieAnimationStreamDecoder : IAnimationStreamDecoder
    {
        private int _frameIndex;
        private readonly Animation _animation;
        private readonly int _frameCount;

        public LottieAnimationStreamDecoder(IAnimation animation)
        {
            _animation = (animation as LottieJsonAnimation)?.SkottieAnimation
                         ?? throw new ArgumentNullException(nameof(animation));
            _frameCount = (int)(_animation.Fps * _animation.Duration.TotalMilliseconds / 1000);

            // Initialize the reusable SKBitmap
            FrameSize = new Size(_animation.Size.Width, _animation.Size.Height);
        }

        public Size FrameSize { get; }

        public void Dispose()
        {
            _animation?.Dispose();
        }

        public bool TryDecodeNextFrame(SKBitmap bitmap, SKRect dst)
        {
            lock (bitmap) // Ensure thread safety when reusing _bitmap
            {
                try
                {
  
                    using (var canvas = new SKCanvas(bitmap))
                    {
                        canvas.Clear(SKColors.Transparent);
                        _animation.SeekFrame(_frameIndex);
                        _animation.Render(canvas, dst);
                    }

                    // Increment the frame index
                    _frameIndex++;
                    if (_frameIndex >= _frameCount)
                        _frameIndex = 0;

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error("Error decoding Lottie animation frame: {Message}", ex.Message);
                    return false;
                }
            }
        }
    }
}
