using System.IO.Compression;
using AmbinityCore.DataStream;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device.Provider;
using AmbinityServer;
using Serilog;

namespace AmbinityCore.Models.Device.Controller;

public class OpenRGBControllerRepository : CollectableItemRepository
{
    private readonly OpenRGBControllerProvider _controllerProvider;
    private List<IDataStream> _dataStreams;
    private DataStreamProvider _streamProvider;
    private readonly AmbinityClient _client;
    private string dbPath => Path.Combine(Constants.AppDataFolder, "Hardwares");
    private string FolderPath => Path.Combine(dbPath, "Controllers", "OpenRGB");
    public event Action<IController> NewControllerAdded;
    public event Action<IController> OldDeviceReconnected;
    public event Action<IController> OldDeviceDetected;
    public event Action<IController> ControllerDisconnected;
    public event Action<IController> LoadingFromDisk;

    public OpenRGBControllerRepository(OpenRGBControllerProvider controllerProvider, DataStreamProvider streamProvider,
        AmbinityClient client)
    {
        _client = client;
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
        if (result.Item1)
        {
            AddItem(result.Item2);
            await UpdateDeviceSetup(result.Item2,true);
            NewControllerAdded?.Invoke(result.Item2);
           
        }
        //populate default layout that predefined based on hardware type
        controller.LedController.PopulateDefaultLayout();
        //wait for serialstream to start first
        _controllerProvider.Resume();
        SaveToDisk();
    }

    private async Task OnOldDeviceLoaded(OpenRGBController controller)
    {
        var result = await RegisterController(controller);
        AddItem(controller);
        NewControllerAdded?.Invoke(controller);
        SaveToDisk();
    }

    /// <summary>
    /// return fail if controller already exist, true if it's a new one
    /// </summary>
    /// <param name="controller"></param>
    /// <returns></returns>
    private async Task<(bool,OpenRGBController)> RegisterController(OpenRGBController controller)
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
            else
            {
                //refresh the stream
                (dataStream as OpenRGBStream).Refresh();
            }

            await Task.Run(() => Task.Delay(2000));
            OldDeviceReconnected?.Invoke(controller);
            Log.Information("Old Device Reconnected " + controller.Name);
        }

        return (isNew,controller);
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
            .Where(d => (d as OpenRGBStream).Port == controller.SerialPort && d.ID == controller.SerialNumber &&
                        d.Controller.Name == controller.Name)
            .FirstOrDefault();
    }

    public override void CreateDefault()
    {
        //todo add default controller
    }

    private string LegacyOpenRGBFolder => _client.HomeAddress + "ftp/files/OpenRGBDevices";

    public async Task UpdateDeviceSetup(OpenRGBController controller,bool force = false)
    {
        var match = await _client.SftpServer.GetFileByNameMatching(controller.Name,
            LegacyOpenRGBFolder + "/" + controller.HardwareType);
        if (match == null)
        {
            Log.Information("OpenRGB device not is not implemented: " + controller.Name);
        }
        else
        {
            if(!force)
                return;
            Log.Warning("Force update device setup will clear all physical settings of device layouts");
            //add dependencies to single output
            var cachePath = Path.Combine(Constants.CacheFolderPath, match.Name);
            Log.Information("Device found: " + match.FullName);
            Log.Information("Downloading: " + match.FullName);
            Directory.CreateDirectory(Constants.CacheFolderPath);
            _client.SftpServer.DownloadFile(match.FullName, cachePath);
            //extract
            ZipFile.ExtractToDirectory(cachePath, Constants.CacheFolderPath, true);
            // find first folder
            var extractedPath = Directory.GetDirectories(Constants.CacheFolderPath).First();
            if (extractedPath == null)
            {
                Log.Error("Downloaded archive is corrupted");
                return;
            }

            var dependenciesPath = Path.Combine(extractedPath, "dependencies", "SlaveDevices");
            if (!Directory.Exists(dependenciesPath))
            {
                Log.Error("Device contains no dependencies");
                return;
            }

            var thumbnailPath = Path.Combine(extractedPath, "thumbnail.png");
            //rename thumbnail and copy to image folder
            File.Copy(thumbnailPath, Path.Combine(Constants.ImageResourceFolder, controller.Name + ".png"), true);
            //foreach dependency in dependencies, add each dependency to output chain, this dependency is keep in private folder
            var privateDependencies = Path.Combine(controller.LocalPath, "dependencies");
            Directory.CreateDirectory(privateDependencies);
            LocalFileHelpers.CopyDirectory(dependenciesPath, privateDependencies, true);
            //load dependencies
            controller.LedController.Outputs.Clear();
            int outputCount=0;
            foreach (var dir in Directory.GetDirectories(privateDependencies))
            {
                //load layout, each dir represent an separate output
             
                var layout = new AmbinityDeviceLayout(dir);
                controller.LedController.Outputs.Add(new LEDOutput(64,outputCount++,new AmbinityDevice(layout)));
            }
            //finally clear cache for next request
            ClearCache();
        }
    }
    public void ClearCache()
    {
        if (Directory.Exists(Constants.CacheFolderPath))
            Directory.Delete(Constants.CacheFolderPath, true);
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
                foreach (var device in output.Devices)
                {
                    device.LoadLayout();
                }
            }

            controller.RegisterLEDController();
            LoadingFromDisk?.Invoke(controller);
            await Task.Run(() => OnOldDeviceLoaded(controller));
        }

        _controllerProvider.Resume();
    }
}