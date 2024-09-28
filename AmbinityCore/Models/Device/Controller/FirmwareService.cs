using AmbinityCore.Models.Ultilities;
using AmbinityServer;
using AmbinityServer.OnlineItem;
using Serilog;

namespace AmbinityCore.Models.Device.Controller;

public class FirmwareService
{
    private string _firmwarePath;
    private DownloadService _downloadService;
    private readonly AmbinityClient _client;
    private bool _isInit;

    public FirmwareService(DownloadService downloadService, AmbinityClient client)
    {
        _downloadService = downloadService;
        _firmwarePath = client.HomeAddress + "ftp/files/Firmwares";
        _client = client;
    }

    private async Task<bool> Init()
    {
        var result = await _client.Init();
        if (!result)
        {
            Log.Error("Server or network is not available");
            _isInit = false;
            return false;
        }

        _isInit = true;
        return true;
    }

    public async Task<string> DownloadFirmware(FirmwareInformation firmwareInformation)
    {
        //create cache folder path
        if (Directory.Exists(Constants.CacheFolderPath))
            Directory.Delete(Constants.CacheFolderPath, true);
        Directory.CreateDirectory(Constants.CacheFolderPath);
        return await _downloadService.DownloadItemWithName(firmwareInformation.Path, firmwareInformation.TargetHardware,
            Constants.CacheFolderPath, null);
    }

    public async Task UpdateFirmware(string fwPath, FirmwareInformation firmwareInformation,
        SerialController controller, IProgress<ProgressInformation> progress = null)
    {
        if (fwPath == null)
        {
            progress.Report(new ProgressInformation("Firmware not found!",100));
            return;
        }
           
        var updater = new FirmwareUpdater();
        var result = await updater.Init(controller);
        Log.Information("FirmwareService: Device DFU Mode switch signal is sent");
        //device could already in the dfu mode already, just continue anyway and let updater handle the error
        await updater.DownloadToDevice(fwPath, firmwareInformation, progress);
    }

    /// <summary>
    /// check for update of controller, return update status and firmware version
    /// </summary>
    /// <param name="controller"></param>
    /// <returns></returns>
    public async Task<(bool, FirmwareInformation, List<FirmwareInformation>)> CheckForUpdate(
        SerialController controller)
    {
        if (!_isInit)
        {
            var result = await Init();
            if (!result)
                return (false, null, null);
        }

        var updateAvailable = false;
        var availableFirmwares = await GetAvailableFirmwares(controller);
        if (availableFirmwares == null)
            return (false, null, null);
        var latestFirmware = availableFirmwares.OrderByDescending(i => i.Version).First();
        var latestVersion = new Version(latestFirmware.Version);
        var currentVersion = new Version(controller.FirmwareVersion);
        if (latestVersion > currentVersion)
            updateAvailable = true;
        return (updateAvailable, latestFirmware, availableFirmwares);
    }

    private async Task<List<FirmwareInformation>> GetAvailableFirmwares(SerialController controller)
    {
        var controllerPath =
            _firmwarePath + "/" + controller.HardwareType.ToString() + "/" + controller.HardwareVersion;
        var availableFirmwareFile = await _client.SftpServer.GetAllFilesAddressInFolder(controllerPath);
        if (availableFirmwareFile == null)
            return null;
        var availableFirmwareInfo = new List<FirmwareInformation>();
        foreach (var file in availableFirmwareFile)
        {
            var infoPath = file + "/" + "info.json";
            var firmwareInfo = await _client.SftpServer.GetFiles<FirmwareInformation>(infoPath);
            firmwareInfo.Path = file;
            availableFirmwareInfo.Add(firmwareInfo);
            Log.Information("Firmware available: " + firmwareInfo.Version);
        }


        return availableFirmwareInfo ?? null;
    }
}