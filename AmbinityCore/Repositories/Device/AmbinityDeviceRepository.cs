using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.Device;

namespace AmbinityCore.Repositories;

public class AmbinityDeviceRepository
{
    //Manage devices that being added to the system
    public event Action DevicesListUpdated;

    public AmbinityDeviceRepository(DeviceBitmapCaptureFactory captureFactory,
        SerialControllerRepository serialControllerRepository, OpenRGBControllerRepository openRgbControllerRepository,
        FanOutputServiceFactory fanOutputServiceFactory)
    {
        _fanOutputServiceFactory = fanOutputServiceFactory;
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

        _captures = new List<AmbinityDeviceBitmapCapture>();
    }

    private List<AmbinityDeviceBitmapCapture> _captures;
    public object Lock { get; }= new object();
    private void OnNewControllerAdded(IController controller)
    {
        lock (Lock)
        {
            foreach (var output in controller.LedController.Outputs)
            {
                output.OutputEnabled += OnOutputEnabled;
                output.OutputDisabled += OnOutputDisabled;
                output.DevicesUpdated += OnDevicesUpdated;
                if (!output.IsEnabled)
                    continue;
                foreach (var device in output.Devices)
                {
                    if (!Devices.Contains(device))
                        Devices.Add(device);
                    var capture = _captureFactory.RegisterDevice(device);
                    _captures.Add(capture);
                }
            }

            if (controller.FanController != null)
            {
                foreach (var output in controller.FanController.Outputs)
                {
                    var service = _fanOutputServiceFactory.GetFanOutputService(output);
                    service.Init();
                }
            }

            DevicesListUpdated?.Invoke();
        }
      
    }

//todo implement device remove
    private void OnDevicesUpdated(string action, AmbinityDevice device)
    {
        switch (action)
        {
            case "Remove":
                if (Devices.Contains(device))
                    Devices.Remove(device);
                var capture = GetCapture(device);
                capture?.Dispose();
                _captures.Remove(capture);
                break;
            case "Add":
                if (!Devices.Contains(device))
                    Devices.Add(device);
                var cpt = _captureFactory.RegisterDevice(device);
                _captures.Add(cpt);
                break;
        }
        //remove all devices that attached to this output
    }

    private void OnOutputDisabled(LEDOutput output)
    {
        foreach (var device in output.Devices)
        {
            if (Devices.Contains(device))
                Devices.Remove(device);
            var capture = GetCapture(device);
            capture?.Dispose();
            _captures.Remove(capture);
        }
    }

    private AmbinityDeviceBitmapCapture GetCapture(AmbinityDevice device)
    {
        if (_captures == null || _captures.Count == 0)
            return null;
        var capture = _captures.FirstOrDefault(c => c.Device == device);
        return capture;
    }

    private void OnOutputEnabled(LEDOutput output)
    {
        foreach (var device in output.Devices)
        {
            if (Devices.Contains(device))
                return;
            Devices.Add(device);
            var capture = _captureFactory.RegisterDevice(device);
            _captures.Add(capture);
        }
    }

    // public void UpdateDeviceTransform()
    // {
    //     foreach (var device in Devices)
    //     {
    //         device.TransformLeds();
    //     }
    // }

    private DeviceBitmapCaptureFactory _captureFactory;
    private FanOutputServiceFactory _fanOutputServiceFactory;
    private SerialControllerRepository _serialControllerRepository;
    private readonly OpenRGBControllerRepository _openRGBControllerRepository;
    public List<AmbinityDevice> Devices { get; set; }
}