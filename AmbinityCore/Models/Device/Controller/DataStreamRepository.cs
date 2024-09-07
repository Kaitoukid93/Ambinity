using AmbinityCore.DataStream;
using AmbinityCore.OpenRGB;

namespace AmbinityCore.Models.Device.Controller;

public  class DataStreamProvider
{
    private AmbinityOpenRGBClient _client;
    public DataStreamProvider(AmbinityOpenRGBClient client)
    {
        _client = client;
    }
    public  IDataStream CreateDeviceStreamService(IController controller)
    {
        if (controller is SerialController)
            return new SerialStream(controller);
        else if (controller is OpenRGBController)
            return new OpenRGBStream(controller,_client);
        return null;
    }
}