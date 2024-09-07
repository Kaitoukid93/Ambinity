using AmbinityCore.DataStream;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device.Provider;
using Serilog;

namespace AmbinityCore.Models.Device.Controller;

public class OpenRGBControllerRepository : CollectableItemRepository
{
    private readonly OpenRGBControllerProvider _controllerProvider;
    private List<IDataStream> _dataStreams;
    private DataStreamProvider _streamProvider;
    private string dbPath => Path.Combine(Constants.AppDataFolder, "Hardwares");
    private string FolderPath => Path.Combine(dbPath, "Controllers", "OpenRGB");
    public event Action<IController> NewControllerAdded;
    public event Action<IController> OldDeviceReconnected;
    public event Action<IController> OldDeviceDetected;
    public event Action<IController> ControllerDisconnected;
    public event Action<IController> LoadingFromDisk;
    public OpenRGBControllerRepository(OpenRGBControllerProvider controllerProvider, DataStreamProvider streamProvider)
    {
        _streamProvider = streamProvider;
        LocalFolderPath = FolderPath;
        _controllerProvider = controllerProvider;
        _controllerProvider.NewDeviceFound += OnNewDeviceFound;
    }

    public override void Init()
    {
         base.Init();
        _controllerProvider.Init();
    }

    private async void OnNewDeviceFound(OpenRGBController controller)
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

    private async Task OnOldDeviceLoaded(OpenRGBController controller)
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
    private async Task<bool> RegisterController(OpenRGBController controller)
    {
        bool isNew = false;
        if (_dataStreams == null)
            _dataStreams = new List<IDataStream>();
        var dataStream = GetOpenRGBStream(controller);
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
            (dataStream as OpenRGBStream).Controller.SerialPort = controller.SerialPort;
            OldDeviceDetected?.Invoke(dataStream.Controller);
            if (!dataStream.IsRunning)
                dataStream.Init();
            await Task.Run(() => Task.Delay(2000));
            OldDeviceReconnected?.Invoke(controller);
            Log.Information("Old Device Reconnected " + controller.Name);
        }

        return isNew;
    }

    private void SerialControllerDisconnected(IController controller)
    {
        ControllerDisconnected?.Invoke(controller as OpenRGBController);
    }

    private IDataStream GetOpenRGBStream(IController controller)
    {
        if (_dataStreams == null)
            return null;
        if (_dataStreams.Count == 0)
            return null;
        return _dataStreams
            .Where(d => (d as OpenRGBStream).Port == controller.SerialPort && d.ID == controller.SerialNumber)
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
        string[] files = Directory.GetDirectories(FolderPath);
        foreach (var file in files)
        {
            var controllerPath = Path.Combine(file, "controller.json");
            var controller = JsonHelpers.DeserializeJson<OpenRGBController>(controllerPath);
            if (controller == null)
            {
                Log.Error("Can not load " + file);
                continue;
            }

            controller.LocalPath = controllerPath;
            foreach (var output in controller.LedController.Outputs)
            {
                output.Device.LoadLayout();
            }

            controller.RegisterLEDController();
            LoadingFromDisk?.Invoke(controller);
            await Task.Run(() => OnOldDeviceLoaded(controller));
        }

        _controllerProvider.Resume();
    }
}