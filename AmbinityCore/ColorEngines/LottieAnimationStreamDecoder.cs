
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
    /// A simple wrapper to make lottie animation work with
    /// sameanimation decoder engine
    /// in the future, if you have another animation type such as gif, just
    /// add new wrapper
    /// </summary>
    public sealed unsafe class LottieAnimationStreamDecoder : IAnimationStreamDecoder
    {

        private int _frameIndex;
        private Animation _animation;
        private int _frameCount;
        public LottieAnimationStreamDecoder(IAnimation animation)
        {
            _animation = (animation as LottieJsonAnimation).SkottieAnimation;
            _frameCount = (int)(_animation.Fps * _animation.Duration.TotalMilliseconds / 1000);
        }

        public Size FrameSize { get; }

        public void Dispose()
        {
            _animation?.Dispose();
        }

        public bool TryDecodeNextFrame(SKBitmap bitmap, SKRect dst)
        {
            bool result = false;
            try
            {
                using (var canvas = new SKCanvas(bitmap))
                {
                    _animation.SeekFrame(_frameIndex);
                    _animation.Render(canvas, dst);
                    _frameIndex++;
                }
                if (_frameIndex >= _frameCount)
                    _frameIndex = 0;
                return true;

            }
            catch (Exception ex)
            {
                Log.Error("Error decoding Lottie animation frame");
                return false;
            }

        }

    }
}
