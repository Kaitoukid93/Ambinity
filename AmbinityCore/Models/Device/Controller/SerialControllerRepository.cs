using AmbinityCore.DataStream;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device.Provider;
using Serilog;

namespace AmbinityCore.Models.Device.Controller;

public class SerialControllerRepository : CollectableItemRepository
{
    public event Action<IController> NewControllerAdded;
    public event Action<IController> OldDeviceReconnected;
    public event Action<IController> OldDeviceDetected;
    public event Action<IController> ControllerDisconnected;
    public event Action<IController> LoadingFromDisk;
    private string dbPath => Path.Combine(Constants.AppDataFolder, "Hardwares");
    private string FolderPath => Path.Combine(dbPath, "Controllers", "Serial");
    private readonly SerialControllerProvider _controllerProvider;
    private List<IDataStream> _dataStreams;
    private readonly DataStreamProvider _streamProvider;

    public SerialControllerRepository(SerialControllerProvider controllerProvider, DataStreamProvider streamProvider)
    {
        _streamProvider = streamProvider;
        LocalFolderPath = FolderPath;
        _controllerProvider = controllerProvider;
        _controllerProvider.NewDeviceFound += OnNewDeviceFound;
        Name = "Controllers";
    }

    public override void Init()
    {
        base.Init();
        _controllerProvider.Init();
    }

    public override void RemoveItem(ICollectableItem item)
    {
        base.RemoveItem(item);
        if (item is SerialController controller)
        {
            var stream = GetSerialStream(controller);
            if (stream != null)
            {
                stream.ControllerDisconnected -= SerialControllerDisconnected;
                _dataStreams.Remove(stream);
                stream.Stop();
                _controllerProvider.ControllerDisconnected(controller);
            }
        }
    }

    private async void OnNewDeviceFound(IController controller)
    {
        _controllerProvider.Hold();
        //wait for 1sec because the discovery routine take 1 sec to update
        await Task.Run(() => Task.Delay(1000));
        var result = await RegisterController(controller);
        if (result)
        {
            AddItem(controller);
            NewControllerAdded?.Invoke(controller);
        }

        //wait for serialstream to start first
        _controllerProvider.Resume();

        SaveToDisk();
    }

    private async Task OnOldDeviceLoaded(SerialController controller)
    {
        var result = await RegisterController(controller);
        if (result)
        {
            AddItem(controller);
            NewControllerAdded?.Invoke(controller);
        }

        //wait for serialstream to start first

        SaveToDisk();
    }

    /// <summary>
    /// return fail if controller already exist, true if it's a new one
    /// </summary>
    /// <param name="controller"></param>
    /// <returns></returns>
    private async Task<bool> RegisterController(IController controller)
    {
        bool isNew = false;
        if (_dataStreams == null)
            _dataStreams = new List<IDataStream>();
        var dataStream = GetSerialStream(controller);
        if (dataStream == null)
        {
            dataStream = _streamProvider.CreateDeviceStreamService(controller);
            dataStream.ControllerDisconnected += SerialControllerDisconnected;
            dataStream.Init();
            await Task.Run(() => Task.Delay(2000));
            _dataStreams.Add(dataStream);
            isNew = true;
        }
        else
        {
            //oldevice but the port changed
            var oldController = (dataStream as SerialStream).Controller;
            oldController.SerialPort = controller.SerialPort;
            OldDeviceDetected?.Invoke(oldController);
            if (!dataStream.IsRunning)
                dataStream.Init();
            await Task.Run(() => Task.Delay(2000));
            OldDeviceReconnected?.Invoke(oldController);
            Log.Information("Old Device Reconnected " + oldController.Name);
        }

        return isNew;
    }

    private void SerialControllerDisconnected(IController controller)
    {
        ControllerDisconnected?.Invoke(controller as SerialController);
        _controllerProvider.ControllerDisconnected(controller);
    }

    public IDataStream GetSerialStream(IController controller)
    {
        if (_dataStreams == null)
            return null;
        return _dataStreams
            .FirstOrDefault(d => (d as SerialStream).Port == controller.SerialPort || d.ID == controller.SerialNumber);
    }

    public override void CreateDefault()
    {
        //todo add default controller
    }

    public override async void LoadFromDisk()
    {
        _controllerProvider.Hold();
        //wait for update routine to pickup hold signal
        await Task.Run(() => Task.Delay(1000));
        Items?.Clear();
        string[] files = Directory.GetDirectories(FolderPath);
        foreach (var file in files)
        {
            var controllerPath = Path.Combine(file, "controller.json");
            var controller = JsonHelpers.DeserializeJson<SerialController>(controllerPath);
            if (controller == null)
            {
                Log.Error("Can not load " + file);
                continue;
            }

            controller.LocalPath = controllerPath;
            foreach (var output in controller.LedController.Outputs)
            {
                foreach (var device in output.Devices)
                {
                    device.LoadLayout();
                }
            }

            controller.RegisterLEDController();
            controller.RegisterFanController();
            LoadingFromDisk?.Invoke(controller);
            await Task.Run(() => OnOldDeviceLoaded(controller));
        }

        _controllerProvider.Resume();
    }
}
