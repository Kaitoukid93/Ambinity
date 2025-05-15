using System;
using System.Collections.Generic;
using System.IO;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Repositories;
using Avalonia;
using Avalonia.Gif;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace AmbinityCore.ColorEngines
{
    public sealed class GifAnimationStreamDecoder : IAnimationStreamDecoder
    {
        private readonly LightingZone _zone;
        private readonly GifInstance _gifInstance; // Use GifInstance for GIF decoding
        private readonly int _frameCount;
        private int _currentFrameIndex;
        private readonly object _lock = new();
        private readonly Dictionary<int, SKBitmap> _frameBuffer = new(); // Buffer to store frames on demand

        public GifAnimationStreamDecoder(IAnimation animation, LightingZone zone)
        {
            var gifPath = (animation as GifAnimation)?.AnimationPath ?? throw new ArgumentNullException(nameof(animation));

            if (!File.Exists(gifPath))
            {
                throw new FileNotFoundException("The specified GIF file was not found.", gifPath);
            }
            var gifStream = new FileStream(gifPath, FileMode.Open, FileAccess.Read);
            // Initialize GifInstance for GIF decoding
            _gifInstance = new GifInstance(gifStream);

            // Ensure the file is an animation
            _frameCount = _gifInstance.GifFrameCount;
            if (_frameCount <= 1)
            {
                throw new InvalidOperationException("The file is not a GIF animation.");
            }

            _zone = zone;

            CodecName = "GIF";
            FrameSize = new Size(_gifInstance.GifPixelSize.Width, _gifInstance.GifPixelSize.Height);
        }

        public string CodecName { get; }
        public Size FrameSize { get; }

        private SKBitmap LoadFrame(int frameIndex)
        {
            var writeableBitmap = _gifInstance.ProcessFrameIndex(frameIndex);
            if (writeableBitmap == null)
            {
                throw new InvalidOperationException($"Failed to decode frame {frameIndex}.");
            }

            var skBitmap = new SKBitmap((int)FrameSize.Width, (int)FrameSize.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using (var lockedBitmap = writeableBitmap.Lock())
            {
                int totalBytes = (int)(FrameSize.Width * FrameSize.Height * 4);

                unsafe
                {
                    void* sourcePtr = lockedBitmap.Address.ToPointer();
                    void* destinationPtr = skBitmap.GetPixels().ToPointer();
                    Buffer.MemoryCopy(sourcePtr, destinationPtr, totalBytes, totalBytes);
                }
            }

            // Check if the frame is black
            if (IsFrameBlack(skBitmap))
            {
                skBitmap.Dispose(); // Dispose of the black frame
                throw new InvalidOperationException($"Frame {frameIndex} is completely black and will be skipped.");
            }

            return skBitmap;
        }
        private bool IsFrameBlack(SKBitmap bitmap)
        {
            unsafe
            {
                var pixels = (uint*)bitmap.GetPixels().ToPointer();
                int totalPixels = bitmap.Width * bitmap.Height;

                for (int i = 0; i < totalPixels; i++)
                {
                    if (pixels[i] != 0) // Non-black pixel found
                    {
                        return false;
                    }
                }
            }
            return true; // All pixels are black
        }
        public bool TryDecodeNextFrame(SKBitmap bitmap, SKRect rect)
        {
            lock (_lock)
            {
                if (_frameCount == 0)
                {
                    return false;
                }

                // Load the current frame into the buffer if not already loaded
                while (!_frameBuffer.ContainsKey(_currentFrameIndex))
                {
                    try
                    {
                        var frame = LoadFrame(_currentFrameIndex);
                        _frameBuffer.Add(_currentFrameIndex, frame);
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Skip black frames and move to the next frame
                        Console.WriteLine(ex.Message);
                        _currentFrameIndex = (_currentFrameIndex + 1) % _frameCount;
                    }
                }

                // Get the current frame from the buffer
                var currentFrame = _frameBuffer[_currentFrameIndex];

                // Draw the SKBitmap onto the provided SKCanvas
                using var canvas = new SKCanvas(bitmap);
                canvas.DrawBitmap(currentFrame, rect);

                // Move to the next frame
                _currentFrameIndex = (_currentFrameIndex + 1) % _frameCount;
                return true;
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _gifInstance?.Dispose();
                foreach (var frame in _frameBuffer.Values)
                {
                    frame.Dispose();
                }
                _frameBuffer.Clear();
            }
        }

        public IReadOnlyDictionary<string, string> GetContextInfo()
        {
            return new Dictionary<string, string>
            {
                { "CodecName", CodecName },
                { "FrameWidth", FrameSize.Width.ToString() },
                { "FrameHeight", FrameSize.Height.ToString() },
                { "FrameCount", _frameCount.ToString() }
            };
        }
    }
}
