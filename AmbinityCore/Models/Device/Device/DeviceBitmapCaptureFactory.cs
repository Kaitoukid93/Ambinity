using Draw2D.Core.Graphic;

namespace AmbinityCore.Models.Device.Device;

public class DeviceBitmapCaptureFactory
{
    //Create and manage device bitmap capture
    public DeviceBitmapCaptureFactory(FrameBuffer buffer)
    {
        _buffer = buffer;
    }

    private FrameBuffer _buffer;

    //Register device for bitmap capturing
    public AmbinityDeviceBitmapCapture RegisterDevice(AmbinityDevice device)
    {
        var capture = new AmbinityDeviceBitmapCapture(_buffer, device);
        capture.Init();
        return capture;
    }
}