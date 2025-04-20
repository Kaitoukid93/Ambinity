using System;
using System.Runtime.InteropServices;
using AudioToolbox;
using CoreMedia;
using ObjCRuntime;
using ScreenCaptureKit;
using Serilog;

namespace ScreenCapture.NET.SCK
{
    public class AudioCaptureDelegate : NSObject, ISCStreamOutput, INativeObject, IDisposable, ISCStreamDelegate
    {
        public event Action<float[]> AudioSamplesReceived;
        public event Action StreamStopped;

        [Foundation.Export("stream:didOutputSampleBuffer:ofType:")]
        public void DidOutputSampleBuffer(SCStream stream, CMSampleBuffer sampleBuffer, SCStreamOutputType type)
        {
            try
            {
                if (type == SCStreamOutputType.Audio)
                {
                    // Process audio buffer
                    AudioBuffers outputBuffer = null;
                    var error = sampleBuffer.CopyPCMDataIntoAudioBufferList(0, (int)sampleBuffer.NumSamples, outputBuffer);
                    if (error != null)
                    {
                        Log.Error("Error capturing output buffer");
                    }
                    else
                    {
                        ProcessAudioBuffer(outputBuffer);
                        outputBuffer.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Error processing audio sample buffer: " + e);
            }
        }

        private unsafe void ProcessAudioBuffer(AudioBuffers audioBufferList)
        {
            for (int i = 0; i < audioBufferList.Count; i++)
            {
                IntPtr audioData = audioBufferList[i].Data;
                int audioDataByteSize = audioBufferList[i].DataByteSize;

                // Convert audio data to a float array for processing
                float[] audioSamples = new float[audioDataByteSize / sizeof(float)];
                Marshal.Copy(audioData, audioSamples, 0, audioSamples.Length);

                // Perform further processing on the audioSamples array (e.g., FFT, visualization, etc.)
                Console.WriteLine($"Processed {audioSamples.Length} audio samples.");
            }

        }

        [Export("stream:didStopWithError:")]
        public void DidStop(SCStream stream, NSError error)
        {
            if (error != null)
            {
                Log.Error("Error while capturing audio: " + error.ToString());
            }
            StreamStopped?.Invoke();
        }
    }
}
