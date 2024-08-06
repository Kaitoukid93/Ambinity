using AmbinityCore.DataStream;

namespace AmbinityCore.Models.Device.Controller;

public static class DataStreamRepository
{
    public static IDataStream CreateDeviceStreamService(IController controller)
    {
        if (controller is SerialController)
            return new SerialStream(controller);
        return null;
    }
}