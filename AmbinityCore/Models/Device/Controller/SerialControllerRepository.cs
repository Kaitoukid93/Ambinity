using System.IO.Ports;
using AmbinityCore.DataStream;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device.Provider;
using Serilog;

namespace AmbinityCore.Models.Device.Controller;

public class SerialControllerRepository : CollectableItemRepository
{
    private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Ambinity\\");

    public event Action<SerialController> NewControllerAdded;
    public event Action<SerialController> OldDeviceReconnected;
    public event Action<SerialController> OldDeviceDetected;
    public event Action<SerialController> ControllerDisconnected;
    public event Action<SerialController> LoadingFromDisk;
    private string dbPath => Path.Combine(JsonPath, "Hardwares");
    private string FolderPath => Path.Combine(dbPath, "Controllers");
    private SerialControllerProvider _controllerProvider;
    private List<IDataStream> _dataStreams;

    public SerialControllerRepository(SerialControllerProvider controllerProvider)
    {
        LocalFolderPath = FolderPath;
        _controllerProvider = controllerProvider;
        _controllerProvider.NewDeviceFound += OnNewDeviceFound;
        Name = "Controllers";
    }
    private async void OnNewDeviceFound(SerialController controller)
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
    private async Task<bool> RegisterController(SerialController controller)
    {
        bool isNew = false;
        if (_dataStreams == null)
            _dataStreams = new List<IDataStream>();
        var dataStream = GetSerialStream(controller);
        if (dataStream == null)
        {
            dataStream = DataStreamRepository.CreateDeviceStreamService(controller);
            dataStream.ControllerDisconnected += SerialControllerDisconnected;
            dataStream.Init();
            await Task.Run(() => Task.Delay(2000));
            _dataStreams.Add(dataStream);
            isNew = true;
        }
        else
        {
            //oldevice but the port changed
            (dataStream as SerialStream).Controller.SerialPort = controller.SerialPort;
            OldDeviceDetected?.Invoke( (dataStream as SerialStream).Controller);
            if (!dataStream.IsRunning)
                dataStream.Init();
            await Task.Run(() => Task.Delay(2000));
            OldDeviceReconnected?.Invoke(controller);
        }
        return isNew;
    }
    private void SerialControllerDisconnected(IController controller)
    {
        ControllerDisconnected?.Invoke(controller as SerialController);
    }
    private IDataStream GetSerialStream(IController controller)
    {
        if (_dataStreams == null)
            return null;
        return _dataStreams
            .Where(d => (d as SerialStream).Port == controller.SerialPort || d.ID == controller.SerialNumber)
            .FirstOrDefault();
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
        string[] files = Directory.GetFiles(FolderPath);
        foreach (var file in files)
        {
            
            var controller = JsonHelpers.DeserializeJson<SerialController>(file);
            if (controller == null)
                continue;
            foreach (var output in controller.LedController.Outputs)
            {
                output.Device.LoadLayout();
            }
            controller.RegisterLEDController();
            controller.RegisterFanController();
            LoadingFromDisk?.Invoke(controller);
            await Task.Run(() => OnOldDeviceLoaded(controller));
        }
        _controllerProvider.Resume();
    }

    
}