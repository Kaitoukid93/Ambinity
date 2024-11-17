using System.Diagnostics;
using System.IO.Ports;
using AmbinityCore.Models.Ultilities;
using AmbinityServer;
using AmbinityServer.OnlineItem;
using Serilog;

namespace AmbinityCore.Models.Device.Controller;

public class FirmwareUpdater
{
    public event Action<bool, string> FirmwareUpdateFinish;

    public FirmwareUpdater(DownloadService downloadService, AmbinityClient ambinityClient)
    {
        _ambinityClient = ambinityClient;
        _downloadService = downloadService;
    }

    private string _fwUpdateLog;
    private SerialController _controller;
    private int _currentProgress;
    private readonly DownloadService _downloadService;
    private readonly AmbinityClient _ambinityClient;

    //prepare controller for firmware updating
    public async Task<bool> Init(SerialController controller)
    {
        _controller = controller;
        _fwUpdateLog = string.Empty;
        var result = await EnterDFU();
        return result;
    }

    private async Task<bool> EnterDFU()
    {
        var _serialPort = new SerialPort(_controller.SerialPort, 1200);
        _serialPort.DtrEnable = true;
        _serialPort.ReadTimeout = 5000;
        _serialPort.WriteTimeout = 1000;
        try
        {
            if (!_serialPort.IsOpen)
                _serialPort.Open();
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
            // return false;
        }

        Thread.Sleep(1000);
        if (_serialPort.IsOpen)
            _serialPort?.Close();
        return true;
    }

    public async Task DownloadToDevice(string fwPath, FirmwareInformation firmwareInformation,
        IProgress<ProgressInformation> progress = null)
    {
        //download newest firmware
        if (fwPath == null)
            return;
        if (!File.Exists(fwPath))
            return;
        if (_controller.HardwareType == HardwareTypeEnum.AmbinoHUBV2)
        {
            return;
        }
        if (firmwareInformation.Tool == "Ch55x")
        {
            //check for existence of firmware tool
            if (!Directory.Exists(Constants.FirmwareToolsFolderPath))
            {
                FirmwareUpdateFinish?.Invoke(false, "Firmware tools not found for this device!!!");
                progress.Report(new ProgressInformation("Firmware tools not found for this device!!!", 100));
                return;
            }
            StartCh55xFWTool(fwPath, progress);
        }

        else if (firmwareInformation.Tool == "RPI")
        {
            await Task.Run(() => CopyUf2Fw(fwPath, progress));
        }
    }

    private void StartCh55xFWTool(string fwPath, IProgress<ProgressInformation> progress = null)
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            WorkingDirectory = Constants.FirmwareToolsFolderPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false,
            FileName = "cmd.exe",
            Arguments = "/C vnproch55x " + fwPath
        };
        var proc = new Process()
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        // see below for output handler
        proc.ErrorDataReceived += (sender, e) => proc_DataReceived(sender, e, progress);
        proc.OutputDataReceived += (sender, e) => proc_DataReceived(sender, e, progress);

        proc.Start();

        proc.BeginErrorReadLine();
        proc.BeginOutputReadLine();
        proc.Exited += (sender, e) => proc_FinishUploading(sender, e, progress);
    }

    private void proc_DataReceived(object sender, DataReceivedEventArgs e, IProgress<ProgressInformation> progress)
    {
        if (e.Data != null)
        {
            if (e.Data.Contains("[2K")) //clear current line
            {
                _currentProgress++;
                var percent = _currentProgress * 80 / 308;
                progress.Report(new ProgressInformation("Flashing...", percent));
            }
            else
            {
                _fwUpdateLog += Environment.NewLine + e.Data;
               // progress.Report(new ProgressInformation(e.Data, 0));
                Log.Information(e.Data);
            }
        }
    }

    private void proc_FinishUploading(object sender, System.EventArgs e, IProgress<ProgressInformation> progress)
    {
        Thread.Sleep(5000);

        if (_fwUpdateLog.Split('\n').Last() == "Found no CH55x USB")
        {
            FirmwareUpdateFinish?.Invoke(false, "DFU Device not found");
        }
        else
        {
            // show success
            progress.Report(new ProgressInformation("Firmware is successfully installed", 100));
            FirmwareUpdateFinish?.Invoke(true, "Firmware is successfully installed");
        }
    }

    private async Task CopyUf2Fw(string fwPath, IProgress<ProgressInformation> progress = null)
    {
        progress?.Report(new ProgressInformation("Flashing uf2...", 0));
        var drive = DriveInfo.GetDrives().Where(drv => drv.VolumeLabel == "RPI-RP2").FirstOrDefault();
        if (drive == null)
        {
            FirmwareUpdateFinish?.Invoke(false, "DFU Device not found");
            return;
        }

        string target = drive.RootDirectory.ToString();
        try
        {
            progress?.Report(new ProgressInformation("Flashing uf2...", 50));
            File.Copy(fwPath, Path.Combine(target, Path.GetFileName(fwPath)));
        }
        catch (Exception ex)
        {
        }

        //wait for device 
        progress?.Report(new ProgressInformation("Waiting for device to reboot...", 80));
        Thread.Sleep(2000);
        progress.Report(new ProgressInformation("Firmware is successfully installed", 100));
        FirmwareUpdateFinish?.Invoke(true, "Firmware is successfully installed");
    }
}