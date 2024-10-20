using AmbinityCore.Models.Profile;
using Draw2D.Core.Graphic;

namespace AmbinityCore.Models.Device.Device;

public class DeviceBitmapCaptureFactory
{
    //Create and manage device bitmap capture
    public DeviceBitmapCaptureFactory(FrameBuffer buffer, LightingProfileDecoder decoder)
    {
        _buffer = buffer;
        _decoder = decoder;
    }

    private FrameBuffer _buffer;
    private readonly LightingProfileDecoder _decoder;

    //Register device for bitmap capturing
    public AmbinityDeviceBitmapCapture RegisterDevice(AmbinityDevice device)
    {
        var capture = new AmbinityDeviceBitmapCapture(_buffer, device,_decoder);
        capture.Init();
        return capture;
    }
}