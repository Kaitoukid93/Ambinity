using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.Device;

namespace AmbinityCore.Repositories;

public class AmbinityDeviceRepository
{
    //Manage devices that being added to the system\
    public event Action NewDevicesAdded;

    public AmbinityDeviceRepository(DeviceBitmapCaptureFactory captureFactory,
        SerialControllerRepository serialControllerRepository, OpenRGBControllerRepository openRgbControllerRepository)
    {
        _captureFactory = captureFactory;
        _serialControllerRepository = serialControllerRepository;
        _serialControllerRepository.NewControllerAdded += OnNewControllerAdded;
        _openRGBControllerRepository = openRgbControllerRepository;
        _openRGBControllerRepository.NewControllerAdded += OnNewControllerAdded;

        Devices = new List<AmbinityDevice>();
        foreach (var item in serialControllerRepository.Items)
        {
            var controller = item as SerialController;
            OnNewControllerAdded(controller);
        }
    }

    private void OnNewControllerAdded(IController controller)
    {
        foreach (var output in controller.LedController.Outputs)
        {
            var device = output.Device;
            _captureFactory.RegisterDevice(device);
            if (!Devices.Contains(device))
                Devices.Add(device);
        }

        NewDevicesAdded?.Invoke();
    }

    public void UpdateDeviceTransform()
    {
        foreach (var device in Devices)
        {
            device.TransformLeds();
        }
    }

    private DeviceBitmapCaptureFactory _captureFactory;
    private SerialControllerRepository _serialControllerRepository;
    private readonly OpenRGBControllerRepository _openRGBControllerRepository;
    public List<AmbinityDevice> Devices { get; set; }
}