using OpenRGB.NET;

namespace adrilight
{
    public interface IOpenRGBStream
    {
        bool IsRunning { get; }

        void Start();
        void Stop();
        //bool IsValid();
        void DFU();
        void RefreshTransferState();
        OpenRGBClient AmbinityClient { get; set; }


    }
}